namespace HaKafkaNet;

public record AutomationDetailResponse(
    string Name,
    string? Description,
    string KeyRequest,
    string GivenKey,
    string EventTimings,
    string Mode,
    IEnumerable<string> TriggerIds,
    IEnumerable<string> AdditionalEntities,
    string Type,
    string Source,
    bool IsDelayable,
    string LastTriggered,
    string? LastExecuted,
    IEnumerable<AutomationTraceResponse> Traces
);

public record AutomationTraceResponse(
    TraceEvent Event,
    IEnumerable<LogInfo> Logs
);