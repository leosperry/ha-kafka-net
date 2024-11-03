using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface ISystemObserver
{
    bool IsInitialized{get;}
    event Action? StateHandlerInitialized;

    void InitializeMonitors(IEnumerable<ISystemMonitor> monitors);

    void OnStateHandlerInitialized();
    void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception);

    void OnBadStateDiscovered(BadEntityState badState);

    /// <summary>
    /// Will be called when a Home Assistant service is called and the response is not in the 200/300 range or if the call throws an exception.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    void OnHaServiceBadResponse(HaServiceResponseArgs args, CancellationToken ct);

    void OnHaNotification(HaNotification notification, CancellationToken ct);

    void OnHaStartUpShutdown(StartUpShutDownEvent evt, CancellationToken ct);

    void OnEntityStateUpdate(HaEntityState state);
    void RegisterThreadSafeEntityUpdater(string entityId, UpdateEntity updater);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns>true if at least one monitor is wired. Otherwise false</returns>
    bool OnInitializationFailure(List<InitializationError> errors);

    void OnAutomationTypeConversionFailure(Exception ex, IAutomationBase automation, HaEntityStateChange stateChange, CancellationToken ct);
}

public record HaServiceResponseArgs(string Domain, string Service, object Data, HttpResponseMessage? Response, Exception? Exception);

/// <summary>
/// Kafka handlers should be initialized by kafka flow and not other classes via DI.
/// Therefore, we need an observer to report on thier state.
/// </summary>
internal class SystemObserver : ISystemObserver
{
    ILogger _logger;

    public bool IsInitialized { get; private set; }
    public event Action? StateHandlerInitialized;
    internal event Action<AutomationMetaData, Exception>? UnhandledException;
    internal event Action<BadEntityState>? BadEntityState;
    internal event Action<HaNotification, CancellationToken>? Notify;
    internal event Action<StartUpShutDownEvent, CancellationToken>? HaStartUpShutdown;
    internal event Action<HaServiceResponseArgs, CancellationToken>? HaApiResponse;
    internal event Action<InitializationError[]>? InitializatinFailure;
    internal event Action<IAutomationBase, HaEntityStateChange, Exception, CancellationToken>? AutomationTypeConversionFailure;

    // used for auto-updating entities
    ConcurrentDictionary<string, UpdateEntity> updaters = new();

    public SystemObserver(ILogger<SystemObserver> logger)
    {
        _logger = logger;
    }

    private async Task WrapTask(string taskType, Func<Task> funcToErrorHandle)
    {
        Task t;
        Dictionary<string, object> scope = new()
        {
            {"systemMonitorCall",DateTime.Now},
            {"systemMonitorCallType", taskType}
        };
        using(_logger.BeginScope(scope))
        {
            try
            {
                t = funcToErrorHandle();
                await t;
                t.Wait();
            }
            catch (Exception cancelEx) when (cancelEx is TaskCanceledException || cancelEx is OperationCanceledException)
            {
                _logger.LogInformation(cancelEx, "Task Canceled in System Observer");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error in System Observer");
                //swallow this so that other handlers continue to run
            }
        }
    }

    public void OnStateHandlerInitialized()
    {
        IsInitialized = true;
        StateHandlerInitialized?.Invoke();
    }

    public void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception)
        => UnhandledException?.Invoke(automationMetaData, exception);

    public void OnBadStateDiscovered(BadEntityState badState)
        => BadEntityState?.Invoke(badState);

    public void OnHaNotification(HaNotification notification, CancellationToken ct)
        => Notify?.Invoke(notification, ct);

    public void OnHaStartUpShutdown(StartUpShutDownEvent evt, CancellationToken ct)
        => HaStartUpShutdown?.Invoke(evt, ct);

    public void OnHaServiceBadResponse(HaServiceResponseArgs args, CancellationToken ct)
        => HaApiResponse?.Invoke(args, ct);

    public void OnAutomationTypeConversionFailure(Exception ex, IAutomationBase automation, HaEntityStateChange stateChange, CancellationToken ct)
    {
        if (AutomationTypeConversionFailure is not null)
        {
            try
            {
                AutomationTypeConversionFailure?.Invoke(automation, stateChange, ex, ct);
                return;
            }
            catch (System.Exception caught)
            {
                _logger.LogError(caught, "Error handling Type Conversion Failure");
            }
        }
        _logger.LogError("Automation Type Conversion Failure in {automation}. Data: {state_change}", automation.GetType().Name, stateChange);
    }

    public bool OnInitializationFailure(List<InitializationError> errors)
    {
        if (InitializatinFailure is not null)
        {
            try
            {
                InitializatinFailure?.Invoke(errors.ToArray());
                return true;
            }
            catch (System.Exception ex)
            {
                errors.Add(new("Error Executing user implemented OnInitializationFailure", ex, this));
                return false;
            }
        }
        return false;
    }

    public void InitializeMonitors(IEnumerable<ISystemMonitor> monitors)
    {
        foreach (var monitor in monitors)
        {
            StateHandlerInitialized += () =>    _ = WrapTask("State Handler Initialized", ()=> monitor.StateHandlerInitialized());
            UnhandledException += (meta, ex) => _ = WrapTask("Unhandled Exception", ()=> monitor.UnhandledException(meta, ex));
            BadEntityState += (state) =>        _ = WrapTask("Bad Entity State", ()=> monitor.BadEntityStateDiscovered(state));
            Notify += (note, ct) =>             _ = WrapTask("HA Notification", () => monitor.HaNotificationUpdate(note, ct));
            HaStartUpShutdown += (evt, ct) =>   _ = WrapTask("HA StartupShutDown", () => monitor.HaStartUpShutDown(evt, ct));
            HaApiResponse += (args, ct) =>      _ = WrapTask("HA API Response", () => monitor.HaApiResponse(args, ct));
            InitializatinFailure += (errors) => _ = monitor.InitializationFailure(errors); // don't wrap this one, we want exceptions to bubble up
            AutomationTypeConversionFailure += (auto, sc, ex, ct) => _ = WrapTask("Automation Type Conversion Failure", () => monitor.AutomationTypeConversionFailure(auto, sc, ex, ct));
        }    
    }

    public void OnEntityStateUpdate(HaEntityState state)
    {
        if (updaters.TryGetValue(state.EntityId, out var updateMethod))
        {
            try
            {
                updateMethod(state);
            }
            catch (System.Exception ex)
            {
                // re-throwing here could cause automations to not run
                _logger.LogError(ex, "error updating auto-updating entity - raw data : {rawState}", state.ToString());
            }       
        }   
    }

    public void RegisterThreadSafeEntityUpdater(string entityId, UpdateEntity updater)
    {
        updaters.GetOrAdd(entityId, updater);
    }
}

internal delegate void UpdateEntity(HaEntityState state);

