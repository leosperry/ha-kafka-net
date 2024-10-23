namespace HaKafkaNet;

internal class AutomationBuilder : IAutomationBuilder
{   
    readonly IHaServices _services;

    public AutomationBuilder(IHaServices haServices)
    {
        _services = haServices;
    }

    public SimpleAutomationBuildingInfo CreateSimple(bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup
        };
    }

    public TypedAutomationBuildingInfo<Tstate, Tatt> CreateSimple<Tstate, Tatt>(bool enabledAtStartup = true)
    {
        return new TypedAutomationBuildingInfo<Tstate, Tatt>()
        {
            EnabledAtStartup = enabledAtStartup
        };
    }
    
    public ConditionalAutomationBuildingInfo CreateConditional(bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup
        };
    }

    public SchedulableAutomationBuildingInfo CreateSchedulable(bool reschdulable = false, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschdulable
        };
    }

    public TypedConditionalBuildingInfo<Tstate, Tatt> CreateConditional<Tstate, Tatt>(bool enabledAtStartup = true)
    {
        return new TypedConditionalBuildingInfo<Tstate, Tatt>()
        {
            EnabledAtStartup = enabledAtStartup
        };
    }

    public  SunAutommationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            SunEvent = sunEvent
        };
    }

    public TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> CreateSchedulable<Tstate, Tatt>(bool reschdulable = false, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschdulable
        };
    }
}
