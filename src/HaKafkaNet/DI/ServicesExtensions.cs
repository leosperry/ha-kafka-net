﻿using System.Reflection;
using System.Text.Json.Serialization;
using FastEndpoints;
using HaKafkaNet.Implementations.Core;
using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using NLog;


namespace HaKafkaNet;

public static class ServicesExtensions
{
    public static IServiceCollection AddHaKafkaNet(this IServiceCollection services, Action<HaKafkaNetConfig> options, Action<IKafkaConfigurationBuilder, IClusterConfigurationBuilder>? kafabuilder = null)
    {
        HaKafkaNetConfig config = new();
        options(config);

        AddHaKafkaNet(services, config, kafabuilder);
        return services;
    }

    public  static IServiceCollection AddHaKafkaNet(this IServiceCollection services, HaKafkaNetConfig config, Action<IKafkaConfigurationBuilder, IClusterConfigurationBuilder>? kafabuilder = null)
    {
        services.AddSingleton(config);
        services.AddKafka(kafka => 
        {
            kafka
                .AddCluster(cluster =>
                {
                    cluster.WithBrokers(config.KafkaBrokerAddresses);

                    WireState(services, cluster, config);

                    cluster
                        .EnableAdminMessages("kafka-flow.admin")
                        .EnableTelemetry("kafka-flow.admin");
                    
                    kafabuilder?.Invoke(kafka, cluster);
                }
            );
        });

        services.AddHttpClient();
        services.AddSingleton(config.HaConnectionInfo);
        services.AddSingleton<IHaApiProvider, HaApiProvider>();

        if(config.UseDashboard)
        {
            services.Configure<JsonOptions>(o => {
                o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddFastEndpoints();
            services.AddMvc().AddApplicationPart(Assembly.GetAssembly(typeof(ServicesExtensions))!);
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
                RequestPath = "/assets",
                FileProvider = new PhysicalFileProvider(Path.Combine(rootPath, "www/assets")),
            });
            
            app.UseFastEndpoints();
            app.UseRouting();
            app.MapControllers();
        }

        if (config.ExposeKafkaFlowDashboard)
        {
            app.UseKafkaFlowDashboard();
        }

        //wire up logging before any user code executes
        try
        {
            LogManager.Configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, app.Services.GetRequiredService<HknLogTarget>());
            LogManager.ReconfigExistingLoggers();
        }
        catch (Exception)
        {
            Console.WriteLine("**************************************");
            Console.WriteLine("** Log tracing could not be enabled **");
            Console.WriteLine("** Configure NLog to enable tracing **");
            Console.WriteLine("**************************************");
        }

        //wire up monitors to observer
        var observer = app.Services.GetRequiredService<ISystemObserver>();

        // first user code is constructors of monitor
        var monitors = app.Services.GetRequiredService<IEnumerable<ISystemMonitor>>();
        observer.InitializeMonitors(monitors);
        
        // kafka handler requires automation manager and in turn user automations
        var kafkaBus = app.Services.CreateKafkaBus();
        await kafkaBus.StartAsync();
    }

    private static void WireState(IServiceCollection services, IClusterConfigurationBuilder cluster, HaKafkaNetConfig config)
    {
        cluster
            .AddConsumer(consumer => consumer
                .Topic(config.KafkaTopic)
                .WithGroupId(config.StateHandler.GroupId)
                .WithWorkersCount(config.StateHandler.WorkerCount)
                .WithBufferSize(config.StateHandler.BufferSize)
                .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                .WithoutStoringOffsets()
                .AddMiddlewares(middlewares => middlewares
                    .AddDeserializer<JsonCoreDeserializer, HaMessageResolver>()
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
        services.AddSingleton<IInternalRegistrar, AutomationRegistrar>();

        services.AddSingleton<IUpdatingEntityProvider, UpdatingEntityProvider>();

        services.AddSingleton<IAutomationTraceProvider, TraceLogProvider>();
        services.AddSingleton<HknLogTarget>();

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
            // handle interfaced classes
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
                case var _ when typeof(ISchedulableAutomation).IsAssignableFrom(type):
                    ServiceDescriptor schedulable = new(typeof(ISchedulableAutomation), type, ServiceLifetime.Singleton);
                    services.TryAddEnumerable(schedulable);
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
