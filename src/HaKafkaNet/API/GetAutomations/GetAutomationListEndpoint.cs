using FastEndpoints;

namespace HaKafkaNet;

internal class GetAutomationListEndpoint : EndpointWithoutRequest<ApiResponse<AutomationListResponse>>
{
    readonly IAutomationManager _manager;
    public GetAutomationListEndpoint(IAutomationManager manager)
    {
        _manager = manager;
    }

    public override void Configure()
    {
        Get("api/automations");
        AllowAnonymous();
    }

    public override Task<ApiResponse<AutomationListResponse>> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(new ApiResponse<AutomationListResponse>()
        {
            Data = new AutomationListResponse()
            {
                Automations = _manager.GetAll().Select( a => {
                    var meta = a.GetMetaData();
                    return new AutomationInfo()
                    {
                        Key = meta.GivenKey,
                        Name = meta.Name,
                        Description = meta.Description ?? string.Empty,
                        TypeName = meta.UnderlyingType ?? string.Empty,
                        Source = meta.Source ?? string.Empty,
                        IsDelayable = meta.IsDelayable,
                        TriggerIds = a.TriggerEntityIds(),
                        AdditionalEntitiesToTrack = meta.AdditionalEntitiesToTrack ?? Enumerable.Empty<string>(),
                        Enabled = meta.Enabled,
                        LastTriggered = meta.LastTriggered?.ToString() ?? "None",
                        LastExecuted = meta.LastExecuted?.ToString()
                    };
                }).ToArray()
            }
        });
    }
}
