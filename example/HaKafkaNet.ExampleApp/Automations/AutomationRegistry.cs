
namespace HaKafkaNet.ExampleApp;

public class AutomationRegistry : IAutomationRegistry
{
    private readonly IHaServices _services;
    private readonly IAutomationBuilder _builder;
    IAutomationFactory _automationFactory;

    public AutomationRegistry(IHaServices services, IAutomationBuilder builder, IAutomationFactory automationFactory)
    {
        _services = services;
        _builder = builder;
        _automationFactory = automationFactory;
    }

    public IEnumerable<IAutomation> Register()
    {
        return Enumerable.Empty<IAutomation>();
    }

    public IEnumerable<IConditionalAutomation> RegisterContitionals()
    {
        return Enumerable.Empty<IConditionalAutomation>();
    }
}
