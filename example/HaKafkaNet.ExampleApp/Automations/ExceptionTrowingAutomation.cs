
namespace HaKafkaNet.ExampleApp;

/// <summary>
/// This automation is for demonstration/testing purposes
/// The execute method has several exceptions that can be 
/// commented/uncomented for testing different scenarios
/// </summary>
[ExcludeFromDiscovery]
public class ExceptionTrowingAutomation : IConditionalAutomation, IAutomationMeta
{
    readonly ILogger _logger;
    readonly TimeSpan _for = TimeSpan.FromSeconds(5);
    public TimeSpan For => _for;

    private bool _switchState = false;

    public ExceptionTrowingAutomation(ILogger<ExceptionTrowingAutomation> logger)
    {
        _logger = logger;
    }

    public Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken _)
    {
        var onOff = haEntityStateChange.ToOnOff();
        _switchState = onOff.IsOn();
        if (_switchState)
        {
            _logger.LogWarning("The switch is on at: {OnTime}", haEntityStateChange.New.LastUpdated);
        }
        else
        {
            _logger.LogInformation("The switch is off at {OffTime}", haEntityStateChange.New.LastUpdated);
        }
        
        return Task.FromResult(true);
    }

    public Task Execute(CancellationToken ct)
    {
        if (_switchState)
        {
            _logger.LogWarning("Switch is still on!");
        }
        else
        {
            // use this WhenAll to test an un-awaited exception.
            // The tracing system should report both errors
            // return Task.WhenAll(
            //     Task.Delay(100).ContinueWith(t => throw new Exception("ha ha")),
            //     Task.Delay(100).ContinueWith(t => throw new Exception("ho ho"))
            // );
            try
            {
                throw new HaKafkaNetException("Example Exception!!!");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            
            throw new Exception("Test Override Exception");
        }
        return Task.CompletedTask;
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        yield return "input_boolean.test_switch";
    }

    readonly AutomationMetaData _meta = new()
    {
        Name = "Example Logging and Exceptions",
        Description = "This automation is for Trace and Log Capturing demonstration purposes. It logs at various levels and conditinally throws an exception.", 
        Enabled = false
    };
    public AutomationMetaData GetMetaData() => _meta;
}
