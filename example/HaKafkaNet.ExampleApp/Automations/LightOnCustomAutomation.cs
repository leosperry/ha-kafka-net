
namespace HaKafkaNet.ExampleApp;

/// <summary>
/// Very important if you want to resuse this autommation in the registry for multiple devices that 
/// you decorate with the ExcludeFromDiscovery attribute
/// </summary>
[ExcludeFromDiscovery] 
public class LightOnCustomAutomation : IAutomation, IAutomationMeta
{
    private readonly IHaApiProvider _api;
    private readonly string _motionId;
    private readonly string _lightId;
    private readonly byte _brightness;
    readonly AutomationMetaData _meta;

    public LightOnCustomAutomation(IHaApiProvider api, string motionId, string lightId, byte brightness, string name, string description)
    {
        _api = api;
        _motionId = motionId;
        _lightId = lightId;
        _brightness = brightness;
        _meta = new AutomationMetaData()
        {
            Name = name,
            Description = description,
            Enabled = false // for demonstration purposes only
        };
    }

    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        var motion = stateChange.ToOnOff();
        if ((motion.Old is null || motion.Old.State != OnOff.On) && motion.New.State == OnOff.On)
        {
            return _api.LightSetBrightness(_lightId, _brightness, cancellationToken);
        }
        return Task.CompletedTask;
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return _motionId;
    }

    // IAutomationMeta implementation
    // you could omit this and use the extension method
    // as shown in LightOnRegistry.cs
    public AutomationMetaData GetMetaData() => _meta;
}
