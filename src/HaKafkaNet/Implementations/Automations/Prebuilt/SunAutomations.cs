namespace HaKafkaNet;

public abstract class SunAutomation : SchedulableAutomationBase, ISetAutomationMeta
{
    readonly TimeSpan _offset;
    readonly Func<CancellationToken, Task> _execute;
    private TimeProvider _timeProvider;

    public SunAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(["sun.sun"], executePast)
    {
        this._timeProvider = timeProvider;
        base.IsReschedulable = false;
        base.EventTimings = timings;
        _execute = execution;
        _offset = offset ?? TimeSpan.Zero;
    }

    public override Task<DateTime?> CalculateNext(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        DateTime? next = base.GetNextScheduled();
        if (next is null || next < _timeProvider.GetLocalNow().LocalDateTime)
        {
            var sunAtts = stateChange.New.GetAttributes<SunAttributes>()!;
            next = this.GetNextSunEvent(sunAtts) + _offset;
        }
        return Task.FromResult(next);
    }

    protected abstract DateTime GetNextSunEvent(SunAttributes atts);

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
public sealed class SunRiseAutomation : SunAutomation
{
    public SunRiseAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(timeProvider, execution, offset, timings, executePast) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextRising;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunSetAutomation : SunAutomation
{
    public SunSetAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(timeProvider, execution, offset, timings, executePast) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextSetting;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunDawnAutomation : SunAutomation
{
    public SunDawnAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(timeProvider, execution, offset, timings, executePast) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextDawn;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunDuskAutomation : SunAutomation
{
    public SunDuskAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(timeProvider, execution, offset, timings, executePast) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextDusk;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunMidnightAutomation : SunAutomation
{
    public SunMidnightAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(timeProvider, execution, offset, timings, executePast) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextMidnight;
}

/// <summary>
/// Requires Home Assistant to have sun.sun configured in Kafka Integration
/// May not work in arctic circle
/// </summary>
[ExcludeFromDiscovery]
public sealed class SunNoonAutomation : SunAutomation
{
    public SunNoonAutomation(TimeProvider timeProvider, Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
        : base(timeProvider, execution, offset, timings, executePast) { }

    protected override DateTime GetNextSunEvent(SunAttributes atts) => atts.NextNoon;
}

