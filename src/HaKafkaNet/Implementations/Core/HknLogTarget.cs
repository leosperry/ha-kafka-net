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
            var rendered = base.RenderLogEvent(_layout, logEvent);
            _trace.AddLog(rendered, logEvent, scoped);
        }
    }
}