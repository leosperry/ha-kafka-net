using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class ConditionalAutomationWrapperTests
{
    [Fact]
    public async Task When1EventTrue_ShouldTrigger()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(),default))
            .ReturnsAsync(true);
        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            Task.Delay(delay + 100)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task When1EventFalse_ShouldNotTrigger()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(),default))
            .ReturnsAsync(false);
        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            Task.Delay(delay + delay)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task When2EventTrue_ShouldTrigger()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();

        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), default))
            .ReturnsAsync(true)
            .ReturnsAsync(true);

        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            Task.Delay(delay + delay)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(2));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenFalseThenTrue_ShouldTrigger()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();

        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), default))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            Task.Delay(100).ContinueWith(t => sut.Execute(stateChange, default)),
            Task.Delay(delay + delay)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(2));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenTrueThenFalse_ShouldNotTrigger()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();

        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), default))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            Task.Delay(100).ContinueWith(t => sut.Execute(stateChange, default)),
            Task.Delay(delay + delay)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(2));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenTrueThenTimeElapseThenTrue_ShoulTriggerTwice()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();

        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), default))
            .ReturnsAsync(true)
            .ReturnsAsync(true);

        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            Task.Delay(delay + delay).ContinueWith(t => sut.Execute(stateChange, default)),
            Task.Delay(delay * 4)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(2));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task WhenAbunchTrue_ShoulTriggerOnce()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();

        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), default))
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true);

        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            Task.Delay(delay * 2)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(6));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenAbunchTrueWith1False_ShouldNotTriggerOnce()
    {
        // Given
        int delay = 500;
        Mock<IConditionalAutomation> auto = new();

        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), default))
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            Task.Delay(delay * 2)
        );
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(4));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async void WhenExecutionThrows_ShouldRaiseEvent()
    {
        // Given
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        auto.Setup(a => a.Execute(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("shields failing"));
        
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
    
        // When
        await sut.Execute(stateChange, default);
        await Task.Delay(300);
    
        // Then
        observer.Verify(o => o.OnUnhandledException(It.IsAny<AutomationMetaData>(), It.IsAny<Exception>()));
    }

    [Fact]
    public async Task WhenForIsZero_RunImmediately()
    {
        // Given
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(),default))
            .ReturnsAsync(true);
        auto.Setup(a => a.For).Returns(TimeSpan.Zero);
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<ConditionalAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        ConditionalAutomationWrapper sut = new ConditionalAutomationWrapper(auto.Object, observer.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()));
    }
}
