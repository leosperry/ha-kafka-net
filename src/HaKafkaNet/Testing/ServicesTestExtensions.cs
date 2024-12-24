using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace HaKafkaNet.Testing
{
    public static class ServicesTestExtensions
    {
        public static IServiceCollection ConfigureForIntegrationTests(this IServiceCollection services, IHaApiProvider apiProvider)
        {
            IOptions<MemoryDistributedCacheOptions> options = Options.Create(new MemoryDistributedCacheOptions());
            var cache = new MemoryDistributedCache(options);

            services
                .AddTransient<TestMode>()
                .RemoveAll<TimeProvider>()
                .AddSingleton<TimeProvider, FakeTimeProvider>()
                .RemoveAll<IDistributedCache>()
                .AddSingleton<IDistributedCache>(cache)
                .RemoveAll<IHaApiProvider>()
                .AddSingleton<IHaApiProvider>(apiProvider)
                .AddSingleton<IMessageHandler<HaEntityState>, HaStateHandler>(); return services;
        }
    }

    internal record TestMode;
}
