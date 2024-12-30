using System.Text;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class AutomationWrapper : IAutomationWrapper
{
    readonly IAutomation _auto;
    readonly IAutomationTraceProvider _trace;
    private readonly TimeProvider _timeProvider;
    readonly IAutomationExecutor _executor;

    AutomationMetaData _meta;
    IEnumerable<string> _triggers;
   
    public EventTiming EventTimings { get => _auto.EventTimings; }
    public bool IsActive { get => _auto.IsActive; }

    public IAutomationBase WrappedAutomation
    {
        get => _auto;
    }

    public AutomationWrapper(IAutomation automation, IAutomationTraceProvider traceProvider, TimeProvider timeProvider, string source, IExecutorFactory? executorFactory = null)
    {
        _auto = automation;
        _trace = traceProvider;
        _timeProvider = timeProvider;
        
        bool hasTriggerError = false;
        try
        {
            _triggers = automation.TriggerEntityIds();
        }
        catch (System.Exception)
        {
            hasTriggerError = true;
            _triggers = Enumerable.Empty<string>();
        }

        _meta = GetOrMakeMeta(source);
        _meta.UserTriggerError = hasTriggerError;

        _executor = executorFactory?.GetExecutor(_meta.Mode) ?? new ParallelExecutor();    
    }

    AutomationMetaData GetOrMakeMeta(string source)
    {
        AutomationMetaData meta;

        var auto = _auto is IAutomationWrapperBase wrapperBase ? wrapperBase.GetRoot() : _auto;
        
        IAutomationMeta? autoImplementingMeta = _auto as IAutomationMeta;
        
        IAutomationBase target = _auto;
        while(autoImplementingMeta is null && target is IAutomationWrapperBase targetWrapper)
        {
            target = targetWrapper.WrappedAutomation;
            autoImplementingMeta = target as IAutomationMeta;
        }

        var underlyingType = auto.GetType();

        if(autoImplementingMeta is not null)
        {
            // use it's data
            try
            {
                // user implemented, could throw an exception
                meta = autoImplementingMeta.GetMetaData();
            }
            catch
            {
                meta = new AutomationMetaData()
                {
                    Name = "unknown",
                    Description = $"GetMetaData threw exception from automation created via {source}",
                    Enabled = true,
                    KeyRequest = GenerateKey(source, underlyingType.Name),
                    UserMetaError = true
                };
            }
        }
        else
        {
            // not a wrapper, likely user implemented
            // make the meta 
            meta = new AutomationMetaData()
            {
                Name = underlyingType.Name,
                Description = underlyingType.FullName,
                Enabled = true,
                KeyRequest = GenerateKey(source, underlyingType.Name),
            };
        }

        meta.UnderlyingType = underlyingType.PrettyPrint();

        if (string.IsNullOrEmpty(meta.KeyRequest))
        {
            meta.KeyRequest = meta.Name;
        }

        meta.Source = source;

        return meta;
    }

    public AutomationMetaData GetMetaData()
    {
        return _meta;
    }

    /// <summary>
    /// this one is async because we want to capture that log scope
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        this._meta.LastTriggered = _timeProvider.GetLocalNow().LocalDateTime;
        TraceEvent evt = new()
        {
            EventType = "Trigger",
            AutomationKey = this._meta.GivenKey,
            EventTime = _timeProvider.GetLocalNow().LocalDateTime,
            StateChange = stateChange,
        };

        return _trace.Trace(evt, _meta, 
            () => _executor.Execute(
                ct => _auto.Execute(stateChange, ct), 
            cancellationToken));
    }

    private string GenerateKey(string source, string name)
    {
        var triggers = _triggers.Any() ? _triggers.Aggregate((s1,s2) => $"{s1}-{s2}") : string.Empty;
        return $"{source}-{name}-{triggers}";
    }    

    public IEnumerable<string> TriggerEntityIds() => _triggers;
}

static class PrettyPrintTypeNameExtensions
{
    public static string PrettyPrint(this Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }
        StringBuilder builder = new();
        builder.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
        builder.Append('<');
        builder.Append(string.Join(", ", type.GetGenericArguments().Select(t => t.PrettyPrint())));
        builder.Append('>');

        return builder.ToString();
    }
}

