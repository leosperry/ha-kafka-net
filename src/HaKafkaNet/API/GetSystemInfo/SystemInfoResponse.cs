#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HaKafkaNet;

public record SystemInfoResponse
{
    public bool StateHandlerInitialized { get; init; }
    public required string Version { get; init; }
}

public record AutomationInfo
{
    public required string Key { get; set; }
    public required string Name { get; init; }
    public required string Description { get; set; }
    public required string TypeName { get; init; }
    public required string Source { get; set; }
    public required bool IsDelayable { get; set; }
    public required IEnumerable<string> TriggerIds { get; init; }
    public required IEnumerable<string> AdditionalEntitiesToTrack { get; set; }
    public bool Enabled { get; set; }
    public string? LastTriggered { get; set; }
    public string? LastExecuted { get; set; }
    public string? NextScheduled { get; set; }
}
