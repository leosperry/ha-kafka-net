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

        IEnumerable<ISchedulableAutomation> schedulables = Enumerable.Empty<ISchedulableAutomation>();

        
        Mock<ILogger<AutomationWrapper>> logger = new();
        Mock<IAutomationTraceProvider> trace = new();

        var sut = new AutomationRegistrar(
            autos, conditionals, schedulables, trace.Object, logger.Object);
        // When

        var result = sut.RegisteredAutomations;
    
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

        Mock<ISchedulableAutomation> schedulable = new();
        IEnumerable<ISchedulableAutomation> schedulables = [schedulable.Object];
        
        Mock<ILogger<AutomationWrapper>> logger = new();
                Mock<IAutomationTraceProvider> trace = new();


        var sut = new AutomationRegistrar(
            autos, conditionals, schedulables, trace.Object, logger.Object);
        // When

        sut.Register(auto.Object);
        sut.RegisterDelayed(conditional.Object);
        sut.RegisterDelayed(schedulable.Object);
        var result = sut.RegisteredAutomations;
    
        // Then

        Assert.Equal(6, result.Count());
    }
}
