using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NLog;

namespace HaKafkaNet;

public interface IAutomationTraceProvider
{
    Task Trace(TraceEvent evt, Func<Task> traceFunction);

    Task<IEnumerable<TraceData>> GetTraces(string automationKey);

    void AddLog(string renderedMessage, LogEventInfo logEvent, IDictionary<string, object> scopes);
}

internal class AutomationTraceProvider : IAutomationTraceProvider
{
    readonly IDistributedCache _cache;
    readonly ILogger<AutomationTraceProvider> _logger;

    const string CachKeyPrefix = "hkn.tracedata.";
    ConcurrentDictionary<string, SemaphoreSlim> _automationLocks = new();

    readonly DistributedCacheEntryOptions _cacheOptions = new ()
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };


    const string 
        automationKey = "automationKey",
        automationEventType = "automationEventType",
        automationEventTime = "automationEventTime";
    
    // first string: automationKey, second string: composite from event
    ConcurrentDictionary<string, ConcurrentDictionary<string,(TraceEvent evt, ConcurrentQueue<LogInfo>)>> _activeTraces = new();

    public AutomationTraceProvider(IDistributedCache cache, ILogger<AutomationTraceProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void AddLog(string renderedMessage, LogEventInfo logEvent, IDictionary<string, object> scopes)
    {          
        if (scopes.TryGetValue(automationKey, out var autoKeyRaw) &&
            scopes.TryGetValue(automationEventType, out var evtType) &&
            scopes.TryGetValue(automationEventTime, out var evtTime))
        {
            // add to local
            if (_activeTraces.TryGetValue(autoKeyRaw.ToString()!, out var automationRecords))
            {
                var instanceKey = MakeKey(evtType.ToString()!, evtTime.ToString()!);
                automationRecords[instanceKey].Item2.Enqueue(new LogInfo()
                {
                    LogLevel = logEvent.Level.ToString(), 
                    Message = renderedMessage, 
                    Properties = logEvent.Properties.ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value), 
                    Scopes = scopes,
                    Exception = logEvent.Exception is null ? null : ExecptionInfo.Create(logEvent.Exception)
                });
            }
        }
    }

    public async Task Trace(TraceEvent evt, Func<Task> traceFunction)
    {
        var data = AddTrace(evt);
        var scopeData = new Dictionary<string, object>()
        {
            {automationKey, evt.AutomationKey},
            {automationEventType, evt.EventType},
            {automationEventTime, evt.EventTime.ToString("O")},
        };

        using(_logger.BeginScope(scopeData))
        {
            var task = traceFunction();
            try
            {
                // if the task is from Task.WhenAll()
                // awaiting here will only catch the first exception
                task.Wait();
            }
            catch (System.Exception)
            {
            }
            if (task.IsFaulted)
            {
                // at this point we should have all the exceptions
                data.Item1.Exception = ExecptionInfo.Create(task.Exception);
                await WriteToCache(new TraceData()
                {
                    TraceEvent = data.Item1,
                    Logs = data.Item2
                });
                // rethrow 
                // will loose the call stack but we already captured it
                throw task.Exception;
            }
            await WriteToCache(new TraceData()
            {
                TraceEvent = data.Item1,
                Logs = data.Item2
            });        
        }
    }

    public async Task<IEnumerable<TraceData>> GetTraces(string automationKey)
    {
        var loc = _automationLocks.GetOrAdd(automationKey, new SemaphoreSlim(1));
        await loc.WaitAsync();
        try
        {
            IEnumerable<TraceData>? locals = null;
            if (_activeTraces.TryGetValue(automationKey, out var data))
            {
                locals =
                    from key in data.Keys
                    let tuple = data[key]
                    select new TraceData()
                    {
                        TraceEvent = tuple.evt,
                        Logs = tuple.Item2
                    };
            }
            var cached = await ReadFromCache(CachKeyPrefix + automationKey);
            return (cached ?? Enumerable.Empty<TraceData>()).Union(locals ?? Enumerable.Empty<TraceData>()).Reverse().ToArray();            
        }
        finally
        {
            loc.Release();
        }
    }

    private (TraceEvent, ConcurrentQueue<LogInfo>) AddTrace(TraceEvent evt)
    {
        _ = _activeTraces.TryAdd(evt.AutomationKey, new());

        var key = MakeKey(evt.EventType, evt.EventTime.ToString("O"));

        var retVal = _activeTraces[evt.AutomationKey].GetOrAdd(key, (evt, new ConcurrentQueue<LogInfo>()));

        return retVal;
    }

    private async Task WriteToCache(TraceData traceData)
    {
        var key = CachKeyPrefix + traceData.TraceEvent.AutomationKey;

        var loc = _automationLocks.GetOrAdd(traceData.TraceEvent.AutomationKey, new SemaphoreSlim(1));

        await loc.WaitAsync();
        try
        {
            var existing = await ReadFromCache(key);
            Queue<TraceData>? q = null;
            if (existing is not null)
            {
                q = new Queue<TraceData>(existing);
                while (q.Count > 50)
                {
                    q.Dequeue();
                }
            }
            if (q is null)
            {
                q = new();
            }
            q.Enqueue(traceData);
            var value = JsonSerializer.SerializeToUtf8Bytes(q.ToArray());
            await _cache.SetAsync(key, value, _cacheOptions);

            // removed from active
            var instanceKey = MakeKey(traceData.TraceEvent.EventType, traceData.TraceEvent.EventTime.ToString("O"));
            _activeTraces[traceData.TraceEvent.AutomationKey].TryRemove(instanceKey, out _);
        }
        finally
        {
            loc.Release();
        }
    }

    private async Task<IEnumerable<TraceData>?> ReadFromCache(string prefixedAutomationKey)
    {
        var cachedBytes = await _cache.GetAsync(prefixedAutomationKey);
        TraceData[]? cachedData = null;
        if (cachedBytes is not null)
        {
            try
            {
                cachedData = JsonSerializer.Deserialize<TraceData[]>(cachedBytes)!;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "could not deserialize trace data from cache");
                throw;
            }
            
        } 
        return cachedData;
    }

    private string MakeKey(string eventType, string time)
    {
        
        return eventType + time;
    }
}

public record TraceData
{
    public required TraceEvent TraceEvent { get; set; }
    public required IEnumerable<LogInfo> Logs {get; set; }
}

public record LogInfo
{
    public required string LogLevel { get; set; }
    public required string Message { get; set; }
    public required IDictionary<string,object> Scopes { get; set; }
    public required IDictionary<string,object> Properties { get; set; }
    public ExecptionInfo? Exception { get; set; }
}
