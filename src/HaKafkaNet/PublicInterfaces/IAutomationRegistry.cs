using FastEndpoints;

namespace HaKafkaNet;

/// <summary>
/// This is the registry users create implementations of
/// </summary>
public interface IAutomationRegistry
{
    void Register(IRegistrar reg);
}

public interface IRegistrar
{
    void Register(params IAutomation[] automations);

    void RegisterDelayed(params IDelayableAutomation[] automations);

    void Register<Tstate, Tatt>(params IAutomation<Tstate,Tatt>[] automations);

    [Obsolete("Please implement ISchedulableAutomation", true)]
    void RegisterWithDelayEvaluator<T>(T automation, DelayEvaluator<T> delayEvaluator)
        where T : IDelayableAutomation;

    [Obsolete("Please implement ISchedulableAutomation", true)]
    void RegisterMultipleWithDelayEvaluator<T>(IEnumerable<T> automations, DelayEvaluator<T> delayEvaluator)
        where T : IDelayableAutomation;

    bool TryRegister(params IAutomationBase[] automations);
    bool TryRegister(params Func<IAutomationBase>[] activators);



    [Obsolete("Please use RegisterDelayed or TryRegister", false)]
    void Register(IDelayableAutomation automation) => RegisterDelayed(automation);

    [Obsolete("Please use Register or TryRegister", false)]
    void RegisterMultiple(IEnumerable<IAutomation> automations) => Register(automations.ToArray());

    [Obsolete("Please use RegisterDelayed or TryRegister", false)]
    void RegisterMultiple(IEnumerable<IDelayableAutomation> automations) => RegisterDelayed(automations.ToArray());

    [Obsolete("Please use Register or TryRegister", false)]
    void RegisterMultiple(params IAutomation[] automations) => Register(automations);

    [Obsolete("Please use RegisterDelayed or TryRegister", false)]
    void Register(params IDelayableAutomation[] automation) => RegisterDelayed(automation);

    [Obsolete("Please use RegisterDelayed or TryRegister", false)]
    void RegisterMultiple(params IDelayableAutomation[] automations) => RegisterDelayed(automations);
}

internal interface IInternalRegistrar : IRegistrar
{
    IEnumerable<IAutomationWrapper> Registered { get; }
}

public delegate TimeSpan DelayEvaluator<in T>(T automation) where T : IDelayableAutomation;