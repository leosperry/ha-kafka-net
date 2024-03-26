using FastEndpoints;

namespace HaKafkaNet;

public class GetErrorLogEndpoint : EndpointWithoutRequest<ApiResponse<IEnumerable<LogInfo>>>
{
    readonly IAutomationTraceProvider _trace;

    public GetErrorLogEndpoint(IAutomationTraceProvider trace)
    {
        _trace = trace;
    }

    public override void Configure()
    {
        Get("api/errorlog");
        AllowAnonymous();
    }

    public override async Task<ApiResponse<IEnumerable<LogInfo>>> ExecuteAsync(CancellationToken ct)
    {
        var logs = await _trace.GetErrorLogs();
        return new ApiResponse<IEnumerable<LogInfo>>()
        {
            Data = logs
        };
    }
}
