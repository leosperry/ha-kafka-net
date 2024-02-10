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

    public SimpleAutomationWithServicesBuildingInfo CreateSimpleWithServices(bool enabledAtStartup = true)
    {
        return new(_services)
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

    public ConditionalAutomationWithServicesBuildingInfo CreateConditionalWithServices(bool enabledAtStartup = true)
    {
        return new(_services)
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

    public SchedulableAutomationWithServicesBuildingInfo CreateSchedulableWithServices(bool reschdulable = false, bool enabledAtStartup = true)
    {
        return new(_services)
        {
            EnabledAtStartup = enabledAtStartup,
            IsReschedulable = reschdulable
        };
    }


}
