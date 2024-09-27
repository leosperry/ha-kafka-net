using HaKafkaNet;
namespace MyHome.Dev;

/// <summary>
/// https://github.com/leosperry/ha-kafka-net/wiki/Tutorial:-Creating-Automations
/// </summary>
[ExcludeFromDiscovery] //remove this line in your implementation
public class MotionBehaviorTutorial : IAutomation, IAutomationMeta
{
    readonly string _motion, _light;
    readonly IHaServices _services;
    public MotionBehaviorTutorial(string motion, string light, IHaServices services)
    {
        _motion = motion;
        _light = light;
        _services = services;    
    }

    public IEnumerable<string> TriggerEntityIds() => [_motion];

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        var homeState = await _services.EntityProvider.GetPersonEntity("person.name", cancellationToken);
        var isHome = homeState?.IsHome() ?? false;

        if (isHome)
            await _services.Api.TurnOn(_light);
        else
            await _services.Api.NotifyGroupOrDevice(
                "device_tracker.my_phone", $"Motion was detected by {_motion}", cancellationToken: cancellationToken);
    }

    public AutomationMetaData GetMetaData() =>
        new()
        {
            Name = $"Motion Behavior{_motion}",
            Description = $"Turn on {_light} if we're home, otherwise notify",
            AdditionalEntitiesToTrack = [_light]
        };
}

static class FactoryExtensions
{
    public static IAutomation CreateMotionBehavior(this IAutomationFactory factory, string motion, string light)
        => new MotionBehaviorTutorial(motion, light, factory.Services);
}