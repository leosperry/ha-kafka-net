using System.Text.Json;
using HaKafkaNet.Implementations.Core;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class HaStateHandlerComponentTests
{
    Mock<IAutomationActivator> _activator = new();

    const int DELAY = 300;

    [Fact]
    public async Task WhenNotCached_ShouldCacheState_andCallObserver()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(default(byte[])));
        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<IInternalRegistrar> registrar = new();

        Mock<ISystemObserver> observer = new();
        AutomationManager manager = new(
            Enumerable.Empty<IAutomationRegistry>(), 
            registrar.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        var fakeState = TestHelpers.GetState();
        //act
        StateHandler sut = new StateHandler(cache.Object, manager, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        
        observer.Verify(o => o.OnStateHandlerInitialized(), Times.Once);

        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        //assert
        var bytes = JsonSerializer.SerializeToUtf8Bytes(fakeState);
        cache.Verify(c => c.SetAsync("enterprise", bytes, 
            It.IsAny<DistributedCacheEntryOptions>(), default));
    }

    [Fact]
    public async Task WhenOlderThanCache_ShouldNotOverrideCache()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>(MockBehavior.Strict);
        
        var cachedState = TestHelpers.GetState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        
        var newState = TestHelpers.GetState(lastUpdated: DateTime.Now);
        
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getBytes(cachedState)));

        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<IInternalRegistrar> registrar = new();

        Mock<ISystemObserver> observer = new();
        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        StateHandler sut = new StateHandler(cache.Object,collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        
        //act
        await sut.Handle(newState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        //assert
        cache.Verify(c =>  c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), 
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Never);
    }

    [Fact]
    public async Task WhenNewerThanCache_ShouldOverrideCache()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        
        var cachedState = TestHelpers.GetState(lastUpdated: DateTime.Now - TimeSpan.FromHours(1));
        
        var newState = TestHelpers.GetState(lastUpdated: DateTime.Now);
        
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getBytes(cachedState)));

        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<IInternalRegistrar> registrar = new();

        Mock<ISystemObserver> observer = new();

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        
        //act
        await sut.Handle(newState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        //assert
        cache.Verify(c =>  c.SetAsync("enterprise", It.IsAny<byte[]>(), 
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }

    [Fact]
    public async Task WhenEntityIdMatches_ShouldExecuteAutomation()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(default(byte[])));

        Mock<IAutomationWrapper> auto1 = new ();
        auto1.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto1.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto1.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto1.Object]);

        Mock<ISystemObserver> observer = new();

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);
        collector.Initialize(new List<InitializationError>());

        Mock<ILogger<HaStateHandler>> logger = new();

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);

        var fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        //act
        await sut.Handle(fakeState);
        await Task.Delay(200); //sometimes the verification can run before the task is scheduled

        //assert
        var bytes = JsonSerializer.SerializeToUtf8Bytes(fakeState);
        cache.Verify(c => c.SetAsync("enterprise", bytes, 
            It.IsAny<DistributedCacheEntryOptions>(), default));

        // will execute on another thread; wait 1 second
        await Task.Delay(1000);
        auto1.Verify(a => a.Execute(It.Is<HaEntityStateChange>(sc => sc.EntityId == "enterprise" 
                && sc.New == fakeState)
            ,default), Times.Once);
    }

    [Fact]
    public async Task WhenEntityIdDoesNotMatch_ShouldNotExecuteAutomation()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(default(byte[])));

        Mock<IAutomationWrapper> auto1 = new Mock<IAutomationWrapper>();
        auto1.Setup(a => a.TriggerEntityIds()).Returns(["excelsior"]);
        auto1.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto1.Object]);
        Mock<ISystemObserver> observer = new();

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);

        var fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        //act
        await sut.Handle(fakeState);
        await Task.Delay(200); //sometimes the verification can run before the task is scheduled

        //assert
        var bytes = JsonSerializer.SerializeToUtf8Bytes(fakeState);
        cache.Verify(c => c.SetAsync("enterprise", bytes, 
            It.IsAny<DistributedCacheEntryOptions>(), default));

        // will execute on another thread; wait 1 second
        await Task.Delay(1000);
        auto1.Verify(a => a.Execute(It.Is<HaEntityStateChange>(sc => sc.EntityId == "enterprise" 
                && sc.New == fakeState)
            ,default), Times.Never);
    }

    [Fact]
    public async Task WhenPostStartupSpecified_AndEventPostStartup_ShouldTriggerAutomation()
    {
        // Given
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomationWrapper> auto = new Mock<IAutomationWrapper>();
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(10));

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);
        collector.Initialize(new List<InitializationError>());


        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default));
    }

    [Fact]
    public async Task WhenPostStartupSpecified_AndEventPostStartup_AndAutomationDisabled_ShouldNotTriggerAutomation()
    {
        // Given
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomationWrapper> auto = new();
        var autoWithMeta = auto.As<IAutomationMeta>();
        autoWithMeta.Setup(a => a.GetMetaData())
            .Returns(new AutomationMetaData()
            {
                Name = "fire photon torpedoes",
                Enabled = false
            });
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name="", Enabled=false});

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(10));

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);
    }

    [Fact]
    public async Task WhenPostStartupSpecified_AndEventPreStartup_ShouldNotTriggerAutomation()
    {
        // Given
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(-1));

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);
        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);    
    }

    [Fact]
    public async Task WhenPreStartupSpecified_AndEventPostStartup_ShouldNotTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PreStartupNotCached);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(1));

        Mock<IAutomationFactory> factory = new();
        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);    
    }

    [Fact]
    public async Task WhenPostStartupOrPreStartupPostCacheSpecified_AndEventPreStartupPostCache_ShouldTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(
            getBytes<HaEntityState>(TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1))));
        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddHours(-1));

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);
        collector.Initialize(new List<InitializationError>());

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default));    
    }

    [Fact]
    public async Task WhenPostStartupOrPreStartupPostCacheSpecified_AndEventPostStartup_ShouldTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(
            getBytes<HaEntityState>(TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1))));
        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddHours(1));

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);
        collector.Initialize(new List<InitializationError>());

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default));

    }

    [Fact]
    public async Task WhenPostStartupOrPreStartupPostCacheSpecified_AndEventPreStartupPreLastCached_ShouldNotTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(
            getBytes<HaEntityState>(TestHelpers.GetState(lastUpdated: DateTime.Now.AddHours(-1))));
        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1));

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);    
    }

    [Fact]
    public async Task WhenPostStartupOrPreStartupPostCacheSpecified_AndEventPreStartupNotCached_ShouldNotTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1));

        Mock<IAutomationFactory> factory = new();
        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);
        // When
        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);
    }

    [Fact]
    public async Task WhenEntityBadState_ShouldNotRun()
    {
        // arrange
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));

        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name=""});

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(1), state:"unknown");

        // act
        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);

        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        // assert
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);
    }

    [Fact]
    public async Task WhenEntityBadState_andMetaTriggerBad_ShouldRun()
    {
        // arrange
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));

        Mock<IAutomationWrapper> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){
            Name="", TriggerOnBadState = true
        });

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered).Returns([auto.Object]);

        AutomationManager collector = new(
            Enumerable.Empty<IAutomationRegistry>(), registrar.Object);
        collector.Initialize(new List<InitializationError>());

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(1), state:"unknown");

        // act
        StateHandler sut = new StateHandler(cache.Object, collector, observer.Object, TimeProvider.System, _activator.Object, logger.Object);

        await sut.Handle(fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        // assert
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Once);
    }

    byte[]? getBytes<T>(T o)
    {
        return JsonSerializer.SerializeToUtf8Bytes(o);
    }
}

