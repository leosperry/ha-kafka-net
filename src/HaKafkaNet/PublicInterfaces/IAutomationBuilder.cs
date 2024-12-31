
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json;

namespace HaKafkaNet;

public interface IAutomationBuilder
{
    SimpleAutomationBuildingInfo CreateSimple(bool enabledAtStartup = true);
    TypedAutomationBuildingInfo<Tstate, Tatt> CreateSimple<Tstate, Tatt>(bool enabledAtStartup = true);
    TypedAutomationBuildingInfo<Tstate, JsonElement> CreateSimple<Tstate>(bool enabledAtStartup = true) 
        => CreateSimple<Tstate, JsonElement>(enabledAtStartup);

    ConditionalAutomationBuildingInfo CreateConditional(bool enabledAtStartup = true);
    TypedConditionalBuildingInfo<Tstate, Tatt> CreateConditional<Tstate, Tatt>(bool enabledAtStartup = true);
    TypedConditionalBuildingInfo<Tstate, JsonElement> CreateConditional<Tstate>(bool enabledAtStartup = true) 
        => CreateConditional<Tstate, JsonElement>(enabledAtStartup);

    SchedulableAutomationBuildingInfo CreateSchedulable(bool reschedulable = false, bool enabledAtStartup = true);
    TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> CreateSchedulable<Tstate, Tatt>(bool reschedulable = false, bool enabledAtStartup = true);
    TypedSchedulableAutomationBuildingInfo<Tstate, JsonElement> CreateSchedulable<Tstate>(bool reschedulable = false, bool enabledAtStartup = true) 
        => CreateSchedulable<Tstate, JsonElement>(reschedulable, enabledAtStartup);

    SunAutomationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true);
}