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
    private AutomationManager GetManager(Mock<IAutomationWrapper> auto, Mock<ISystemObserver> observer)
    {
        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        return new AutomationManager(
            Enumerable.Empty<IAutomationRegistry>(),
            registrar.Object,
            observer.Object,
            new Mock<ILogger<AutomationManager>>().Object
        );
    }

    [Fact]
    public async Task WhenAutomationThrows_ShouldCallObserver()
    {
        //arrange
        Mock<IAutomationWrapper> auto = new ();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData()
        {
            Name = "test self destruct"
        });

        Mock<ISystemObserver> oabserver = new();

        Exception exception = new Exception("self destruct");
        auto.Setup(a => a.Execute(It.IsAny<HaEntityStateChange>(), default)).ThrowsAsync(exception);

        var fakeStateChange = TestHelpers.GetStateChange();
        AutomationManager sut = GetManager(auto, oabserver);
        //act
        sut.TriggerAutomations(fakeStateChange, default);
        await Task.Delay(1000);

        //assert
        oabserver.Verify(o => o.OnUnhandledException(It.IsAny<AutomationMetaData>(), It.Is<AggregateException>(ae => ae.InnerExceptions.Any(e => e.Message == "self destruct"))));
    }
}
