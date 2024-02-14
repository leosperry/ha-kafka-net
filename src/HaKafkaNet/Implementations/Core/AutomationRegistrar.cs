
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class AutomationRegistrar : IInternalRegistrar
{
    private ISystemObserver _observer;
    private ILogger<AutomationRegistrar> _logger;

    internal List<AutomationWrapper> RegisteredAutomations { get; private set; } = new();

    public IEnumerable<IAutomationWrapper> Registered => RegisteredAutomations.Select(a => a);

    public AutomationRegistrar(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<ISchedulableAutomation> schedulableAutomations,
        ISystemObserver observer, ILogger<AutomationRegistrar> logger)
    {
        _observer = observer;
        _logger = logger;

        RegisterMultiple(automations);
        RegisterMultiple(conditionalAutomations);
        RegisterMultiple(schedulableAutomations);
    }

    public void Register(IAutomation automation)
    {
        AddSimple(automation);
    }

    public void Register(IConditionalAutomation automation)
    {
        AddDelayable(automation);
    }

    public void Register(ISchedulableAutomation automation)
    {
        AddDelayable(automation);
    }

    public void Register<T>(T automation, DelayEvaluator<T> delayEvaluator) where T : IDelayableAutomation
    {
        AddDelayableWithEvaluator(automation, delayEvaluator);
    }

    public void RegisterMultiple(IEnumerable<IAutomation> automations)
    {
        foreach (var item in automations)
        {
            AddSimple(item);
        }
    }

    public void RegisterMultiple(IEnumerable<IConditionalAutomation> automations)
    {
        foreach (var item in automations)
        {
            AddDelayable(item);
        }    
    }

    public void RegisterMultiple(IEnumerable<ISchedulableAutomation> automations)
    {
        foreach (var item in automations)
        {
            AddDelayable(item);
        }    
    }

    public void RegisterMultiple<T>(IEnumerable<T> automations, DelayEvaluator<T> delayEvaluator) 
        where T : IDelayableAutomation
    {
        foreach (var item in automations)
        {
            AddDelayableWithEvaluator(item, delayEvaluator);
        }
    }

    private void AddSimple(IAutomation automation)
    {
        var aWrapped = new AutomationWrapper(automation, _logger, GetSourceTypeName());
        RegisteredAutomations.Add(aWrapped);    
    }

    private void AddDelayable(IDelayableAutomation automation)
    {
        var dWrapped = new DelayablelAutomationWrapper(automation, _observer, _logger);
        var aWrapped = new AutomationWrapper(dWrapped, _logger, GetSourceTypeName());
        RegisteredAutomations.Add(aWrapped);
    }

    private void AddDelayableWithEvaluator<T>(T automation, DelayEvaluator<T> evaluator)
        where T : IDelayableAutomation
    {
        var dWrapped = new DelayablelAutomationWrapper(automation, _observer, _logger, () => evaluator(automation));
        var aWrapped = new AutomationWrapper(dWrapped, _logger, GetSourceTypeName());
    }

    private string GetSourceTypeName()
    {
        StackTrace trace = new StackTrace();
        var name = trace.GetFrame(3)?.GetMethod()?.DeclaringType?.Name;
        if (name == nameof(AutomationRegistrar))
        {
            name = "discovery";
        }
        return name ?? "unknown";
    }
}
