using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet.Implementations.Core;

internal class UpdatingEntityProvider : IUpdatingEntityProvider
{
    ISystemObserver _sysObserver;

    ConcurrentDictionary<string, object> _instances = new();

    public UpdatingEntityProvider(ISystemObserver systemObserver)
    {
        _sysObserver = systemObserver;
    }

    public IUpdatingEntity<string, JsonElement> GetEntity(string entityId)
        => (ThreadSafeEntity<string, JsonElement>)_instances.GetOrAdd(entityId, Create<string, JsonElement>);

    public IUpdatingEntity<Tstate, JsonElement> GetEntity<Tstate>(string entityId) where Tstate : class
        => (ThreadSafeEntity<Tstate, JsonElement>)_instances.GetOrAdd(entityId, Create<Tstate, JsonElement>);

    public IUpdatingEntity<Tstate, Tatt> GetEntity<Tstate, Tatt>(string entityId)
        where Tstate : class
        where Tatt : class
        => (ThreadSafeEntity<Tstate, Tatt>)_instances.GetOrAdd(entityId, Create<Tstate, Tatt>);

    public IUpdatingEntity<Tstate, JsonElement> GetEnumEntity<Tstate>(string entityId) where Tstate : Enum
        => (ThreadSafeEntity<Tstate, JsonElement>)_instances.GetOrAdd(entityId, Create<Tstate, JsonElement>);

    public IUpdatingEntity<Tstate, Tatt> GetEnumEntity<Tstate, Tatt>(string entityId)
        where Tstate : Enum
        where Tatt : class
        => (ThreadSafeEntity<Tstate, Tatt>)_instances.GetOrAdd(entityId, Create<Tstate, Tatt>);

    public IUpdatingEntity<Tstate?, JsonElement> GetValueTypeEntity<Tstate>(string entityId) where Tstate : struct
        => (ThreadSafeEntity<Tstate?, JsonElement>)_instances.GetOrAdd(entityId, Create<Tstate?, JsonElement>);

    public IUpdatingEntity<Tstate?, Tatt> GetValueTypeEntity<Tstate, Tatt>(string entityId)
        where Tstate : struct
        where Tatt : class
        => (ThreadSafeEntity<Tstate?, Tatt>)_instances.GetOrAdd(entityId, Create<Tstate?, Tatt>);

    private ThreadSafeEntity<Tstate, Tatt> Create<Tstate, Tatt>(string entityId)
    {
        var retVal = new ThreadSafeEntity<Tstate, Tatt>(entityId);
        _sysObserver.RegisterThreadSafeEntityUpdater(entityId, state => retVal.Set(() => (HaEntityState<Tstate, Tatt>)state));
        return retVal;
    }
}
