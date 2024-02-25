using System.Reflection.Metadata;
using System.Text.Json;

namespace HaKafkaNet.Tests;

public class HaEntityStateConversionTests
{
    [Fact]
    public void WhenStateIsDate_ConvertsCorrectly()
    {
        // Given
        SceneControllerEvent evt = new()
        {
            EventType = "self desctruct",
        };
        var atts = JsonSerializer.SerializeToElement(evt);
        var state = new HaEntityState()
        {
            EntityId =  "NCC-1701",
            State = DateTime.Now.ToString("o"),
            Attributes = atts,
            LastUpdated = DateTime.Now
        };
    
        // When
        var typed = (HaEntityState<DateTime?, SceneControllerEvent>)state;

    
        // Then
        Assert.NotNull(typed.State);
        Assert.NotNull(typed.Attributes?.EventType);
    }

    [Fact]
    public void WhenStateIsDouble_ConvertsCorrectly()
    {
        // Given
        var atts = JsonSerializer.SerializeToElement(new{});
        var state = new HaEntityState()
        {
            EntityId =  "NCC-1701",
            State = "1000.12345",
            Attributes = atts,
            LastUpdated = DateTime.Now
        };
    
        // When
        var typed = (HaEntityState<double?, JsonElement>)state;

    
        // Then
        Assert.NotNull(typed.State);
    }
}
