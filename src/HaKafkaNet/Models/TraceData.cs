#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HaKafkaNet;

public record TraceData
{
    public required TraceEvent TraceEvent { get; set; }
    public required IEnumerable<LogInfo> Logs {get; set; }
}

public record LogInfo
{
    public required string LogLevel { get; set; }
    public required string Message { get; set; }
    public string? RenderedMessage { get;set; }
    public IDictionary<string,object>? Scopes { get; set; }
    public required IDictionary<string,object> Properties { get; set; }
    public ExceptionInfo? Exception { get; set; }
    public DateTime? TimeStamp { get; set; }
}
