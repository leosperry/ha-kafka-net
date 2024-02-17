
namespace HaKafkaNet;

[ExcludeFromDiscovery]
public class LightOnMotionAutomation : SimpleAutomationBase
{
    private readonly List<string> _motionSensors = new();
    private readonly List<string> _lights = new();
    private readonly IHaServices _services;

    public LightOnMotionAutomation(IEnumerable<string> motionSensor, IEnumerable<string> light, IHaServices entityProvider)
        : base(motionSensor, EventTiming.PostStartup)
    {
        _motionSensors.AddRange(motionSensor);
        _lights.AddRange(light);
        this._services = entityProvider;
    }

    public override Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        if (stateChange.New.State == "on")
        {
            //turn on any lights that are not
            return Task.WhenAll(
                from lightId in _lights
                select _services.EntityProvider.GetOnOffEntity(lightId, cancellationToken)
                    .ContinueWith(t => 
                        t.Result!.State == OnOff.Off
                            ? _services.Api.TurnOn(lightId, cancellationToken)
                            : Task.CompletedTask
                    , cancellationToken, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current)
            );
        }
        return Task.CompletedTask;
    }
}
