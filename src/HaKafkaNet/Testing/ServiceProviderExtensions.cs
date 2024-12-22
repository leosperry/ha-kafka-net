using HaKafkaNet.Models.JsonConverters;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HaKafkaNet.Testing
{
    public static class ServiceProviderExtensions
    {
        static FakeMessageContext _fakeMessageContext = new();

        public static T GetRegistry<T>(this IServiceProvider services)
        {
            return (T)services.GetRequiredService<IEnumerable<IAutomationRegistry>>()
                .First(r => r.GetType() == typeof(T));
        }

        public static async Task SendState(this IServiceProvider Services, HaEntityState state)
        {
            var handler = Services.GetRequiredService<IMessageHandler<HaEntityState>>();
            await handler.Handle(_fakeMessageContext, state);
        }

        public static async Task SendState<Tstate>(this IServiceProvider Services, HaEntityState<Tstate, JsonElement> state)
        {
            var handler = Services.GetRequiredService<IMessageHandler<HaEntityState>>();
            await handler.Handle(_fakeMessageContext, Convert(state));
        }

        public static async Task SendState<Tstate, Tatt>(this IServiceProvider Services, HaEntityState<Tstate, Tatt> state)
        {
            if (state.Attributes is null)
            {
                throw new ArgumentException("state.Attribute cannot be null", nameof(state));
            }
            var handler = Services.GetRequiredService<IMessageHandler<HaEntityState>>();
            await handler.Handle(_fakeMessageContext, Convert(state));
        }

        private static HaEntityState Convert(object state)
        {
            return JsonSerializer.Deserialize<HaEntityState>(JsonSerializer.Serialize(state, GlobalConverters.StandardJsonOptions))!;
        }
    }

    class FakeMessageContext : IMessageContext
    {
        static FakeConsumerContext fakeConsumerContext = new(); 

        public Message Message => throw new NotImplementedException();

        public IMessageHeaders Headers => throw new NotImplementedException();

        public IConsumerContext ConsumerContext => fakeConsumerContext;

        public IProducerContext ProducerContext => throw new NotImplementedException();

        public IDictionary<string, object> Items => throw new NotImplementedException();

        public IDependencyResolver DependencyResolver => throw new NotImplementedException();

        public IReadOnlyCollection<string> Brokers => throw new NotImplementedException();

        public IMessageContext SetMessage(object key, object value)
        {
            throw new NotImplementedException();
        }
    }

    class FakeConsumerContext : IConsumerContext
    {
        public string ConsumerName => throw new NotImplementedException();

        public CancellationToken WorkerStopped => default;

        public int WorkerId => throw new NotImplementedException();

        public string Topic => throw new NotImplementedException();

        public int Partition => throw new NotImplementedException();

        public long Offset => throw new NotImplementedException();

        public TopicPartitionOffset TopicPartitionOffset => throw new NotImplementedException();

        public string GroupId => throw new NotImplementedException();

        public DateTime MessageTimestamp => throw new NotImplementedException();

        public bool AutoMessageCompletion { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ShouldStoreOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IDependencyResolver ConsumerDependencyResolver => throw new NotImplementedException();

        public IDependencyResolver WorkerDependencyResolver => throw new NotImplementedException();

        public Task<TopicPartitionOffset> Completion => throw new NotImplementedException();

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public IOffsetsWatermark GetOffsetsWatermark()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }
    }


}
