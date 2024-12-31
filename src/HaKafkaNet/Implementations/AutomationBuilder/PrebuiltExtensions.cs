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
                var now = info.TimeProvider.GetLocalNow();
                    
                var executeTime = new DateTimeOffset(now.Date.Add(helperTime));

                if(executeTime < now)
                {
                    executeTime += TimeSpan.FromDays(1);
                }
                return Task.FromResult<DateTimeOffset?>(executeTime);;
            });
        
        return info;
    }
}
