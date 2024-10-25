using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class GetAllTests
{
    [Fact]
    public void WhenNonePassedIn_returnsEmptyEnumerable()
    {
        // Given
        var autos = Enumerable.Empty<IAutomation>();
        
        Mock<ILogger<AutomationWrapper>> logger = new();
        Mock<IAutomationTraceProvider> trace = new();
        Mock<ISystemObserver> observer = new();
        Mock<IWrapperFactory> wrapperFactory = new();

        var sut = new AutomationRegistrar(wrapperFactory.Object,
            autos, trace.Object, observer.Object, new List<InitializationError>(), logger.Object);
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

        Mock<ISchedulableAutomation> schedulable = new();
        
        Mock<ILogger<AutomationWrapper>> logger = new();
                Mock<IAutomationTraceProvider> trace = new();
        
        Mock<ISystemObserver> observer = new();

        Mock<IWrapperFactory> wrapperFactory = new();
        wrapperFactory.Setup(w => w.GetWrapped(It.IsAny<IAutomationBase>()))
            .Returns([new Mock<IAutomation>().Object]);


        var sut = new AutomationRegistrar(
            wrapperFactory.Object,
            autos, trace.Object, observer.Object, new List<InitializationError>(), logger.Object);
        
        // When
        sut.Register(auto.Object);
        sut.RegisterDelayed(conditional.Object);
        sut.RegisterDelayed(schedulable.Object);
        var result = sut.RegisteredAutomations;
    
        // Then
        Assert.Equal(4, result.Count());
    }
}
