using System;

namespace HaKafkaNet.Implementations.Core;

interface IAutomationActivator
{
    Task Activate(IAutomationWrapper automation, CancellationToken cancellationToken);
    event Action<HaEntityState>? Activated;
}

internal class AutomationActivator : IAutomationActivator
{
    private readonly IHaEntityProvider _entityProvider;

    public AutomationActivator(IHaEntityProvider entityProvider)
    {
        this._entityProvider = entityProvider;
    }

    public event Action<HaEntityState>? Activated;

    public async Task Activate(IAutomationWrapper automation, CancellationToken cancellationToken)
    {
        // step 1 : get all the triggers
        // step 2 : sort and get the most recent
        // step 3 : call the state handler to re-handle

        HaEntityState? mostRecent = null;
        await foreach (var item in GetEntities(automation.TriggerEntityIds()))
        {
            mostRecent = (mostRecent is null || item.LastUpdated > mostRecent.LastUpdated) ? item : mostRecent;
        }

        if (mostRecent is null)
        {
            throw new HaKafkaNetException("could not find state to activate automation with");
        }

        //await _stateHandler.Handle(mostRecent, cancellationToken);
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
        }
    }

    private void OnActivated(HaEntityState state)
    {
        Activated?.Invoke(state);
    }
}
