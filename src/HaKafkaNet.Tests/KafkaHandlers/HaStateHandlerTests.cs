
using System.Text.Json;
using Castle.Core.Logging;
using HaKafkaNet;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace HaKafkaNet.Tests;

public class HaStateHandlerTests
{
    [Fact]
    public async Task WhenNotCached_ShouldCacheState()
    {
        //arrange
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(default(byte[])));
        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<IAutomationCollector> collector = new();
        collector.Setup(c => c.GetAll()).Returns([auto1.Object]);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector.Object, logger.Object);

        Mock<IMessageContext> context = new();
        var cancellationToken = new CancellationToken();
        Mock<IConsumerContext> consumerContext = new();
        consumerContext.SetupGet(cc => cc.WorkerStopped).Returns(cancellationToken);

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);
        var fakeState = TestHelpers.GetFakeState();
        //act
        await sut.Handle(context.Object, fakeState);

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
        
        var cachedState = TestHelpers.GetFakeState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        
        var newState = TestHelpers.GetFakeState(lastUpdated: DateTime.Now);
        
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getBytes(cachedState)));

        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<IAutomationCollector> collector = new();
        collector.Setup(c => c.GetAll()).Returns([auto1.Object]);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object,collector.Object, logger.Object);
        
        //act
        await sut.Handle(null!, newState);

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
        
        var cachedState = TestHelpers.GetFakeState(lastUpdated: DateTime.Now - TimeSpan.FromHours(1));
        
        var newState = TestHelpers.GetFakeState(lastUpdated: DateTime.Now);
        
        cache.Setup(c => c.GetAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getBytes(cachedState)));

        Mock<IAutomation> auto1 = new Mock<IAutomation>();

        Mock<IAutomationCollector> collector = new();
        collector.Setup(c => c.GetAll()).Returns([auto1.Object]);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector.Object, logger.Object);
        
        Mock<IMessageContext> context = new();
        var cancellationToken = new CancellationToken();
        Mock<IConsumerContext> consumerContext = new();
        consumerContext.SetupGet(cc => cc.WorkerStopped).Returns(cancellationToken);

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);

        //act
        await sut.Handle(context.Object, newState);

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
        var cancellationToken = new CancellationToken();
        Mock<IConsumerContext> consumerContext = new();
        consumerContext.SetupGet(cc => cc.WorkerStopped).Returns(cancellationToken);

        context.Setup(c => c.ConsumerContext).Returns(consumerContext.Object);
        

        Mock<IAutomation> auto1 = new Mock<IAutomation>();
        auto1.Setup(a => a.TriggerEntityIds()).Returns(["enterprise"]);

        Mock<IAutomationCollector> collector = new();
        collector.Setup(c => c.GetAll()).Returns([auto1.Object]);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector.Object, logger.Object);

        var fakeState = TestHelpers.GetFakeState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        //act
        await sut.Handle(context.Object, fakeState);

        //assert
        var bytes = JsonSerializer.SerializeToUtf8Bytes(fakeState);
        cache.Verify(c => c.SetAsync("enterprise", bytes, 
            It.IsAny<DistributedCacheEntryOptions>(), default));

        // will execute on another thread; wait 1 second
        await Task.Delay(1000);
        auto1.Verify(a => a.Execute(It.Is<HaEntityStateChange>(sc => sc.EntityId == "enterprise" 
                && sc.New == fakeState)
            ,cancellationToken), Times.Once);
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

        Mock<IAutomationCollector> collector = new();
        collector.Setup(c => c.GetAll()).Returns([auto1.Object]);

        Mock<ILogger<HaStateHandler>> logger = new();

        HaStateHandler sut = new HaStateHandler(cache.Object, collector.Object, logger.Object);

        var fakeState = TestHelpers.GetFakeState(lastUpdated: DateTime.Now + TimeSpan.FromHours(1));
        //act
        await sut.Handle(context.Object, fakeState);

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

    byte[]? getBytes<T>(T o)
    {
        return JsonSerializer.SerializeToUtf8Bytes(o);
    }

}

class FakeAutomation : IAutomation
{
    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "Kirk";
        yield return "Spock";
    }
}
