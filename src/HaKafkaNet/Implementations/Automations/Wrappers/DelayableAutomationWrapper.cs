using HaKafkaNet.Implementations.Core;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal abstract class DelayableAutomationWrapper
{
    protected internal abstract void StopIfRunning(StopReason reason);
}


[ExcludeFromDiscovery]
internal class DelayableAutomationWrapper<T> : DelayableAutomationWrapper, IAutomationWrapper where T : IDelayableAutomation 
{
    public EventTiming EventTimings { get => _automation.EventTimings; }
    AutomationMetaData? _meta;
    private readonly T _automation;
    public IAutomationBase WrappedAutomation { get => _automation; }

    public bool IsActive => _automation.IsActive;

    readonly IAutomationTraceProvider _trace;
    private readonly TimeProvider _timeProvider;
    private readonly IAutomationActivator _activator;
    private readonly ILogger _logger;

    private readonly SemaphoreSlim lockObj = new (1);
    private CancellationTokenSource? _cts;

    private Func<TimeSpan> _getDelay;
    private DateTimeOffset? _timeForScheduled;

    public DelayableAutomationWrapper(T automation, IAutomationTraceProvider traceProvider, TimeProvider timeProvider, IAutomationActivator activator, ILogger<T> logger)
    {
        this._automation = automation;
        this._trace = traceProvider;
        this._timeProvider = timeProvider;
        this._activator = activator;
        _logger = logger;

        IAutomationBase target = ((IAutomationWrapperBase)this).GetRoot();

        if (automation is ISchedulableAutomationBase || (automation is TypedDelayedAutomationWrapper typed && typed.WrappedAutomation is ISchedulableAutomationBase))
        {
            _getDelay = () => _timeForScheduled switch
            {
                null => TimeSpan.MinValue,
                var time => time.Value - _timeProvider.GetLocalNow().LocalDateTime
            };
        }
        else if (automation is IConditionalAutomationBase conditional)
        {
            _getDelay = () => conditional.For;
        }
        else if(automation is TypedDelayedAutomationWrapper typed2 && typed2.WrappedAutomation is IConditionalAutomationBase conditional2)
        {
            _getDelay = () => conditional2.For;
        }
        else 
        {
            throw new HaKafkaNetException("Delay evaluator not provided");
        }
    }

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        ISchedulableAutomationBase? schedulableAutomation
            = _automation as ISchedulableAutomationBase ?? (_automation as TypedDelayedAutomationWrapper)?.WrappedAutomation as ISchedulableAutomationBase;

        if (schedulableAutomation is not null)
        {
            var previous = _timeForScheduled;

            // should we continue
            var shouldContinue = await InternalContinueToBeTrue(stateChange, cancellationToken);

            if (!shouldContinue)
            {
                StopIfRunning(StopReason.Canceled);
                return;
            }

            // we need to be running
            //find out if we are
            bool alreadyScheduled;
            await lockObj.WaitAsync();
            try
            {
                alreadyScheduled = _cts is not null;
            }
            finally
            {
                lockObj.Release();
            }
            // found out

            if (alreadyScheduled && !schedulableAutomation.IsReschedulable)
            {
                return;
            }

            //we are either not running or running and we need to reschedule
            _timeForScheduled = schedulableAutomation.GetNextScheduled();
            _logger.LogDebug("GetNextScheduled returned {scheduleTime}", _timeForScheduled?.LocalDateTime);
            if (!alreadyScheduled)
            {
                _ = StartIfNotStarted(cancellationToken);
                return;
            }

            //if the time hasn't changed, don't do anything
            if (previous == _timeForScheduled)
            {
                return;
            }

            //for sure we need to reschedule
            StopIfRunning(StopReason.Rescheduled);
            _ = StartIfNotStarted(cancellationToken);
        }
        else
        {
            // handle all other delayed automations
            bool shouldContinue = await InternalContinueToBeTrue(stateChange, cancellationToken);

            if (shouldContinue)
            {
                // cannot await here. it will block
                _ = StartIfNotStarted(cancellationToken);
            }
            else
            {
                StopIfRunning(StopReason.Canceled);
            }
        }
    }

    public Task<bool> InternalContinueToBeTrue(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        // this is running inside IAutomation.Execute() and a trace has already been started.
        return _automation.ContinuesToBeTrue(stateChange, cancellationToken)
            .ContinueWith<bool>(t => {
                if (t.IsFaulted)
                {
                    return _automation.ShouldExecuteOnContinueError;
                }
                else
                {
                    this._logger.LogTrace("ContinuesToBeTrue returned {continueResult}", t.Result);
                }
                return t.Result;
            }, cancellationToken);
    }

    private Task StartIfNotStarted(CancellationToken cancellationToken)
    {
        TimeSpan delay = _getDelay();

        if (delay == TimeSpan.MinValue)
        {
            return Task.CompletedTask;
        }

        if (delay < TimeSpan.Zero)
        {
            if (_automation.ShouldExecutePastEvents)
            {
                delay = TimeSpan.Zero;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        // run with delay
        if (_cts is null)
        {
            _logger.LogDebug("automation scheduled in {automationCalculatedDelay}", delay);
            GetMetaData().NextScheduled = _timeProvider.GetLocalNow().LocalDateTime + delay;
            // run with delay
            try
            {
                lockObj.Wait();

                if (_cts is null)
                {
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    return CustomDelay(delay, _cts.Token).ContinueWith(t => ActualExecute(_cts.Token, CleanUpTokenSource), _cts.Token);
                }
            }
            finally
            {
                lockObj.Release();
            }
        }
        // if we haven't returned by now, another thread has already stared
        _logger.LogDebug("automation is already scheduled");
        return Task.CompletedTask;
    }

    static TimeSpan maxDelay = TimeSpan.FromDays(49); // ~ number of milliseconds in uint.MaxValue

    private async Task CustomDelay(TimeSpan delay, CancellationToken ct)
    {
        if (delay > maxDelay)
        {
            var next = _timeProvider.GetLocalNow() + delay;

            PeriodicTimer timer = new PeriodicTimer(maxDelay, _timeProvider);

            while(await timer.WaitForNextTickAsync(ct))
            {
                var now = _timeProvider.GetLocalNow();
                delay = next - now;

                if (delay < maxDelay)
                {
                    timer.Dispose();
                }
            }
        }
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, _timeProvider, ct);
        }
    }

    private async Task ActualExecute(CancellationToken token, Action? postRun = null)
    {
        var meta = GetMetaData();
        meta.LastExecuted = _timeProvider.GetLocalNow().LocalDateTime;
        meta.NextScheduled = null;
        var evt = new TraceEvent(){
            EventType = "Delayed-Execution",
            AutomationKey = meta.GivenKey,
            EventTime = _timeProvider.GetLocalNow().LocalDateTime,
        };
        try
        {
            await _trace.Trace(evt, meta, () => _automation.Execute(token));
            if (this.IsActive)
            {
                await _activator.Activate(this, token);
            }
        }
        finally
        {
            postRun?.Invoke();
        }
    }

    private void CleanUpTokenSource()
    {
        if (_cts is not null)
        {
            lockObj.Wait();
            try
            {
                if (_cts is not null)
                {
                    _cts.Dispose();
                    _cts = null;
                }
            }
            finally
            {
                lockObj.Release();
            }
        }
    }

    protected internal override void StopIfRunning(StopReason reason)
    {
        if (_cts is not null)
        {
            lockObj.Wait();
            try
            {
                if (_cts is not null)
                {
                    _logger.LogInformation("Automation was running at {stopTime} and is being stopped because {stopReason}", _timeProvider.GetLocalNow().LocalDateTime, reason.ToString());
                    GetMetaData().NextScheduled = null;
                    try
                    {
                        _cts.Cancel();
                    }
                    finally
                    {
                        _cts.Dispose();
                        _cts = null;
                    }                
                }
            }
            finally
            {
                lockObj.Release();
            }
        }
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _automation.TriggerEntityIds();
    }

    public AutomationMetaData GetMetaData()
    {
        return _meta ??= GetOrMakeMetaData();
    }

    private AutomationMetaData GetOrMakeMetaData()
    {
        IAutomationMeta? autoImplementingMeta = _automation as IAutomationMeta;
        IAutomationBase target = _automation;
        while(autoImplementingMeta is null && target is IAutomationWrapperBase targetWrapper)
        {
            target = targetWrapper.WrappedAutomation;
            autoImplementingMeta = target as IAutomationMeta;
        }

        var meta = autoImplementingMeta is null ? AutomationMetaData.Create(target) : autoImplementingMeta.GetMetaData();
        meta.IsDelayable = true;
        return meta;
    }
}

internal enum StopReason
{
    Canceled,
    Rescheduled,
    Disabled
}
