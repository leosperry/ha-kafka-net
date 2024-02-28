
namespace HaKafkaNet;

public abstract class DelayableAutomationBase : IDelayableAutomation
{
    EventTiming _timings = EventTiming.PostStartup;
    public EventTiming EventTimings 
    { 
        get => _timings;
        set => _timings = value; 
    }
    public bool ShouldExecutePastEvents { get; set; }
    public bool ShouldExecuteOnContinueError { get; set; }

    protected readonly IEnumerable<string> _triggerEntities;


    public DelayableAutomationBase(IEnumerable<string> triggerEntities,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false)
    {
        _triggerEntities = triggerEntities;
        ShouldExecutePastEvents = shouldExecutePastEvents;
        ShouldExecuteOnContinueError = shouldExecuteOnError;
    }

    public abstract Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken);

    public abstract Task Execute(CancellationToken cancellationToken);

    public IEnumerable<string> TriggerEntityIds() => _triggerEntities;
}
