using System.Text.Json;
using HaKafkaNet;
using HaKafkaNet.Tests;
using Moq;

namespace HaKafkaNet.ExampleApp.Tests;


public class LightOnRegistryTests
{
    [Fact]
    public async Task LightOnRegistry_TurnsOnLights()
    {
        // Given
        TestHarness harness = new TestHarness("off");

        harness.SetServicesGenericDefaults<OnOff,JsonElement>(OnOff.Off, default);
        
        var sut = new LightOnRegistry(harness.Services.Object, harness.Builder, harness.Factory);
        harness.Initialize(sut);
        harness.EnableAllAutomations(); // not normally required. The example registry ships with automations disabled
        
        // When
        var motionOnState = TestHelpers.GetState(LightOnRegistry.OFFICE_MOTION, "on");
        await harness.SendState(motionOnState);
        await Task.Delay(300); // conditional automation execute on another thread and need to be scheduled

        // Then
        harness.ApiProvider.Verify(api => api.CallService("homeassistant", "turn_on", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
        harness.ApiProvider.Verify(api => api.CallService("light", "turn_on", It.IsAny<object>(), It.IsAny<CancellationToken>()));
        // six similar automations set up different ways
    }
}
