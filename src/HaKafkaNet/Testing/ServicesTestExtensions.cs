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
        public static IServiceCollection ConfigureForIntegrationTests(this IServiceCollection services, 
            IHaApiProvider apiProvider, IDistributedCache? cache = null)
        {
            services
                .AddSingleton<TestHelper>()
                .RemoveAll<TimeProvider>()
                .AddSingleton<TimeProvider, FakeTimeProvider>()
                .RemoveAll<IDistributedCache>()
                .AddSingleton<IDistributedCache>(cache ?? MakeCache())
                .RemoveAll<IHaApiProvider>()
                .AddSingleton<IHaApiProvider>(apiProvider)
                .AddSingleton<IMessageHandler<HaEntityState>, HaStateHandler>(); return services;
        }

        static IDistributedCache MakeCache()
        {
            IOptions<MemoryDistributedCacheOptions> options = Options.Create(new MemoryDistributedCacheOptions());
            var cache = new MemoryDistributedCache(options);
            return cache;
        }
    }
}
