namespace HaKafkaNet;

public record AutomationDetailResponse(
    string Name,
    string KeyRequest,
    string GivenKey,
    string EventTimings,
    IEnumerable<string> TriggerIds,
    IEnumerable<string> AdditionalEntities,
    string Type,
    string Source,
    bool IsDelayable,
    string LastTriggered,
    string LastExecuted,
    HaEntityStateChange? LatestStateChange,
    IEnumerable<AutomationTraceResponse> Trace
);

public record AutomationTraceResponse(
    string TraceType,
    DateTime Time,
    string TraceData
);