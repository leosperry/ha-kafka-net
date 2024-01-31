using HaKafkaNet;
using HaKafkaNet.Tests;
using Moq;

namespace HaKafkaNet.ExampleApp.Tests;


public class LightOnRegistryTests
{
    [Fact]
    public async Task TestName()
    {
        // Given
        TestHarness harness = new TestHarness();

        // this is called by the prebuilt automation to make sure only lights that are off get turned on
        harness.EntityProvider.Setup(ep => ep.GetEntityState(LightOnRegistry.OFFICE_LIGHT, default))
            .ReturnsAsync(TestHelpers.GetState(LightOnRegistry.OFFICE_LIGHT, "off"));

        var sut = new LightOnRegistry(harness.Services.Object, harness.Builder, harness.Factory);
        harness.Initialize(sut);
        harness.EnableAllAutomations(); // not normally required. The example registry ships with most automations disabled
        
        // When
        var motionOnState = TestHelpers.GetState(LightOnRegistry.OFFICE_MOTION, "on");
        await harness.SendState(motionOnState);

        await Task.Delay(500);
        // Then
        harness.ApiProvider.Verify(api => api.LightTurnOn(LightOnRegistry.OFFICE_LIGHT, It.IsAny<CancellationToken>()), Times.Exactly(5));
        harness.ApiProvider.Verify(api => api.LightSetBrightness(LightOnRegistry.OFFICE_LIGHT, 200, default));
        // six of the same automation set up different ways
    }
}
