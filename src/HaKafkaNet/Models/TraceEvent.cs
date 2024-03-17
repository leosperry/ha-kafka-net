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
    public string Type { get; private set; }
    public string Message { get; private set; }
    public string? StackTrace { get; private set; }
    public ExecptionInfo? InnerException { get; private set; }
    public IEnumerable<ExecptionInfo>? InnerExceptions { get; private set; }

    public ExecptionInfo(Exception ex)
    {
        this.Type = ex.GetType().FullName ?? ex.GetType().Name;
        this.Message = ex.Message;
        this.StackTrace = ex.StackTrace;
        this.InnerException = ex.InnerException is null ?  null : new ExecptionInfo(ex.InnerException);
        if (ex is AggregateException agg)
        {
            this.InnerExceptions = agg.InnerExceptions.Select(e => new ExecptionInfo(e));
        }
    }
}

