using HaKafkaNet;
using System.Text.Json;

namespace HaKafkaNet.ExampleApp.Automations;

public class UpdatingEntityRegistry : IAutomationRegistry
{
    readonly IAutomationBuilder _builder;
    readonly IHaApiProvider _api;
    readonly IHaEntity<float?, JsonElement> _illuminationSensor;

    public UpdatingEntityRegistry(IUpdatingEntityProvider updatingEntityProvider, 
        IAutomationBuilder builder, IHaApiProvider api)
    {
        this._builder = builder;
        this._api = api;

        this._illuminationSensor = updatingEntityProvider.GetFloatEntity("sensor.illumination_sensor");
    }

    public void Register(IRegistrar reg)
    {
        reg.Register(_builder.CreateSimple()
            .WithName("Turn On Light")
            .WithTriggers("binary_sensor.motion_sensor")
            .WithExecution(async (sc, ct) => {
                if (sc.ToOnOff().New.IsOn() && _illuminationSensor.State < 100)
                {
                    await _api.TurnOn("light.my_light");
                }
            })
            .Build());
    }
}