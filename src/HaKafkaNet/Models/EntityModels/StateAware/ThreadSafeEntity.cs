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

internal record ThreadSafeEntity<Tstate, Tatt> : IHaEntity<Tstate, Tatt>
{

#region interface properties
    public string EntityId {get; private set; }

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
                return _state ?? default!;
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
                return _atts;
            }
            finally
            {
                _loc.ExitReadLock();
            }
        }
    }
#endregion

    private ReaderWriterLockSlim _loc = new();

    internal void Set(HaEntityState<Tstate, Tatt> state)
    {
        _loc.EnterWriteLock();
        try
        {
            this._lastChanged = state.LastChanged;
            this._lastUpdated = state.LastUpdated;
            this._context = state.Context;
            this._state = state.State;
            this._atts = state.Attributes;
        }
        finally
        {
            _loc.ExitWriteLock();
        }
    }

    public ThreadSafeEntity(string entity_id)
    {
        this.EntityId = entity_id;
        this._lastChanged = DateTime.MinValue;
        this._lastUpdated = DateTime.MinValue;
    } 
}
