using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class GetSystemInfoEndpoint : EndpointWithoutRequest<ApiResponse<SystemInfoResponse>>
{
    private readonly StateHandlerObserver _observer;
    private readonly IAutomationCollector _manager;
    private readonly ILogger<GetSystemInfoEndpoint> _logger;

    public GetSystemInfoEndpoint(StateHandlerObserver observer ,IAutomationCollector manager, ILogger<GetSystemInfoEndpoint> logger )
    {
        this._observer = observer;
        _manager = manager;
        this._logger = logger;
    }

    public override void Configure()
    {
        //base.Configure();
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
                Automations = _manager.GetAll().Select( a => new AutomationInfo()
                {
                    Name = a.Name,
                    TypeName = (a is ConditionalAutomationWrapper ca)? ca.WrappedConditional.GetType().Name : a.GetType().Name,
                    TriggerIds = a.TriggerEntityIds()
                })
            }
        });
    }
}
