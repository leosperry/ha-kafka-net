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
        
        Mock<ILogger<AutomationManager>> logger = new();

        var sut = new AutomationManager(
            autos, conditionals, registries, logger.Object);
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
        
        Mock<IAutomationRegistry> registry = new();
        Mock<IAutomation> registeredAuto = new();
        registry.Setup(r => r.Register())
            .Returns([registeredAuto.Object]);
        Mock<IConditionalAutomation> registeredConditional = new();
        registry.Setup(r => r.RegisterContitionals())
            .Returns([registeredConditional.Object]);
        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> logger = new();

        var sut = new AutomationManager(
            autos, conditionals, registries, logger.Object);
        // When

        var result = sut.GetAll();
    
        // Then
        registry.Verify(r => r.Register(), Times.Once);
        registry.Verify(r => r.RegisterContitionals(), Times.Once);
        Assert.Equal(4, result.Count());
    }
}
