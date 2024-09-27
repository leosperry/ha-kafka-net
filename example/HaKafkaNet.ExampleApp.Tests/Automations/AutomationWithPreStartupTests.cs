using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;

namespace HaKafkaNet.ExampleApp.Tests;

public class SimpleAutomationTests
{
    [Fact]
    public async Task WhenTestButtonPushedAfterStartup_SendsNotification()
    {
        //arrange
        Mock<IHaApiProvider> mockApi = new Mock<IHaApiProvider>();
        Mock<ILogger<AutomationWithPreStartup>> logger = new();

        AutomationWithPreStartup sut = new AutomationWithPreStartup(mockApi.Object, logger.Object);

        var stateChange = getFakeStateChange();

        // act
        await sut.Execute(stateChange, default);

        // assert
        mockApi.Verify(a => a.CallService(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default), Times.Once);
    }

    private HaEntityStateChange getFakeStateChange()
    {
        return new HaEntityStateChange()
        {
            EntityId = "input_button.test_button",
            EventTiming = EventTiming.PostStartup,
            New = getButtonPush()
        };
    }

    private HaEntityState getButtonPush()
    {
        return new HaEntityState()
            {
                EntityId = "input_button.test_button",
                State = "I exist",
                Attributes = new JsonElement()
            };
    }

}
