using System.Net;

namespace HaKafkaNet.Tests;

public class EntityTrackerTests
{
    [Fact]
    public void WhenInitialized_RunsCheck()
    {
        // Given
        EntityTrackerConfig config = new();
        Mock<ISystemObserver> observer = new();
        observer.Setup(o => o.OnStateHandlerInitialized())
            .Raises(o => o.StateHandlerInitialized += null);

        Mock<IAutomationManager> mgr = new();
        Mock<IHaStateCache> cache = new();
        Mock<IHaApiProvider> provider = new();

        EntityTracker sut = new EntityTracker(config, observer.Object, mgr.Object, cache.Object, provider.Object);
        // When
        
        observer.Object.OnStateHandlerInitialized();
    
        // Then
        mgr.Verify(m => m.GetAllEntitiesToTrack(), Times.Once);
    }

    [Fact]
    public void WhenEntityIdsExist_andInCache_andRecent_DoesNotTrigger()
    {
        // Given
        EntityTrackerConfig config = new();
        Mock<ISystemObserver> observer = new();
        observer.Setup(o => o.OnStateHandlerInitialized())
            .Raises(o => o.StateHandlerInitialized += null);

        Mock<IAutomationManager> mgr = new();
        mgr.Setup(m => m.GetAllEntitiesToTrack())
            .Returns(["enterprise"]);

        Mock<IHaStateCache> cache = new();
        cache.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.GetState());
        Mock<IHaApiProvider> provider = new();

        EntityTracker sut = new EntityTracker(config, observer.Object, mgr.Object, cache.Object, provider.Object);
        // When
        
        observer.Object.OnStateHandlerInitialized();
    
        // Then
        mgr.Verify(m => m.GetAllEntitiesToTrack(), Times.Once);
        observer.Verify(o => o.OnBadStateDiscovered(It.IsAny<IEnumerable<BadEntityState>>()), Times.Never);
    }

    [Fact]
    public void WhenEntityIdsExist_andNotInCache_andRecent_DoesNotTrigger()
    {
        // Given
        EntityTrackerConfig config = new();
        Mock<ISystemObserver> observer = new();
        observer.Setup(o => o.OnStateHandlerInitialized())
            .Raises(o => o.StateHandlerInitialized += null);

        Mock<IAutomationManager> mgr = new();
        mgr.Setup(m => m.GetAllEntitiesToTrack())
            .Returns(["enterprise"]);

        Mock<IHaStateCache> cache = new();
        
        Mock<IHaApiProvider> provider = new();
        provider.Setup(c => c.GetEntityState(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new HttpResponseMessage(HttpStatusCode.OK), TestHelpers.GetState(state: "all systems normal")));
        EntityTracker sut = new EntityTracker(config, observer.Object, mgr.Object, cache.Object, provider.Object);
        // When
        
        observer.Object.OnStateHandlerInitialized();
    
        // Then
        mgr.Verify(m => m.GetAllEntitiesToTrack(), Times.Once);
        observer.Verify(o => o.OnBadStateDiscovered(It.IsAny<IEnumerable<BadEntityState>>()), Times.Never);
    }

    [Fact]
    public void WhenNotInCache_andOld_andGood_DoesNotTrigger()
    {
        // Given
        EntityTrackerConfig config = new();
        Mock<ISystemObserver> observer = new();
        observer.Setup(o => o.OnStateHandlerInitialized())
            .Raises(o => o.StateHandlerInitialized += null);

        Mock<IAutomationManager> mgr = new();
        mgr.Setup(m => m.GetAllEntitiesToTrack())
            .Returns(["enterprise"]);

        Mock<IHaStateCache> cache = new();
        
        Mock<IHaApiProvider> provider = new();
        provider.Setup(c => c.GetEntityState(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (new HttpResponseMessage(HttpStatusCode.OK), 
                TestHelpers.GetState(lastUpdated : DateTime.Now.AddDays(-1), state: "all systems normal")));
        EntityTracker sut = new EntityTracker(config, observer.Object, mgr.Object, cache.Object, provider.Object);
        // When
        
        observer.Object.OnStateHandlerInitialized();
    
        // Then
        mgr.Verify(m => m.GetAllEntitiesToTrack(), Times.Once);
        provider.Verify(p => p.GetEntityState("enterprise", It.IsAny<CancellationToken>()), Times.Once);
        observer.Verify(o => o.OnBadStateDiscovered(It.IsAny<IEnumerable<BadEntityState>>()), Times.Never);
    }

    [Fact]
    public void WhenNotInCache_andOld_andBad_DoesTrigger()
    {
        // Given
        EntityTrackerConfig config = new();
        Mock<ISystemObserver> observer = new();
        observer.Setup(o => o.OnStateHandlerInitialized())
            .Raises(o => o.StateHandlerInitialized += null);

        Mock<IAutomationManager> mgr = new();
        mgr.Setup(m => m.GetAllEntitiesToTrack())
            .Returns(["enterprise"]);

        Mock<IHaStateCache> cache = new();
        
        Mock<IHaApiProvider> provider = new();
        provider.Setup(c => c.GetEntityState(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (new HttpResponseMessage(HttpStatusCode.OK), 
                TestHelpers.GetState(state: "unknown", lastUpdated : DateTime.Now.AddDays(-1))));
        EntityTracker sut = new EntityTracker(config, observer.Object, mgr.Object, cache.Object, provider.Object);
        // When
        
        observer.Object.OnStateHandlerInitialized();
    
        // Then
        mgr.Verify(m => m.GetAllEntitiesToTrack(), Times.Once);
        provider.Verify(p => p.GetEntityState("enterprise", It.IsAny<CancellationToken>()), Times.Once);
        observer.Verify(o => o.OnBadStateDiscovered(It.IsAny<IEnumerable<BadEntityState>>()), Times.Once);
    }

    [Fact]
    public void WhenInCache_andOld_andBad_DoesTrigger()
    {
        // Given
        EntityTrackerConfig config = new();
        Mock<ISystemObserver> observer = new();
        observer.Setup(o => o.OnStateHandlerInitialized())
            .Raises(o => o.StateHandlerInitialized += null);

        Mock<IAutomationManager> mgr = new();
        mgr.Setup(m => m.GetAllEntitiesToTrack())
            .Returns(["enterprise"]);

        Mock<IHaStateCache> cache = new();
        cache.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.GetState(lastUpdated: DateTime.Now.AddDays(-1)));
        
        Mock<IHaApiProvider> provider = new();
        provider.Setup(c => c.GetEntityState(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (new HttpResponseMessage(HttpStatusCode.OK), 
                TestHelpers.GetState(state: "unknown", lastUpdated : DateTime.Now.AddDays(-1))));
        EntityTracker sut = new EntityTracker(config, observer.Object, mgr.Object, cache.Object, provider.Object);
        // When
        
        observer.Object.OnStateHandlerInitialized();
    
        // Then
        mgr.Verify(m => m.GetAllEntitiesToTrack(), Times.Once);
        provider.Verify(p => p.GetEntityState("enterprise", It.IsAny<CancellationToken>()), Times.Once);
        observer.Verify(o => o.OnBadStateDiscovered(It.IsAny<IEnumerable<BadEntityState>>()), Times.Once);
    }


}
