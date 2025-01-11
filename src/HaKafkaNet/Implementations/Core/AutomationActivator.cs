using System;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Implementations.Core;

interface IAutomationActivator
{
    Task Activate(IAutomationWrapper automation, CancellationToken cancellationToken);
    event Action<HaEntityState>? Activated;
}

internal class AutomationActivator : IAutomationActivator
{
    private readonly IHaEntityProvider _entityProvider;
    private readonly ILogger<AutomationActivator> _logger;

    public AutomationActivator(IHaEntityProvider entityProvider, ILogger<AutomationActivator> logger)
    {
        this._entityProvider = entityProvider;
        this._logger = logger;
    }

    public event Action<HaEntityState>? Activated;

    public async Task Activate(IAutomationWrapper automation, CancellationToken cancellationToken)
    {
        // Get the most recent state of the trigger entities
        HaEntityState? mostRecent = null;
        await foreach (var item in GetEntities(automation.TriggerEntityIds()))
        {
            mostRecent = (mostRecent is null || item.LastUpdated > mostRecent.LastUpdated) ? item : mostRecent;
        }

        if (mostRecent is null)
        {
            _logger.LogCritical("Could not find state to activate automation with");
            return;
        }

        OnActivated(mostRecent);
    }

    private async IAsyncEnumerable<HaEntityState> GetEntities(IEnumerable<string> entityIds)
    {
        foreach (var id in entityIds)
        {
            var entity = await _entityProvider.GetEntity<HaEntityState>(id);
            if (entity is not null)
            {
                yield return entity;  
            }
            else
            {
                _logger.LogWarning("Entity with id {id} not found", id);
            }
        }
    }

    private void OnActivated(HaEntityState state)
    {
        using (_logger.BeginScope("{state}", state))
            _logger.LogDebug("Activating {activated_entity}", state.EntityId);
        Activated?.Invoke(state);
    }
}
