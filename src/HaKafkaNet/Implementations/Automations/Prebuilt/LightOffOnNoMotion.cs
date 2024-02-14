
namespace HaKafkaNet;

[ExcludeFromDiscovery]
public class LightOffOnNoMotion : ConditionalAutomationBase
{
    private readonly List<string> _motionIds = new();
    private readonly List<string> _lightIds = new();
    private readonly TimeSpan _duration;
    private readonly IHaServices _services;

    public LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration, IHaServices services)
        : base(motionIds, duration)
    {
        this._motionIds.AddRange(motionIds);
        this._lightIds.AddRange(lightIds);
        this._duration = duration;
        this._services = services;
    }


    public override Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        var motionStates = 
            from m in _motionIds
            select _services.EntityProvider.GetEntityState(m, cancellationToken); // all should be off
        
        var lightStates = 
            from l in _lightIds
            select _services.EntityProvider.GetEntityState(l, cancellationToken);  // any should be on

        Task<HaEntityState[]> motionResults;
        Task<HaEntityState[]> lightResults;

        return Task.WhenAll(
            motionResults = Task.WhenAll(motionStates),
            lightResults = Task.WhenAll(lightStates)).ContinueWith(t =>
                motionResults.Result.All(m => m.State == "off") && lightResults.Result.Any(l => l.State == "on")
            ,cancellationToken, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current);        
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            from lightId in _lightIds
            select _services.EntityProvider.GetEntityState(lightId, cancellationToken)
                .ContinueWith(t => 
                    t.Result!.State == "on" 
                        ? _services.Api.TurnOff(lightId, cancellationToken)
                        : Task.CompletedTask
                , cancellationToken, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current)
        );
    }
}
