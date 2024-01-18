
namespace HaKafkaNet.ExampleApp;

public class AutomationRegistry : IAutomationRegistry
{
    private readonly IHaServices _services;

    public AutomationRegistry(IHaServices services)
    {
        _services = services;
    }

    public IEnumerable<IAutomation> Register(IAutomationFactory automationFactory)
    {
        yield return GetSimpleAutomation(automationFactory);
    }

    private IAutomation GetSimpleAutomation(IAutomationFactory automationFactory)
    {
        return automationFactory.SimpleAutomation(
            ["input_button.test_button"],
            (stateChange, CancellationToken) =>
            {
                // access services if needed
                Console.WriteLine($"simple automation triggered from factory");
                return Task.CompletedTask;
            });
    }
}
