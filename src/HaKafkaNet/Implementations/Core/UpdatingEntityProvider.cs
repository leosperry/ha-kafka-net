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

    public IHaEntity<Tstate, Tatt> GetEntity<Tstate, Tatt>(string entityId)
    {
        var entity = UpdatingEntityStorage<Tstate,Tatt>._instances.GetOrAdd(
            entityId, Create<Tstate, Tatt>);

        return entity;    
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
                instance.Set((HaEntityState<Tstate, Tatt>)state);
            }
        } 
    }
}


