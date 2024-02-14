
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

    /// <summary>
    /// Add this method when upgrading to version 3 
    /// with the 2 calls to RegisterMultiple
    /// </summary>
    /// <param name="reg"></param>
    public void Register(IRegistrar reg)
    {
        reg.RegisterMultiple(Register());
        reg.RegisterMultiple(RegisterContitionals());

        // new feature! Turn on front porch light 15 minutes before sunset
        reg.Register(_automationFactory.SunSetAutomation(
            ct => _services.Api.TurnOn("light.front_porch"), 
            TimeSpan.FromMinutes(-15)));
    }

    [Obsolete("No longer a part of IAutomationRegistry", false)] // for information only
    public IEnumerable<IAutomation> Register()
    {
        return Enumerable.Empty<IAutomation>();
    }


    [Obsolete("No longer a part of IAutomationRegistry", false)] // for information only
    public IEnumerable<IConditionalAutomation> RegisterContitionals()
    {
        return Enumerable.Empty<IConditionalAutomation>();
    }
}
