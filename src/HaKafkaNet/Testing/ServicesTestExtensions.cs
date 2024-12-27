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
        /// <summary>
        /// Configures the services collection for integration tests
        /// </summary>
        /// <param name="services"></param>
        /// <param name="apiProvider">A fake or mock of the API provider</param>
        /// <param name="cache">Optional fake or mock cache. Defaults to a MemoryDistributedCache</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureForIntegrationTests(this IServiceCollection services, 
            IHaApiProvider apiProvider, IDistributedCache? cache = null)
        {
            ServicesExtensions._isTestMode = true;
            services
                .AddSingleton<TestHelper>()
                .RemoveAll<TimeProvider>()
                .AddSingleton<TimeProvider, FakeTimeProvider>()
                .RemoveAll<IDistributedCache>()
                .AddSingleton<IDistributedCache>(cache ?? MakeCache())
                .RemoveAll<IHaApiProvider>()
                .AddSingleton<IHaApiProvider>(apiProvider)
                .AddSingleton<IMessageHandler<HaEntityState>, HaStateHandler>(); 

            return services;
        }

        static IDistributedCache MakeCache()
        {
            IOptions<MemoryDistributedCacheOptions> options = Options.Create(new MemoryDistributedCacheOptions());
            var cache = new MemoryDistributedCache(options);
            return cache;
        }
    }
}
