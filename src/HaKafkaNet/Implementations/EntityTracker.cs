namespace HaKafkaNet;

internal class EntityTracker : IDisposable
{

    HashSet<string> badStates = ["unknown","unavailable","none"];

    TimeSpan _interval;
    TimeSpan _maxEntityReportTime;
    
    readonly CancellationTokenSource _cancelSource = new();
    readonly PeriodicTimer _timer;
    Task? _timerTask;
    
    readonly SystemObserver _observer;
    readonly IAutomationManager _automationMgr;
    readonly IHaServices _services;
    
    public EntityTracker(SystemObserver observer, IAutomationManager automationManager, IHaServices services)
    {
        _observer = observer;
        _automationMgr = automationManager;
        _services = services;
        
        _interval = TimeSpan.FromHours(1); // make configurable?
        _maxEntityReportTime = TimeSpan.FromHours(12); // make configurable?
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
            // check the cache, if they've been updated in the interval, ignore
            var cached = await _services.Cache.Get(item, _cancelSource.Token);
            if (cached is null || DateTime.Now - cached.LastUpdated > _maxEntityReportTime)
            {
                var (response, entityState) = await _services.Api.GetEntityState(item, _cancelSource.Token);
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

