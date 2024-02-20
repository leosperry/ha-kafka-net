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

    /// <summary>
    /// Use when you want to specify both state and attribute types
    /// </summary>
    /// <typeparam name="Tstate">The State Type</typeparam>
    /// <typeparam name="Tatt">The Type for Attributes</typeparam>
    /// <param name="change"></param>
    /// <returns></returns>
    public static HaEntityStateChange<HaEntityState<Tstate,Tatt>> ToTyped<Tstate, Tatt>(this HaEntityStateChange change) =>  Transform<HaEntityState<Tstate,Tatt>, Tstate, Tatt>(change);

    /// <summary>
    /// Use when you want to only specify the Ttate type. Attributes will be JsonElement
    /// </summary>
    /// <typeparam name="Tstate">The State Type</typeparam>
    /// <param name="change"></param>
    /// <returns></returns>
    public static HaEntityStateChange<HaEntityState<Tstate,JsonElement>> ToStateTyped<Tstate>(this HaEntityStateChange change) =>  Transform<HaEntityState<Tstate,JsonElement>, Tstate, JsonElement>(change);

    /// <summary>
    /// Use when you want to only specify the Attributes type. State will be string
    /// </summary>
    /// <typeparam name="Tatt">The Type for Attributes</typeparam>
    /// <param name="change"></param>
    /// <returns></returns>
    public static HaEntityStateChange<HaEntityState<string,Tatt>> ToAttributeTyped<Tatt>(this HaEntityStateChange change) =>  Transform<HaEntityState<string,Tatt>, string, Tatt>(change);


    // specialized types
    public static HaEntityStateChange<HaEntityState<OnOff, JsonElement>> ToOnOff(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, JsonElement>, OnOff, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<OnOff, T>> ToOnOff<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<OnOff, T>, OnOff, T>(change);
    
    public static HaEntityStateChange<HaEntityState<int?, JsonElement>> ToIntTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<int?, JsonElement>, int?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<int?, T>> ToIntTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<int?, T>, int?, T>(change);
    
    public static HaEntityStateChange<HaEntityState<double?, JsonElement>> ToDoubleTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<double?, JsonElement>, double?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<double?, T>> ToDoubleTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<double?, T>, double?, T>(change);

    public static HaEntityStateChange<HaEntityState<float?, JsonElement>> ToFloatTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<float?, JsonElement>, float?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<float?, T>> ToFloatTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<float?, T>, float?, T>(change);

    public static HaEntityStateChange<HaEntityState<DateTime?, JsonElement>> ToDateTimeTyped(this HaEntityStateChange change) =>  Transform<HaEntityState<DateTime?, JsonElement>, DateTime?, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<DateTime?, T>> ToDateTimeTyped<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<DateTime?, T>, DateTime?, T>(change);
    
    public static HaEntityStateChange<HaEntityState<BatteryState, JsonElement>> ToBatteryState(this HaEntityStateChange change) =>  Transform<HaEntityState<BatteryState, JsonElement>, BatteryState, JsonElement>(change);
    public static HaEntityStateChange<HaEntityState<BatteryState, T>> ToBatteryState<T>(this HaEntityStateChange change) =>  Transform<HaEntityState<BatteryState, T>, BatteryState, T>(change);
    
    public static HaEntityStateChange<HaEntityState<SunState, SunAttributes>> ToSun(this HaEntityStateChange change) =>  Transform<HaEntityState<SunState, SunAttributes>, SunState, SunAttributes>(change);
    
    public static HaEntityStateChange<HaEntityState<OnOff, LightModel>> ToLight(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, LightModel>, OnOff, LightModel>(change);
    public static HaEntityStateChange<HaEntityState<OnOff, ColoredLightModel>> ToColoredLight(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, ColoredLightModel>, OnOff, ColoredLightModel>(change);
    
    public static HaEntityStateChange<HaEntityState<DateTime?, SceneControllerEvent>> ToSceneControllerEvent(this HaEntityStateChange change) => Transform<HaEntityState<DateTime?, SceneControllerEvent>, DateTime?, SceneControllerEvent>(change);
}
