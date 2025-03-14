﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NLog;

namespace HaKafkaNet;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IAutomationTraceProvider
{
    Task Trace(TraceEvent evt, AutomationMetaData meta, Func<Task> traceFunction);
    Task<IEnumerable<LogInfo>> GetErrorLogs();
    Task<IEnumerable<LogInfo>> GetGlobalLogs();
    Task<IEnumerable<LogInfo>> GetTrackerLogs();
    Task<IEnumerable<TraceData>> GetTraces(string automationKey);
    void AddLog(string renderedMessage, LogEventInfo logEvent, IDictionary<string, object> scopes);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


internal class TraceLogProvider : IAutomationTraceProvider
{
    readonly IDistributedCache _cache;
    readonly ISystemObserver _observer;
    readonly ILogger<TraceLogProvider> _logger;

    const string CacheKeyPrefix = "hkn.tracedata.";
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
        scopeTrackerKey = "tracker_runtime",
        automation_fault = "Automation Fault";

    // first string: automationKey, second string: composite from event
    ConcurrentDictionary<string, ConcurrentDictionary<string, (TraceEvent evt, ConcurrentQueue<LogInfo> logQueue)>> _activeTraces = new();

    static ActivitySource _activitySource = new ActivitySource(Telemetry.TraceAutomationName);
    UpDownCounter<int> _activeTraceCounter;
    Counter<int> _traceCounter;


    public TraceLogProvider(IDistributedCache cache, ISystemObserver observer, ILogger<TraceLogProvider> logger)
    {
        _cache = cache;
        _observer = observer;
        _logger = logger;

        Meter m = new Meter(Telemetry.MeterTracesName);
        _activeTraceCounter = m.CreateUpDownCounter<int>("ha_kafka_net.active_traces_total");
        _traceCounter = m.CreateCounter<int>("ha_kafka_net.trace_count");
    }

    public void AddLog(string renderedMessage, LogEventInfo logEvent, IDictionary<string, object> scopes)
    {
        bool writtenToTrace = false;

        ExceptionInfo? exInfo = null;
        if (logEvent.Exception is not null)
        {
            exInfo = ExceptionInfo.Create(logEvent.Exception);
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
                potential improvement to be made for locking a specific trace instead of all traces
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

    public async Task Trace(TraceEvent evt, AutomationMetaData meta, Func<Task> traceFunction)
    {
        var scopeData = new Dictionary<string, object?>()
        {
            // required for trace functionality
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

        _traceCounter.Add(1, 
            new KeyValuePair<string, object?>("given_key", meta.GivenKey),
            new KeyValuePair<string, object?>("event_type", evt.EventType),
            new KeyValuePair<string, object?>("trigger_entity_id", evt.StateChange?.New.EntityId ?? "na")
            );

        _activeTraceCounter.Add(1);
        try
        {
            var data = AddTrace(evt);
            
            using (_logger.BeginScope(scopeData))
            using (var act = _activitySource.StartActivity($"ha_kafka_net.automation_trace"))
            {
                if(act is not null)
                {
                    act.AddTag("given_key", meta.GivenKey);
                    act.AddTag("event_type", evt.EventType);
                    act.AddBaggage("ha_context_id", evt.StateChange?.New.Context?.ID ?? "na");
                    act.AddTag("trigger_entity_id", evt.StateChange?.New.EntityId ?? "na");
                }
                Task task;
                _logger.LogDebug("Starting automation trace for {automationKey} with event {eventType}", meta.GivenKey, evt.EventType);
                try
                {
                    // this line will throw an exception
                    // if the automation threw an exception
                    task = traceFunction();
                    // if the task is not-awaited
                    // this line could throw an exception
                    // this will also catch all exceptions from Task.WhenAll()
                    task.Wait();
                    await task;
                    _logger.LogDebug("Completed automation trace for {automationKey} with event {eventType}", meta.GivenKey, evt.EventType);
                }
                catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException || 
                    (ex is AggregateException agg && agg.InnerExceptions.Any(e => e is TaskCanceledException || e is OperationCanceledException)))
                {
                    _logger.LogDebug("Automation canceled for {automationKey} with event {eventType}", meta.GivenKey, evt.EventType);
                }
                catch (Exception ex)
                {
                    act?.AddEvent(new ActivityEvent(automation_fault));
                    _logger.LogError(ex, automation_fault);

                    _observer.OnUnhandledException(meta, ex);
                    evt.Exception = ExceptionInfo.Create(ex);
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
            }
        }
        finally
        {
            _activeTraceCounter.Add(-1);
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
            var cached = await ReadTracesFromCache(CacheKeyPrefix + automationKey);
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
        var key = CacheKeyPrefix + traceData.TraceEvent.AutomationKey;

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
