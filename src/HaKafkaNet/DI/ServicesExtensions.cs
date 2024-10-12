using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using FastEndpoints;
using HaKafkaNet.Implementations.Core;
using KafkaFlow;
using KafkaFlow.Admin.Dashboard;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
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

        services.AddHttpClient("HaKafkaNet");
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

        InitializeUserCode(app.Services, config);

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

    private static void InitializeUserCode(
        IServiceProvider services,
        HaKafkaNetConfig config)
    {
        List<InitializationError> errors = new();

        // check hard requirements first
        CheckValidHaConfig(config.HaConnectionInfo, errors);
        CheckCacheRequirement(services, errors);

        if (errors.Any())
        {
            System.Console.WriteLine("***********************************************************");
            System.Console.WriteLine("* * * * * * HaKafkaNet Prerequisites not met * * * * * * **");
            System.Console.WriteLine("** Valid Home Assistant connection information required  **");
            System.Console.WriteLine("** Implementation of IDistributed Cache must be provided **");
            PrintErrors(errors);
            System.Console.WriteLine("***********************************************************");
            return;
        }

        var observer = services.GetRequiredService<ISystemObserver>();

        try
        {
            //wire up monitors to observer
            InitializeMonitors(services, observer, errors);

            InitializeRegistries(services, errors);

            IAutomationManager automationManager;
            try
            {
                // this line could throw from user constructors
                automationManager = services.GetRequiredService<IAutomationManager>();
            }
            catch (System.Exception ex)
            {
                errors.Add(new("Error initializing Automation Manager", ex));
                Notify(services, errors, observer);
                return;
            }

            // Calls Register method of all registries
            // gets/sets all metadata for automations
            // automation builder exceptions are caught
            automationManager.Initialize(errors);

            InitializeAutomations(automationManager, errors);
        }
        catch (System.Exception ex)
        {
            var message = "Critical error initializing user code";
            errors.Add(new(message, ex));
            System.Console.WriteLine(message);
            System.Console.WriteLine(ex.Message);
        }

        Notify(services, errors, observer);
    }

    private static void CheckValidHaConfig(HomeAssistantConnectionInfo connectionInfo, List<InitializationError> errors)
    {
        if (string.IsNullOrEmpty(connectionInfo.BaseUri))
        {
            errors.Add(new("BaseUri for Home Assisstant instance is not set."));
        }
        try
        {
            var uri = new Uri(connectionInfo.BaseUri);
        }
        catch (System.Exception ex)
        {
            errors.Add(new("BaseUri for Home Assistant is invalid.", ex));
        }

        if (string.IsNullOrEmpty(connectionInfo.AccessToken))
        {
            errors.Add(new("AccessToken for home assistant is not set."));
        }
        try
        {
            var header = new AuthenticationHeaderValue("Bearer", connectionInfo.AccessToken);
        }
        catch (Exception ex)
        {
            errors.Add(new("AccessToken is invalid.", ex));
        }
    }

    private static void CheckCacheRequirement(IServiceProvider services, List<InitializationError> errors)
    {
        try
        {
            var cache = services.GetRequiredService<IDistributedCache>();
            Task.Run(async () => await cache.SetStringAsync("HaKafKanet.StartTime", DateTime.Now.ToString())).Wait();
        }
        catch(AggregateException agg)
        {
            foreach (var er in agg.InnerExceptions)
            {
                errors.Add(new("IDistributedCache Aggregate Exception", er, null));
            }
        }
        catch (Exception ex)
        {
            errors.Add(new("IDistributedCache is required", ex, null));
        }
    }

    private static void PrintErrors(IEnumerable<InitializationError> errors)
    {
        foreach (var er in errors)
        {
            System.Console.WriteLine($"** {er.Message}");
            System.Console.WriteLine($"** Source: {er.Source?.GetType().Name ?? "unknown"}");
            if (er.Exception is not null)
            {
                System.Console.WriteLine($"** Exception: {er.Exception.Message}");
                System.Console.WriteLine($"** Stack: {er.Exception.StackTrace}");
            }
        }
    }

    private static void Notify(IServiceProvider services, List<InitializationError> errors, ISystemObserver observer)
    {
        if (errors.Any())
        {
            // attempt to log
            try
            {
                var logger = services.GetRequiredService<ILogger<HaKafkaNetConfig>>();
                foreach (var er in errors)
                {
                    if (er.Exception is not null)
                    {
                        logger.LogCritical(er.Exception, $"{er.Message} - Source: {er.Source?.GetType().Name ?? "unspecified"}");
                    }
                    else
                    {
                        logger.LogCritical($"{er.Message} - Source: {er.Source?.GetType().Name ?? "unspecified"}");
                    }
                    
                }
            }
            catch (System.Exception)
            {
                
                throw;
            }

            // then send to Home Assistant
            var api = services.GetRequiredService<IHaApiProvider>();
            WireNotificationSend(api);
            if(!observer.OnInitializationFailure(errors))
            {
                // no monitors are configured
                try
                {
                    Task.Run(() => SendNotificationToHomeAssistant(api, errors)).Wait();
                }
                catch (System.Exception ex)
                {
                    // last resort
                    errors.Add(new("Error sending notification to Home Assistant", ex, api));
                    // print to console
                    PrintErrors(errors);
                }
            };
        }
    }

    private static Action<InitializationError[]>? _notifyErrors; 
    internal static bool TrySendNotification(InitializationError[] errors)
    {
        if (_notifyErrors is not null)
        {
            _notifyErrors(errors);
            return true;
        }
        else
        {
            PrintErrors(errors);
        }
        return false;
    }

    private static void WireNotificationSend(IHaApiProvider api)
    {
        _notifyErrors = er => Task.Run(async () =>
        {
            await SendNotificationToHomeAssistant(api, er);
        });
    }

    private static async Task SendNotificationToHomeAssistant(IHaApiProvider api, IEnumerable<InitializationError> errors)
    {
        string message = getFormatedMessage(errors);
        await api.PersistentNotificationDetail(message, "HaKafkanNet did not initialize completely");
    }

    private static string getFormatedMessage(IEnumerable<InitializationError> errors)
    {
        StringBuilder sb = new();
        foreach (var error in errors)
        {
            // start with a bullet
            sb.AppendLine($"## {error.Message}");
            sb.AppendLine($"* Source: {error.Source?.GetType().Name ?? "unknown"}");
            if(error.Exception is not null)
            {
                sb.AppendLine($"* {error.Exception.Message}");
                sb.AppendLine($"* Stacktrace: {error.Exception.StackTrace}");
            }
        }
        return sb.ToString();
    }


    private static void InitializeMonitors(IServiceProvider services, ISystemObserver observer, List<InitializationError> errors)
    {
        try
        {
            var monitors = services.GetRequiredService<IEnumerable<ISystemMonitor>>();
            observer.InitializeMonitors(monitors);        
        }
        catch (System.Exception ex)
        {
            var message = "System Monitors could not be initialized";
            errors.Add(new(message, ex));
            // threoretically only errors here should be from user constructors
            // in which case, NO monitors would be enabled.
            System.Console.WriteLine();
        }
    }

    private static void InitializeRegistries(IServiceProvider services, List<InitializationError> errors)
    {
        IEnumerable<IAutomationRegistry> registries;
        try
        {
            registries = services.GetRequiredService<IEnumerable<IAutomationRegistry>>();
        }
        catch (System.Exception ex)
        {
            errors.Add(new("Error constructing registries", ex));
            return;
        }
        if(registries is null) return;

        List<Task> tasks = new();
        foreach (var reg in registries)
        {
            if (reg is IInitializeOnStartup init)
            {
                var t = Task.Run(() => init.Initialize()) 
                    .ContinueWith(t =>  
                    {
                        if (t.Exception is not null)
                        {
                            errors.Add(new($"Error initializing {reg.GetType().Name}", t.Exception, reg));
                        }
                    });

                tasks.Add(t);
            }
        }

        try
        {
            if (tasks.Count > 0)
            {
                Task.WaitAll(tasks.ToArray());
            }
        }
        catch (AggregateException ex)
        {
            // in theory, this line should not be hit
            // becaues in the continuation, we did not access the result property
            errors.Add(new($"Error initializing automations", ex));
        }
    }

    private static void InitializeAutomations(IAutomationManager automationManager, List<InitializationError> errors)
    {
        List<Task> tasks = new();
        foreach (var auto in automationManager.GetAll())
        {
            if(auto.WrappedAutomation is IInitializeOnStartup init)
            {
                tasks.Add(Task.Run(() => init.Initialize())
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            var meta = auto.GetMetaData();
                            errors.Add(new($"Error initializing {meta.Name} of type {meta.UnderlyingType}", t.Exception, auto.WrappedAutomation));
                        }
                    }));
            }
        }

        try
        {
            if (tasks.Count > 0)
            {
                Task.WaitAll(tasks.ToArray());
            }
        }
        catch (AggregateException ex)
        {
            errors.Add(new($"Error initializing automations", ex));
        }
    }
}

public record InitializationError(string Message, Exception? Exception = null, object? Source = null);

