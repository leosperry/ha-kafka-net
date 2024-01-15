using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Consumers.DistributionStrategies;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HaKafkaNet;

public static class ServicesExtensions
{
    public  static IServiceCollection AddHaKafkaNet(this IServiceCollection services, HaKafkaNetConfig config, Action<IClusterConfigurationBuilder>? kafabuilder = null)
    {
        services.AddKafka(kafka => 
        {
            kafka
                .UseConsoleLog()
                .AddCluster(cluster =>
                {
                    cluster.WithBrokers(config.KafkaBrokerAddresses);

                    WireTransformer(cluster, config);
                    WireState(services, cluster, config);

                    cluster
                        .EnableAdminMessages("kafka-flow.admin")
                        .EnableTelemetry("kafka-flow.admin");
                    
                    kafabuilder?.Invoke(cluster);
                }
            );
        });

        if (config.Api.Enabled)
        {
            services.AddHttpClient();
            services.AddSingleton(config.Api);
            services.AddSingleton<IHaApiProvider, HaApiProvider>();
        }
        return services;
    }

    public static async Task StartHaKafkaNet(this WebApplication app, HaKafkaNetConfig config)
    {
        if (config.ExposeKafkaFlowDashboard)
        {
            app.UseKafkaFlowDashboard();
        }
        
        var kafkaBus = app.Services.CreateKafkaBus();
        await kafkaBus.StartAsync();
    }

    private static void WireState(IServiceCollection services, IClusterConfigurationBuilder cluster, HaKafkaNetConfig config)
    {
        if (config.StateHandler.Enabled)
        {
            cluster
                .AddConsumer(consumer => consumer
                    .Topic(config.TransofrmedTopic)
                    .WithGroupId(config.StateHandler.GroupId)
                    .WithWorkersCount(config.StateHandler.WorkerCount)
                    .WithBufferSize(config.StateHandler.BufferSize)
                    .WithWorkerDistributionStrategy<FreeWorkerDistributionStrategy>()
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .WithoutStoringOffsets()
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h.
                            AddHandler<HaStateHandler>())
                    )
                );

            services.AddSingleton<IHaServices, HaServices>();
            services.AddSingleton<IHaStateCache, HaStateCache>();

            // get all the automation types
            var automationTypes =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where
                    typeof(IAutomation).IsAssignableFrom(t) &&
                    t.IsClass &&
                    !t.IsAbstract
                select t;

            foreach (var item in automationTypes)
            {
                services.AddSingleton(typeof(IAutomation), item);
            }
        }
    }

    private static void WireTransformer(IClusterConfigurationBuilder cluster, HaKafkaNetConfig config)
    {
        if (config.Transformer.Enabled)
        {
            cluster
                .AddConsumer(consumer => consumer
                    .Topic(config.Transformer.HaRawTopic)
                    .WithGroupId(config.Transformer.GroupId)
                    .WithWorkersCount(config.Transformer.WorkerCount)
                    .WithBufferSize(config.Transformer.BufferSize)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer, HaMessageResolver>()
                        .AddTypedHandlers(h => h.
                            AddHandler<HaTransformerHandler>())
                    )
                )
                .AddProducer("ha-producer", producer => producer
                    .DefaultTopic(config.TransofrmedTopic)
                    .AddMiddlewares(middlewares => middlewares
                        .AddSerializer<JsonCoreSerializer>())
                );
        }
    }
}
