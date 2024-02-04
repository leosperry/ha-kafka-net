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

        var sut = new LightOnRegistry(harness.Services.Object, harness.Builder, harness.Factory);
        harness.Initialize(sut);
        harness.EnableAllAutomations(); // not normally required. The example registry ships with automations disabled
        
        // When
        var motionOnState = TestHelpers.GetState(LightOnRegistry.OFFICE_MOTION, "on");
        await harness.SendState(motionOnState);
        await Task.Delay(300); // conditional automation execute on another thread and need to be scheduled

        // Then
        harness.ApiProvider.Verify(api => api.LightTurnOn(LightOnRegistry.OFFICE_LIGHT, It.IsAny<CancellationToken>()), Times.Exactly(5));
        harness.ApiProvider.Verify(api => api.LightSetBrightness(LightOnRegistry.OFFICE_LIGHT, 200, It.IsAny<CancellationToken>()));
        // six of the same automation set up different ways
    }
}
