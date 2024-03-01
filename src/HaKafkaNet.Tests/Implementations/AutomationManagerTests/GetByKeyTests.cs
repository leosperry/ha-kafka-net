using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class GetByKeyTests
{
    [Fact]
    public void WheMetaNotSet_ReturnsByCleanedKey()
    {
        // Given
        Mock<ILogger> logger = new();

        AutomationWrapper wrapper = new AutomationWrapper(new FakeAuto(), logger.Object, "test");
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([wrapper]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> mgrLogger = new();
        Mock<ISystemObserver> observer = new();
   
        // When
        var sut = new AutomationManager(registries, registrar.Object, observer.Object, mgrLogger.Object); 

        var result = sut.GetByKey("test-FakeAuto-crew_spock-crew_evil_spock");
    
        // Then
        Assert.Equal("FakeAuto", result.GetMetaData().Name);
    }

    [Fact]
    public void WheMultiple_ReturnsByCleanedKey()
    {
        // Given
        Mock<ILogger> logger = new();

        AutomationWrapper wrapper1 = new AutomationWrapper(new FakeAuto(), logger.Object, "test");
        AutomationWrapper wrapper2 = new AutomationWrapper(new FakeAuto(), logger.Object, "test");
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([wrapper1, wrapper2]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> mgrLogger = new();
        Mock<ISystemObserver> observer = new();
   
        // When
        var sut = new AutomationManager(registries, registrar.Object, observer.Object, mgrLogger.Object); 

        var result1 = sut.GetByKey("test-FakeAuto-crew_spock-crew_evil_spock");
        var result2 = sut.GetByKey("test-FakeAuto-crew_spock-crew_evil_spock2");
    
        // Then
        Assert.Equal("FakeAuto", result1.GetMetaData().Name);
        Assert.Equal("FakeAuto", result2.GetMetaData().Name);
    }

    [Fact]
    public void WheMetaSet_ReturnsByCleanedKey()
    {
        // Given
        Mock<ILogger> logger = new();
        var fake = new FakeAutoWithMeta();
        fake.SetKey("!@#$ Evil !@#$ Spock !@#$");
        AutomationWrapper wrapper = new AutomationWrapper(new FakeAutoWithMeta(), logger.Object, "test");
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([wrapper]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> mgrLogger = new();
        Mock<ISystemObserver> observer = new();
   
        // When
        var sut = new AutomationManager(registries, registrar.Object, observer.Object, mgrLogger.Object); 

        var result = sut.GetByKey("Evil_Spock");
    
        // Then
        Assert.Equal("Spock", result.GetMetaData().Name);
    }
}
