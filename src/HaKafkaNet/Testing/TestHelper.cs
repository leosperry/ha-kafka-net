using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HaKafkaNet.Models.JsonConverters;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;

namespace HaKafkaNet.Testing
{
    public class TestHelper
    {
        private int _delay = 50;
        static FakeMessageContext _fakeMessageContext = new();
        private readonly IServiceProvider _services;

        public TestHelper(IServiceProvider services)
        {
            this._services = services;
        }

        public JsonElement EmptyAttributes() => JsonSerializer.SerializeToElement("{}");
        public HttpResponseMessage OkResponse() => new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        public HaEntityState Make(string entityId, string state, DateTimeOffset time = default)
        {
            return new HaEntityState()
            {
                EntityId = entityId,
                State = state,
                Attributes = EmptyAttributes(),
                LastChanged = time == default ? DateTime.Now : time.LocalDateTime,
                LastUpdated = time == default ? DateTime.Now : time.LocalDateTime
            };
        }

        public HaEntityState<Tstate, JsonElement> Make<Tstate>(string entityId, Tstate state, DateTimeOffset time = default)
        {
            return new HaEntityState<Tstate, JsonElement>()
            {
                EntityId = entityId,
                State = state,
                Attributes = EmptyAttributes(),
                LastChanged = time == default ? DateTime.Now : time.LocalDateTime,
                LastUpdated = time == default ? DateTime.Now : time.LocalDateTime,
            };
        }

        public HaEntityState<Tstate, Tatt> Make<Tstate, Tatt>(string entityId, Tstate state, Tatt attributes, DateTimeOffset time = default)
        {
            return new HaEntityState<Tstate, Tatt>()
            {
                EntityId = entityId,
                State = state,
                Attributes = attributes,
                LastChanged = time == default ? DateTime.Now : time.LocalDateTime,
                LastUpdated = time == default ? DateTime.Now : time.LocalDateTime
            };
        }

        public Func<string, CancellationToken, (HttpResponseMessage, HaEntityState?)> Api_GetEntity_Response(string state = "unknown")
            => (string entityId, CancellationToken ct) => (OkResponse(), Make(entityId, state));
        public Func<string, CancellationToken, (HttpResponseMessage, HaEntityState<Tstate, JsonElement>?)> Api_GetEntity_Response<Tstate>(Tstate state)
            => (string entityId, CancellationToken ct) => (OkResponse(), Make<Tstate>(entityId, state));

        public Func<string, CancellationToken, (HttpResponseMessage, HaEntityState<Tstate, Tatt>?)> Api_GetEntity_Response<Tstate, Tatt>(
            Tstate state, Tatt attributes)
            => (string entityId, CancellationToken ct) => (OkResponse(), Make<Tstate, Tatt>(entityId, state, attributes));


        public T GetRegistry<T>()
        {
            return (T)_services.GetRequiredService<IEnumerable<IAutomationRegistry>>()
                .First(r => r.GetType() == typeof(T));
        }

        public async Task SendState(HaEntityState state)
        {
            var handler = _services.GetRequiredService<IMessageHandler<HaEntityState>>();
            await handler.Handle(_fakeMessageContext, state);
            await Task.Delay(_delay);
        }

        public async Task SendState<Tstate>(HaEntityState<Tstate, JsonElement> state)
        {
            var handler = _services.GetRequiredService<IMessageHandler<HaEntityState>>();
            await handler.Handle(_fakeMessageContext, Convert(state));
            await Task.Delay(_delay);
        }

        public async Task SendState<Tstate, Tatt>(HaEntityState<Tstate, Tatt> state)
        {
            if (state.Attributes is null)
            {
                throw new ArgumentException("state.Attribute cannot be null", nameof(state));
            }
            var handler = _services.GetRequiredService<IMessageHandler<HaEntityState>>();
            await handler.Handle(_fakeMessageContext, Convert(state));
            await Task.Delay(_delay);
        }

        private static HaEntityState Convert(object state)
        {
            return JsonSerializer.Deserialize<HaEntityState>(JsonSerializer.Serialize(state, GlobalConverters.StandardJsonOptions))!;
        }    
    }

    
}
