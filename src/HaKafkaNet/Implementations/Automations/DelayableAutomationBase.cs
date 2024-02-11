
namespace HaKafkaNet;

public abstract class DelayableAutomationBase : IDelayableAutomation
{
    EventTiming _timings = EventTiming.PostStartup;
    public EventTiming EventTimings 
    { 
        get => _timings;
        internal set => _timings = value; 
    }

    protected readonly IEnumerable<string> _triggerEntities;

    bool _shouldExecutePastEvents = false;
    public bool ShouldExecutePastEvents { get => _shouldExecutePastEvents; protected set => _shouldExecutePastEvents = value; }

    bool _shouldExecuteOnError = false;
    public bool ShouldExecuteOnContinueError { get => _shouldExecuteOnError; protected set => _shouldExecuteOnError = value; }

    public DelayableAutomationBase(IEnumerable<string> triggerEntities,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false)
    {
        _triggerEntities = triggerEntities;
        _shouldExecutePastEvents = shouldExecutePastEvents;
        _shouldExecuteOnError = shouldExecuteOnError;
    }

    public abstract Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken);

    public abstract Task Execute(CancellationToken cancellationToken);

    public IEnumerable<string> TriggerEntityIds() => _triggerEntities;
}
