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
    void Register<Tstate, Tatt>(params IAutomation<Tstate,Tatt>[] automations);
    void RegisterDelayed(params IDelayableAutomation[] automations);

    bool TryRegister(params IAutomationBase[] automations);

    /// <summary>
    /// Recommended
    /// </summary>
    /// <param name="activators"></param>
    /// <returns></returns>
    bool TryRegister(params Func<IAutomationBase>[] activators);
}

internal interface IInternalRegistrar : IRegistrar
{
    IEnumerable<IAutomationWrapper> Registered { get; }
}

public delegate TimeSpan DelayEvaluator<in T>(T automation) where T : IDelayableAutomation;