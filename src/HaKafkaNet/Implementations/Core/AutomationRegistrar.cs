
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class AutomationRegistrar : IInternalRegistrar
{
    private readonly IWrapperFactory _wrapperFactory;
    readonly IAutomationTraceProvider _trace;
    readonly ILogger<AutomationWrapper> _logger;
    private readonly ISystemObserver _observer;
    private readonly List<InitializationError> _errors;


    internal List<AutomationWrapper> RegisteredAutomations { get; private set; } = new();

    public IEnumerable<IAutomationWrapper> Registered => RegisteredAutomations.Select(a => a);

    public AutomationRegistrar(
        IWrapperFactory wrapperFactory,
        IEnumerable<IAutomation> automations,
        IAutomationTraceProvider traceProvider,
        ISystemObserver observer,
        List<InitializationError> errors,
        ILogger<AutomationWrapper> logger
        )
    {
        this._wrapperFactory = wrapperFactory;
        _trace = traceProvider;
        _logger = logger;
        this._errors = errors;
        this._observer = observer;
        Register(automations.ToArray());
    }

    public void Register(params IAutomation[] automations)
    {
        foreach (var item in automations)
        {
            AddSimple(item, 0);
        }
    }

    public void RegisterDelayed(params IDelayableAutomation[] automations)
    {
        foreach (var item in automations)
        {
            AddDelayable(item, 0);
        }
    }

    public void Register<Tstate, Tatt>(params IAutomation<Tstate,Tatt>[] automations)
    {
        foreach (var item in automations)
        {
            var wrapped = new TypedAutomationWrapper<IAutomation<Tstate, Tatt>, Tstate, Tatt>(item, _observer);
            AddSimple(wrapped, 0);
        }
    }

    public void RegisterDelayed<Tstate, Tatt>(params IDelayableAutomation<Tstate, Tatt>[] automations)
    {
        foreach (var item in automations)
        {
            var wrapped = new TypedDelayedAutomationWrapper<IDelayableAutomation<Tstate, Tatt>, Tstate, Tatt>(item, _observer);
            AddDelayable(wrapped, 0);
        }
    }

    public void RegisterWithDelayEvaluator<T>(T automation, DelayEvaluator<T> delayEvaluator) where T : IDelayableAutomation
    {
        throw new NotImplementedException("deprecated");
    }

    public void RegisterMultipleWithDelayEvaluator<T>(IEnumerable<T> automations, DelayEvaluator<T> delayEvaluator) 
        where T : IDelayableAutomation
    {
        throw new NotImplementedException("deprecated");
    }

    public bool TryRegister(params Func<IAutomationBase>[] activators)
    {
        bool success = true;
        foreach (var activator in activators)
        {
            try
            {
                var auto = activator();
                AddAny(auto, 0);
            }
            catch (System.Exception ex)
            {
                var stack = new StackTrace();
                _errors.Add(new("activator in TryRegister threw exception or returned invalid type", ex, (object?)stack.GetFrame(1)?.GetMethod()?.DeclaringType ?? "unknown"));
                success = false;
            }
        }
        return success;
    }

    public bool TryRegister(params IAutomationBase[] automations)
    {
        bool success = true;
        foreach (var auto in automations)
        {
            try
            {
                AddAny(auto, 0);
            }
            catch (System.Exception ex)
            {
                success = false;
                _errors.Add(new("Could not register automation", ex, auto));
            }
        }
        return success;
    }

    private void AddAny(IAutomationBase auto, int frameCount)
    {
        if (auto is IAutomation simple)
        {
            AddSimple(simple, ++frameCount);
            return;
        }
        if (auto is IDelayableAutomation delayable)
        {
            AddDelayable(delayable, ++frameCount);
            return;
        }

        try
        {
            var wrapped = _wrapperFactory.GetWrapped(auto);
            frameCount++;
            foreach (var item in wrapped)
            {
                AddAny(item, frameCount);
            }
        }
        catch (System.Exception)
        {
            throw;
        }

    }

    private void AddSimple(IAutomation automation, int frameCount)
    {
        var aWrapped = new AutomationWrapper(automation, _trace, GetSourceTypeName(frameCount));
        RegisteredAutomations.Add(aWrapped);    
    }

    private void AddDelayable<T>(T automation, int frameCount) where T: IDelayableAutomation
    {
        var wrapped = _wrapperFactory.GetWrapped(automation);
        frameCount++;
        foreach (var item in wrapped)
        {
            AddAny(item, frameCount);
        }
    }

    private string GetSourceTypeName(int frameCount)
    {
        StackTrace trace = new StackTrace();
        var name = trace.GetFrame(frameCount + 3)?.GetMethod()?.DeclaringType?.Name;
        if (name == nameof(AutomationRegistrar))
        {
            name = "discovery";
        }
        return name ?? "unknown";
    }
}
