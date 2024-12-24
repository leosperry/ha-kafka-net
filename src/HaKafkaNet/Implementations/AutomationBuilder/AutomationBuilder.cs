namespace HaKafkaNet;

internal class AutomationBuilder : IAutomationBuilder
{   
    private readonly TimeProvider _timeProvider;

    public AutomationBuilder(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public SimpleAutomationBuildingInfo CreateSimple(bool enabledAtStartup = true)
    {
        return new SimpleAutomationBuildingInfo()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup
        };
    }

    public TypedAutomationBuildingInfo<Tstate, Tatt> CreateSimple<Tstate, Tatt>(bool enabledAtStartup = true)
    {
        return new TypedAutomationBuildingInfo<Tstate, Tatt>()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup
        };
    }
    
    public ConditionalAutomationBuildingInfo CreateConditional(bool enabledAtStartup = true)
    {
        return new ConditionalAutomationBuildingInfo()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup
        };
    }

    public SchedulableAutomationBuildingInfo CreateSchedulable(bool reschedulable = false, bool enabledAtStartup = true)
    {
        return new SchedulableAutomationBuildingInfo()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschedulable
        };
    }

    public TypedConditionalBuildingInfo<Tstate, Tatt> CreateConditional<Tstate, Tatt>(bool enabledAtStartup = true)
    {
        return new TypedConditionalBuildingInfo<Tstate, Tatt>()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup
        };
    }

    public  SunAutomationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true)
    {
        return new SunAutomationBuildingInfo()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup,
            SunEvent = sunEvent,
            Mode = AutomationMode.Parallel
        };
    }

    public TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> CreateSchedulable<Tstate, Tatt>(bool reschedulable = false, bool enabledAtStartup = true)
    {
        return new TypedSchedulableAutomationBuildingInfo<Tstate, Tatt>()
        {
            TimeProvider = _timeProvider,
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschedulable
        };
    }
}
