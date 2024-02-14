namespace HaKafkaNet;

public record AutomationMetaData
{
    public bool Enabled { get; set; } = true;
    public required string Name { get; init; }
    public string? Description { get; init;}
    public Guid Id { get; init; } = Guid.NewGuid();
    public IEnumerable<string>? AdditionalEntitiesToTrack { get; set; }
    internal string? UnderlyingType { get; set; }
    internal bool IsDelayable { get; set; }
    internal string? Source { get; set; }
    internal DateTime? LastTriggered { get; set; }
    internal DateTime? LastExecuted { get; set; }
}
