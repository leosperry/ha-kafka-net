using FastEndpoints;

namespace HaKafkaNet;

internal class GetSystemInfoEndpoint : EndpointWithoutRequest<ApiResponse<SystemInfoResponse>>
{
    private readonly ISystemObserver _observer;
    private readonly IAutomationManager _manager;
    private static readonly string _version;

    static GetSystemInfoEndpoint()
    {
        var ver = System.Reflection.Assembly.GetAssembly(typeof(IAutomation))?.GetName().Version;
        
        _version = ver?.ToString(3) ?? "unknown";
    }

    public GetSystemInfoEndpoint(ISystemObserver observer ,IAutomationManager manager)
    {
        this._observer = observer;
        _manager = manager;
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
                Version = _version,
                Automations = _manager.GetAll().Select( a => {
                    var meta = a.GetMetaData();
                    return new AutomationInfo()
                    {
                        Id = meta.Id,
                        Name = meta.Name,
                        Description = meta.Description ?? string.Empty,
                        TypeName = meta.UnderlyingType ?? string.Empty,
                        Source = meta.Source ?? string.Empty,
                        TriggerIds = a.TriggerEntityIds(),
                        AdditionalEntitiesToTrack = meta.AdditionalEntitiesToTrack ?? Enumerable.Empty<string>(),
                        Enabled = meta.Enabled,
                        LastTriggered = meta.LastTriggered?.ToString() ?? "None",
                        LastExecuted = meta.IsDelayable ? meta.LastExecuted?.ToString() ?? "None" : "N/A"
                    };
                }).ToDictionary(item => item.Id)
            }
        });
    }
}
