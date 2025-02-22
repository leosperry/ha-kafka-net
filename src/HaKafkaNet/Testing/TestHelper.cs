﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HaKafkaNet.Models.JsonConverters;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HaKafkaNet.Testing
{
    /// <summary>
    /// utility class for working with integration tests
    /// </summary>
    public class TestHelper
    {
        private int _delay = 100;
        private readonly IServiceProvider _services;

        /// <summary>
        /// A time provider for working with delayable automations
        /// </summary>
        public FakeTimeProvider Time { get; private set;} 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public TestHelper(IServiceProvider services)
        {
            this._services = services;
            this.Time = (FakeTimeProvider)services.GetRequiredService<TimeProvider>();
        }

        /// <summary>
        /// for use with mocking state returns from api
        /// </summary>
        /// <returns></returns>
        public JsonElement EmptyAttributes() => JsonSerializer.SerializeToElement("{}");

        /// <summary>
        /// a basic ok response
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage OkResponse() => new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        /// <summary>
        /// Sets the default delay after sending state and advancing time
        /// </summary>
        /// <param name="delay">milliseconds</param>
        public void SetDelay(int delay)
        {
            _delay = delay;
        }

        /// <summary>
        /// Advances the FakeTimeHelper and pauses to allow automations on other threads to run
        /// </summary>
        /// <param name="time">The amount of time to advance</param>
        /// <param name="delay">The number of milliseconds to delay after advancing time</param>
        /// <returns></returns>
        public async Task AdvanceTime(TimeSpan time, int delay = -1)
        {
            Time.Advance(time);
            await Task.Delay(delay == -1 ? _delay : delay);
        }

        /// <summary>
        /// makes an entity state
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="state"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public HaEntityState Make(string entityId, string state, DateTimeOffset time = default)
        {
            return new HaEntityState()
            {
                EntityId = entityId,
                State = state,
                Attributes = EmptyAttributes(),
                LastChanged = time == default ? Time.GetLocalNow().LocalDateTime : time.LocalDateTime,
                LastUpdated = time == default ? Time.GetLocalNow().LocalDateTime : time.LocalDateTime
            };
        }

        /// <summary>
        /// makes an entity state
        /// </summary>
        /// <typeparam name="Tstate"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="state"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public HaEntityState<Tstate, JsonElement> Make<Tstate>(string entityId, Tstate state, DateTimeOffset time = default)
        {
            return new HaEntityState<Tstate, JsonElement>()
            {
                EntityId = entityId,
                State = state,
                Attributes = EmptyAttributes(),
                LastChanged = time == default ? Time.GetLocalNow().LocalDateTime : time.LocalDateTime,
                LastUpdated = time == default ? Time.GetLocalNow().LocalDateTime : time.LocalDateTime,
            };
        }

        /// <summary>
        /// makes an entity state
        /// </summary>
        /// <typeparam name="Tstate"></typeparam>
        /// <typeparam name="Tatt"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="state"></param>
        /// <param name="attributes"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public HaEntityState<Tstate, Tatt> Make<Tstate, Tatt>(string entityId, Tstate state, Tatt attributes, DateTimeOffset time = default)
        {
            return new HaEntityState<Tstate, Tatt>()
            {
                EntityId = entityId,
                State = state,
                Attributes = attributes,
                LastChanged = time == default ? Time.GetLocalNow().LocalDateTime : time.LocalDateTime,
                LastUpdated = time == default ? Time.GetLocalNow().LocalDateTime : time.LocalDateTime,
            };
        }

        /// <summary>
        /// For use with Moq Setup methods
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Func<string, CancellationToken, (HttpResponseMessage, HaEntityState?)> Api_GetEntity_Response(string state = "unknown")
            => (string entityId, CancellationToken ct) => (OkResponse(), Make(entityId, state));

        /// <summary>
        /// For use with Moq Setup methods
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>        
        public Func<string, CancellationToken, (HttpResponseMessage, HaEntityState<Tstate, JsonElement>?)> Api_GetEntity_Response<Tstate>(Tstate state)
            => (string entityId, CancellationToken ct) => (OkResponse(), Make<Tstate>(entityId, state));

        /// <summary>
        /// For use with Moq Setup methods
        /// </summary>
        /// <param name="state"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public Func<string, CancellationToken, (HttpResponseMessage, HaEntityState<Tstate, Tatt>?)> Api_GetEntity_Response<Tstate, Tatt>(
            Tstate state, Tatt attributes)
            => (string entityId, CancellationToken ct) => (OkResponse(), Make<Tstate, Tatt>(entityId, state, attributes));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRegistry<T>() where T : IAutomationRegistry
        {
            return (T)_services.GetRequiredService<IEnumerable<IAutomationRegistry>>()
                .First(r => r.GetType() == typeof(T));
        }

        /// <summary>
        /// Sends a state for the framework to handle
        /// </summary>
        /// <param name="state"></param>
        /// <param name="delay">The number of milliseconds to delay after sending the state to allow automations on other threads to run.</param>
        /// <returns></returns>
        public async Task SendState(HaEntityState state, int delay = default)
        {
            var handler = _services.GetRequiredService<IStateHandler>();
            await handler.Handle(state);
            await Task.Delay(delay == default ? _delay : delay);
        }

        /// <summary>
        /// Sends a state for the framework to handle
        /// </summary>
        /// <param name="state"></param>
        /// <param name="delay">The number of milliseconds to delay after sending the state to allow automations on other threads to run.</param>
        /// <returns></returns>
        public Task SendState<Tstate, Tatt>(HaEntityState<Tstate, Tatt> state, int delay = default)
            => SendState(Convert(state));

        private static HaEntityState Convert(object state)
        {
            return JsonSerializer.Deserialize<HaEntityState>(
                JsonSerializer.Serialize(state, GlobalConverters.StandardJsonOptions), 
            GlobalConverters.StandardJsonOptions)!;
        }    
    }
}
