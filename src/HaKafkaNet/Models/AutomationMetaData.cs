namespace HaKafkaNet;

public record AutomationMetaData
{
    public bool Enabled { get; set; } = true;
    public required string Name { get; init; }
    public string? Description { get; init;}
    public string? KeyRequest { get; set; }
    public string GivenKey { get; internal set; } = string.Empty;

    public bool TriggerOnBadState { get; set; } = false;
    
    public IEnumerable<string>? AdditionalEntitiesToTrack { get; set; }
    public string? UnderlyingType { get; internal set; }
    public bool IsDelayable { get; internal set; }
    public string? Source { get; internal set; }
    public DateTime? LastTriggered { get; internal set; }
    public DateTime? LastExecuted { get; internal set; }
    public DateTime? NextScheduled { get; internal set; }
}
