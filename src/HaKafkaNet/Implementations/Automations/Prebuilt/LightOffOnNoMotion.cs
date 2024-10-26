
using System.Text.Json;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
public class LightOffOnNoMotion : ConditionalAutomationBase, IConditionalAutomation
{
    private readonly List<string> _motionIds = new();
    private readonly List<string> _lightIds = new();
    private readonly IHaServices _services;

    public LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration, IHaServices services)
        : base(motionIds.Union(lightIds), duration)
    {
        this._motionIds.AddRange(motionIds);
        this._lightIds.AddRange(lightIds);
        this._services = services;
    }


    public override Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        var motionStates = 
            from m in _motionIds
            select _services.EntityProvider.GetOnOffEntity(m, cancellationToken); // all should be off
        
        var lightStates = 
            from l in _lightIds
            select _services.EntityProvider.GetOnOffEntity(l, cancellationToken);  // any should be on

        Task<IHaEntity<OnOff, JsonElement>?[]> motionResults;
        Task<IHaEntity<OnOff, JsonElement>?[]> lightResults;

        return Task.WhenAll(
            motionResults = Task.WhenAll(motionStates),
            lightResults = Task.WhenAll(lightStates)).ContinueWith(t =>
                motionResults.Result.All(m => m?.State == OnOff.Off) && lightResults.Result.Any(l => l?.State == OnOff.On)
            ,cancellationToken, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current);        
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            from lightId in _lightIds
            select _services.EntityProvider.GetOnOffEntity(lightId, cancellationToken)
                .ContinueWith(t => 
                    t.Result?.Bad() != true && t.Result?.State == OnOff.On
                        ? _services.Api.TurnOff(lightId, cancellationToken)
                        : Task.CompletedTask
                , cancellationToken, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current)
        );
    }
}
