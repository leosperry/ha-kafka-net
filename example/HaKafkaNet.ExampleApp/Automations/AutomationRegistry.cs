
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

    public IEnumerable<IConditionalAutomation> RegisterContitionals(IAutomationFactory automationFactory)
    {
        yield return automationFactory.LightOffOnNoMotion("binary_sensor.office_motion","light.office_led_light", TimeSpan.FromSeconds(20));
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
