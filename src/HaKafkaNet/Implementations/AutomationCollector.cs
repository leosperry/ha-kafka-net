using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationCollector
{
    IEnumerable<IAutomation> GetAll();
}

internal class AutomationCollector : IAutomationCollector
{
    private readonly IEnumerable<IAutomation> _automations;
    private readonly IEnumerable<IConditionalAutomation> _conditionalAutomations;
    private readonly IEnumerable<IAutomationRegistry> _registries;
    private readonly IAutomationFactory _automationFactory;
    private readonly ILogger<AutomationCollector> _logger;
    private readonly ILogger<ConditionalAutomationWrapper> _conditionalWrapperlogger;

    public AutomationCollector(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<IAutomationRegistry> registries,
        IAutomationFactory automationFactory,
        ILogger<AutomationCollector> logger,
        ILogger<ConditionalAutomationWrapper> conditionalWrapperlogger)
    {
        this._automations = automations;
        this._conditionalAutomations = conditionalAutomations;
        this._registries = registries;
        this._automationFactory = automationFactory;
        this._logger = logger;
        this._conditionalWrapperlogger = conditionalWrapperlogger;
    }

    public IEnumerable<IAutomation> GetAll()
    {
        var conditionals =
            from ca in _conditionalAutomations
            select new ConditionalAutomationWrapper(ca, _conditionalWrapperlogger);

        IEnumerable<IAutomation> registered = GetRegistered();

        return _automations
            .Union(conditionals)
            .Union(registered);
    }

    private IEnumerable<IAutomation> GetRegistered()
    {
        try
        {
            return 
                from r in _registries
                from a in r.Register(_automationFactory)
                select a;
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration Failed");
            throw;
        }
        
    }
}
