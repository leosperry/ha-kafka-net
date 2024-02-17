using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Routing.Internal;

namespace HaKafkaNet;

public record HaEntityStateChange<T>
{
    public required EventTiming EventTiming { get; set;}

    /// <summary>
    /// Id of the entity which changed state
    /// </summary>
    public required string EntityId { get; set; }
    
    /// <summary>
    /// The most recent item from the cach
    /// </summary>
    public T? Old { get ; set; }

    /// <summary>
    /// new state of the entity
    /// </summary>
    public required T New { get ; set; }
}

public record HaEntityStateChange : HaEntityStateChange<HaEntityState>
{
    public HaEntityStateChange<T> Transform<T,Tstate,Tatt>() 
        where T : HaEntityState<Tstate, Tatt>
    {
        return new HaEntityStateChange<T>
        {
            EventTiming = this.EventTiming,
            EntityId = this.EntityId,
            New = (T)this.New,
            Old = this.Old is null? null : (T?)this.Old
        };
    }
}

public static class StateChangeExtensions
{
    public static HaEntityStateChange<HaEntityState<Tstate,Tatt>> ToTyped<Tstate, Tatt>(this HaEntityStateChange change) =>  change.Transform<HaEntityState<Tstate,Tatt>, Tstate, Tatt>();
    public static HaEntityStateChange<HaEntityState<Tstate,JsonElement>> ToStateTyped<Tstate>(this HaEntityStateChange change) =>  change.Transform<HaEntityState<Tstate,JsonElement>, Tstate, JsonElement>();
    public static HaEntityStateChange<HaEntityState<string,Tatt>> ToAttributeTyped<Tatt>(this HaEntityStateChange change) =>  change.Transform<HaEntityState<string,Tatt>, string, Tatt>();
    public static HaEntityStateChange<OnOffEnity> ToOnOff(this HaEntityStateChange change) =>  change.Transform<OnOffEnity, OnOff, JsonElement>()!;
    public static HaEntityStateChange<OnOffEnity<T>> ToOnOff<T>(this HaEntityStateChange change) =>  change.Transform<OnOffEnity<T>, OnOff, T>();
    public static HaEntityStateChange<IntegerEnity> ToIntTyped(this HaEntityStateChange change) =>  change.Transform<IntegerEnity, int, JsonElement>();
    public static HaEntityStateChange<IntegerEnity<T>> ToIntTyped<T>(this HaEntityStateChange change) =>  change.Transform<IntegerEnity<T>, int, T>();
    public static HaEntityStateChange<DoubleEnity> ToDoubleTyped(this HaEntityStateChange change) =>  change.Transform<DoubleEnity, double, JsonElement>();
    public static HaEntityStateChange<DoubleEnity<T>> ToDoubleTyped<T>(this HaEntityStateChange change) =>  change.Transform<DoubleEnity<T>, double, T>();
    public static HaEntityStateChange<DateTimeEnity> ToDateTimeTyped(this HaEntityStateChange change) =>  change.Transform<DateTimeEnity, DateTime, JsonElement>();
    public static HaEntityStateChange<DateTimeEnity<T>> ToDateTimeTyped<T>(this HaEntityStateChange change) =>  change.Transform<DateTimeEnity<T>, DateTime, T>();
    public static HaEntityStateChange<BatteryStateEntity> ToBatteryState(this HaEntityStateChange change) =>  change.Transform<BatteryStateEntity, BatteryState, JsonElement>();
    public static HaEntityStateChange<BatteryStateEntity<T>> ToBatteryState<T>(this HaEntityStateChange change) =>  change.Transform<BatteryStateEntity<T>, BatteryState, T>();
    public static HaEntityStateChange<SunModel> ToSun(this HaEntityStateChange change) =>  change.Transform<SunModel, SunState, SunAttributes>();
}
