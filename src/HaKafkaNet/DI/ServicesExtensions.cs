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
            services.AddSingleton<IHaEntityProvider, HaEntityProvider>();

            // get all the automation types
            var eligibleTypes = 
                (from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where
                    t.IsClass &&
                    !t.IsAbstract &&
                    t != typeof(ConditionalAutomationWrapper)// otherwise the container constructs one with a random inner conditional implementation
                select t).ToArray();

            foreach (var item in eligibleTypes.Where(t => typeof(IAutomation).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(IAutomation), item);
            }

            foreach (var item in eligibleTypes.Where(t => typeof(IConditionalAutomation).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(IConditionalAutomation), item);
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
