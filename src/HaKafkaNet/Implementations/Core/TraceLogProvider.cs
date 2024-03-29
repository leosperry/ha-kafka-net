using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NLog;

namespace HaKafkaNet;

public interface IAutomationTraceProvider
{
    Task Trace(TraceEvent evt, AutomationMetaData meta, Func<Task> traceFunction);
    Task<IEnumerable<LogInfo>> GetErrorLogs();
    Task<IEnumerable<LogInfo>> GetGlobalLogs();
    Task<IEnumerable<LogInfo>> GetTrackerLogs();
    Task<IEnumerable<TraceData>> GetTraces(string automationKey);
    void AddLog(string renderedMessage, LogEventInfo logEvent, IDictionary<string, object> scopes);
}

internal class TraceLogProvider : IAutomationTraceProvider
{
    readonly IDistributedCache _cache;
    readonly ISystemObserver _observer;
    readonly ILogger<TraceLogProvider> _logger;

    const string CachKeyPrefix = "hkn.tracedata.";
    ConcurrentDictionary<string, SemaphoreSlim> _automationLocks = new();

    IReadOnlyDictionary<string, SemaphoreSlim> _globalLocks = new Dictionary<string, SemaphoreSlim>()
    {
        { _errorLogKey, new SemaphoreSlim(1) },
        { _globalLogKey, new SemaphoreSlim(1) },
        { _trackerLogKey, new SemaphoreSlim(1) }
    };

    static readonly HashSet<NLog.LogLevel> _errorLogLevels = new()
    {
        NLog.LogLevel.Warn, NLog.LogLevel.Error , NLog.LogLevel.Fatal
    };

    readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };

    const string
        _errorLogKey = "hkn.logs.error",
        _globalLogKey = "hkn.logs.global",
        _trackerLogKey = "hkn.logs.tracker",
        scopeTraceAutomationKey = "automationKey",
        scopeTraceAutomationEventType = "automationEventType",
        scopeTraceAutomationEventTime = "automationEventTime",
        scopeTrackerKey = "tracker_runtime";

    // first string: automationKey, second string: composite from event
    ConcurrentDictionary<string, ConcurrentDictionary<string, (TraceEvent evt, ConcurrentQueue<LogInfo> logQueue)>> _activeTraces = new();

    public TraceLogProvider(IDistributedCache cache, ISystemObserver observer, ILogger<TraceLogProvider> logger)
    {
        _cache = cache;
        _observer = observer;
        _logger = logger;
    }

    public void AddLog(string renderedMessage, LogEventInfo logEvent, IDictionary<string, object> scopes)
    {
        bool writtenToTrace = false;

        ExecptionInfo? exInfo = null;
        if (logEvent.Exception is not null)
        {
            exInfo = ExecptionInfo.Create(logEvent.Exception);
        }
        LogInfo info = new LogInfo()
        {
            LogLevel = logEvent.Level.ToString(),
            TimeStamp = logEvent.TimeStamp,
            Message = logEvent.Message,
            RenderedMessage = renderedMessage,
            Properties = logEvent.Properties.ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value),
            Scopes = scopes,
            Exception = exInfo
        };

        // see if the log should be part of a trace
        if (scopes is not null)
        {
            if (scopes.TryGetValue(scopeTrackerKey, out _))
            {
                _ = UpdateNonTraceLog(info, _trackerLogKey);
                writtenToTrace = true;
            }
            else if (scopes.TryGetValue(scopeTraceAutomationKey, out var autoKeyRaw) &&
                scopes.TryGetValue(scopeTraceAutomationEventType, out var evtType) &&
                scopes.TryGetValue(scopeTraceAutomationEventTime, out var evtTime))
            {
                // add to local
                string key = autoKeyRaw.ToString()!;

                var loc = _automationLocks.GetOrAdd(key, new SemaphoreSlim(1));
                loc.Wait();
                /*
                this locks all active traces for a specific automation
                potential improvment to be made for locking a specific trace instead of all traces
                this is only an issue where an automation has long running tasks
                */
                try
                {
                    if (_activeTraces.TryGetValue(key, out var automationRecords))
                    {
                        var instanceKey = MakeKey(evtType.ToString()!, evtTime.ToString()!);

                        // there is a chance here that an automation could have a long running task on another thread
                        // where the specific trace already completed
                        if (automationRecords.TryGetValue(instanceKey, out var traceInfo))
                        {
                            if (exInfo is not null)
                            {
                                traceInfo.evt.Exception = exInfo;
                            }

                            traceInfo.logQueue.Enqueue(info);
                            writtenToTrace = true;
                        }
                        else
                        {
                            // no trace found.
                        }
                    }
                }
                finally
                {
                    loc.Release();
                }
            }
        }

        //handle error log
        if (_errorLogLevels.Contains(logEvent.Level))
        {
            _ = UpdateNonTraceLog(info, _errorLogKey);
        }

        // handle non-trace
        if (!writtenToTrace)
        {
            _ = UpdateNonTraceLog(info, _globalLogKey);
        }
    }

    public Task Trace(TraceEvent evt, AutomationMetaData meta, Func<Task> traceFunction)
    {
        var data = AddTrace(evt);
        var scopeData = new Dictionary<string, object>()
        {
            // required for trace funcionality
            {scopeTraceAutomationKey, meta.GivenKey},
            {scopeTraceAutomationEventType, evt.EventType},
            {scopeTraceAutomationEventTime, evt.EventTime.ToString("O")},

            // added for benefit of user
            {"automationName", meta.Name},
            {"automationType", meta.UnderlyingType ?? "unknown type"}
        };

        if (evt.StateChange is not null)
        {
            scopeData["haContextId"] = evt.StateChange.New.Context?.ID ?? "unknown";
            scopeData["triggerEntityId"] = evt.StateChange.EntityId;
        }

        using (_logger.BeginScope(scopeData))
        {
            Task task;

            try
            {
                // this line will throw an exception
                // if the automation threw an excption
                task = traceFunction();
                // if the task is a non-awaited
                // this line could throw an exception
                // this will also catch all exceptions from Task.WhenAll()
                task.Wait();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Automation Fault");

                _observer.OnUnhandledException(meta, ex);
                evt.Exception = ExecptionInfo.Create(ex);
                //give any last logs a chance to make it in
                _ = Task.Delay(500).ContinueWith(t => WriteTraceToCache(new TraceData()
                {
                    TraceEvent = data.evt,
                    Logs = data.logQueue
                }));
                throw;
            }
            //give any last logs a chance to make it in
            _ = Task.Delay(500).ContinueWith(t => WriteTraceToCache(new TraceData()
            {
                TraceEvent = data.evt,
                Logs = data.logQueue
            }));
            return task;
        }
    }

    public async Task<IEnumerable<LogInfo>> GetErrorLogs()
    {
        var cached = await ReadLogsFromCache(_errorLogKey);
        return (cached ?? Enumerable.Empty<LogInfo>()).Reverse().ToArray();
    }

    public async Task<IEnumerable<LogInfo>> GetGlobalLogs()
    {
        var cached = await ReadLogsFromCache(_globalLogKey);
        return (cached ?? Enumerable.Empty<LogInfo>()).Reverse().ToArray();
    }

    public async Task<IEnumerable<LogInfo>> GetTrackerLogs()
    {
        var cached = await ReadLogsFromCache(_trackerLogKey);
        return (cached ?? Enumerable.Empty<LogInfo>()).Reverse().ToArray();
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
                        Logs = tuple.logQueue
                    };
            }
            var cached = await ReadTracesFromCache(CachKeyPrefix + automationKey);
            return (cached ?? Enumerable.Empty<TraceData>()).Union(locals ?? Enumerable.Empty<TraceData>()).Reverse().ToArray();
        }
        finally
        {
            loc.Release();
        }
    }

    private (TraceEvent evt, ConcurrentQueue<LogInfo> logQueue) AddTrace(TraceEvent evt)
    {
        try
        {
            _ = _activeTraces.TryAdd(evt.AutomationKey, new());

            var key = MakeKey(evt.EventType, evt.EventTime.ToString("O"));

            var retVal = _activeTraces[evt.AutomationKey].GetOrAdd(key, (evt, new ConcurrentQueue<LogInfo>()));

            return retVal;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task UpdateNonTraceLog(LogInfo logInfo, string key)
    {
        await _globalLocks[key].WaitAsync();
        try
        {
            var existing = await ReadLogsFromCache(key);
            Queue<LogInfo>? q = null;
            if (existing is not null)
            {
                q = new Queue<LogInfo>(existing);
                while (q.Count >= 200)
                {
                    q.Dequeue();
                }
            }
            q ??= new();
            q.Enqueue(logInfo);

            var value = JsonSerializer.SerializeToUtf8Bytes(q.ToArray());
            await _cache.SetAsync(key, value, _cacheOptions);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("error writing error log to cache");
            System.Console.WriteLine(ex.Message);
        }
        finally
        {
            _globalLocks[key].Release();
        }
    }

    private async Task WriteTraceToCache(TraceData traceData)
    {
        var key = CachKeyPrefix + traceData.TraceEvent.AutomationKey;

        var loc = _automationLocks.GetOrAdd(traceData.TraceEvent.AutomationKey, new SemaphoreSlim(1));

        await loc.WaitAsync();
        try
        {
            var existing = await ReadTracesFromCache(key);
            Queue<TraceData>? q = null;
            if (existing is not null)
            {
                q = new Queue<TraceData>(existing);
                while (q.Count >= 50)
                {
                    q.Dequeue();
                }
            }
            q ??= new();
            q.Enqueue(traceData);
            var value = JsonSerializer.SerializeToUtf8Bytes(q.ToArray());
            await _cache.SetAsync(key, value, _cacheOptions);

            // removed from active
            var instanceKey = MakeKey(traceData.TraceEvent.EventType, traceData.TraceEvent.EventTime.ToString("O"));
            _activeTraces[traceData.TraceEvent.AutomationKey].TryRemove(instanceKey, out _);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("error writing trace to cache");
            System.Console.WriteLine(ex.Message);
        }
        finally
        {
            loc.Release();
        }
    }

    private async Task<IEnumerable<LogInfo>?> ReadLogsFromCache(string key)
    {
        var cachedBytes = await _cache.GetAsync(key);
        LogInfo[]? cachedData = null;
        if (cachedBytes is not null)
        {
            try
            {
                cachedData = JsonSerializer.Deserialize<LogInfo[]>(cachedBytes)!;
            }
            catch (System.Exception ex)
            {
                // don't cause an infinite loop by logging
                Console.WriteLine("could not deserialize error logs from cache");
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        return cachedData;
    }

    private async Task<IEnumerable<TraceData>?> ReadTracesFromCache(string prefixedAutomationKey)
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
                // don't cause an infinite loop by logging
                Console.WriteLine("could not deserialize trace data from cache");
                Console.WriteLine(ex.Message);
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
