using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HaKafkaNet.Testing
{
    public class TestHelper
    {
        public static JsonElement EmptyAttributes() => JsonSerializer.SerializeToElement("{}");
        public static HttpResponseMessage OkResponse() => new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        public static HaEntityState Make(string entityId, string state)
        {
            return new HaEntityState()
            {
                EntityId = entityId,
                State = state,
                Attributes = EmptyAttributes(),
            };
        }

        public static HaEntityState<Tstate, JsonElement> Make<Tstate>(string entityId, Tstate state)
        {
            return new HaEntityState<Tstate, JsonElement>()
            {
                EntityId = entityId,
                State = state,
                Attributes = EmptyAttributes(),
            };
        }

        public static HaEntityState<Tstate, Tatt> Make<Tstate, Tatt>(string entityId, Tstate state, Tatt attributes)
        {
            return new HaEntityState<Tstate, Tatt>()
            {
                EntityId = entityId,
                State = state,
                Attributes = attributes,
            };
        }

        public static Func<string, CancellationToken, (HttpResponseMessage, HaEntityState?)> Api_GetEntity_Response(string state = "unknown")
            => (string entityId, CancellationToken ct) => (TestHelper.OkResponse(), TestHelper.Make(entityId, state));
        public static Func<string, CancellationToken, (HttpResponseMessage, HaEntityState<Tstate, JsonElement>?)> Api_GetEntity_Response<Tstate>(Tstate state)
            => (string entityId, CancellationToken ct) => (TestHelper.OkResponse(), TestHelper.Make<Tstate>(entityId, state));
    }
}
