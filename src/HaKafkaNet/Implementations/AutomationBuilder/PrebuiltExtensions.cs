using System;
using System.Text.Json;

namespace HaKafkaNet.Implementations.AutomationBuilder;

/// <summary>
/// 
/// </summary>
public static class PrebuiltExtensions
{
    /// <summary>
    /// Provides a base for scheduling an automation daily based on a Time Only Helper entity. 
    /// You must call WithExecution
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="helperId">ID of your Time Only Helper entity</param>
    /// <returns></returns>
    public static TypedSchedulableAutomationBuildingInfo<TimeSpan, JsonElement> 
        CreateDailyFromTimeHelper(this IAutomationBuilder builder, string helperId)
    {
        var info = builder.CreateSchedulable<TimeSpan>()
            .WithTriggers(helperId)
            .MakeActive()
            .SetReschedulable();

        info
            .GetNextScheduled((sc, ct) => {
                var helperTime = sc.New.State;
                var now = info.TimeProvider.GetLocalNow().LocalDateTime;
                    
                var executeTime = now.Date + helperTime;

                if(executeTime < now)
                {
                    executeTime += TimeSpan.FromDays(1);
                }
                return Task.FromResult<DateTimeOffset?>(executeTime);
            });
        
        return info;
    }

    public static TypedSchedulableAutomationBuildingInfo<DateTime, JsonElement> CreateNaggingReminder(this IAutomationBuilder builder, string entityId, TimeSpan nagAfter, TimeSpan nagInterval, 
        Func<HaEntityStateChange<HaEntityState<DateTime, JsonElement>>, CancellationToken, Task>? onClear = null)
    {
        bool isStartup = true;
        var info = builder.CreateSchedulable<DateTime>()
            .WithTriggers(entityId)
            .MakeActive()
            .SetReschedulable();

        info
            .GetNextScheduled((sc, ct) => {
                bool wasStartup = isStartup;
                isStartup = false;
                DateTimeOffset? retVal = null;

                var lastChanged = sc.New.State;
                var now = info.TimeProvider.GetLocalNow().LocalDateTime;

                var timeSinceLastChanged = now - lastChanged;
                
                if (timeSinceLastChanged >= nagAfter)
                {
                    // we need to send an alert at some point
                    if (wasStartup)
                    {
                        retVal = info.TimeProvider.GetLocalNow();
                    }
                    else if(sc.EventTiming == EventTiming.PostStartup && sc.Old?.State != sc.New.State)
                    {
                        // it was changed, but still longer than nagAfter
                        retVal = info.TimeProvider.GetLocalNow().AddSeconds(10); // add 10 second delay just in case
                    }
                    else
                    {
                        // be a nag
                        retVal = info.TimeProvider.GetLocalNow().Add(nagInterval);
                    }
                }
                else
                {
                    try
                    {
                        onClear?.Invoke(sc, ct);
                    }
                    catch
                    {
                        //todo: do better
                        System.Console.WriteLine("error invoking clearing action");
                    }
                    
                    retVal = lastChanged.Add(nagAfter);
                }

                return Task.FromResult(retVal);
            });
        return info;
    }
}
