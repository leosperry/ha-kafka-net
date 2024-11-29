using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IExecutorFactory
{
    IAutomationExecutor GetExecutor(AutomationMode mode);
}

class ExecutorFactory : IExecutorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExecutorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAutomationExecutor GetExecutor(AutomationMode mode)
    {
        return _serviceProvider.GetRequiredKeyedService<IAutomationExecutor>(mode);
    }
}

internal interface IAutomationExecutor
{
    Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
}

class ParallelExecutor : IAutomationExecutor
{
    public Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        return action(cancellationToken);
    }
}

class QueuedExecutor : IAutomationExecutor
{
    SemaphoreSlim _sem = new(1);

    public async Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await _sem.WaitAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
        }
        finally
        {
            _sem.Release();
        }
    }
}

class RestartExecutor : IAutomationExecutor 
{
    CancellationTokenSource? _source = null;
    SemaphoreSlim _sem = new(1);
    public async Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        CancellationToken token;
        await _sem.WaitAsync(cancellationToken);
        try
        {
            if (_source is not null)
            {
                await _source.CancelAsync();
                _source.Dispose();
                _source = null;
            }
            _source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            token = _source.Token;
        }
        finally
        {
            _sem.Release();
        }

        await action(token);

        await _sem.WaitAsync(cancellationToken);
        try
        {
            _source = null;
        }
        finally
        {
            _sem.Release();
        }
    }
}

class SingleExecutor : IAutomationExecutor
{
    bool _running = false;
    ILogger<SingleExecutor> _logger;

    public SingleExecutor(ILogger<SingleExecutor> logger)
    {
        _logger = logger;
    }

    public async Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (_running)
        {
            _logger.LogWarning("Automation was already executing with automation mode single. Returning without execution");
            return;
        }
        try
        {
            _running = true;
            await action(cancellationToken);
        }
        finally
        {
            _running = false;
        }
    }
}

class SmartExecutor : IAutomationExecutor
{
    bool _running = false;
    ILogger<SmartExecutor> _logger;

    SemaphoreSlim _sem = new(1);
    Func<CancellationToken, Task>? _lastAction = null;

    public SmartExecutor(ILogger<SmartExecutor> logger)
    {
        _logger = logger;
    }
    
    public async Task Execute(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (_running)
        {
            await _sem.WaitAsync(cancellationToken);
            try
            {
                _lastAction = action;
            }
            finally
            {
                _sem.Release();
            }
            return;
        }
        try
        {
            _running = true;
            await action(cancellationToken);
            Func<CancellationToken, Task> act;
        
            while (_lastAction is not null)
            {

                await _sem.WaitAsync(cancellationToken);
                try
                {
                    act = _lastAction;
                    _lastAction = null;
                }
                finally
                {
                    _logger.LogInformation("running last action");
                    _sem.Release();
                }

                await act(cancellationToken);
            }
        }
        finally
        {
            _running = false;
        }
    }
}