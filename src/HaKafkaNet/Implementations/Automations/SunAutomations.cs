namespace HaKafkaNet;

public abstract class SunAutomationBase : SchedulableAutomationBase
{
    readonly TimeSpan _offset;
    readonly Func<CancellationToken, Task> _execute;

    public SunAutomationBase(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(["sun.sun"])
    {
        base.IsReschedulable = false;
        base.EventTimings = timings;
        _execute = execution;
        _offset = offset ?? TimeSpan.Zero;
    }

    public override Task<DateTime?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        DateTime? next = base.GetNextScheduled();
        if (next < DateTime.Now)
        {
            var sunAtts = GetSunAttributes(stateChange);
            next = this.GetNextSunEvent(sunAtts) + _offset;
        }
        return Task.FromResult(next);
    }

    protected abstract DateTime GetNextSunEvent(SunAttributes atts);

    private SunAttributes GetSunAttributes(HaEntityStateChange haEntityStateChange)
    {
        var sunAtts = haEntityStateChange.New.Convert<SunAttributes>().Attributes;
        if (sunAtts is null)
        {
            throw new HaKafkaNetException("Could not calculate sunrise. Sun schema invalid");
        }
        return sunAtts;
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return _execute(cancellationToken);
    }
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunRiseAutomation : SunAutomationBase
{
    public SunRiseAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(execution, offset, timings) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextRising;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunSetAutomation : SunAutomationBase
{
    public SunSetAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(execution, offset, timings) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextSetting;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunDawnAutomation : SunAutomationBase
{
    public SunDawnAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(execution, offset, timings) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextDawn;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunDuskAutomation : SunAutomationBase
{
    public SunDuskAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(execution, offset, timings) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextDusk;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunMidnightAutomation : SunAutomationBase
{
    public SunMidnightAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(execution, offset, timings) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextMidnight;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunNoonAutomation : SunAutomationBase
{
    public SunNoonAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.PostStartup): base(execution, offset, timings) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextNoon;
}

