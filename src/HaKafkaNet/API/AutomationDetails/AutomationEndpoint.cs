using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HaKafkaNet;

internal record AutomationDetailRequest(string Key);

internal class AutomationEndpoint : Endpoint< AutomationDetailRequest, Results<Ok<ApiResponse<AutomationDetailResponse>>,NotFound>> 
//Endpoint<AutomationDetailRequest, ApiResponse<AutomationDetailResponse>>
{
    readonly IAutomationManager _autoMgr;

    public AutomationEndpoint(IAutomationManager automationManager)
    {
        _autoMgr = automationManager;
    }

    public override void Configure()
    {
        Get("api/automation/{Key}");
        AllowAnonymous();
    }

    public override Task<Results<Ok<ApiResponse<AutomationDetailResponse>>, NotFound>> ExecuteAsync(AutomationDetailRequest req, CancellationToken ct)
    {
        Results<Ok<ApiResponse<AutomationDetailResponse>>, NotFound> response;
        var auto = _autoMgr.GetByKey(req.Key);
        if (auto is null)
        {
            response = TypedResults.NotFound();
            return Task.FromResult(response);
        }
        var meta = auto.GetMetaData();

        var autoResponse = new AutomationDetailResponse(
            meta.Name,
            meta.KeyRequest ??  "none",
            meta.GivenKey,
            auto.EventTimings.ToString(),
            auto.TriggerEntityIds(),
            meta.AdditionalEntitiesToTrack ?? Enumerable.Empty<string>(),
            meta.UnderlyingType!,
            meta.Source ?? "source error",
            meta.IsDelayable,
            meta.LastTriggered?.ToString() ?? "never",
            meta.LastExecuted.ToString() ?? (meta.IsDelayable ? "never" : "N/A"),
            meta.LatestStateChange,
            Enumerable.Empty<AutomationTraceResponse>()
        );

        response = TypedResults.Ok(new ApiResponse<AutomationDetailResponse>()
        {
            Data = autoResponse
        });
        return Task.FromResult(response);
    }



}


