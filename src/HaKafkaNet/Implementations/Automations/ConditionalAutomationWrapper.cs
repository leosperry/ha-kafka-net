﻿
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class ConditionalAutomationWrapper : IAutomation, IAutomationMeta
{
    AutomationMetaData _meta;
    private readonly IConditionalAutomation _automation;
    internal IConditionalAutomation WrappedConditional
    {
        get => _automation;
    }

    private readonly ISystemObserver _observer;
    private readonly ILogger _logger;

    private readonly object lockObj = new{};
    private CancellationTokenSource? _cts;

    public ConditionalAutomationWrapper(IConditionalAutomation automation, ISystemObserver observer, ILogger logger)
    {
        this._automation = automation;
        this._observer = observer;
        _logger = logger;

        if (automation is IAutomationMeta metaAuto)
        {
            _meta = metaAuto.GetMetaData();
            _meta.UnderlyingType = _automation.GetType().Name;
        }
        else
        {
            _meta = new AutomationMetaData()
            {
                Name = _automation.GetType().Name,
                Description = _automation.GetType().Name,
                Enabled = true,
                Id = Guid.NewGuid(),
                UnderlyingType = _automation.GetType().Name
            };
        }
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
        if (_automation.For == TimeSpan.Zero)
        {
            //execute immediately
            return _automation.Execute(cancellationToken)
                .ContinueWith(t =>{
                    if (t.IsFaulted)
                    {
                        _observer.OnUnhandledException(this._meta, t.Exception);
                    }
                });
        }
        else if (_cts is null)
        {
            // if _cts is not null, we're already running
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
                                if (t.IsFaulted)
                                {
                                    _observer.OnUnhandledException(this._meta, t.Exception);
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

    internal Task StopIfRunning()
    {
        if (_cts is not null)
        {
            lock(lockObj)
            {
                if (_cts is not null)
                {
                    _logger.LogInformation("Canceling {automation}", _automation.GetType().Name);
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

    public AutomationMetaData GetMetaData() => _meta;
}
