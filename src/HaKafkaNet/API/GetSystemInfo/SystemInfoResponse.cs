namespace HaKafkaNet;

public record SystemInfoResponse
{
    public bool StateHandlerInitialized { get; init; }
    public required Dictionary<Guid, AutomationInfo> Automations{ get; init; }
}

public record AutomationInfo
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public required string Description { get; set; }
    public required string TypeName { get; init; }
    public required string Source { get; set; }
    public required IEnumerable<string> TriggerIds { get; init; }
    public bool Enabled { get; set; }
    
}
