namespace HaKafkaNet;

internal class AutomationBuilder : IAutomationBuilder
{   
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

    public SchedulableAutomationBuildingInfo CreateSchedulable(bool reschedulable = false, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschedulable
        };
    }

    public TypedConditionalBuildingInfo<Tstate, Tatt> CreateConditional<Tstate, Tatt>(bool enabledAtStartup = true)
    {
        return new TypedConditionalBuildingInfo<Tstate, Tatt>()
        {
            EnabledAtStartup = enabledAtStartup
        };
    }

    public  SunAutomationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            SunEvent = sunEvent
        };
    }

    public TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> CreateSchedulable<Tstate, Tatt>(bool reschedulable = false, bool enabledAtStartup = true)
    {
        return new()
        {
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschedulable
        };
    }
}
