﻿
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class AutomationRegistrar : IInternalRegistrar
{
    readonly IAutomationTraceProvider _trace;
    readonly ILogger<AutomationWrapper> _logger;

    internal List<AutomationWrapper> RegisteredAutomations { get; private set; } = new();

    public IEnumerable<IAutomationWrapper> Registered => RegisteredAutomations.Select(a => a);

    public AutomationRegistrar(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<ISchedulableAutomation> schedulableAutomations,
        IAutomationTraceProvider traceProvider,
        ILogger<AutomationWrapper> logger)
    {
        _trace = traceProvider;
        _logger = logger;

        RegisterMultiple(automations);
        RegisterMultiple(conditionalAutomations);
        RegisterMultiple(schedulableAutomations);
    }

    public void Register(IAutomation automation)
    {
        AddSimple(automation);
    }

    public void Register(IDelayableAutomation automation)
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

    public void RegisterMultiple(IEnumerable<IDelayableAutomation> automations)
    {
        foreach (var item in automations)
        {
            AddDelayable(item);
        }    
    }


    public void RegisterMultiple(params IAutomation[] automations)
    {
        foreach (var item in automations)
        {
            AddSimple(item);
        }
    }

    public void RegisterMultiple(params IDelayableAutomation[] automations)
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
        var aWrapped = new AutomationWrapper(automation, _trace, GetSourceTypeName());
        RegisteredAutomations.Add(aWrapped);    
    }

    private void AddDelayable(IDelayableAutomation automation)
    {
        var dWrapped = new DelayablelAutomationWrapper(automation, _trace, _logger);
        var aWrapped = new AutomationWrapper(dWrapped, _trace, GetSourceTypeName());
        RegisteredAutomations.Add(aWrapped);
    }

    private void AddDelayableWithEvaluator<T>(T automation, DelayEvaluator<T> evaluator)
        where T : IDelayableAutomation
    {
        var dWrapped = new DelayablelAutomationWrapper(automation, _trace, _logger, () => evaluator(automation));
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
