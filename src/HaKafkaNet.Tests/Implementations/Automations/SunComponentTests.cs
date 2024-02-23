using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

/// <summary>
/// These tests will test one implementation of the abstract SchedulableAutomationBase base class
/// They will also test durability features
/// </summary>
public class SunComponentTests
{
    [Fact]
    public async Task WhenStartup_ShouldScheduleAndExecute()
    {
        // Given
        Mock<ISystemObserver> observer = new();
        Mock<ILogger> logger = new();
        Mock<ILogger<AutomationManager>> autoLogger = new();

        bool didRun = false;
        Func<CancellationToken, Task> execution = ct => Task.FromResult(didRun = true);

        SunRiseAutomation sut = new SunRiseAutomation(execution);

        DelayablelAutomationWrapper wrapper = new(sut, observer.Object, logger.Object);
        AutomationWrapper autoWrapper = new(wrapper, logger.Object, "test");

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns(Enumerable.Repeat<IAutomationWrapper>(autoWrapper, 1));

        AutomationManager autoMgr = new AutomationManager(null, registrar.Object, observer.Object,  autoLogger.Object);


        // When
        await autoMgr.TriggerAutomations(getSunChange(EventTiming.PreStartupNotCached));
        await Task.Delay(1000);
    
        // Then
        Assert.True(didRun);
    }

    [Fact]
    public async Task WhenStartup_andEventInPast_ShouldExecuteByDefault()
    {
        // Given
        Mock<ISystemObserver> observer = new();
        Mock<ILogger> logger = new();
        Mock<ILogger<AutomationManager>> autoLogger = new();

        bool didRun = false;
        Func<CancellationToken, Task> execution = ct => Task.FromResult(didRun = true);

        SunRiseAutomation sut = new SunRiseAutomation(execution);

        DelayablelAutomationWrapper wrapper = new(sut, observer.Object, logger.Object);
        AutomationWrapper autoWrapper = new(wrapper, logger.Object, "test");

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns(Enumerable.Repeat<IAutomationWrapper>(autoWrapper, 1));

        AutomationManager autoMgr = new AutomationManager(null, registrar.Object, observer.Object,  autoLogger.Object);


        // When
        await autoMgr.TriggerAutomations(getSunChange(EventTiming.PreStartupNotCached, -1000));
        await Task.Delay(500);
    
        // Then
        Assert.True(didRun);
    }

    [Fact]
    public async Task WhenStartup_andEventInPast_AndUserSpecifiedNoPast_ShouldNotExecute()
    {
        // Given
        Mock<ISystemObserver> observer = new();
        Mock<ILogger> logger = new();
        Mock<ILogger<AutomationManager>> autoLogger = new();

        bool didRun = false;
        Func<CancellationToken, Task> execution = ct => Task.FromResult(didRun = true);

        SunRiseAutomation sut = new SunRiseAutomation(execution);
        sut.ShouldExecutePastEvents = false;

        DelayablelAutomationWrapper wrapper = new(sut, observer.Object, logger.Object);
        AutomationWrapper autoWrapper = new(wrapper, logger.Object, "test");

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns(Enumerable.Repeat<IAutomationWrapper>(autoWrapper, 1));

        AutomationManager autoMgr = new AutomationManager(null, registrar.Object, observer.Object,  autoLogger.Object);

        // When
        await autoMgr.TriggerAutomations(getSunChange(EventTiming.PreStartupNotCached, -1000));
        await Task.Delay(500);
    
        // Then
        Assert.False(didRun);
    }

    static HaEntityStateChange getSunChange(EventTiming timing, int millisecondOffSetFromNow = 200) 
    {
        var sunState = TestHelpers.GetSun(SunState.Above_Horizon, nextDawn: DateTime.Now.AddMilliseconds(millisecondOffSetFromNow));
        var sunGenericState = JsonSerializer.Deserialize<HaEntityState>(JsonSerializer.Serialize(sunState, typeof(SunModel)));

        return new HaEntityStateChange()
        {
            EntityId = "sun.sun",
            New = sunGenericState!,
            EventTiming = timing
        };
    }
}
