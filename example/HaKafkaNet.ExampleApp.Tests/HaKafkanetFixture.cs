using System.Text.Json;
using HaKafkaNet;
using HaKafkaNet.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
    public TestHelper Helpers { get => Services.GetRequiredService<TestHelper>(); }

    public HaKafkaNetFixture()
    {
        // todo: find a better setup
        // calling helpers here will cause an infinite loop at startup
        // when active automations are used or anything needing IHaApiProvider or FakeTimeProvider at startup
        this.API.Setup(api => api.GetEntity<HaEntityState>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new Func<string, CancellationToken, (HttpResponseMessage, HaEntityState?)>(
              (id, ct) => (
                new HttpResponseMessage(System.Net.HttpStatusCode.OK),
                new HaEntityState()
                {
                    EntityId = id,
                    State = "0",
                    Attributes = JsonSerializer.SerializeToElement("{}"),
                    LastChanged = DateTime.Now,
                    LastUpdated = DateTime.Now
                })));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test"); // add an appsettings.Test.json file to your application

        builder.ConfigureServices(services => {
            // call this method with the fake or mock of your choice
            // optionally pass an IDistributed cache. 
            services.ConfigureForIntegrationTests(API.Object);
        });
    }
}
