
namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class ConditionalAutomation : IConditionalAutomation
{
    private readonly IEnumerable<string> _triggerEntities;
    private readonly Func<HaEntityStateChange, CancellationToken, Task<bool>> _continuesToBeTrue;
    private readonly TimeSpan _for;
    private readonly Func<CancellationToken, Task> _execute;

    public ConditionalAutomation(
        IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue, TimeSpan @for, Func<CancellationToken, Task> execute
    )
    {
        this._triggerEntities = triggerEntities;
        this._continuesToBeTrue = continuesToBeTrue;
        this._for = @for;
        this._execute = execute;
    }
    public TimeSpan For => _for;

    public Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        return _continuesToBeTrue(haEntityStateChange, cancellationToken);
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        return _execute(cancellationToken);
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _triggerEntities;
    }
}
