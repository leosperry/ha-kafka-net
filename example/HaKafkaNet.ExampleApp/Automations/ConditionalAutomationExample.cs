using Confluent.Kafka;

namespace HaKafkaNet.ExampleApp;

public class ConditionalAutomationExample : IConditionalAutomation
{
    public TimeSpan For => TimeSpan.FromSeconds(5);

    private bool _buttonTracker = default;
    private bool _colorTracker = default;
    private readonly IHaServices _services;
    private readonly ILogger<ConditionalAutomationExample> _logger;
    const string LIGHT_ID = "light.office_led_light";

    public ConditionalAutomationExample(IHaServices services, ILogger<ConditionalAutomationExample> logger)
    {
        this._services = services;
        this._logger = logger;
    }

    public Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        _buttonTracker = !_buttonTracker;
        _logger.LogInformation("tracker = {value}", _buttonTracker);
        return Task.FromResult(_buttonTracker);
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        _buttonTracker = false;
        LightTurnOnModel color1 = new LightTurnOnModel()
        {
            EntityId = LIGHT_ID,
            RgbColor = (255,255,0)
        };

        LightTurnOnModel color2 = new LightTurnOnModel()
        {
            EntityId = LIGHT_ID,
            RgbColor = (255,0, 255)
        };

        return _services.Api.LightTurnOn((_colorTracker = !_colorTracker) ? color1 : color2, cancellationToken);

    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "input_button.test_button_3";
    }
}

