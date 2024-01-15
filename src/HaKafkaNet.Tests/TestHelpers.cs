using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HaKafkaNet.Tests;

public class TestHelpers
{
    
    public static HaEntityState GetFakeState(string entityId = "enterprise", object atttributes = null!, 
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


}

