using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.Xml;
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

}

public static class StateChangeExtensions
{
    static HaEntityStateChange<T> Transform<T, Tstate, Tatt>(HaEntityStateChange change)
        where T : HaEntityState<Tstate, Tatt>
    {
        return new HaEntityStateChange<T>
        {
            EventTiming = change.EventTiming,
            EntityId = change.EntityId,
            New = (T)change.New,
            Old = change.Old is null? null : (T?)change.Old
        };
    }    

    public static HaEntityStateChange<HaEntityState<Tstate,Tatt>> ToTyped<Tstate, Tatt>(this HaEntityStateChange change) =>  Transform<HaEntityState<Tstate,Tatt>, Tstate, Tatt>(change);
    public static HaEntityStateChange<HaEntityState<Tstate,JsonElement>> ToStateTyped<Tstate>(this HaEntityStateChange change) =>  Transform<HaEntityState<Tstate,JsonElement>, Tstate, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<string,Tatt>> ToAttributeTyped<Tatt>(this HaEntityStateChange change) =>  Transform<HaEntityState<string,Tatt>, string, Tatt>(change);
    public static HaEntityStateChange<HaEntityState<OnOff, JsonElement>> ToOnOff(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, JsonElement>, OnOff, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<OnOff, T>> ToOnOff<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<OnOff, T>, OnOff, T>(change);
    public static HaEntityStateChange<HaEntityState<int?, JsonElement>> ToIntTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<int?, JsonElement>, int?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<int?, T>> ToIntTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<int?, T>, int?, T>(change);
    public static HaEntityStateChange<HaEntityState<double?, JsonElement>> ToDoubleTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<double?, JsonElement>, double?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<double?, T>> ToDoubleTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<double?, T>, double?, T>(change);
    public static HaEntityStateChange<HaEntityState<DateTime?, JsonElement>> ToDateTimeTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<DateTime?, JsonElement>, DateTime?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<DateTime?, T>> ToDateTimeTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<DateTime?, T>, DateTime?, T>(change);
    public static HaEntityStateChange<HaEntityState<BatteryState, JsonElement>> ToBatteryState(this HaEntityStateChange change) =>  Transform<HaEntityState<BatteryState, JsonElement>, BatteryState, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<BatteryState, T>> ToBatteryState<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<BatteryState, T>, BatteryState, T>(change);
    public static HaEntityStateChange<HaEntityState<DateTime?, SceneControllerEvent>> ToSceneControllerEvent(this HaEntityStateChange change) => Transform<HaEntityState<DateTime?, SceneControllerEvent>, DateTime?, SceneControllerEvent>(change);
    
    //public static HaEntityStateChange<SunModel> ToSun(this HaEntityStateChange change) =>  change.Transform<SunModel, SunState, SunAttributes>();
}
