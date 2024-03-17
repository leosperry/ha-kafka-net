using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace HaKafkaNet
{
    [Target("HknTarget")]
    public sealed class HknLogTarget: TargetWithContext
    {
        private IAutomationTraceProvider _trace;
        private readonly Layout _layout;

        public HknLogTarget(IAutomationTraceProvider traceProvider)
        {
            this._trace = traceProvider;
            base.IncludeScopeNested = true;
            base.IncludeScopeProperties = true;
            this._layout = new SimpleLayout("${longdate} | ${logger} | ${message}");
            this.Name = "HaKafkaNet Target";
        }

        protected override void Write(LogEventInfo logEvent) 
        {            
            var scoped = this.GetScopeContextProperties(logEvent);        
            if (scoped is not null && scoped.ContainsKey("automationKey"))
            {
                var rendered = base.RenderLogEvent(_layout, logEvent);
                _trace.AddLog(rendered, logEvent, scoped);
            } 
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            base.Write(logEvent);
        }
    }
}