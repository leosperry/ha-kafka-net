using HaKafkaNet;

namespace MyHome.Dev;

/// <summary>
/// https://github.com/leosperry/ha-kafka-net/wiki/Durable-Automations
/// </summary>
[ExcludeFromDiscovery] //remove this line in your implementation
public class ExampleDurableAutomation : ISchedulableAutomation
{
    private DateTime? _nextScheduled;

    public bool IsReschedulable => true;

    /// <summary>
    /// This property is a part of IAutomation and has a default implementation
    /// To create a durable automation, you should override this behavior
    /// </summary>
    public EventTiming EventTimings { get => EventTiming.Durable; }

    /// <summary>
    /// This property is a part of IDelayableAutomation and has a default implementation
    /// If you want to handle events when the time elapsed prior to start up (during a restart)
    /// you should override this behavior and return true.
    /// </summary>
    public bool ShouldExecutePastEvents { get => true; }

    public ExampleDurableAutomation(/*inject any services you need*/)
    {
        
    }

    public Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken ct)
    {
        /*
        this method will be called when a state change happens
        you should track what time you want the automation to run
        in this case you would set _nextScheduled.
        If this method returns true, GetNextScheduled will be called
        */
        bool shouldContinue = false; // add your logic here
        if (shouldContinue)
        {
            // set _nextScheduled
            _nextScheduled = haEntityStateChange.New.LastChanged.AddMinutes(5);
        }
        else
        {
            _nextScheduled = null;
        }
        return Task.FromResult(shouldContinue);
    }

    public Task Execute(CancellationToken ct)
    {
        // add your execution logic here
        return Task.CompletedTask;
    }

    public DateTime? GetNextScheduled()
    {
        /*
        the framework will call this method to get the next scheduled time
        if you return null here, it will cancel the automation the same
        as if ContinuesToBeTrue returned false
        */
        return _nextScheduled;
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "domain.entity";
    }
}
