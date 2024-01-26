using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class GetAllTests
{
    [Fact]
    public void WhenNonePassedIn_returnsEmptyEnumerable()
    {
        // Given
        var autos = Enumerable.Empty<IAutomation>();
        var conditionals = Enumerable.Empty<IConditionalAutomation>();
        var registries = Enumerable.Empty<IAutomationRegistry>();
        
        Mock<IAutomationFactory> factory = new();
        Mock<ILogger<AutomationManager>> logger = new();

        var sut = new AutomationManager(
            autos, conditionals, registries, factory.Object, logger.Object);
        // When

        var result = sut.GetAll();
    
        // Then
        Assert.Empty(result);
    }

    [Fact]
    public void When1EachPassedIn_ReturnsAll()
    {
        // Given
        Mock<IAutomation> auto = new();
        IEnumerable<IAutomation> autos = [auto.Object];
        
        Mock<IConditionalAutomation> conditional = new();
        IEnumerable<IConditionalAutomation> conditionals = [conditional.Object];
        
        Mock<IAutomationFactory> factory = new();

        Mock<IAutomationRegistry> registry = new();
        Mock<IAutomation> registeredAuto = new();
        registry.Setup(r => r.Register(factory.Object))
            .Returns([registeredAuto.Object]);
        Mock<IConditionalAutomation> registeredConditional = new();
        registry.Setup(r => r.RegisterContitionals(factory.Object))
            .Returns([registeredConditional.Object]);
        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> logger = new();

        var sut = new AutomationManager(
            autos, conditionals, registries, factory.Object, logger.Object);
        // When

        var result = sut.GetAll();
    
        // Then
        registry.Verify(r => r.Register(factory.Object), Times.Once);
        registry.Verify(r => r.RegisterContitionals(factory.Object), Times.Once);
        Assert.Equal(4, result.Count());
    }
}
