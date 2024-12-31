#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json;

namespace HaKafkaNet;

public static class StateChangeExtensions
{
    static HaEntityStateChange<T> Transform<T, Tstate, Tatt>(HaEntityStateChange change)
        where T : HaEntityState<Tstate, Tatt>
    {
        T? old;
        if(change.Old is null)
        {
            old = null; 
        }
        else
        {
            try
            {
                old = (T?)change.Old;
            }
            catch
            {
                old = null;
            }
        }
        
        return new HaEntityStateChange<T>
        {
            EventTiming = change.EventTiming,
            EntityId = change.EntityId,
            New = (T)change.New,
            Old = old
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
    public static HaEntityStateChange<HaEntityState<OnOff, ColorLightModel>> ToColorLight(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, ColorLightModel>, OnOff, ColorLightModel>(change);
    
    public static HaEntityStateChange<HaEntityState<DateTime?, SceneControllerEvent>> ToSceneControllerEvent(this HaEntityStateChange change) => Transform<HaEntityState<DateTime?, SceneControllerEvent>, DateTime?, SceneControllerEvent>(change);


    public static HaEntityStateChange<HaEntityState<string, DeviceTrackerModel>> ToDeviceTracker(this HaEntityStateChange change) => Transform<HaEntityState<string, DeviceTrackerModel>, string, DeviceTrackerModel>(change);
    public static HaEntityStateChange<HaEntityState<string, PersonModel>> ToPerson(this HaEntityStateChange change) => Transform<HaEntityState<string, PersonModel>, string, PersonModel>(change);
    public static HaEntityStateChange<HaEntityState<int, ZoneModel>> ToZone(this HaEntityStateChange change) => Transform<HaEntityState<int, ZoneModel>, int, ZoneModel>(change);

    public static HaEntityStateChange<HaEntityState<OnOff, CalendarModel>> ToCalendar(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, CalendarModel>, OnOff, CalendarModel>(change);

    public static HaEntityStateChange<HaEntityState<OnOff, HaAutomationModel>> ToHaAutomation(this HaEntityStateChange change) => Transform<HaEntityState<OnOff, HaAutomationModel>, OnOff, HaAutomationModel>(change);
}

