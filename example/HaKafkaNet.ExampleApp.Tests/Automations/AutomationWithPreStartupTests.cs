using System.Text.Json;
using Moq;
using Xunit;

namespace HaKafkaNet.ExampleApp.Tests;

public class SimpleAutomationTests
{
    [Fact]
    public async Task WhenTestButtonPushedAfterStartup_SendsNotification()
    {
        //arrange
        Mock<IHaApiProvider> mockApi = new Mock<IHaApiProvider>();

        AutomationWithPreStartup sut = new AutomationWithPreStartup(mockApi.Object);

        var stateChange = getFakeStateChange();

        // act
        await sut.Execute(stateChange, CancellationToken.None);

        // assert
        mockApi.Verify(a => a.PersistentNotification(It.IsAny<string>()), Times.Once);
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
