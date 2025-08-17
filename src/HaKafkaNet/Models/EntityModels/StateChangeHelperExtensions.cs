#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Numerics;

namespace HaKafkaNet;

public static class StateChangeHelperExtensions
{
    public static bool IsOn<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change)
        => change.New.IsOn();

    public static bool IsOff<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change)
        => change.New.IsOff();

    public static bool TurnedOn<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change, bool allowOldNull = true)
        => Turned(change, OnOff.On, allowOldNull);

    public static bool TurnedOff<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change, bool allowOldNull = true)
        => Turned(change, OnOff.Off, allowOldNull);

    public static bool Turned<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change, OnOff val, bool allowOldNull = true)
    {
        return (change.Old?.State ?? (allowOldNull ? (OnOff)(-1) : val)) != val && change.New.State == val;
    }

    public static bool IsHome<_>(this HaEntityStateChange<HaEntityState<string, _>> change) where _ : TrackerModelBase
        => change.New.IsHome();

    public static bool CameHome<_>(this HaEntityStateChange<HaEntityState<string, _>> change, bool allowOldNull = true) where _ : TrackerModelBase
    {
        return (!change.Old?.IsHome() ?? allowOldNull) && change.New.IsHome();
    }

    public static bool LeftHome<_>(this HaEntityStateChange<HaEntityState<string, _>> change, bool allowOldNull = true) where _ : TrackerModelBase
    {
        return (change.Old?.IsHome() ?? allowOldNull) && !change.New.IsHome();
    }

    public static bool Decreased<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change) where T : struct, IComparisonOperators<T, T, bool>
    {
        return change.Old is not null && change.New.State < change.Old.State;
    }

    public static bool Decreased<T, _>(this HaEntityStateChange<HaEntityState<T,_>> change) where T : struct, IComparisonOperators<T, T, bool>
    {
        return change.Old is not null && change.New.State < change.Old.State;
    }

    public static bool Increased<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change) where T : struct, IComparisonOperators<T, T, bool>
    {
        return change.Old is not null && change.New.State > change.Old.State;
    }

    public static bool Increased<T, _>(this HaEntityStateChange<HaEntityState<T,_>> change) where T : struct, IComparisonOperators<T, T, bool>
    {
        return change.Old is not null && change.New.State > change.Old.State;
    }

    public static bool BecameGreaterThan<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old <= val) && change.New.State > val;
    }

    public static bool BecameGreaterThan<T, _>(this HaEntityStateChange<HaEntityState<T,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old <= val) && change.New.State > val;
    }

    public static bool BecameGreaterThanOrEqual<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old < val) && change.New.State >= val;
    }

    public static bool BecameGreaterThanOrEqual<T, _>(this HaEntityStateChange<HaEntityState<T,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old < val) && change.New.State >= val;
    }

    public static bool BecameLessThan<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old >= val) && change.New.State < val;
    }

    public static bool BecameLessThan<T, _>(this HaEntityStateChange<HaEntityState<T,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old >= val) && change.New.State < val;
    }

    public static bool BecameLessThanOrEqual<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old > val) && change.New.State <= val;
    }
    public static bool BecameLessThanOrEqual<T, _>(this HaEntityStateChange<HaEntityState<T,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old > val) && change.New.State <= val;
    }
}
