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

    public TypedAutomationBuildingInfo<Tstate, Tatt> CreateSimpleTyped<Tstate, Tatt>(bool enabledAtStartup = true)
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

    public  SunAutommationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            SunEvent = sunEvent
        };
    }
}
