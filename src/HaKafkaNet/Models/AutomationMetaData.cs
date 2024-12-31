namespace HaKafkaNet;

/// <summary>
/// used by the framework to track data about automations
/// </summary>
public record AutomationMetaData
{
    /// <summary>
    /// When disabled, the automation will not run from state changes
    /// also, any currently running delayable automations will be canceled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// user specified name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// user specified description
    /// </summary>
    public string? Description { get; init;}

    /// <summary>
    /// user specified key. Actual key used may be different if this is not unique
    /// </summary>
    public string? KeyRequest { get; set; }

    /// <summary>
    /// see: https://github.com/leosperry/ha-kafka-net/wiki/Parallelism-and-threads-in-automations
    /// </summary>
    public AutomationMode Mode { get; set; }

    /// <summary>
    /// key given by the framework. Guarenteed to be unique
    /// </summary>
    public string GivenKey { get; internal set; } = string.Empty;

    /// <summary>
    /// Defaults to false
    /// when true, entity states of "unknown" or "unavailable" will trigger automation
    /// </summary>
    public bool TriggerOnBadState { get; set; } = false;

    /// <summary>
    /// see: https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor
    /// </summary>
    public IEnumerable<string>? AdditionalEntitiesToTrack { get; set; }

    /// <summary>
    /// the non-wrapped type of automation
    /// </summary>
    public string? UnderlyingType { get; internal set; }

    /// <summary>
    /// true for conditional and schedulable automations
    /// </summary>
    public bool IsDelayable { get; internal set; }

    /// <summary>
    /// the registry or where the automation is defined or "Discovery" if discovered via reflection at startup
    /// </summary>
    public string? Source { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? LastTriggered { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? LastExecuted { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? NextScheduled { get; internal set; }

    internal bool UserMetaError { get; set; } = false;
    internal bool UserTriggerError {get; set; } = false;

    internal static AutomationMetaData Create(object automation)
    {
        return new AutomationMetaData()
        {
            Name = automation.GetType().Name,
            Description = automation.GetType().Name,
            Enabled = true,
        };
    }
}

/// <summary>
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Parallelism-and-threads-in-automations
/// </summary>
public enum AutomationMode
{
    /// <summary>
    /// If running, the latest state change will be tracked.
    /// after execution, if a tracked state exists, it will trigger one more time
    /// with the latest state. This is the default behavior
    /// </summary>
    Smart = 0,
    /// <summary>
    /// If running, new state changes will be ignored
    /// Most performant with data loss
    /// </summary>
    Single,
    /// <summary>
    /// If running, will be canceled and retriggered
    /// prevents memory pressure, guarantees latest state is used
    /// user must account for cancel request at any time
    /// </summary>
    Restart,
    /// <summary>
    /// state changes will be queued and run sequentially
    /// limits CPU pressure, potential memory pressure issues
    /// </summary>
    Queued,
    /// <summary>
    /// all state changes will trigger on separate threads
    /// most immediate
    /// potential thread exhaustion and/or memory pressure
    /// </summary>
    Parallel
}

