using System.Reflection;
using FastEndpoints;
using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

namespace HaKafkaNet;

public static class ServicesExtensions
{
    public static IServiceCollection AddHaKafkaNet(this IServiceCollection services, Action<HaKafkaNetConfig> options, Action<IClusterConfigurationBuilder>? kafabuilder = null)
    {
        HaKafkaNetConfig config = new();
        options(config);

        AddHaKafkaNet(services, config, kafabuilder);
        return services;
    }

    public  static IServiceCollection AddHaKafkaNet(this IServiceCollection services, HaKafkaNetConfig config, Action<IClusterConfigurationBuilder>? kafabuilder = null)
    {
        services.AddSingleton(config);
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

        if (config.EntityTracker.Enabled)
        {
            services.AddSingleton(config.EntityTracker);
        }

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

    public static async Task StartHaKafkaNet(this WebApplication app)
    {
        var config = app.Services.GetRequiredService<HaKafkaNetConfig>();
        if(config.UseDashboard)
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "",
                FileProvider = new PhysicalFileProvider(Path.Combine(rootPath, "www")),
            });
            if (config.UseDashboard)
            {
                app.UseFastEndpoints();
            }
        }

        if (config.ExposeKafkaFlowDashboard)
        {
            app.UseKafkaFlowDashboard();
        }
        
        var kafkaBus = app.Services.CreateKafkaBus();
        await kafkaBus.StartAsync();

        if (config.EntityTracker.Enabled)
        {
            var entityTracker = app.Services.GetRequiredService<EntityTracker>();
            app.Lifetime.ApplicationStopping.Register(() => entityTracker.Dispose());
        }
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
            services.AddSingleton<IAutomationManager, AutomationManager>();
            services.AddSingleton<IAutomationFactory, AutomationFactory>();
            services.AddSingleton<ISystemObserver, SystemObserver>();
            services.AddSingleton<IAutomationBuilder, AutomationBuilder>();
            services.AddSingleton<EntityTracker>();

            var eligibleTypes = 
                (from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.GetCustomAttributes(typeof(ExcludeFromDiscoveryAttribute)).Any()
                select t).ToArray();

            foreach (var type in eligibleTypes)
            {
                switch (type)
                {
                    case var _ when typeof(IAutomation).IsAssignableFrom(type):
                        ServiceDescriptor auto = new(typeof(IAutomation), type, ServiceLifetime.Singleton);
                        services.TryAddEnumerable(auto);
                        break;
                    case var _ when typeof(IConditionalAutomation).IsAssignableFrom(type):
                        ServiceDescriptor conditional = new(typeof(IConditionalAutomation), type, ServiceLifetime.Singleton);
                        services.TryAddEnumerable(conditional);
                        break;
                    case var _ when typeof(IAutomationRegistry).IsAssignableFrom(type):
                        ServiceDescriptor registry = new(typeof(IAutomationRegistry), type, ServiceLifetime.Singleton);
                        services.TryAddEnumerable(registry);
                        break;
                    case var _ when typeof(ISystemMonitor).IsAssignableFrom(type):
                        ServiceDescriptor monitor = new(typeof(ISystemMonitor), type, ServiceLifetime.Singleton);
                        services.TryAddEnumerable(monitor);
                        break;
                    default:
                        //do nothing
                        break;
                }
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
