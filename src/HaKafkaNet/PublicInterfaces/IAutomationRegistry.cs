using FastEndpoints;

namespace HaKafkaNet;

/// <summary>
/// This is the registry users create implementations of
/// </summary>
public interface IAutomationRegistry
{
    /// <summary>
    /// Called by the framework to register automations
    /// </summary>
    /// <param name="reg"></param>
    void Register(IRegistrar reg);
}

/// <summary>
/// provides methods for registering automations
/// </summary>
public interface IRegistrar
{
    /// <summary>
    /// original method used to register automations
    /// </summary>
    /// <param name="automations"></param>
    void Register(params IAutomation[] automations);

    /// <summary>
    /// registers strongly typed automations
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="automations"></param>
    void Register<Tstate, Tatt>(params IAutomation<Tstate,Tatt>[] automations);

    /// <summary>
    /// registers delayable automations 
    /// </summary>
    /// <param name="automations"></param>
    void RegisterDelayed(params IDelayableAutomation[] automations);

    /// <summary>
    /// registers different types of automations
    /// </summary>
    /// <param name="automations"></param>
    /// <returns></returns>
    bool TryRegister(params IAutomationBase[] automations);

    /// <summary>
    /// USE THIS ONE
    /// Registers different types of automations and wraps their construction 
    /// in a try/catch so that any failures do not prevent the rest of your 
    /// automations from being constructed.
    /// </summary>
    /// <param name="activators"></param>
    /// <returns></returns>
    bool TryRegister(params Func<IAutomationBase>[] activators);
}

internal interface IInternalRegistrar : IRegistrar
{
    IEnumerable<IAutomationWrapper> Registered { get; }
}
