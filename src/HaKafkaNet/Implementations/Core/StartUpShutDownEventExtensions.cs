using System;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

public static class StartUpShutDownEventExtensions
{
    static SemaphoreSlim _sem = new (1,1);
    static CancellationTokenSource _source = new();

    public static async Task ShutdownStartupActionsAsync(this StartUpShutDownEvent evt, Func<Task> shutdown, Func<Task> startup, int timeout, ILogger? logger = default)
    {
        logger?.LogInformation("Home Assistant {HaEvent}", evt.Event);

        if (evt.Event == "shutdown")
        {
            var token = await ExecuteShutdownAsync(shutdown, logger);
            _ = ExecuteStartupFallbackAsync(startup, timeout, token, logger);
        }
        else
        {
            await ExecuteStartupNowAsync(startup, logger);
        }
    }

    public static async Task ShutdownStartupActions(this StartUpShutDownEvent evt, Action shutdown, Action startup, int timeout, ILogger? logger = default)
    {
        logger?.LogInformation("Home Assistant {HaEvent}", evt.Event);

        if (evt.Event == "shutdown")
        {
            var token = await ExecuteShutdown(shutdown, logger);
            _ = ExecuteStartupFallback(startup, timeout, token, logger);
        }
        else
        {
            await ExecuteStartupNow(startup, logger);
        }       
    }

    private static async Task<CancellationToken> ExecuteShutdown(Action shutDownAction, ILogger? logger)
    {
        try
        {
            shutDownAction();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error in shutdown action");
        }
        
        logger?.LogDebug("Shutdown action executed");
        await _sem.WaitAsync();
        try
        {
            _source = new();
            return _source.Token;
        }
        finally
        {
            _sem.Release();
        }
    }

    private static async Task<CancellationToken> ExecuteShutdownAsync(Func<Task> shutDownAction, ILogger? logger)
    {
        try
        {
            await shutDownAction();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error in shutdown action");
        }
        
        logger?.LogDebug("Shutdown action executed");
        await _sem.WaitAsync();
        try
        {
            _source = new();
            return _source.Token;
        }
        finally
        {
            _sem.Release();
        }
    }

    private static async Task ExecuteStartupFallbackAsync(Func<Task> startupAction, int timeout, CancellationToken token, ILogger? logger)
    {
        try
        {
            await Task.Delay(timeout, token);
        }
        catch (TaskCanceledException)
        {
            logger?.LogDebug("startup timeout canceled");
            return;
        }

        await startupAction();
        logger?.LogWarning("Home Assistant did not report it had restarted within timeout. Startup action executed. You may need to adjust your timeout");
    }

    private static async Task ExecuteStartupFallback(Action startupAction, int timeout, CancellationToken token, ILogger? logger)
    {
        try
        {
            await Task.Delay(timeout, token);
        }
        catch (TaskCanceledException)
        {
            logger?.LogDebug("startup timeout canceled");
            return;
        }
        try
        {
            startupAction();
        }
        catch (System.Exception ex)
        {
            logger?.LogError(ex, "failure in startup action");
        }
        
        logger?.LogWarning("Home Assistant did not report it had restarted within timeout. Startup action executed. You may need to adjust your timeout");
    }

    private static async Task ExecuteStartupNowAsync(Func<Task> startupAction, ILogger? logger)
    {
        
        await _sem.WaitAsync();
        try
        {
            _source.Cancel();
            await startupAction();
            logger?.LogDebug("Startup action executed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failure in startup action");
        }        
        finally
        {
            _sem.Release();
        }
    }
    
    private static async Task ExecuteStartupNow(Action startupAction, ILogger? logger)
    {
        await _sem.WaitAsync();
        try
        {
            _source.Cancel();
            startupAction();
            logger?.LogDebug("Startup action executed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failure in startup action");
        }
        finally
        {
            _sem.Release();
        }        
    }
}

