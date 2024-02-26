namespace HaKafkaNet.Tests;

public class StateChangeExtensionTests
{
    [Fact]
    public void WhenOnOff_OldIsNull_andOnMatches_ReturnsTrue()
    {
        // Given
        HaEntityStateChange<HaEntityState<OnOff,FakeModel>> sut = new()
        {
            EntityId = "enterprise",
            Old = null,
            New = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.On),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.Turned(OnOff.On);
    
        // Then
        Assert.True(result);
    }

    [Fact]
    public void WhenOnOff_OldIsNull_andOnDoesNotMatch_ReturnsFalse()
    {
        // Given
        HaEntityStateChange<HaEntityState<OnOff,FakeModel>> sut = new()
        {
            EntityId = "enterprise",
            Old = null,
            New = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.Off),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.Turned(OnOff.On);
    
        // Then
        Assert.False(result);
    }

    [Fact]
    public void WhenOnOff_OldIsNull_andNullNotAllowed_ReturnsFalse()
    {
        // Given
        HaEntityStateChange<HaEntityState<OnOff,FakeModel>> sut = new()
        {
            EntityId = "enterprise",
            Old = null,
            New = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.On),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.Turned(OnOff.On, false);
    
        // Then
        Assert.False(result);
    }

    [Fact]
    public void WhenOnOff_OldIsOn_andNewIsOn_returnFalse()
    {
        // Given
        HaEntityStateChange<HaEntityState<OnOff,FakeModel>> sut = new()
        {
            EntityId = "enterprise",
            Old = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.On),
            New = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.On),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.Turned(OnOff.On);
    
        // Then
        Assert.False(result);    
    }

    [Fact]
    public void WhenOnOff_OldIsOff_andNewIsOn_returnTrue()
    {
        // Given
        HaEntityStateChange<HaEntityState<OnOff,FakeModel>> sut = new()
        {
            EntityId = "enterprise",
            Old = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.Off),
            New = TestHelpers.GetState<OnOff, FakeModel>("enterprise", OnOff.On),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.Turned(OnOff.On);
    
        // Then
        Assert.True(result);    
    }

    [Fact]
    public void WhenOldIsNull_andHome_CameHome_returnsTrue()
    {
        // Given
        HaEntityStateChange<HaEntityState<string, PersonModel>> sut = new()
        {
            EntityId = "Leo",
            Old = null,
            New = TestHelpers.GetState<string, PersonModel>("Leo", "home" ,default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.CameHome();
    
        // Then
        Assert.True(result);
    }

    [Fact]
    public void WhenOldIsNull_andNullNotAllowed_CameHome_returnsFalse()
    {
        // Given
        HaEntityStateChange<HaEntityState<string, PersonModel>> sut = new()
        {
            EntityId = "Leo",
            Old = null,
            New = TestHelpers.GetState<string, PersonModel>("Leo", "home" ,default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.CameHome(false);
    
        // Then
        Assert.False(result);
    }

    [Fact]
    public void WhenOldIsNull_andNotHome_CameHome_returnsFalse()
    {
        // Given
        HaEntityStateChange<HaEntityState<string, PersonModel>> sut = new()
        {
            EntityId = "Leo",
            Old = null,
            New = TestHelpers.GetState<string, PersonModel>("Leo", "not_home" ,default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.CameHome();
    
        // Then
        Assert.False(result);
    }

    
    [Fact]
    public void WhenOldIsHome_andHome_CameHome_returnsFalse()
    {
        // Given
        HaEntityStateChange<HaEntityState<string, PersonModel>> sut = new()
        {
            EntityId = "Leo",
            Old = TestHelpers.GetState<string, PersonModel>("Leo", "home" ,default),
            New = TestHelpers.GetState<string, PersonModel>("Leo", "home" ,default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.CameHome();
    
        // Then
        Assert.False(result);
    }

    [Fact]
    public void WhenOldIsNotHome_andHome_CameHome_returnsTrue()
    {
        // Given
        HaEntityStateChange<HaEntityState<string, PersonModel>> sut = new()
        {
            EntityId = "Leo",
            Old = TestHelpers.GetState<string, PersonModel>("Leo", "not_home" ,default),
            New = TestHelpers.GetState<string, PersonModel>("Leo", "home" ,default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result = sut.CameHome();
    
        // Then
        Assert.True(result);
    }

    [Fact]
    public void WhenOldNull_andGreaterThan_BecameGreaterThan_True()
    {
        // Given
        HaEntityStateChange<HaEntityState<double?,FakeModel>> sut1 = new()
        {
            EntityId = "enterprise",
            Old = null, 
            New = TestHelpers.GetState<double?,FakeModel>("enterprise", 100, default),
            EventTiming = EventTiming.PostStartup
        };
    
        HaEntityStateChange<HaEntityState<double?,FakeModel>> sut2 = new()
        {
            EntityId = "enterprise",
            Old = TestHelpers.GetState<double?,FakeModel>("enterprise", null, default), 
            New = TestHelpers.GetState<double?,FakeModel>("enterprise", 100, default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result1 = sut1.BecameGreaterThan(50);
        var result2 = sut1.BecameGreaterThan(50);
    
        // Then
        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void WhenOldNull_andNulNotAllowed_BecameGreaterThan_False()
    {
        // Given
        HaEntityStateChange<HaEntityState<double?,FakeModel>> sut1 = new()
        {
            EntityId = "enterprise",
            Old = null, 
            New = TestHelpers.GetState<double?,FakeModel>("enterprise", 100, default),
            EventTiming = EventTiming.PostStartup
        };
    
        HaEntityStateChange<HaEntityState<double?,FakeModel>> sut2 = new()
        {
            EntityId = "enterprise",
            Old = TestHelpers.GetState<double?,FakeModel>("enterprise", null, default), 
            New = TestHelpers.GetState<double?,FakeModel>("enterprise", 100, default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result1 = sut1.BecameGreaterThan(50, false);
        var result2 = sut2.BecameGreaterThan(50, false);
    
        // Then
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void WhenOldLess_andGreater_BecameGreaterThan_True()
    {
        // Given
    
        HaEntityStateChange<HaEntityState<double?,FakeModel>> sut2 = new()
        {
            EntityId = "enterprise",
            Old = TestHelpers.GetState<double?,FakeModel>("enterprise", 50, default), 
            New = TestHelpers.GetState<double?,FakeModel>("enterprise", 100, default),
            EventTiming = EventTiming.PostStartup
        };
    
        // When
        var result1 = sut2.BecameGreaterThan(50);
        var result2 = sut2.BecameGreaterThan(50, false);
    
        // Then
        Assert.True(result1);
        Assert.True(result2);
    }
    

    class FakeModel
    {

    }
}
