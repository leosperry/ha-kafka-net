using HaKafkaNet;
using HaKafkaNet.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;

namespace HaKafkaNet.ExampleApp.Tests;

public class LightOnRegistryTests : IClassFixture<HaKafkaNetFixture>
{
    private HaKafkaNetFixture _fixture;

    public LightOnRegistryTests(HaKafkaNetFixture fixture)
    {
        this._fixture = fixture;
    }

    [Fact]
    public async Task LightOnRegistry_TurnsOnLights()
    {
        // Given

        _fixture.API.Setup(api => api.GetEntity<HaEntityState<OnOff, JsonElement>>(LightOnRegistry.OFFICE_LIGHT, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelper.Api_GetEntity_Response<OnOff>(OnOff.Off));

        var sut = _fixture.Services.GetRegistry<LightOnRegistry>();

        // When

        var motionOnState = new HaEntityState<OnOff, object>()
        {
            EntityId = LightOnRegistry.OFFICE_MOTION,
            State = OnOff.On,
            Attributes = new { },
            LastChanged = DateTime.UtcNow.AddMinutes(1),
            LastUpdated = DateTime.UtcNow.AddMinutes(1),
        };

        await _fixture.Services.SendState(motionOnState);

        await Task.Delay(300); // conditional automation execute on another thread and need to be scheduled

        // Then
        _fixture.API.Verify(api => api.TurnOn(LightOnRegistry.OFFICE_LIGHT, It.IsAny<CancellationToken>()), Times.Exactly(5));
        _fixture.API.Verify(api => api.LightSetBrightness(LightOnRegistry.OFFICE_LIGHT, 200, It.IsAny<CancellationToken>()));
        // six similar automations set up different ways
    }
}
