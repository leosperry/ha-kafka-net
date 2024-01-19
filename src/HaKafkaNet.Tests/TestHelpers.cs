using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HaKafkaNet.Tests;

public class TestHelpers
{
    
    public static HaEntityState GetState(string entityId = "enterprise", object atttributes = null!, 
        DateTime lastUpdated = default )
    {
        var atts = JsonSerializer.SerializeToElement(atttributes != null ? atttributes: new{ prop = "somevalue"});

        return new HaEntityState()
        {
            EntityId = entityId,
            Attributes = atts,
            State = "warp factor 1",
            LastUpdated = lastUpdated
        };
    }

    public static HaEntityStateChange GetStateChange(string entityId = "enterprise", object atttributes = null!, EventTiming timing = EventTiming.PostStartup)
    {
         return new HaEntityStateChange()
         {
            EntityId = entityId,
            EventTiming = EventTiming.PostStartup,
            New = GetState(entityId, atttributes)
         };
    }


}

