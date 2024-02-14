using System.Data;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;

namespace HaKafkaNet;

public interface IAutomationBuilder
{
    SimpleAutomationBuildingInfo CreateSimple(bool enabledAtStartup = true);

    [Obsolete("Future versions of the builder will not support injecting services. Instead, inject them to where you are creating builder objects", false)]
    SimpleAutomationWithServicesBuildingInfo CreateSimpleWithServices(bool enabledAtStartup = true);
    ConditionalAutomationBuildingInfo CreateConditional(bool enabledAtStartup = true);

    [Obsolete("Future versions of the builder will not support injecting services. Instead, inject them to where you are creating builder objects", false)]
    ConditionalAutomationWithServicesBuildingInfo CreateConditionalWithServices(bool enabledAtStartup = true);
    SchedulableAutomationBuildingInfo CreateSchedulable(bool reschdulable = false, bool enabledAtStartup = true);

    SunAutommationBuildingInfo CreateSunAutomation(SunEventType sunEvent, bool enabledAtStartup = true);
}

public enum SunEventType
{
    Dawn,
    Rise,
    Noon,
    Set,
    Dusk,
    Midnight
}



