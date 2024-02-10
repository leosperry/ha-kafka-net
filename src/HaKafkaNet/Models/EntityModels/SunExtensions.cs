using System.Text.Json;

namespace HaKafkaNet;

public static class SunExtensions
{
    public static TimeSpan GetTimeUntilSunrise(this JsonElement atts, TimeSpan? offset = null)
    {
        var sunAtts = JsonSerializer.Deserialize<SunAttributes>(atts);
        if (sunAtts is null)
        {
            throw new HaKafkaNetException("Could not calculate sunrise. Sun schema invalid");
        }
        return GetTimeUntilSunrise(sunAtts, offset);
    }

    public static TimeSpan GetTimeUntilSunrise(this SunAttributes atts, TimeSpan? offset = null)
    {
        TimeSpan newOffset = offset ?? TimeSpan.Zero;
        return atts.NextRising - DateTime.Now + newOffset;
    }
    
    public static TimeSpan GetTimeUntilSunSet(this JsonElement atts, TimeSpan? offset = null)
    {
        var sunAtts = JsonSerializer.Deserialize<SunAttributes>(atts);
        if (sunAtts is null)
        {
            throw new HaKafkaNetException("Could not calculate sunrise. Sun schema invalid");
        }
        return GetTimeUntilSunSet(sunAtts, offset);
    }

    public static TimeSpan GetTimeUntilSunSet(this SunAttributes atts, TimeSpan? offset = null)
    {
        TimeSpan newOffset = offset ?? TimeSpan.Zero;
        return atts.NextSetting - DateTime.Now + newOffset;
    }
    
}
