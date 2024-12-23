using HaKafkaNet;
using HaKafkaNet.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

/// <summary>
/// Reminder: Updates to the framework may require updates to this file.
/// If there are breaking changes to the framework re-copy this file from
/// https://raw.githubusercontent.com/leosperry/ha-kafka-net/refs/heads/main/example/HaKafkaNet.ExampleApp.Tests/HaKafkanetFixture.cs
/// </summary>
public class HaKafkaNetFixture : WebApplicationFactory<Program>
{
    public Mock<IHaApiProvider> API { get; } = new Mock<IHaApiProvider>();

    public HaKafkaNetFixture()
    {
        // This ensures nothing explicitly configured will return something the framework can handle.
        API.Setup(api => api.GetEntity(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelper.Api_GetEntity_Response());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            services.ConfigureForIntegrationTests(API.Object);
        });
    }
}

