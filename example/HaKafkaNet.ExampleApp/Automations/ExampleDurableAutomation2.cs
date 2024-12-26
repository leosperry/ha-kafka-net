
namespace HaKafkaNet.ExampleApp;

[ExcludeFromDiscovery] //remove this line in your implementation
public class ExampleDurableAutomation2 : SchedulableAutomationBase
{
    // constants defined for code clarity only
    const bool _shouldExecutePast = true; 
    const bool _shouldExecuteOnError = true; 

    public ExampleDurableAutomation2(IEnumerable<string> triggerIds)
        : base(triggerIds, _shouldExecutePast, _shouldExecuteOnError)
    {
        // set these values appropriately
        // https://github.com/leosperry/ha-kafka-net/wiki/Automation-Types#ischedulableautomation
        this.IsReschedulable = false;

        // https://github.com/leosperry/ha-kafka-net/wiki/Event-Timings#druable
        this.EventTimings = EventTiming.DurableIfCached;
    }

    /// <summary>
    /// This method replaces Continues to be true
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<DateTimeOffset?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        // returning null is the same as ContinuesToBeTrue returning false
        // if you want the automation to continue, you must return a non-null value
        // if your automation is not reschedulable, the value will be ignored

        // in this example we will take an action 1 hour after an entity turns on 
        if (stateChange.ToOnOff().New.State == OnOff.On)
        {
            return Task.FromResult<DateTimeOffset?>(stateChange.New.LastUpdated.AddHours(1));
        }
        
        // the entity was off, cancel execution
        return Task.FromResult<DateTimeOffset?>(default);
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        // you execution logic here
        return Task.CompletedTask;
    }
}
