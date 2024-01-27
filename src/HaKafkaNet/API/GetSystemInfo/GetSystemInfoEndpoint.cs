using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class GetSystemInfoEndpoint : EndpointWithoutRequest<ApiResponse<SystemInfoResponse>>
{
    private readonly StateHandlerObserver _observer;
    private readonly IAutomationManager _manager;
    private readonly ILogger<GetSystemInfoEndpoint> _logger;

    public GetSystemInfoEndpoint(StateHandlerObserver observer ,IAutomationManager manager, ILogger<GetSystemInfoEndpoint> logger )
    {
        this._observer = observer;
        _manager = manager;
        this._logger = logger;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("api/systeminfo");
        AllowAnonymous();
    }

    public override Task<ApiResponse<SystemInfoResponse>> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(new ApiResponse<SystemInfoResponse>()
        {
            Data = new SystemInfoResponse()
            {
                StateHandlerInitialized = _observer.IsInitialized,
                Automations = _manager.GetAll().Select( a => {
                    var meta = a.GetMetaData();
                    return new AutomationInfo()
                    {
                        Name = meta.Name,
                        Description = meta.Description ?? string.Empty,
                        TypeName = meta.UnderlyingType ?? string.Empty,
                        TriggerIds = a.TriggerEntityIds(),
                        Enabled = meta.Enabled
                    };
                })
            }
        });
    }
}
