using System.Text.Json;

namespace HaKafkaNet;

public interface IHaEntity : IHaEntity<string, JsonElement>;
public interface IHaEntity<T> : IHaEntity<T, JsonElement>;

public interface IHaEntity<Tstate, Tatt>
{
    string EntityId { get; }

    DateTime LastChanged { get; }
    
    DateTime LastUpdated { get; }
    
    HaEventContext? Context { get; }

    Tstate State { get; }

    Tatt? Attributes { get; }
}

public interface IUpdatingEntity<Tstate,Tatt> : IHaEntity<Tstate, Tatt>
{
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
                // throw exception only if non-nullalble and bad
                return (_nonNullableValueType && _badState) ? throw new HaKafkaNetException("non nullable state has bad value") : _state!;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }

    Tatt? _atts;
    public Tatt? Attributes
    {
        get
        {
            _loc.EnterReadLock();
            try
            {
                return _badAttributes ? throw new HaKafkaNetException("auto updating entity has bad attributes") : _atts;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }
#endregion

    private ReaderWriterLockSlim _loc = new();
    private bool _badState = true;
    private bool _badAttributes = true;

    internal void Set(HaEntityState raw)
    {
        _loc.EnterWriteLock();
        try
        {
            this._context = raw.Context;
            this._lastChanged = raw.LastChanged;
            this._lastUpdated = raw.LastUpdated;
            
            try
            {
                this._state = raw.GetState<Tstate>();
                this._badState = false;
            }
            catch (System.Exception)
            {
                this._badState = true;
                this._state = default;
            }

            try
            {
                this._atts = raw.GetAttributes<Tatt>();
                this._badAttributes = false;
            }
            catch (System.Exception)
            {
                this._badAttributes = true;
                this._atts = default;
            }
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
