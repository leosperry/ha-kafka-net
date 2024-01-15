using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class HaEntityProviderTests
{
    [Fact]
    public async Task WhenInCache_ReturnsFromCache()
    {
        // Given
        Mock<IHaStateCache> cache = new();
        var fakeState = TestHelpers.GetFakeState();
        cache.Setup(c => c.Get(fakeState.EntityId, default))
            .ReturnsAsync(fakeState);
        
        Mock<IHaApiProvider> api = new();

        Mock<ILogger<HaEntityProvider>> logger = new();

        HaEntityProvider sut = new HaEntityProvider(cache.Object, api.Object, logger.Object);

        // When
        var result = await sut.GetEntityState(fakeState.EntityId);
    
        // Then
        cache.Verify(c => c.Get(fakeState.EntityId, default));
        api.Verify(a => a.GetEntityState(It.IsAny<string>(), default), Times.Never);
        Assert.Equal(fakeState, result);
    }

    [Fact]
    public async Task WhenNotInCache_GetsFromAPI()
    {
        // Given
        Mock<IHaStateCache> cache = new();
        HaEntityState fakeState = TestHelpers.GetFakeState();
        cache.Setup(c => c.Get(fakeState.EntityId, default))
            .ReturnsAsync(default(HaEntityState)).Verifiable();
        
        Mock<IHaApiProvider> api = new();
        (HttpResponseMessage, HaEntityState) apiReturn = (default(HttpResponseMessage), fakeState)!;
        api.Setup(a => a.GetEntityState(fakeState.EntityId, default))
            .ReturnsAsync(apiReturn).Verifiable();

        Mock<ILogger<HaEntityProvider>> logger = new();

        HaEntityProvider sut = new HaEntityProvider(cache.Object, api.Object, logger.Object);

        // When
        var result = await sut.GetEntityState(fakeState.EntityId);
    
        // Then
        cache.Verify();
        api.Verify();
    }

    [Fact]
    public async Task WhenCacheThrows_ReturnsFromApi()
    {
        // Given
        Mock<IHaStateCache> cache = new();
        HaEntityState fakeState = TestHelpers.GetFakeState();
        cache.Setup(c => c.Get(fakeState.EntityId, default))
            .Throws(new Exception()).Verifiable();
        
        Mock<IHaApiProvider> api = new();
        (HttpResponseMessage, HaEntityState) apiReturn = (default(HttpResponseMessage), fakeState)!;
        api.Setup(a => a.GetEntityState(fakeState.EntityId, default))
            .ReturnsAsync(apiReturn).Verifiable();

        Mock<ILogger<HaEntityProvider>> logger = new();

        HaEntityProvider sut = new HaEntityProvider(cache.Object, api.Object, logger.Object);

        // When
        var result = await sut.GetEntityState(fakeState.EntityId);
    
        // Then
        cache.Verify();
        api.Verify();
    }

}
