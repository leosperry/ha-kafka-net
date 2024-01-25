using System.Reflection;
using FastEndpoints;
using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Consumers.DistributionStrategies;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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

        if (config.HaConnectionInfo.Enabled)
        {
            services.AddHttpClient();
            services.AddSingleton(config.HaConnectionInfo);
            services.AddSingleton<IHaApiProvider, HaApiProvider>();
        }

        if(config.UseDashboard)
        {
            services.AddFastEndpoints();
        }

        return services;
    }

    public static async Task StartHaKafkaNet(this WebApplication app, HaKafkaNetConfig config)
    {
        if(config.UseDashboard)
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "",
                FileProvider = new PhysicalFileProvider(Path.Combine(rootPath, "www")),
            });
            app.UseFastEndpoints();
        }

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
            services.AddSingleton<IAutomationCollector, AutomationManager>();
            services.AddSingleton<IAutomationFactory, AutomationFactory>();
            services.AddSingleton<StateHandlerObserver>();

            // get all the automation types
            var eligibleTypes = 
                (from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.GetCustomAttributes(typeof(ExcludeFromDiscoveryAttribute)).Any()
                select t).ToArray();

            foreach (var type in eligibleTypes.Where(t => typeof(IAutomation).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(IAutomation), type);
            }

            foreach (var type in eligibleTypes.Where(t => typeof(IConditionalAutomation).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(IConditionalAutomation), type);
            }

            foreach (var type in eligibleTypes.Where(t => typeof(IAutomationRegistry).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(IAutomationRegistry), type);
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
