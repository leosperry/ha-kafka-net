namespace HaKafkaNet;

public record SystemInfoResponse
{
    public bool StateHandlerInitialized { get; init; }
    public required IEnumerable<AutomationInfo> Automations{ get; init; }
}

public record AutomationInfo
{
    public required string Name { get; init; }
    public required string Description { get; set; }
    public required string TypeName { get; init; }
    public required IEnumerable<string> TriggerIds { get; init; }
    public bool Enabled { get; set; }
}
