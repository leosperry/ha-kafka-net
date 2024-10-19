
namespace HaKafkaNet;

public interface IAutomationBuilder
{
    SimpleAutomationBuildingInfo CreateSimple(bool enabledAtStartup = true);

    TypedAutomationBuildingInfo<Tstate, Tatt> CreateSimpleTyped<Tstate, Tatt>(bool enabledAtStartup = true);

    ConditionalAutomationBuildingInfo CreateConditional(bool enabledAtStartup = true);

    SchedulableAutomationBuildingInfo CreateSchedulable(bool reschdulable = false, bool enabledAtStartup = true);

    SunAutommationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true);
}





