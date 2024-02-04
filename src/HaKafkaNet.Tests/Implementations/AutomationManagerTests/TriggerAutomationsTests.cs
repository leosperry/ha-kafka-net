using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

/// <summary>
/// A lot of functionality regarding when automations should run
/// is tested via the state handler tests
/// This set of tests covers what should happen when an
/// automation is fired
/// </summary>
public class TriggerAutomationsTests
{
    private AutomationManager GetManager(Mock<IAutomation> auto, Mock<ISystemObserver> observer)
    {
        return new AutomationManager(
            [auto.Object],
            Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(),
            observer.Object,
            new Mock<ILogger<AutomationManager>>().Object
        );
    }

    [Fact]
    public async Task WhenAutomationThrows_ShouldCallObserver()
    {
        //arrange
        Mock<IAutomation> auto = new ();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);

        Mock<ISystemObserver> oabserver = new();

        Exception exception = new Exception("self destruct");
        auto.Setup(a => a.Execute(It.IsAny<HaEntityStateChange>(), default)).ThrowsAsync(exception);

        var fakeStateChange = TestHelpers.GetStateChange();
        AutomationManager sut = GetManager(auto, oabserver);
        //act
        await sut.TriggerAutomations(fakeStateChange, default);

        //assert
        oabserver.Verify(o => o.OnUnhandledException(It.IsAny<AutomationMetaData>(), It.Is<AggregateException>(ae => ae.InnerExceptions.Any(e => e.Message == "self destruct"))));
    }
}
