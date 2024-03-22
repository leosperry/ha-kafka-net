using ProtoBuf.Serializers;

namespace HaKafkaNet;

public record TraceEvent
{
    public required DateTime EventTime { get; init; }
    public required string EventType { get; init; }
    public required string AutomationKey { get; init; }
    public HaEntityStateChange? StateChange { get; init; }
    public ExecptionInfo? Exception {get; set; }
}

public record ExecptionInfo
{
    public required string Type { get; init; }
    public required string Message { get; init; }
    public string? StackTrace { get; init; }
    public ExecptionInfo? InnerException { get; init; }
    public IEnumerable<ExecptionInfo>? InnerExceptions { get; init; }

    public static ExecptionInfo Create(Exception ex)
    {
        return new ExecptionInfo()
        {
            Type = ex.GetType().FullName ?? ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException is null ?  null : Create(ex.InnerException),
            InnerExceptions = ex is AggregateException agg ? agg.InnerExceptions.Select(e => Create(e)) : null
        };
    }
}

