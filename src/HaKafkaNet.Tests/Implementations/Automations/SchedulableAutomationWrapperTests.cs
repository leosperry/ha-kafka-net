using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class SchedulableAutomationTests
{
    [Fact]
    public void WhenEvaluatorNotProvided_ThrowsException()
    {
        // Given
        Mock<IDelayableAutomation> auto = new();

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        // When
        // Then

        Assert.Throws<HaKafkaNetException>(() => new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object));
    }

    [Fact]
    public async Task WhenContinuesIsFalse_ShouldNotRun()
    {
        Mock<ISchedulableAutomation> auto = new();

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
    
        // Then
        auto.Verify(a => a.GetNextScheduled(), Times.Never);
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);        
    }

    [Fact]
    public async Task WhenContinuesIsTrueThenFalse_ShouldNotRun()
    {
        int delay= 100;

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();

        Mock<ISchedulableAutomation> auto = new();
        auto.SetupSequence(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        auto.Setup(a => a.GetNextScheduled()).Returns(DateTime.Now.AddMilliseconds(delay));
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
        await sut.Execute(stateChange, default);
        await Task.Delay(delay * 2);
    
        // Then
        auto.Verify(a => a.GetNextScheduled(), Times.Once);
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);        
    }

    [Fact]
    public async Task WhenContinuesIsTrue_andDateIsNull_DoesNotRun()
    {
        // Given
        Mock<ISchedulableAutomation> auto = new();

        auto.Setup(a => a.GetNextScheduled()).Returns(default(DateTime?));
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
    
        // Then
        auto.Verify(a => a.GetNextScheduled());
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);        
    }

    [Fact]
    public async Task WhenContinuesIsTrue_andDateIsFuture_DoesRun()
    {
        int delay= 100;
        // Given
        Mock<ISchedulableAutomation> auto = new();

        auto.Setup(a => a.GetNextScheduled()).Returns(default(DateTime?));
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        auto.Setup(a => a.GetNextScheduled()).Returns(DateTime.Now.AddMilliseconds(delay));

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
        await Task.Delay(delay * 2);
    
        // Then
        auto.Verify(a => a.GetNextScheduled());
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);        
    }

    [Fact]
    public async Task WhenContinuesIsTrue_andDateInPast_andShouldExecutePast_DoesRun()
    {
        int delay= 100;
        // Given
        Mock<ISchedulableAutomation> auto = new();

        auto.Setup(a => a.GetNextScheduled()).Returns(default(DateTime?));
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        auto.Setup(a => a.GetNextScheduled()).Returns(DateTime.Now.AddMilliseconds(-delay));
        auto.Setup(a => a.ShouldExecutePastEvents).Returns(true);

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
        await Task.Delay(delay);
    
        // Then
        auto.Verify(a => a.GetNextScheduled());
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);        
    }

    [Fact]
    public async Task WhenShouldReschedleFalse_andEarlierTimePassed_DoesRunButDoesNotReschedule()
    {
        int delay= 200;
        // Given
        Mock<ISchedulableAutomation> auto = new();

        auto.Setup(a => a.GetNextScheduled()).Returns(default(DateTime?));
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        auto.SetupSequence(a => a.GetNextScheduled())
            .Returns(DateTime.Now.AddMilliseconds(delay * 2))
            .Returns(DateTime.Now.AddMilliseconds(delay));

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
        await sut.Execute(stateChange, default);
        await Task.Delay(delay);

        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Never);
        await Task.Delay(delay * 2);
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);
    
        // Then
        auto.Verify(a => a.GetNextScheduled(), Times.Exactly(1));
     
    }

    [Fact]
    public async Task WhenShouldReschedleTrue_andEarlierTimePassed_DoesRunRescheduled()
    {
        int delay= 200;
        // Given
        Mock<ISchedulableAutomation> auto = new();

        auto.Setup(a => a.GetNextScheduled()).Returns(default(DateTime?));
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        auto.Setup(a => a.IsReschedulable).Returns(true);

        auto.SetupSequence(a => a.GetNextScheduled())
            .Returns(() => DateTime.Now.AddMilliseconds(delay * 2))
            .Returns(() => DateTime.Now.AddMilliseconds(delay));

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
    
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);
        // When
        await sut.Execute(stateChange, default);
        await sut.Execute(stateChange, default);
        await Task.Delay(delay);

        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);
        await Task.Delay(delay);
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);
    
        // Then
        auto.Verify(a => a.GetNextScheduled(), Times.Exactly(2));
    }

    [Fact]
    public async Task WhenExecutes_andIsReschedulableFalse_andTriggersAgain_ShouldRun()
    {
        // Given
        int delay= 100;

        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        Mock<ILogger<DelayablelAutomationWrapper>> logger = new();

        var stateChange = TestHelpers.GetStateChange();
        Mock<ISchedulableAutomation> auto = new();
        auto.Setup(a => a.ContinuesToBeTrue(It.IsAny<HaEntityStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        auto.SetupSequence(a => a.GetNextScheduled())
            .Returns(() => DateTime.Now.AddMilliseconds(delay))
            .Returns(() => DateTime.Now.AddMilliseconds(delay));        
        
        DelayablelAutomationWrapper sut = new DelayablelAutomationWrapper(auto.Object, trace.Object, logger.Object);

        // When
        await sut.Execute(stateChange, default);
        await Task.Delay(delay * 2);
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Once);

        await sut.Execute(stateChange, default);
        await Task.Delay(delay * 2);
        auto.Verify(a => a.Execute(It.IsAny<CancellationToken>()), Times.Exactly(2));
    
        // Then
        auto.Verify(a => a.GetNextScheduled(), Times.Exactly(2));
    }
}
