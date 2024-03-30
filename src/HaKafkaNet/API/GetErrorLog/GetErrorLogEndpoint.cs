using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HaKafkaNet;

internal record LogsRequest(string LogType);

internal class GetErrorLogEndpoint : Endpoint< LogsRequest, Results<Ok<ApiResponse<IEnumerable<LogInfo>>>,NotFound>> 
{
    readonly IAutomationTraceProvider _trace;

    public GetErrorLogEndpoint(IAutomationTraceProvider trace)
    {
        _trace = trace;
    }

    public override void Configure()
    {
        Get("api/log/{LogType}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<ApiResponse<IEnumerable<LogInfo>>>, NotFound>> ExecuteAsync(LogsRequest req, CancellationToken ct)
    {
        switch (req.LogType)
        {
            case "error":
                return TypedResults.Ok(new ApiResponse<IEnumerable<LogInfo>>()
                {
                    Data = await _trace.GetErrorLogs()
                });
            case "tracker":
            {
                return TypedResults.Ok(new ApiResponse<IEnumerable<LogInfo>>()
                {
                    Data = await _trace.GetTrackerLogs()
                });
            }
            case "global":
                return TypedResults.Ok(new ApiResponse<IEnumerable<LogInfo>>()
                {
                    Data = await _trace.GetGlobalLogs()
                });
            default:
                return TypedResults.NotFound();
        }
    }
}
