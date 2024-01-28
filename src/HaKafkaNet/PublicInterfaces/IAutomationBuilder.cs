using System.Data;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;

namespace HaKafkaNet;

public interface IAutomationBuilder
{
    SimpleAutomationBuildingInfo CreateSimple(bool enabledAtStartup = true);
    SimpleAutomationWithServicesBuildingInfo CreateSimpleWithServices(bool enabledAtStartup = true);
    ConditionalAutomationBuildingInfo CreateConditional(bool enabledAtStartup = true);
    ConditionalAutomationWithServicesBuildingInfo CreateConditionalWithServices(bool enabledAtStartup = true);
}



