namespace HaKafkaNet;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static class OtelExtensions
{
    public static TracerProviderBuilder AddHaKafkaNetInstrumentation(this TracerProviderBuilder trace)
    {
        trace
            .AddSource("ha_kafka_net.ha_api")
            .AddSource("ha_kafka_net.cache")
            .AddSource("ha_kafka_net.automation")
            .AddSource("ha_kafka_net.entity_tracker");

        return trace;
    }

    public static MeterProviderBuilder AddHaKafkaNetInstrumentation(this MeterProviderBuilder meter)
    {
        meter.AddMeter("ha_kafka_net.state_handler");
        meter.AddMeter("ha_hakfa_net.trace");
        return meter;
    }
}
