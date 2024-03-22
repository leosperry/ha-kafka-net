using Confluent.Kafka;

namespace HaKafkaNet.ExampleApp;

public class ConditionalAutomationExample : IConditionalAutomation, IAutomationMeta
{

    private int _buttonTracker = 0;
    private bool _colorTracker = default;
    private readonly IHaServices _services;
    private readonly ILogger<ConditionalAutomationExample> _logger;
    const string LIGHT_ID = "light.office_led_light";

    public ConditionalAutomationExample(IHaServices services, ILogger<ConditionalAutomationExample> logger)
    {
        this._services = services;
        this._logger = logger;
    }

    //interface implementations
    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "input_button.test_button_3";
    }    

    public Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        _buttonTracker++;
        _logger.LogInformation("tracker = {value}", _buttonTracker);
        // simulate that a motion sensor could report multiple times, a condition that should not cancel, and then eventually does
        // like a motion sensor reporting clear or "off".
        return Task.FromResult(!(_buttonTracker % 3 == 0));
    }

    public TimeSpan For => TimeSpan.FromSeconds(5);

    public Task Execute(CancellationToken cancellationToken)
    {
        // time elapsed without canceling and we are now executing
        // reset the tracker back to known state
        _buttonTracker = 0;

        LightTurnOnModel color1 = new LightTurnOnModel()
        {
            EntityId = [LIGHT_ID],
            RgbColor = (255, 255, 0)
        };

        LightTurnOnModel color2 = new LightTurnOnModel()
        {
            EntityId = [LIGHT_ID],
            RgbColor = (255, 0, 255)
        };

        return _services.Api.LightTurnOn((_colorTracker = !_colorTracker) ? color1 : color2, cancellationToken);
    }

    public AutomationMetaData GetMetaData()
    {
        return new()
        {
            Name = "Example conditional automation",
            Description = "Sets some lights when a test button is pushed",
        };
    }
}

