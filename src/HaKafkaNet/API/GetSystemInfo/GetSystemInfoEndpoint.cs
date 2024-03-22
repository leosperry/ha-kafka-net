using FastEndpoints;

namespace HaKafkaNet;

internal class GetSystemInfoEndpoint : EndpointWithoutRequest<ApiResponse<SystemInfoResponse>>
{
    private readonly ISystemObserver _observer;
    private static readonly string _version;

    static GetSystemInfoEndpoint()
    {
        var ver = System.Reflection.Assembly.GetAssembly(typeof(IAutomation))?.GetName().Version;
        
        _version = ver?.ToString(3)!;
    }

    public GetSystemInfoEndpoint(ISystemObserver observer)
    {
        this._observer = observer;
    }

    public override void Configure()
    {
        Get("api/systeminfo");
        AllowAnonymous();
    }

    public override Task<ApiResponse<SystemInfoResponse>> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(new ApiResponse<SystemInfoResponse>()
        {
            Data = new SystemInfoResponse()
            {
                StateHandlerInitialized = _observer.IsInitialized,
                Version = _version
            }
        });
    }
}
