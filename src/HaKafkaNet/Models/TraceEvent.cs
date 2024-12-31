#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HaKafkaNet;

public record TraceEvent
{
    public required DateTime EventTime { get; init; }
    public required string EventType { get; init; }
    public required string AutomationKey { get; init; }
    public HaEntityStateChange? StateChange { get; init; }
    public ExceptionInfo? Exception {get; set; }
}

public record ExceptionInfo
{
    public required string Type { get; init; }
    public required string Message { get; init; }
    public string? StackTrace { get; init; }
    public ExceptionInfo? InnerException { get; init; }
    public IEnumerable<ExceptionInfo>? InnerExceptions { get; init; }

    public static ExceptionInfo Create(Exception ex)
    {
        return new ExceptionInfo()
        {
            Type = ex.GetType().FullName ?? ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException is null ?  null : Create(ex.InnerException),
            InnerExceptions = ex is AggregateException agg ? agg.InnerExceptions.Select(e => Create(e)) : null
        };
    }
}

