using Confluent.Kafka;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

record GetAutomationRequest(string? sort);

internal class GetAutomationListEndpoint : Endpoint<GetAutomationRequest, ApiResponse<AutomationListResponse>>
{
    readonly IAutomationManager _manager;
    private readonly ILogger<GetAutomationListEndpoint> _logger;

    public GetAutomationListEndpoint(IAutomationManager manager, ILogger<GetAutomationListEndpoint> logger)
    {
        _manager = manager;
        this._logger = logger;
    }

    public override void Configure()
    {
        Get("api/automations");
        AllowAnonymous();
    }

    public override Task<ApiResponse<AutomationListResponse>> ExecuteAsync(GetAutomationRequest req, CancellationToken ct)
    {
        IEnumerable<(IAutomationWrapper wrapper, AutomationMetaData meta)> unsorted = _manager.GetAll().Select(a => (a , a.GetMetaData()));

        _logger.LogInformation("sort {sort}", req.sort);

        var sorted = unsorted.OrderBy(a => a.meta.Name).OrderByDescending(a => a.meta.LastTriggered);
        // var sorted = req.sort?.ToLower() switch
        // {
        //     "name" =>       unsorted.OrderBy(a => a.meta.Name),
        //     "source" =>     unsorted.OrderBy(a => a.meta.Source),
        //     "trigger" =>    unsorted.OrderBy(a => a.meta.LastTriggered),
        //     "execute" =>    unsorted.OrderBy(a => a.meta.LastExecuted),
        //     "any" =>        unsorted.OrderBy(a => Math.Max(a.meta.LastExecuted?.Ticks ?? 0, a.meta.LastTriggered?.Ticks ?? 0)),
        //     _ => unsorted
        // };

        IEnumerable<AutomationInfo> automationList = sorted.Select( a => {
            var meta = a.meta;
            return new AutomationInfo()
            {
                Key = meta.GivenKey,
                Name = meta.Name,
                Description = meta.Description ?? string.Empty,
                TypeName = meta.UnderlyingType ?? string.Empty,
                Source = meta.Source ?? string.Empty,
                IsDelayable = meta.IsDelayable,
                TriggerIds = a.wrapper.TriggerEntityIds(),
                AdditionalEntitiesToTrack = meta.AdditionalEntitiesToTrack ?? Enumerable.Empty<string>(),
                Enabled = meta.Enabled,
                LastTriggered = meta.LastTriggered?.ToString() ?? "None",
                LastExecuted = meta.LastExecuted?.ToString(),
                NextScheduled = meta.NextScheduled?.ToString()
            };
        });

        return Task.FromResult(new ApiResponse<AutomationListResponse>()
        {
            Data = new AutomationListResponse()
            {
                Automations = automationList.ToArray()
            }
        });
    }
}
