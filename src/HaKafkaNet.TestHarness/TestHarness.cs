

using Moq;

namespace HaKafkaNet.TestHarness;


public class TestHarness
{
    public Mock<IHaEntityProvider> EntityProvider { get; private set; }
    public Mock<IHaApiProvider> ApiProvider { get; private set; }
    public Mock<IHaStateCache> Cache { get; private set; }
    public Mock<IHaServices> Services { get; set; }

    public TestHarness()
    {
        ApiProvider = new();
        Cache = new();
        EntityProvider = new();
        Services = new();
        Services.Setup(s => s.Api).Returns(ApiProvider.Object);
        Services.Setup(s => s.Cache).Returns(Cache.Object);
        Services.Setup(s => s.EntityProvider).Returns(EntityProvider.Object);

        
    }

    public void SendState(HaEntityState state)
    {

    }
}
