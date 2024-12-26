namespace HaKafkaNet;

public abstract class SchedulableAutomationBase : DelayableAutomationBase, ISchedulableAutomation, IAutomationMeta, ISetAutomationMeta
{    
    private AutomationMetaData? _meta;
    protected DateTimeOffset? _nextExecution;
    private SemaphoreSlim _lock = new(1);

    public bool IsReschedulable { get; set;}

    public override sealed async Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        var nextEvent = await this.CalculateNext(haEntityStateChange, cancellationToken);

        if (this._nextExecution != nextEvent)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                _nextExecution = nextEvent;
            }
            finally
            {
                _lock.Release();
            }
        }
        return nextEvent is not null;
    }

    public SchedulableAutomationBase(IEnumerable<string> triggerIds,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false): base(triggerIds, shouldExecutePastEvents, shouldExecuteOnError) 
        {
            
        }

    public abstract Task<DateTimeOffset?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken);

    /// <summary>
    /// Thread safe way of getting a copy of the currently next scheduled time
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset? GetNextScheduled()
    {
        try
        {
            _lock.Wait();
            return _nextExecution;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void SetMeta(AutomationMetaData meta)
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
    private readonly GetNextEventFromEntityState _getNext;
    private readonly Func<CancellationToken, Task> _execution;

    public SchedulableAutomation(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState getNextEvent,
        Func<CancellationToken, Task> execution,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false,
        bool reschedulable = false) : base(triggerIds, shouldExecutePastEvents, shouldExecuteOnError)
    {
        _getNext = getNextEvent;
        _execution = execution;
        IsReschedulable = reschedulable;
    }

    public override Task<DateTimeOffset?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return _getNext(stateChange, cancellationToken);
        }
        return Task.FromResult<DateTimeOffset?>(null);
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
public class SchedulableAutomation<Tstate, Tatt> : DelayableAutomationBase<Tstate, Tatt>, ISchedulableAutomation<Tstate, Tatt>
{
    private readonly GetNextEventFromEntityState<Tstate, Tatt> _getNext;
    private readonly Func<CancellationToken, Task> _execution;

    private DateTimeOffset? _nextExecution;
    private SemaphoreSlim _lock = new(1);

    
    public SchedulableAutomation(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState<Tstate, Tatt> getNextEvent,
        Func<CancellationToken, Task> execution,
        bool isReschedulable = false,
        bool shouldExecutePastEvents = false, 
        bool shouldExecuteOnError = false) 
        : base(triggerIds)
    {
        _getNext = getNextEvent;
        _execution = execution;
        base.ShouldExecutePastEvents = shouldExecutePastEvents;
        base.ShouldExecuteOnContinueError = shouldExecuteOnError;
        this.IsReschedulable = isReschedulable;
    }

    public bool IsReschedulable { get; internal set; }

    public override async Task<bool> ContinuesToBeTrue(HaEntityStateChange<HaEntityState<Tstate, Tatt>> stateChange, CancellationToken ct)
    {
        var nextEvent = await this.CalculateNext(stateChange, ct);

        if (this._nextExecution != nextEvent)
        {
            await _lock.WaitAsync(ct);
            try
            {
                _nextExecution = nextEvent;
            }
            finally
            {
                _lock.Release();
            }
        }
        return nextEvent is not null;
    }

    public override Task Execute(CancellationToken ct)
    {
        if (!ct.IsCancellationRequested)
        {
            return _execution(ct);
        }
        return Task.CompletedTask;
    }

    public DateTimeOffset? GetNextScheduled()
    {
        try
        {
            _lock.Wait();
            return _nextExecution;
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<DateTimeOffset?> CalculateNext(HaEntityStateChange<HaEntityState<Tstate, Tatt>> stateChange, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return _getNext(stateChange, cancellationToken);
        }
        return Task.FromResult<DateTimeOffset?>(null);
    }
}

public delegate Task<DateTimeOffset?> GetNextEventFromEntityState(HaEntityStateChange stateChange, CancellationToken cancellationToken); 
public delegate Task<DateTimeOffset?> GetNextEventFromEntityState<Tstate, Tatt>(HaEntityStateChange<HaEntityState<Tstate, Tatt>> stateChange, CancellationToken cancellationToken); 
