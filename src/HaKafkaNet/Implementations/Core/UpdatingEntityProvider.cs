using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Implementations.Core;

internal class UpdatingEntityProvider : IUpdatingEntityProvider
{
    ISystemObserver _sysObserver;

    public UpdatingEntityProvider(ISystemObserver systemObserver)
    {
        _sysObserver = systemObserver;
    }

    public IUpdatingEntity<string, JsonElement> GetEntity(string entityId)
    {
        return UpdatingEntityStorage<string,JsonElement>._instances.GetOrAdd(entityId, Create<string, JsonElement>);
    }

    public IUpdatingEntity<Tstate, JsonElement> GetEntity<Tstate>(string entityId) where Tstate : class
    {
        return UpdatingEntityStorage<Tstate,JsonElement>._instances.GetOrAdd(entityId, Create<Tstate, JsonElement>);
    }

    public IUpdatingEntity<Tstate, Tatt> GetEntity<Tstate, Tatt>(string entityId)
        where Tstate : class
        where Tatt : class
    {
        return UpdatingEntityStorage<Tstate,Tatt>._instances.GetOrAdd(entityId, Create<Tstate, Tatt>);
    }

    public IUpdatingEntity<Tstate, JsonElement> GetEnumEntity<Tstate>(string entityId) where Tstate : Enum
    {
        return UpdatingEntityStorage<Tstate, JsonElement>._instances.GetOrAdd(entityId, Create<Tstate, JsonElement>);
    }

    public IUpdatingEntity<Tstate, Tatt> GetEnumEntity<Tstate, Tatt>(string entityId)
        where Tstate : Enum
        where Tatt : class
    {
        return UpdatingEntityStorage<Tstate,Tatt>._instances.GetOrAdd(entityId, Create<Tstate, Tatt>);
    }

    public IUpdatingEntity<Tstate?, JsonElement> GetValueTypeEntity<Tstate>(string entityId) where Tstate : struct
    {
        return UpdatingEntityStorage<Tstate?,JsonElement>._instances.GetOrAdd(entityId, Create<Tstate?, JsonElement>);
    }

    public IUpdatingEntity<Tstate?, Tatt> GetValueTypeEntity<Tstate, Tatt>(string entityId)
        where Tstate : struct
        where Tatt : class
    {
        return UpdatingEntityStorage<Tstate?,Tatt>._instances.GetOrAdd(entityId, Create<Tstate?, Tatt>);
    }

    private ThreadSafeEntity<Tstate, Tatt> Create<Tstate, Tatt>(string entityId)
    {
         _sysObserver.RegisterThreadSafeEntityUpdater(entityId, UpdatingEntityStorage<Tstate,Tatt>.Update);

        return new ThreadSafeEntity<Tstate, Tatt>(entityId);
    }

    private static class UpdatingEntityStorage<Tstate,Tatt>
    {
        public static ConcurrentDictionary<string, ThreadSafeEntity<Tstate, Tatt>> _instances = new();

        internal static void Update(HaEntityState state)
        {
            if(_instances.TryGetValue(state.EntityId, out var instance))
            {
                // an exception thrown here is caught in the observer
                instance.Set(() => (HaEntityState<Tstate, Tatt>)state);
            }
        } 
    }
}


