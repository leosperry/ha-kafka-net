
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

        if (automation is ISchedulableAutomation schedulableAutomation)
        {
            _getDelay = () => (schedulableAutomation.GetNextScheduled() ?? DateTime.Now) - DateTime.Now;
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
        // need ot call ContinuesToBeTrue
        bool shouldContinue;
        if (_automation is ISchedulableAutomation schedulableAutomation && schedulableAutomation.IsReschedulable)
        {
            var previous = schedulableAutomation.GetNextScheduled();
            shouldContinue = await InternalContinueToBeTrue(stateChange, cancellationToken);
            var next = schedulableAutomation.GetNextScheduled();
            if (previous != next)
            {
                StopIfRunning();
            }
        }
        else
        {
            shouldContinue = await InternalContinueToBeTrue(stateChange, cancellationToken);
        }

        if (shouldContinue)
        {
            // cannot await here. it will block
            _ = StartIfNotStarted(cancellationToken);
        }
        else
        {
            StopIfRunning();
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

                        return Task.Delay(delay, _cts.Token).ContinueWith(t => ActualExecute(cancellationToken, CleanUpTokenSource), _cts.Token);
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
        using (_logger!.BeginScope("Start [{automationType}]", _automation.GetType().Name))
        {
            return _automation.Execute(token)
            .ContinueWith(t =>
            {
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

    internal void StopIfRunning()
    {
        if (_cts is not null)
        {

            lockObj.EnterWriteLock();
            try
            {
                if (_cts is not null)
                {
                    _logger.LogInformation("Canceling {automation}", _automation.GetType().Name);
                    try
                    {
                        _cts.CancelAsync();
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
