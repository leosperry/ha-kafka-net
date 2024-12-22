using HaKafkaNet.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Text.Json;

namespace HaKafkaNet.ExampleApp.Tests
{
    public class HaKafkaNetFixture : WebApplicationFactory<Program>
    {
        public Mock<IHaApiProvider> API { get; } = new Mock<IHaApiProvider>();

        public HaKafkaNetFixture()
        {
            // This ensures nothing explicitly configured will return something the framework can handle.
            API.Setup(api => api.GetEntity(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new HttpResponseMessage()
                { Content = new StringContent(""), StatusCode = System.Net.HttpStatusCode.OK }, new HaEntityState()
                { EntityId = "", State = "unknown", Attributes = JsonSerializer.SerializeToElement("{}") }));
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => {
                services.ConfigureForIntegrationTests(API.Object);
            });
        }

    }
}
