namespace HaKafkaNet;

public abstract class SchedulableAutomationBase : DelayableAutomationBase, ISchedulableAutomation, IAutomationMeta
{    
    private AutomationMetaData? _meta;
    private DateTime? _nextExecution;
    private ReaderWriterLockSlim _lock = new();

    public bool IsReschedulable { get; set;}

    public override sealed async Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        _lock.EnterUpgradeableReadLock();
        try
        {
            var scheduled = this._nextExecution;
            var nextEvent = await this.CalculateNext(haEntityStateChange, cancellationToken);

            if (scheduled != nextEvent)
            {
                try
                {
                    _lock.EnterWriteLock();
                    _nextExecution = nextEvent;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            
            return nextEvent is not null;
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public SchedulableAutomationBase(IEnumerable<string> triggerIds,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false): base(triggerIds, shouldExecutePastEvents, shouldExecuteOnError) 
        {
            
        }

    public abstract Task<DateTime?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken);

    /// <summary>
    /// Thread safe way of getting a copy of the currently next scheduled time
    /// </summary>
    /// <returns></returns>
    public DateTime? GetNextScheduled()
    {
        try
        {
            _lock.EnterReadLock();
            return _nextExecution;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    }

    public virtual AutomationMetaData GetMetaData()
    {
        var thisType = this.GetType();
        return _meta ??= new AutomationMetaData()
        {
            Name = thisType.Name,
            Description = thisType.FullName,
            Enabled = true,
            UnderlyingType = thisType.Name
        };
    }
}

[ExcludeFromDiscovery]
public class SchedulableAutomation : SchedulableAutomationBase
{
    GetNextEventFromEntityState _getNext;
    private readonly Func<CancellationToken, Task> _execution;

    public SchedulableAutomation(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState getNextEvent,
        Func<CancellationToken, Task> execution,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false,
        bool reschedudulable = false) : base(triggerIds, shouldExecutePastEvents, shouldExecuteOnError)
    {
        _getNext = getNextEvent;
        _execution = execution;
        IsReschedulable = reschedudulable;
    }

    public override Task<DateTime?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return _getNext(stateChange, cancellationToken);
        }
        return Task.FromResult<DateTime?>(null);
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return _execution(cancellationToken);
        }
        return Task.CompletedTask;
    }
}

[ExcludeFromDiscovery]
public class SchedulableAutomationWithServices: SchedulableAutomationBase
{
    private readonly IHaServices _services;

    Func<IHaServices, HaEntityStateChange, CancellationToken, Task<DateTime?>> _getNext;
    private readonly Func<IHaServices, CancellationToken, Task> _execution;

    public SchedulableAutomationWithServices(
        IHaServices services, IEnumerable<string> triggerIds, 
        Func<IHaServices, HaEntityStateChange, CancellationToken, Task<DateTime?>> getNextEvent,
        Func<IHaServices, CancellationToken, Task> execution,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false) : base(triggerIds, shouldExecutePastEvents, shouldExecuteOnError)
    {
        _services = services;
        _getNext = getNextEvent;
        _execution = execution;
    }

    public override Task<DateTime?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return _getNext(_services, stateChange, cancellationToken);
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return _execution(_services, cancellationToken);
    }
}

public delegate Task<DateTime?> GetNextEventFromEntityState(HaEntityStateChange stateChange, CancellationToken cancellationToken); 
