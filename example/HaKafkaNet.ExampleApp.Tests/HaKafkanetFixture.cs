using HaKafkaNet;
using HaKafkaNet.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

/// <summary>
/// Reminder: Updates to the framework may require updates to this file.
/// If there are breaking changes to the framework re-copy this file from
/// https://raw.githubusercontent.com/leosperry/ha-kafka-net/refs/heads/main/example/HaKafkaNet.ExampleApp.Tests/HaKafkanetFixture.cs
/// </summary>
public class HaKafkaNetFixture : WebApplicationFactory<Program>
{
    public Mock<IHaApiProvider> API { get; } = new Mock<IHaApiProvider>();
    public TestHelper Helpers { get; private set;}

    public FakeTimeProvider Time { get; private set;} 

    public HaKafkaNetFixture()
    {
        var helper = Services.GetRequiredService<TestHelper>();
        // This ensures nothing explicitly configured will return something the framework can handle.
        // It is helpful during test initialization when auto-updating entities are used.
        API.Setup(api => api.GetEntity(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(helper.Api_GetEntity_Response());
        
        this.Helpers = Services.GetRequiredService<TestHelper>();
        this.Time = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            services.ConfigureForIntegrationTests(API.Object);
        });
    }
}

