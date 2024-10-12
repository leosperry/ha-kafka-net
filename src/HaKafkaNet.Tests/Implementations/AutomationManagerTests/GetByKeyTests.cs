using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class GetByKeyTests
{
    [Fact]
    public void WheMetaNotSet_ReturnsByCleanedKey()
    {
        // Given
        Mock<IAutomationTraceProvider> trace = new();

        AutomationWrapper wrapper = new AutomationWrapper(new FakeAuto(), trace.Object, "test");
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([wrapper]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        // When
        var sut = new AutomationManager(registries, registrar.Object); 
        sut.Initialize(new List<InitializationError>());

        var result = sut.GetByKey("test-fakeauto-crew_spock-crew_evil_spock");
    
        // Then
        Assert.Equal("FakeAuto", result!.GetMetaData().Name);
    }

    [Fact]
    public void WheMultiple_ReturnsByCleanedKey()
    {
        // Given
        Mock<IAutomationTraceProvider> trace = new();

        AutomationWrapper wrapper1 = new AutomationWrapper(new FakeAuto(), trace.Object, "test");
        AutomationWrapper wrapper2 = new AutomationWrapper(new FakeAuto(), trace.Object, "test");
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([wrapper1, wrapper2]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        // When
        var sut = new AutomationManager(registries, registrar.Object); 
        sut.Initialize(new List<InitializationError>());

        var result1 = sut.GetByKey("test-fakeauto-crew_spock-crew_evil_spock");
        var result2 = sut.GetByKey("test-fakeauto-crew_spock-crew_evil_spock2");
    
        // Then
        Assert.Equal("FakeAuto", result1!.GetMetaData().Name);
        Assert.Equal("FakeAuto", result2!.GetMetaData().Name);
    }

    [Fact]
    public void WheMetaSet_ReturnsByCleanedKey()
    {
        // Given
        var fake = new FakeAutoWithMeta();
        fake.SetKey("!@#$ Evil !@#$ Spock !@#$");
        Mock<IAutomationTraceProvider> trace = new();
        AutomationWrapper wrapper = new AutomationWrapper(fake, trace.Object, "test");
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([wrapper]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        // When
        var sut = new AutomationManager(registries, registrar.Object); 
        sut.Initialize(new List<InitializationError>());

        var result = sut.GetByKey("evil_spock");
    
        // Then
        Assert.Equal("Spock", result!.GetMetaData().Name);
    }
}
