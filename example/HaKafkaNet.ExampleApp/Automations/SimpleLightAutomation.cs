using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace HaKafkaNet.ExampleApp;

/// <summary>
/// Simple automation to demonstrate getting typed states from cache
/// it assumes you have a helper button named "Test Button 2"
/// change the id of the light for your setup
/// </summary>
public class SimpleLightAutomation : IAutomation
{
    IHaServices _services;
    string _idOfLightToDim;
    
    public SimpleLightAutomation(IHaServices services)
    {
        _services = services;
        _idOfLightToDim = "light.office_lights";
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "input_button.test_button_2";
    }

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        var currentLightState = await _services.Cache.Get<LightAttributes>(_idOfLightToDim);
        if (currentLightState == null)
        {
            return;
        }
        var brightness = currentLightState.Attributes!.Brightness;

        //call a service to change it
        await _services.Api.CallService("light", "turn_on", new {
            entity_id = _idOfLightToDim,
            brightness = brightness - 5
        }, cancellationToken);
    }

    record LightAttributes
    {
        [JsonPropertyName("brightness")]
        public byte Brightness { get; set; }
    }
}
