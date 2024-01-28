
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
        yield return _automationFactory.SimpleAutomation(
            ["input_button.test_button"],
            (stateChange, CancellationToken) =>
            {
                // access services if needed
                Console.WriteLine($"simple automation triggered from factory");
                return Task.CompletedTask;
            })
            .WithMeta("Simple", "from factory");
        
        yield return _builder.CreateSimple()
            .WithName("office light on")
            .WithDescription("with builder not conditional")
            .WithTriggers("office.motion")
            .WithExecution(async (stateChange, ct) => {
                if (stateChange.New.State == "on")
                {
                    await _services.Api.LightTurnOn("light.office_light", ct);
                }
            })
            .Build();
    }

    public IEnumerable<IConditionalAutomation> RegisterContitionals()
    {
        yield return _automationFactory.LightOffOnNoMotion("binary_sensor.office_motion","light.office_led_light", TimeSpan.FromSeconds(20))
            .WithMeta("office light off", "from factory", false);

        yield return _builder.CreateConditionalWithServices()
            .WithName("office light on")
            .WithDescription("with builder conditional")
            .WithTriggers("office.motion")
            .When(stateChange => stateChange.New.State == "on")
            .Then((svc, ct) => svc.Api.LightTurnOn("light.office_light", ct))
            .Build();

        yield return _builder.CreateConditional()
            .WithName("office light off")
            .WithDescription("with builder")
            .WithTriggers("office.motion")
            .When(stateChange => stateChange.New.State == "off")
            .ForMinutes(5)
            .Then(ct => _services.Api.LightTurnOff("light.office_light", ct))
            .Build();
    }
}
