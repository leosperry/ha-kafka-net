
namespace HaKafkaNet.ExampleApp;

/// <summary>
/// https://github.com/leosperry/ha-kafka-net/wiki/Scene-Controllers
/// </summary>
[ExcludeFromDiscovery] //remove this line in your implementation
public class SceneControllerAutomation : IAutomation
{
    public Task Execute(HaEntityStateChange stateChange, CancellationToken ct)
    {
        var sceneState = stateChange.ToSceneControllerEvent();
        if (!sceneState.New.StateAndLastUpdatedWithin1Second()) return Task.CompletedTask;

        var btn = sceneState.EntityId.Last();
        var key = sceneState.New.Attributes?.GetKeyPress();

        return (btn, key) switch
        {
            {btn: '1', key: KeyPress.KeyPressed} => HandleKey1Pressed(),
            {btn: '2', key: KeyPress.KeyPressed} => HandleKey2Pressed(),
            {btn: '3' or '4', key: KeyPress.KeyPressed2x} => HandleKey3or4DoublePressed(),
            _ => Task.CompletedTask
        };
    }

    // implement and await as needed

    private Task HandleKey3or4DoublePressed() => Task.CompletedTask; 

    private Task HandleKey2Pressed() => Task.CompletedTask;

    private Task HandleKey1Pressed() => Task.CompletedTask;

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "event.my_scene_controller_001";
        yield return "event.my_scene_controller_002";
        yield return "event.my_scene_controller_003";
        yield return "event.my_scene_controller_004";
    }
}
