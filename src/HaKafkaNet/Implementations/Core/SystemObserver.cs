namespace HaKafkaNet;

internal interface ISystemObserver
{
    bool IsInitialized{get;}
    event Action? StateHandlerInitialized;
    void OnStateHandlerInitialized();
    void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception);
    void OnBadStateDiscovered(IEnumerable<BadEntityState> badStates);
}

/// <summary>
/// Kafka handlers should be initialized by kafka flow and not other classes via DI.
/// Therefore, we need an observer to report on thier state.
/// </summary>
internal class SystemObserver : ISystemObserver
{
    readonly ISystemMonitor[] _monitors;
    public bool IsInitialized { get; private set; }

    public event Action? StateHandlerInitialized;
    internal event Action<AutomationMetaData, Exception>? UnhandledException;
    internal event Action<IEnumerable<BadEntityState>>? BadEntityState;

    public SystemObserver(IEnumerable<ISystemMonitor> monitors)
    {
        _monitors = monitors.ToArray();

        foreach (var monitor in monitors)
        {
            StateHandlerInitialized += () =>    _ = monitor.StateHandlerInitialized();
            UnhandledException += (meta, ex) => _ = monitor.UnhandledException(meta, ex);
            BadEntityState += (states) =>       _ = monitor.BadEntityStateDiscovered(states);
        }
    }

    public void OnStateHandlerInitialized()
    {
        IsInitialized = true;
        StateHandlerInitialized?.Invoke();
    }

    public void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception)
        => UnhandledException?.Invoke(automationMetaData, exception);

    public void OnBadStateDiscovered(IEnumerable<BadEntityState> badStates)
        => BadEntityState?.Invoke(badStates);
}
