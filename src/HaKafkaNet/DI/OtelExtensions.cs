namespace HaKafkaNet;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

static class Telemetry
{
    public const string 
        TraceApiName = "ha_kafka_net.ha_api",
        TraceCacheName = "ha_kafka_net.cache",
        TraceAutomationName = "ha_kafka_net.automation",
        TraceTrackerName = "ha_kafka_net.entity_tracker",
        MeterStateHandler = "ha_kafka_net.state_handler",
        MeterTracesName = "ha_hakfa_net.trace",
        MeterCacheName = "ha_kafka_net.cache_meter" 
        ;
}

/// <summary>
/// Extension methods for adding OTEL instrumentation
/// </summary>
public static class OtelExtensions
{
    /// <summary>
    /// Adds tracing for automations
    /// </summary>
    /// <param name="trace"></param>
    /// <returns></returns>
    public static TracerProviderBuilder AddHaKafkaNetInstrumentation(this TracerProviderBuilder trace)
    {
        trace
            .AddSource(Telemetry.TraceApiName)
            .AddSource(Telemetry.TraceCacheName)
            .AddSource(Telemetry.TraceAutomationName)
            .AddSource(Telemetry.TraceTrackerName);
        return trace;
    }

    /// <summary>
    /// Adds metrics for things like entity states and automation triggers
    /// </summary>
    /// <param name="meter"></param>
    /// <returns></returns>
    public static MeterProviderBuilder AddHaKafkaNetInstrumentation(this MeterProviderBuilder meter)
    {
        meter
            .AddMeter(Telemetry.MeterStateHandler)
            .AddMeter(Telemetry.MeterTracesName)
            .AddMeter(Telemetry.MeterCacheName);
        return meter;
    }
}
