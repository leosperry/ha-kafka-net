namespace HaKafkaNet;

public abstract class SchedulableAutomationBase : ISchedulableAutomation, IAutomationMeta
{    
    private AutomationMetaData? _meta;
    private DateTime _nextExecution;
    private ReaderWriterLockSlim _lock = new();

    public TimeSpan For => (GetNextScheduled() ?? throw new Exception("blarg")) - DateTime.Now;

    public bool IsReschedulable { get; protected set;}

    public async Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
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
                    _nextExecution = nextEvent.Value;
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

    public abstract Task Execute(CancellationToken cancellationToken);

    public abstract IEnumerable<string> TriggerEntityIds();

    // part of interface
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
            Id = Guid.NewGuid(),
            UnderlyingType = thisType.Name
        };
    }
}

[ExcludeFromDiscovery]
public class SchedulableAutomation : SchedulableAutomationBase
{
    IEnumerable<string> _triggers;
    GetNextEventFromEntityState _getNext;
    private readonly Func<CancellationToken, Task> _execution;

    public SchedulableAutomation(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState getNextEvent,
        Func<CancellationToken, Task> execution) : base()
    {
        _triggers = triggerIds;
        _getNext = getNextEvent;
        _execution = execution;
    }

    public override IEnumerable<string> TriggerEntityIds() => _triggers;

    public override Task<DateTime?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return _getNext(stateChange, cancellationToken);
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return _execution(cancellationToken);
    }
}

public delegate Task<DateTime?> GetNextEventFromEntityState(HaEntityStateChange stateChange, CancellationToken cancellationToken); 
