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
        Mock<IAutomationWrapper<object>> registeredAuto = new();
        registeredAuto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData()
        {
            Name = "auto"
        });

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([registeredAuto.Object]);

        IEnumerable<IAutomationRegistry> registries = [registry.Object];

        var sut = new AutomationManager(
            registries, registrar.Object);
        // When
        var result = sut.GetByTriggerEntityId(string.Empty);

        // Then
        Assert.Empty(result);
    }
    
    [Fact]
    public void WhenAllTriggerIdMatch_ReturnsAll()
    {
        string triggerId = "NCC-1701";
        Mock<IAutomationWrapper<object>> auto = new();
        auto.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        auto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name = "auto"});

        
        Mock<IConditionalAutomation> conditional = new();
        conditional.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        IEnumerable<IConditionalAutomation> conditionals = [conditional.Object];
        
        Mock<IAutomationRegistry> registry = new();

        Mock<IAutomationWrapper<object>> registeredAuto = new();
        registeredAuto.Setup(a => a.TriggerEntityIds()).Returns([triggerId]);
        registeredAuto.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name="a"});
        
        IEnumerable<IAutomationRegistry> registries = [registry.Object];

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([auto.Object, registeredAuto.Object]);

        var sut = new AutomationManager(
            registries, registrar.Object);
        sut.Initialize(new List<InitializationError>());
        // When
        var result = sut.GetByTriggerEntityId(triggerId);

        // Then
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public void WhenUnionExists_ReturnsCorrectly()
    {
        string enterprise = "NCC-1701";
        string excelsior = "NCC-2000";
        Mock<IAutomationWrapper<object>> auto1 = new();
        auto1.Setup(a => a.TriggerEntityIds()).Returns([enterprise]);
        auto1.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name="a"});
        
        Mock<IAutomationWrapper<object>> auto2 = new();
        auto2.Setup(a => a.TriggerEntityIds()).Returns([enterprise, excelsior]);
        auto2.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name="a"});
        
        Mock<IAutomationWrapper<object>> auto3 = new();
        auto3.Setup(a => a.TriggerEntityIds()).Returns([excelsior]);
        auto3.Setup(a => a.GetMetaData()).Returns(new AutomationMetaData(){Name="a"});
        
        Mock<IAutomationRegistry> registry = new();
        IEnumerable<IAutomationRegistry> registries = [registry.Object];

        Mock<IInternalRegistrar> registrar = new();
        registrar.Setup(r => r.Registered)
            .Returns([auto1.Object, auto3.Object, auto2.Object]);

        var sut = new AutomationManager(registries, registrar.Object);
        sut.Initialize(new List<InitializationError>());
        // 
        var enterpriseResult = sut.GetByTriggerEntityId(enterprise);
        var excelsiorResult = sut.GetByTriggerEntityId(excelsior);
        var voyagerResult = sut.GetByTriggerEntityId("NCC-74656");

        // Then
        Assert.Equal(2, enterpriseResult.Count());
        Assert.Equal(2, excelsiorResult.Count());
        Assert.Empty(voyagerResult);
    }
    
}
