
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class EntityTracker : IDisposable
{
    HashSet<string> badStates = ["unknown","unavailable","none"];

    TimeSpan _interval;
    TimeSpan _maxEntityReportTime;
    
    readonly CancellationTokenSource _cancelSource = new();
    readonly PeriodicTimer _timer;
    Task? _timerTask;
    
    readonly ISystemObserver _observer;
    readonly IAutomationManager _automationMgr;
    readonly IHaStateCache _cache;
    readonly IHaApiProvider _provider;
    readonly ILogger _logger;

    static ActivitySource _activitySource = new ActivitySource(Telemetry.TraceTrackerName);
    
    public EntityTracker(EntityTrackerConfig config, ISystemObserver observer, IAutomationManager automationManager, 
        IHaStateCache cache, IHaApiProvider provider, ILogger<EntityTracker> logger)
    {
        _observer = observer;
        _automationMgr = automationManager;
        _cache = cache;
        _provider = provider;
        _logger = logger;
        
        _interval = TimeSpan.FromMinutes(config.IntervalMinutes); 
        _maxEntityReportTime = TimeSpan.FromHours(config.MaxEntityNonresponsiveHours);
        _timer = new PeriodicTimer(_interval);

        observer.StateHandlerInitialized += () => StartTracking();
    }

    internal void StartTracking()
    {
        // run it once at the start
        _ = CheckEntities();
        // now run on a schedule
        _timerTask = TimerTick();
    }

    private async Task TimerTick()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cancelSource.Token)
                && !_cancelSource.IsCancellationRequested)
            {
                await CheckEntities();
            }
        }
        catch (OperationCanceledException){}
    }

    private async Task CheckEntities()
    {
        Dictionary<string,object> scope = new()
        {
            {"tracker_runtime", DateTime.Now}
        };
        using(_logger.BeginScope(scope))
        using(_activitySource.StartActivity("ha_kafka_net.tracker_run"))
        {
            _logger.LogInformation("starting tracker run");
            // check entities' states
            var entityIds = _automationMgr.GetAllEntitiesToTrack();
            var badIds = FilterIds(entityIds).WithCancellation(_cancelSource.Token);

            List<BadEntityState> badStates = new();
            await foreach (var bad in badIds)
            {
                badStates.Add(bad);
            }
            if (badStates.Any())
            {
                _observer.OnBadStateDiscovered(badStates);
                _logger.LogInformation($"{badStates.Count} bad entities discovered");
            }
            else
            {
                _logger.LogInformation("no bad states discovered");
            }
        }
    }

    async IAsyncEnumerable<BadEntityState> FilterIds (IEnumerable<string> entityIds)
    {
        foreach (var item in entityIds)
        {
            // check the cache, if it has been updated in the interval, ignore
            var cached = await _cache.GetEntity(item, _cancelSource.Token);
            if (cached is null || DateTime.Now - cached.LastUpdated > _maxEntityReportTime)
            {
                var (response, entityState) = await _provider.GetEntity(item, _cancelSource.Token);
                if(response.StatusCode != System.Net.HttpStatusCode.OK || entityState is null || entityState.State is null || badStates.Contains(entityState.State))
                {
                    yield return new(item, entityState);
                }
            }
        }
    }

    public void Dispose()
    {
        _cancelSource.Cancel();
        if (_timerTask is not null)
        {
            _timerTask.Wait();
        }
        
        _timer.Dispose();
        _cancelSource.Dispose();
    }
}
