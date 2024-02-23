
using System.Diagnostics;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class DelayablelAutomationWrapper : IAutomation, IAutomationMeta 
{
    public EventTiming EventTimings { get => _automation.EventTimings; }
    AutomationMetaData _meta;
    private readonly IDelayableAutomation _automation;
    internal IDelayableAutomation WrappedConditional { get => _automation; }

    private readonly ISystemObserver _observer;
    private readonly ILogger _logger;

    private readonly ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();
    private CancellationTokenSource? _cts;

    private Func<TimeSpan> _getDelay;
    private DateTime? _timeForScheduled;

    public DelayablelAutomationWrapper(IDelayableAutomation automation, ISystemObserver observer, ILogger logger, Func<TimeSpan>? evaluator = null)
    {
        this._automation = automation;
        this._observer = observer;
        _logger = logger;

        if (automation is IAutomationMeta metaAuto)
        {
            _meta = metaAuto.GetMetaData();
            _meta.UnderlyingType = _automation.GetType().Name;
        }
        else
        {
            _meta = new AutomationMetaData()
            {
                Name = _automation.GetType().Name,
                Description = _automation.GetType().Name,
                Enabled = true,
                Id = Guid.NewGuid(),
                UnderlyingType = _automation.GetType().Name
            };
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
            lockObj.EnterReadLock();
            try
            {
                alreadyScheduled = _cts is not null;
            }
            finally
            {
                lockObj.ExitReadLock();
            }
            // found out

            if (alreadyScheduled && !schedulableAutomation.IsReschedulable)
            {
                return;
            }

            //we are either not running or running and we need to reschedule
            _timeForScheduled = schedulableAutomation.GetNextScheduled();
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
        return _automation.ContinuesToBeTrue(stateChange, cancellationToken)
            .ContinueWith<bool>(t => {
                if (t.IsFaulted)
                {
                    _observer.OnUnhandledException(this._meta, t.Exception);
                    return _automation.ShouldExecuteOnContinueError;
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
            return ActualExecute(cancellationToken);
        }

        // run with delay
        if (_cts is null)
        {
            // run with delay
            try
            {
                lockObj.EnterUpgradeableReadLock();

                if (_cts is null)
                {
                    lockObj.EnterWriteLock();
                    try
                    {
                        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                        return Task.Delay(delay, _cts.Token).ContinueWith(t => ActualExecute(_cts.Token, CleanUpTokenSource), _cts.Token);
                    }
                    finally
                    {
                        lockObj.ExitWriteLock();
                    }
                }
            }
            finally
            {
                lockObj.ExitUpgradeableReadLock();
            }
        }
        // if we haven't returned by now, another thread has already stared
        return Task.CompletedTask;
    }

    private Task ActualExecute(CancellationToken token, Action? postRun = null)
    {
        using (_logger!.BeginScope("Start [{automationType}]", _meta.UnderlyingType?.GetType().Name ?? _automation.GetType().Name))
        {
            return _automation.Execute(token)
            .ContinueWith(t =>
            {
                this._meta.LastExecuted = DateTime.Now;
                if (t.IsFaulted)
                {
                    _observer.OnUnhandledException(this._meta, t.Exception);
                }
                postRun?.Invoke();
            });
        }
    }

    private void CleanUpTokenSource()
    {
        if (_cts is not null)
        {
            lockObj.EnterWriteLock();
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
                lockObj.ExitWriteLock();
            }
        }
    }

    internal void StopIfRunning(StopReason reason)
    {
        if (_cts is not null)
        {

            lockObj.EnterWriteLock();
            try
            {
                if (_cts is not null)
                {
                    _logger.LogInformation($"{reason} {_meta.UnderlyingType} Named:{_meta.Name} at {DateTime.Now}");              
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
                lockObj.ExitWriteLock();
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
