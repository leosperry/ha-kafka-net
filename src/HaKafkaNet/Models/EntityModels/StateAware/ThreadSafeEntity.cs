using System.Text.Json;

namespace HaKafkaNet;

/// <summary>
/// represents an entity state in HA
/// </summary>
public interface IHaEntity : IHaEntity<string, JsonElement>;

/// <summary>
/// represents an entity state in HA
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHaEntity<T> : IHaEntity<T, JsonElement>;

/// <summary>
/// represents an entity state in HA
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IHaEntity<Tstate, Tatt>
{
    /// <summary>
    /// the ID of an entity in HA
    /// </summary>
    string EntityId { get; }

    /// <summary>
    /// only updates on state changes
    /// </summary>
    DateTime LastChanged { get; }
    
    /// <summary>
    /// updates when attributes or state changes
    /// </summary>
    DateTime LastUpdated { get; }
    
    /// <summary>
    /// used by HA
    /// </summary>
    HaEventContext? Context { get; }

    /// <summary>
    /// The entity's state
    /// </summary>
    Tstate State { get; }

    /// <summary>
    /// attributes defined by an integration in HA
    /// </summary>
    Tatt? Attributes { get; }
}

/// <summary>
/// a representation of an entity that updates automatically
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IUpdatingEntity<Tstate,Tatt> : IHaEntity<Tstate, Tatt>
{
    /// <summary>
    /// creates a snapshot of an entity to avoid thread sync issues
    /// </summary>
    /// <returns></returns>
    IHaEntity<Tstate,Tatt> Snapshot();
}

internal record ThreadSafeEntity<Tstate, Tatt> : IUpdatingEntity<Tstate,Tatt>
{

    static bool _nonNullableValueType;

    static ThreadSafeEntity()
    {
        var stateType = typeof(Tstate);
        if (stateType.IsValueType && Nullable.GetUnderlyingType(stateType) is null)
        {
            _nonNullableValueType = true;
        }
    }


#region interface properties
    public string EntityId { get; private set; }

    DateTime _lastChanged;
    public DateTime LastChanged
    {
        get
        {
            _loc.EnterReadLock();
            try
            {
                return _lastChanged;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }

    DateTime _lastUpdated;
    public DateTime LastUpdated
    {
        get
        {
            _loc.EnterReadLock();
            try
            {
                return _lastUpdated;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }

    HaEventContext? _context;
    public HaEventContext? Context
    {
        get
        {
            _loc.EnterReadLock();
            try
            {
                return _context;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }

    private Tstate? _state;
    public Tstate State 
    { 
        get
        {
            _loc.EnterReadLock();
            try
            {
                // throw exception only if non-nullable and bad
                return (_nonNullableValueType && _badState) ? throw new HaKafkaNetException("non nullable state has bad value") : _state!;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }

    Tatt? _attributes;
    public Tatt? Attributes
    {
        get
        {
            _loc.EnterReadLock();
            try
            {
                return _badAttributes ? throw new HaKafkaNetException("auto updating entity has bad attributes") : _attributes;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }
#endregion

    private ReaderWriterLockSlim _loc = new(LockRecursionPolicy.SupportsRecursion);
    private bool _badState = true;
    private bool _badAttributes = true;

    internal void Set(Func<HaEntityState<Tstate, Tatt>> getIt)
    {
         _loc.EnterWriteLock();
        try
        {
            var raw = getIt();
            this._context = raw.Context;
            this._lastChanged = raw.LastChanged;
            this._lastUpdated = raw.LastUpdated;

            this._state = raw.State;
            this._attributes = raw.Attributes;

            this._badState = false;
            this._badAttributes = false;
        }
        catch
        {
            this._badState = true;
            this._badAttributes = true;
        }
        finally
        {
            _loc.ExitWriteLock();
        }
    }

    public IHaEntity<Tstate, Tatt> Snapshot()
    {
        _loc.EnterReadLock();
        try
        {
            return new HaEntityState<Tstate, Tatt>()
            {
                EntityId = this.EntityId,
                State = this.State,
                Attributes = this.Attributes,
                Context = this._context,
                LastChanged = this._lastChanged,
                LastUpdated = this._lastUpdated            
            };
        }
        finally
        {
            _loc.ExitReadLock();
        }
    }

    public ThreadSafeEntity(string entity_id)
    {
        this.EntityId = entity_id;
        this._lastChanged = DateTime.MinValue;
        this._lastUpdated = DateTime.MinValue;
    } 
}
