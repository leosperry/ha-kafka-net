using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace HaKafkaNet.Tests;

public class ConditionalAutomationWrapperTests
{
    TimeProvider _timeProvider = TimeProvider.System;

    [Fact]
    public async Task When1EventTrue_ShouldTrigger()
    {
        // Given
        int delay = 200;
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(),default))
            .ReturnsAsync(true);
        auto.Setup(a => a.For).Returns(TimeSpan.FromMilliseconds(delay));

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, TimeProvider.System, logger.Object);
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

        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
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

        Mock<ILogger<IConditionalAutomation>> logger = new();
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
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

        Mock<ILogger<IConditionalAutomation>> logger = new();
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
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

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
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

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
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

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            Task.Delay(delay * 3)
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

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
        // When
        await Task.WhenAll(
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            sut.Execute(stateChange, default),
            // make sure the next one runs last. It returns false
            Task.Delay(100).ContinueWith(t => sut.Execute(stateChange, default))
        );
        await Task.Delay(delay);
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default), Times.Exactly(4));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenExecutionThrows_ShouldRaiseEvent()
    {
        // Given
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        auto.Setup(a => a.Execute(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("shields failing"));
        
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
    
        // When
        await sut.Execute(stateChange, default);
        await Task.Delay(300);
    
        // Then
        trace.Verify();
    }

    [Fact]
    public async Task WhenForIsZero_RunImmediately()
    {
        // Given
        Mock<IConditionalAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(),default))
            .ReturnsAsync(true);
        auto.Setup(a => a.For).Returns(TimeSpan.Zero);

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<IConditionalAutomation>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayableAutomationWrapper<IConditionalAutomation> sut = new DelayableAutomationWrapper<IConditionalAutomation>(auto.Object, trace.Object, _timeProvider, logger.Object);
        // When
        await sut.Execute(stateChange, default);
    
        // Then
        auto.Verify(a => a.ContinuesToBeTrue(stateChange, default));
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()));
    }
}
