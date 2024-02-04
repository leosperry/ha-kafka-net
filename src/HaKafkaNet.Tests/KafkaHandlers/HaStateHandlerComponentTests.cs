using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class HaStateHandlerComponentTests
{
    const int DELAY = 300;

    [Fact]
    public async Task WhenNotCached_ShouldCacheState_andCallObserver()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(default(byte[])));
        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        Mock<ISystemObserver> observer = new();
        AutomationManager manager = new(
            Enumerable.Empty<IAutomation>(), Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        Mock<IMessageContext> context = new();
        var cancellationToken = new CancellationToken();
        Mock<IConsumerContext> consumerContext = new();
        consumerContext.SetupGet(cc => cc.WorkerStopped).Returns(cancellationToken);

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);
        var fakeState = TestHelpers.GetState();
        //act
        HaStateHandler sut = new HaStateHandler(cache.Object, manager, observer.Object, logger.Object);
        
        observer.Verify(o => o.OnStateHandlerInitialized(), Times.Once);

        await sut.Handle(context.Object, fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        //assert
        var bytes = JsonSerializer.SerializeToUtf8Bytes(fakeState);
        cache.Verify(c => c.SetAsync("enterprise", bytes, 
            It.IsAny<DistributedCacheEntryOptions>(), default));
    }

    [Fact]
    public async Task WhenOlderThanCach_ShouldNotOverrideCache()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>(MockBehavior.Strict);
        
        var cachedState = TestHelpers.GetState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        
        var newState = TestHelpers.GetState(lastUpdated: DateTime.Now);
        
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getBytes(cachedState)));

        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        Mock<ISystemObserver> observer = new();
        AutomationManager collector = new(
            Enumerable.Empty<IAutomation>(), Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object,collector, observer.Object, logger.Object);
        
        //act
        await sut.Handle(null!, newState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled

        //assert
        cache.Verify(c =>  c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), 
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Never);
    }

    [Fact]
    public async Task WhenNewerThanCach_ShouldOverrideCache()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        
        var cachedState = TestHelpers.GetState(lastUpdated: DateTime.Now - TimeSpan.FromHours(1));
        
        var newState = TestHelpers.GetState(lastUpdated: DateTime.Now);
        
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getBytes(cachedState)));

        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        Mock<ISystemObserver> observer = new();

        AutomationManager collector = new(
            Enumerable.Empty<IAutomation>(), Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        
        Mock<IMessageContext> context = new();
        var cancellationToken = new CancellationToken();
        Mock<IConsumerContext> consumerContext = new();
        consumerContext.SetupGet(cc => cc.WorkerStopped).Returns(cancellationToken);

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        //act
        await sut.Handle(context.Object, newState);
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

        Mock<IMessageContext> context = new();
        Mock<IConsumerContext> consumerContext = new();

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<IAutomation> auto1 = new Mock<IAutomation>();
        auto1.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto1.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto1.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);

        var fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        //act
        await sut.Handle(context.Object, fakeState);
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

        Mock<IMessageContext> context = new();
        var cancellationToken = new CancellationToken();
        Mock<IConsumerContext> consumerContext = new();
        consumerContext.SetupGet(cc => cc.WorkerStopped).Returns(cancellationToken);

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<IAutomation> auto1 = new Mock<IAutomation>();
        auto1.Setup(a => a.TriggerEntityIds()).Returns(["excelsior"]);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto1.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);

        var fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        //act
        await sut.Handle(context.Object, fakeState);
        await Task.Delay(200); //sometimes the verification can run before the task is scheduled

        //assert
        var bytes = JsonSerializer.SerializeToUtf8Bytes(fakeState);
        cache.Verify(c => c.SetAsync("enterprise", bytes, 
            It.IsAny<DistributedCacheEntryOptions>(), default));

        // will execute on another thread; wait 1 second
        await Task.Delay(1000);
        auto1.Verify(a => a.Execute(It.Is<HaEntityStateChange>(sc => sc.EntityId == "enterprise" 
                && sc.New == fakeState)
            ,cancellationToken), Times.Never);
    }

    [Fact]
    public async Task WhenPostStartupSpecified_AndEventPostStartup_ShouldTriggerAutomation()
    {
        // Given
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        //collector.Setup(c => c.GetAll()).Returns([auto.Object]);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(10));

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);


        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
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
        Mock<IAutomation> auto = new Mock<IAutomation>();
        var autoWithMeta = auto.As<IAutomationMeta>();
        autoWithMeta.Setup(a => a.GetMetaData())
            .Returns(new AutomationMetaData()
            {
                Name = "fire photon torpedos",
                Enabled = false
            });
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup);
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(10));

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
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
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(-1));

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);    
    }

    [Fact]
    public async Task WhenPreStartupSpecified_AndEventPostStartup_ShouldNotTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PreStartupNotCached);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddMinutes(1));

        Mock<IAutomationFactory> factory = new();
        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
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
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddHours(-1));

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default));    
    }

    [Fact]
    public async Task WhenPostStartupOrPreStartupPostCacheSpecified_AndEventPostnStartup_ShouldTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(
            getBytes<HaEntityState>(TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1))));
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddHours(1));

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
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
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1));

        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);    
    }

    [Fact]
    public async Task WhenPostStartupOrPreStartupPostCacheSpecified_AndEventPreStartupNotCached_ShouldNotTriggerAutomation()
    {
        Mock<IDistributedCache> cache= new();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(default(byte[]?));
        Mock<IAutomation> auto = new Mock<IAutomation>();
        auto.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);
        auto.Setup(a => a.EventTimings).Returns(EventTiming.PostStartup | EventTiming.PreStartupPostLastCached);
        Mock<IMessageContext> fakeContext = new();
        Mock<IConsumerContext> consumerContext = new();
        fakeContext.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        Mock<ISystemObserver> observer = new();
        Mock<ILogger<HaStateHandler>> logger = new();

        HaEntityState fakeState = TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1));

        Mock<IAutomationFactory> factory = new();
        Mock<ILogger<AutomationManager>> mgrLogger = new();

        AutomationManager collector = new(
            [auto.Object], Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<IAutomationRegistry>(), observer.Object, mgrLogger.Object);

        HaStateHandler sut = new HaStateHandler(cache.Object, collector, observer.Object, logger.Object);
        // When
        await sut.Handle(fakeContext.Object, fakeState);
        await Task.Delay(DELAY); //sometimes the verification can run before the task is scheduled
    
        // Then
        auto.Verify(a => a.Execute(It.IsAny<HaEntityStateChange>(), default), Times.Never);    
    }

    byte[]? getBytes<T>(T o)
    {
        return JsonSerializer.SerializeToUtf8Bytes(o);
    }
}

