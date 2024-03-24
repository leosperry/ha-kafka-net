

using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Tests;

public class AutomationWrapperTests
{
    [Fact]
    public void WhenMetaNotSet_CreatesMeta()
    {
        // Given
        FakeAuto auto = new();
        Mock<IAutomationTraceProvider> trace = new();
    
        // When
        AutomationWrapper sut = new(auto, trace.Object, "test");
    
        // Then
        var meta = sut.GetMetaData();
        Assert.Equal("FakeAuto", meta.Name);
        //$"{source}-{name}-{triggers}"
        Assert.Equal("test-FakeAuto-crew.spock-crew.evil_spock", meta.KeyRequest);
    }

    [Fact]
    public void WhenMetaSet_KeyRequestIsNot_SetsRequestWithName()
    {
        // Given
        FakeAutoWithMeta auto = new();
        Mock<IAutomationTraceProvider> trace = new();
    
        // When
        AutomationWrapper sut = new(auto, trace.Object, "test");
    
        // Then
        var meta = sut.GetMetaData();
        Assert.Equal("Spock", meta.Name);
        //$"{source}-{name}-{triggers}"
        Assert.Equal("Spock", meta.KeyRequest);
    }

    [Fact]
    public void WhenMetaSet_KeyRequestIs_SetsRequest()
    {
        // Given
        FakeAutoWithMeta auto = new();
        auto.SetKey();
        Mock<IAutomationTraceProvider> trace = new();
    
        // When
        AutomationWrapper sut = new(auto, trace.Object, "test");
    
        // Then
        var meta = sut.GetMetaData();
        Assert.Equal("Spock", meta.Name);
        //$"{source}-{name}-{triggers}"
        Assert.Equal("Evil Spock", meta.KeyRequest);
    }
}

class FakeAuto : IAutomation
{
    public Task Execute(HaEntityStateChange stateChange, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public virtual IEnumerable<string> TriggerEntityIds()
    {
        yield return "crew.spock";
        yield return "crew.evil_spock";
    }
}

class FakeAutoWithMeta : FakeAuto, IAutomationMeta
{
    AutomationMetaData _meta = new()
    {
        Name = "Spock"
    };

    public AutomationMetaData GetMetaData()
    {
        return _meta;
    }

    public override IEnumerable<string> TriggerEntityIds()
    {
        return base.TriggerEntityIds().Union(["crew.evil_spock"]);
    }

    public void SetKey(string? key = default)
    {
        _meta.KeyRequest =  key ?? "Evil Spock";
    }

}
