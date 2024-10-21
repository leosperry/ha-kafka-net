
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class AutomationRegistrar : IInternalRegistrar
{
    readonly IAutomationTraceProvider _trace;
    readonly ILogger<AutomationWrapper> _logger;
    private readonly ISystemObserver _observer;

    internal List<AutomationWrapper> RegisteredAutomations { get; private set; } = new();

    public IEnumerable<IAutomationWrapper> Registered => RegisteredAutomations.Select(a => a);

    public AutomationRegistrar(
        IEnumerable<IAutomation> automations,
        IAutomationTraceProvider traceProvider,
        ISystemObserver observer,
        ILogger<AutomationWrapper> logger
        )
    {
        _trace = traceProvider;
        _logger = logger;
        this._observer = observer;
        Register(automations.ToArray());
    }

    public void Register(params IAutomation[] automations)
    {
        foreach (var item in automations)
        {
            AddSimple(item);
        }
    }

    public void RegisterDelayed(params IDelayableAutomation[] automations)
    {
        foreach (var item in automations)
        {
            AddDelayable(item);
        }
    }

    public void RegisterTyped<Tauto, Tstate, Tatt>(params Tauto[] automations)
        where Tauto: IAutomation<Tstate, Tatt>
    {
        foreach (var item in automations)
        {
            var wrapped = new TypedAutomationWrapper<Tauto, Tstate, Tatt>(item, _observer);
            AddSimple(wrapped);
        }
    }

    public void RegisterDelayedTyped<Tauto, Tstate, Tatt>(params Tauto[] automations)
        where Tauto : IDelayableAutomation<Tstate, Tatt>
    {
        foreach (var item in automations)
        {
            var wrapped = new TypedDelayedAutomationWrapper<Tauto, Tstate, Tatt>(item, _observer);
            AddDelayable(wrapped);
        }
    }

    public void RegisterWithDelayEvaluator<T>(T automation, DelayEvaluator<T> delayEvaluator) where T : IDelayableAutomation
    {
        AddDelayableWithEvaluator(automation, delayEvaluator);
    }

    public void RegisterMultipleWithDelayEvaluator<T>(IEnumerable<T> automations, DelayEvaluator<T> delayEvaluator) 
        where T : IDelayableAutomation
    {
        foreach (var item in automations)
        {
            AddDelayableWithEvaluator(item, delayEvaluator);
        }
    }

    private void AddSimple(IAutomation automation)
    {
        var aWrapped = new AutomationWrapper(automation, _trace, GetSourceTypeName());
        RegisteredAutomations.Add(aWrapped);    
    }

    private void AddDelayable<T>(T automation) where T: IDelayableAutomation
    {
        var dWrapped = new DelayablelAutomationWrapper<T>(automation, _trace, _logger);
        var aWrapped = new AutomationWrapper(dWrapped, _trace, GetSourceTypeName());
        RegisteredAutomations.Add(aWrapped);
    }

    private void AddDelayableWithEvaluator<T>(T automation, DelayEvaluator<T> evaluator)
        where T : IDelayableAutomation
    {
        var dWrapped = new DelayablelAutomationWrapper<T>(automation, _trace, _logger, () => evaluator(automation));
        var aWrapped = new AutomationWrapper(dWrapped, _trace, GetSourceTypeName());
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
