namespace HaKafkaNet;

/// <summary>
/// Kafka handlers should be initialized by kafka flow and not other classes via DI.
/// Therefore, we need an observer to report on thier state.
/// </summary>
internal class StateHandlerObserver
{
    //public event Action? Initialized;
    public bool IsInitialized { get; private set; }

    internal void OnInitialized()
    {
        IsInitialized = true;
    }
}
