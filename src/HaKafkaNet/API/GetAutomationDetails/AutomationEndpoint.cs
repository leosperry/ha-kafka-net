using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HaKafkaNet;

internal record AutomationDetailRequest(string Key);

internal class AutomationEndpoint : Endpoint< AutomationDetailRequest, Results<Ok<ApiResponse<AutomationDetailResponse>>,NotFound>> 
{
    readonly IAutomationManager _autoMgr;
    readonly IAutomationTraceProvider _trace;

    public AutomationEndpoint(IAutomationManager automationManager, IAutomationTraceProvider traceProvider)
    {
        _autoMgr = automationManager;
        _trace = traceProvider;
    }

    public override void Configure()
    {
        Get("api/automation/{Key}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<ApiResponse<AutomationDetailResponse>>, NotFound>> ExecuteAsync(AutomationDetailRequest req, CancellationToken ct)
    {
        Results<Ok<ApiResponse<AutomationDetailResponse>>, NotFound> response;
        var auto = _autoMgr.GetByKey(req.Key);
        if (auto is null)
        {
            response = TypedResults.NotFound();
            return response;
        }
        var meta = auto.GetMetaData();
        var traces = (await _trace.GetTraces(req.Key)).Select(t => new AutomationTraceResponse(t.TraceEvent, t.Logs));

        var autoResponse = new AutomationDetailResponse(
            meta.Name,
            meta.Description,
            meta.KeyRequest ?? "none",
            meta.GivenKey,
            auto.EventTimings.ToString(),
            meta.Mode.ToString(),
            auto.TriggerEntityIds(),
            meta.AdditionalEntitiesToTrack ?? Enumerable.Empty<string>(),
            meta.UnderlyingType!,
            meta.Source ?? "source error",
            meta.IsDelayable,
            meta.LastTriggered?.ToLocalTime().ToString() ?? "never",
            meta.LastExecuted?.ToLocalTime().ToString(),
            traces
        );

        response = TypedResults.Ok(new ApiResponse<AutomationDetailResponse>()
        {
            Data = autoResponse
        });
        return response;
    }
}
