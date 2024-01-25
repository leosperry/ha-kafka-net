
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class ConditionalAutomationWrapper : IAutomation
{
    private readonly IConditionalAutomation _automation;
    internal IConditionalAutomation WrappedConditional
    {
        get => _automation;
    }

    private readonly ILogger<ConditionalAutomationWrapper> _logger;

    private readonly object lockObj = new{};
    private CancellationTokenSource? _cts;

    public ConditionalAutomationWrapper(IConditionalAutomation automation, ILogger<ConditionalAutomationWrapper> logger)
    {
        this._automation = automation;
        _logger = logger;
    }

    public string Name
    {
        get => _automation.Name;
    }

    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return _automation.ContinuesToBeTrue(stateChange, cancellationToken)
            .ContinueWith(stillTrueResult => {
                return stillTrueResult.Result switch
                {
                    true => StartIfNotStarted(cancellationToken),   //if running do nothing,    otherwise start
                    false => StopIfRunning()                        //if running cancel         otherwise do nothing
                };
            }, cancellationToken, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current);
    }

    private Task StartIfNotStarted(CancellationToken cancellationToken)
    {
        if (_cts is null)
        {
            lock (lockObj)
            {
                if (_cts is null)
                {
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    return Task.Delay(_automation.For, _cts.Token).ContinueWith(t => {
                        using (_logger!.BeginScope("Start [{automationType}]", _automation.GetType().Name))
                        {
                            return _automation.Execute(_cts.Token)
                            .ContinueWith(t => {
                                //no longer need the ability to cancel because we've just executed
                                lock(lockObj)
                                {
                                    _cts.Dispose();
                                    _cts = null;
                                }
                            });
                        }
                    }, _cts.Token, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Current);
                }
            }
        }
        // if we haven't returned by now, another thread has already stared
        return Task.CompletedTask;
    }

    private Task StopIfRunning()
    {
        _logger.LogInformation("Canceling {automation}", _automation.GetType().Name);
        if (_cts is not null)
        {
            lock(lockObj)
            {
                if (_cts is not null)
                {
                    try
                    {
                        _cts.Cancel();
                    }
                    finally
                    {
                        _cts.Dispose();
                        _cts = null;
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _automation.TriggerEntityIds();
    }
}
