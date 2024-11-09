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

    bool _nonNullableValueType;


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
                return _badState ? throw new HaKafkaNetException("auto updating entity has a bad state") : _atts;
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

    internal void Set(Func<HaEntityState<Tstate, Tatt>> stateGetter)
    {
        _loc.EnterWriteLock();
        try
        {
            var state = stateGetter();
            this._lastChanged = state.LastChanged;
            this._lastUpdated = state.LastUpdated;
            this._context = state.Context;
            this._state = state.State;
            this._atts = state.Attributes;
            _badState = false;
        }
        catch
        {
            _badState = true;
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
                Context = this.Context,
                LastChanged = this.LastChanged,
                LastUpdated = this.LastUpdated            
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

        var stateType = typeof(Tstate);
        if (stateType.IsValueType && Nullable.GetUnderlyingType(stateType) != null)
        {
            _nonNullableValueType = true;
        }
    } 
}
