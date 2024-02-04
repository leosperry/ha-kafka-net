
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
    
    public EntityTracker(EntityTrackerConfig config, ISystemObserver observer, IAutomationManager automationManager, 
        IHaStateCache cache, IHaApiProvider provider)
    {
        _observer = observer;
        _automationMgr = automationManager;
        _cache = cache;
        _provider = provider;
        
        _interval = TimeSpan.FromMinutes(config.IntervalMinutes); 
        _maxEntityReportTime = TimeSpan.FromHours(config.MaxEntityNonresponsiveHours);
        _timer = new PeriodicTimer(_interval);

        observer.StateHandlerInitialized += () => StartTracking();
    }

    internal void StartTracking()
    {
        _ = CheckEntities();
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
        }
    }

    async IAsyncEnumerable<BadEntityState> FilterIds (IEnumerable<string> entityIds)
    {
        foreach (var item in entityIds)
        {
            // check the cache, if it has been updated in the interval, ignore
            var cached = await _cache.Get(item, _cancelSource.Token);
            if (cached is null || DateTime.Now - cached.LastUpdated > _maxEntityReportTime)
            {
                var (response, entityState) = await _provider.GetEntityState(item, _cancelSource.Token);
                if(response.StatusCode != System.Net.HttpStatusCode.OK || entityState is null || badStates.Contains(entityState.State))
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

