using System.Text.Json;

namespace HaKafkaNet.Tests;

public class TestHelpers
{
    
    public static HaEntityState GetState(string entityId = "enterprise", string state = "yellow alert", object atttributes = null!, 
        DateTime lastUpdated = default )
    {
        var atts = JsonSerializer.SerializeToElement(atttributes != null ? atttributes: new{ prop = "somevalue"});

        return new HaEntityState()
        {
            EntityId = entityId,
            Attributes = atts,
            State = state,
            LastUpdated = lastUpdated
        };
    }

    [Obsolete("please use GetState<string, Tatt>()", false)]
    public static HaEntityState<T> GetState<T>(string entityId = "enterprise", string state = "unknown", T atttributes = default(T)!, DateTime lastUpdated = default)
    {
        return new HaEntityState<T>()
        {
            EntityId = entityId,
            State = state,
            Attributes = atttributes,
            LastUpdated = lastUpdated
        };
    }    
    
    public static HaEntityState<Tstate, Tatt> GetState<Tstate, Tatt>(string entityId, Tstate state, Tatt? atttributes = default, DateTime lastUpdated = default)
    {
        return new HaEntityState<Tstate, Tatt>()
        {
            EntityId = entityId,
            State = state,
            Attributes = atttributes,
            LastUpdated = lastUpdated,
            LastChanged = lastUpdated
        };
    }

    public static HaEntityStateChange GetStateChange(string entityId = "enterprise", string state = "unknown", object atttributes = null!, EventTiming timing = EventTiming.PostStartup)
    {
         return new HaEntityStateChange()
         {
            EntityId = entityId,
            EventTiming = EventTiming.PostStartup,
            New = GetState(entityId, state, atttributes)
         };
    }

    public static HaEntityStateChange GetStateChange<T>(
        string entityId = "enterprise", string state = "unknown", object atttributes = null!, 
        DateTime lastUpdated = default, EventTiming timing = EventTiming.PostStartup)
    {
         return new HaEntityStateChange()
         {
            EntityId = entityId,
            EventTiming = EventTiming.PostStartup,
            New = GetState(entityId, state, atttributes, lastUpdated)
         };
    }

    public static SunModel GetSun(SunState state = SunState.Above_Horizon,
        float elevation = default, bool rising = default, float azimuth = default, 
        DateTime nextDawn = default,DateTime nextDusk = default,
        DateTime nextNoon = default, DateTime nextMidnight = default,
        DateTime nextRising = default, DateTime nextSetting = default)
    {
        return new SunModel()
        {
            EntityId = "sun.sun",
            Attributes = GetSunAttributes(elevation, rising, azimuth, 
                nextDawn,nextDusk,nextNoon, nextMidnight,nextRising,nextSetting),
            State = state
        };
    }

    [Obsolete("pleasee use GetSun")]
    public static HaEntityState<SunAttributes> GetSunState(SunState state = SunState.Above_Horizon,
        float elevation = default, bool rising = default, float azimuth = default, 
        DateTime nextDawn = default,DateTime nextDusk = default,
        DateTime nextNoon = default, DateTime nextMidnight = default,
        DateTime nextRising = default, DateTime nextSetting = default)
    {
        return new HaEntityState<SunAttributes>()
        {
            EntityId = "sun.sun",
            Attributes = GetSunAttributes(elevation, rising, azimuth, 
                nextDawn,nextDusk,nextNoon, nextMidnight,nextRising,nextSetting),
            State = state == SunState.Above_Horizon ? "above_horizon" : "below_horizon"
        };
    }

    public static SunAttributes GetSunAttributes( 
        float elevation = default, bool rising = default, float azimuth = default, 
        DateTime nextDawn = default,DateTime nextDusk = default,
        DateTime nextNoon = default, DateTime nextMidnight = default,
        DateTime nextRising = default, DateTime nextSetting = default)
    {
        return new SunAttributes()
        {
            FriendlyName = "Sun",
            Azimuth = azimuth,
            Elevation = elevation,
            NextMidnight = nextMidnight,
            NextDawn = nextDawn,
            NextDusk = nextDusk,
            NextNoon = nextNoon,
            NextRising = nextRising,
            NextSetting = nextSetting,
            Rising = rising,
        };
    }
}

