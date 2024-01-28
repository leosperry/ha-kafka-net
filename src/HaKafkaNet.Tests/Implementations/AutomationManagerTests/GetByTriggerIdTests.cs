using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class GetByTriggerIdTests
{
    [Fact]
    public void WhenTriggerIdDoesntMatch_ReturnsEmpty()
    {
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
        var result = sut.GetByTriggerEntityId(string.Empty);

        // Then
        Assert.Empty(result);
    }
    
    [Fact]
    public void WhenAllTriggerIdMatch_ReturnsAll()
    {
        string triggerId = "NCC-1701";
        Mock<IAutomation> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        IEnumerable<IAutomation> autos = [auto.Object];
        
        Mock<IConditionalAutomation> conditional = new();
        conditional.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        IEnumerable<IConditionalAutomation> conditionals = [conditional.Object];
        
        Mock<IAutomationRegistry> registry = new();
        Mock<IAutomation> registeredAuto = new();
        registeredAuto.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        registry.Setup(r => r.Register())
            .Returns([registeredAuto.Object]);
        Mock<IConditionalAutomation> registeredConditional = new();
        registeredConditional.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        registry.Setup(r => r.RegisterContitionals())
            .Returns([registeredConditional.Object]);
        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> logger = new();

        var sut = new AutomationManager(
            autos, conditionals, registries, logger.Object);
        // When
        var result = sut.GetByTriggerEntityId(triggerId);

        // Then
        Assert.Equal(4, result.Count());
    }
    
    [Fact]
    public void WhenUnionExists_ReturnsCorrectly()
    {
        string enterprise = "NCC-1701";
        string excelsior = "NCC-2000";
        Mock<IAutomation> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns([enterprise]);
        IEnumerable<IAutomation> autos = [auto.Object];
        
        Mock<IConditionalAutomation> conditional = new();
        conditional.Setup(a => a.TriggerEntityIds()).Returns([enterprise, excelsior]);
        IEnumerable<IConditionalAutomation> conditionals = [conditional.Object];
        
        Mock<IAutomationRegistry> registry = new();
        Mock<IAutomation> registeredAuto = new();
        registeredAuto.Setup(a => a.TriggerEntityIds()).Returns([excelsior]);
        registry.Setup(r => r.Register())
            .Returns([registeredAuto.Object]);
        Mock<IConditionalAutomation> registeredConditional = new();
        registeredConditional.Setup(a => a.TriggerEntityIds()).Returns(default(IEnumerable<string>)!);
        registry.Setup(r => r.RegisterContitionals())
            .Returns([registeredConditional.Object]);
        IEnumerable<IAutomationRegistry> registries = [registry.Object];
        
        Mock<ILogger<AutomationManager>> logger = new();

        var sut = new AutomationManager(
            autos, conditionals, registries, logger.Object);
        // When
        var enterpriseResult = sut.GetByTriggerEntityId(enterprise);
        var excelsiorResult = sut.GetByTriggerEntityId(excelsior);
        var voyagerResult = sut.GetByTriggerEntityId("NCC-74656");

        // Then
        Assert.Equal(2, enterpriseResult.Count());
        Assert.Equal(2, excelsiorResult.Count());
        Assert.Empty(voyagerResult);
    }
    
}
