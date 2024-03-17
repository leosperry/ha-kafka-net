using HaKafkaNet;

namespace HaKafkaNet.ExampleApp;

/// <summary>
/// This automation demonstrates 2-way communication with Home Assitant and handling events which happened prior to startup
/// It assumes you have created a Helper Button in Home Assistant named Test Button. It should have an id of "input_button.test_button".
/// When that button is pushed it sends a notification to Home assistant.
/// If the button was pushed before startup, a message is written to the console, but no notiication is sent
/// To see the 4 event timings in action
///     * clear cache, click the button, then start this app
///     * with the app running, click the button, watch the notification go through, then restart app
///     * stop the app, click the button, then restart the app
/// </summary>
public class AutomationWithPreStartup : IAutomation
{
    IHaApiProvider _api;
    ILogger<AutomationWithPreStartup> _logger;

    public AutomationWithPreStartup(IHaApiProvider haApiProvider, ILogger<AutomationWithPreStartup> logger)
    {
        _api = haApiProvider;
        _logger = logger;
    }

    public string Name { get => "Automation with Pre-startup events handled"; }

    public EventTiming EventTimings 
    { 
        get => EventTiming.PreStartupNotCached | EventTiming.PreStartupSameAsLastCached | EventTiming.PreStartupPostLastCached | EventTiming.PostStartup; 
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "input_button.test_button";
    }

    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        var message = $"test button last changed at : {stateChange.New.LastChanged}";
        
        switch (stateChange.EventTiming)
        {
            case EventTiming.PreStartupNotCached:
            case EventTiming.PreStartupSameAsLastCached:
            case EventTiming.PreStartupPostLastCached:
                _logger.LogInformation(message + " - {timing}", stateChange.EventTiming);
                return Task.CompletedTask;
            case EventTiming.PostStartup:
                _logger.LogInformation("Sending Persistent Notification");
                return _api.PersistentNotification(message, cancellationToken);
            default:
                return Task.CompletedTask;
        }
    }
}
