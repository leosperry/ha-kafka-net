﻿using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class DelayablelAutomationWrapper : IAutomationWrapper<IDelayableAutomation>
{
    public EventTiming EventTimings { get => _automation.EventTimings; }
    AutomationMetaData _meta;
    private readonly IDelayableAutomation _automation;
    public IDelayableAutomation WrappedAutomation { get => _automation; }

    readonly IAutomationTraceProvider _trace;
    private readonly ILogger _logger;

    private readonly SemaphoreSlim lockObj = new (1);
    private CancellationTokenSource? _cts;

    private Func<TimeSpan> _getDelay;
    private DateTime? _timeForScheduled;

    public DelayablelAutomationWrapper(IDelayableAutomation automation, IAutomationTraceProvider traceProvider, ILogger logger, Func<TimeSpan>? evaluator = null)
    {
        this._automation = automation;
        this._trace = traceProvider;
        _logger = logger;

        if (automation is IAutomationMeta metaAuto)
        {
            _meta = metaAuto.GetMetaData();
            _meta.UnderlyingType = _automation.GetType().Name;
        }
        else
        {
            _meta = AutomationMetaData.Create(this.WrappedAutomation);
        }
        _meta.IsDelayable = true;

        if (automation is ISchedulableAutomation schedulableAutomation)
        {
            _getDelay = () => _timeForScheduled switch
            {
                null => TimeSpan.MinValue,
                var time => time.Value - DateTime.Now
            };
        }
        else if (automation is IConditionalAutomation conditional)
        {
            _getDelay = () => conditional.For;
        }
        else 
        {
            _getDelay = evaluator ?? throw new HaKafkaNetException("Delay evaluator not provided");
        }
    }

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        if (_automation is ISchedulableAutomation schedulableAutomation)
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
            _logger.LogDebug("GetNextScheduled returned {scheduleTime}", _timeForScheduled);
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

        if (delay == TimeSpan.Zero)
        {
            _logger.LogDebug("automation scheduled now");
            return ActualExecute(cancellationToken);
        }

        // run with delay
        if (_cts is null)
        {
            _logger.LogDebug("automation scheduled in {automationCalculatedDelay}", delay);
            _meta.NextScheduled = DateTime.Now + delay;
            // run with delay
            try
            {
                lockObj.Wait();

                if (_cts is null)
                {
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    return Task.Delay(delay, _cts.Token).ContinueWith(t => ActualExecute(_cts.Token, CleanUpTokenSource), _cts.Token);
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

    private async Task ActualExecute(CancellationToken token, Action? postRun = null)
    {
        this._meta.LastExecuted = DateTime.Now;
        this._meta.NextScheduled = null;
        var evt = new TraceEvent(){
            EventType = "Delayed-Execution",
            AutomationKey = this._meta.GivenKey,
            EventTime = DateTime.Now,
        };
        try
        {
            await _trace.Trace(evt, _meta, () => _automation.Execute(token));
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

    internal void StopIfRunning(StopReason reason)
    {
        if (_cts is not null)
        {
            lockObj.Wait();
            try
            {
                if (_cts is not null)
                {
                    _logger.LogInformation("Automation was running at {stopTime} and is being stopped because {stopReason}", DateTime.Now, reason.ToString());
                    _meta.NextScheduled = null;
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

    public AutomationMetaData GetMetaData() => _meta;
}

internal enum StopReason
{
    Canceled,
    Rescheduled,
    Disabled
}
