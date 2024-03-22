﻿namespace HaKafkaNet;

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
    public required string LastTriggered { get; set; }
    public required string? LastExecuted { get; set; }
}
