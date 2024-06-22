using HaKafkaNet;
using NLog.Web;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog(); // enables log tracing

var services = builder.Services;

// for local development of dashboard only
// services.AddCors(options => {
//     options.AddPolicy("hknDev", policy =>{
//         policy.WithOrigins("*");
//         policy.AllowAnyHeader();
//     });
// });

HaKafkaNetConfig config = new HaKafkaNetConfig();
builder.Configuration.GetSection("HaKafkaNet").Bind(config);
services.AddHaKafkaNet(config);

// programatic example of configuration
// services.AddHaKafkaNet(options =>{
//     //minimum amount of config
//     options.KafkaBrokerAddresses = ["your kafka instance"];
//     options.HaConnectionInfo.AccessToken = "your access token";
//     options.HaConnectionInfo.BaseUri = "your Home Assistant location";
//     //set additional options
// });

// Version 6 overload
// services.AddHaKafkaNet(config, (cluster) => {});

// // Version 7 overload
// services.AddHaKafkaNet(config, (kafka, cluster) => {
//    // add logging, telemetry, and/or topics options as desired
      // https://farfetch.github.io/kafkaflow/docs/guides/open-telemetry/
//});

// provide an IDistributedCache implementation
var redisUri = builder.Configuration.GetConnectionString("RedisConStr");
services.AddStackExchangeRedisCache(options => 
{
    options.Configuration = redisUri;
    /* optionally prefix keys */
    options.InstanceName = "ExampleApp.";
});

// get rid of warning about AspNetCore protecting keys
var redis = ConnectionMultiplexer.Connect(redisUri!);
services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

// Open Telemetry configuration
var otlpEndpoint = "http://your_otlp_endpoint:4317";

services.AddOpenTelemetry()
    .ConfigureResource(resource => {
        resource.AddService(serviceName: "home-automations");
    }).WithTracing(tracing =>{ 
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddHaKafkaNetInstrumentation() // <- HaKafkaNet instrumentation
            //.AddSource(KafkaFlowInstrumentation.ActivitySourceName)
            .AddOtlpExporter(exporterOptions => {
                exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                exporterOptions.Endpoint = new Uri(otlpEndpoint);
                exporterOptions.ExportProcessorType = ExportProcessorType.Batch;
            });
    }).WithMetrics(metrics => {
        metrics.AddAspNetCoreInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddHaKafkaNetInstrumentation() // <- HaKafkaNet instrumentation
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter((exporterOptions, metricReaderOptions) =>{
                exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                exporterOptions.Endpoint = new Uri(otlpEndpoint);
                exporterOptions.ExportProcessorType = ExportProcessorType.Batch;
            });
    });

builder.Logging.AddOpenTelemetry(logging => {
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.ParseStateValues = true;
    logging.AddOtlpExporter((exporterOptions, logProcessOptions) =>{
            exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            exporterOptions.Endpoint = new Uri(otlpEndpoint);
            exporterOptions.ExportProcessorType = ExportProcessorType.Batch;
        });
});

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("hakafkanet"));

// for local development of dashboard only
//app.UseCors("hknDev");

await app.StartHaKafkaNet();

app.Run();

