using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationCollector
{
    IEnumerable<IAutomation> GetAll();
}

internal class AutomationManager : IAutomationCollector
{

    private readonly ILogger<AutomationManager> _logger;

    private readonly List<IAutomation> _internalAutomations = new List<IAutomation>();

    public AutomationManager(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<IAutomationRegistry> registries,
        IAutomationFactory automationFactory,
        ILogger<AutomationManager> logger,
        ILogger<ConditionalAutomationWrapper> conditionalWrapperlogger)
    {
        this._logger = logger;

        Initialize(automations, conditionalAutomations, registries, automationFactory, conditionalWrapperlogger);
    }

    public IEnumerable<IAutomation> GetAll()
    {
        return _internalAutomations;
    }

    private void Initialize(IEnumerable<IAutomation> automations, IEnumerable<IConditionalAutomation> conditionalAutomations, IEnumerable<IAutomationRegistry> registries, IAutomationFactory automationFactory, ILogger<ConditionalAutomationWrapper> conditionalWrapperlogger)
    {
        var conditionals =
            from ca in conditionalAutomations.Union(GetRegisteredConditionals(registries, automationFactory))
            select new ConditionalAutomationWrapper(ca, conditionalWrapperlogger);

        IEnumerable<IAutomation> registered = GetRegistered(registries, automationFactory);

        _internalAutomations.AddRange( 
            automations
            .Union(conditionals)
            .Union(registered));
    }

    private IEnumerable<IConditionalAutomation> GetRegisteredConditionals(IEnumerable<IAutomationRegistry> registries, IAutomationFactory automationFactory)
    {
        try
        {
            return 
                from r in registries
                from a in r.RegisterContitionals(automationFactory)
                select a;
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration for conditionals Failed");
            throw;
        }    
    }

    private IEnumerable<IAutomation> GetRegistered(IEnumerable<IAutomationRegistry> registries, IAutomationFactory automationFactory)
    {
        try
        {
            return 
                from r in registries
                from a in r.Register(automationFactory)
                select a;
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration Failed");
            throw;
        }
    }
}
