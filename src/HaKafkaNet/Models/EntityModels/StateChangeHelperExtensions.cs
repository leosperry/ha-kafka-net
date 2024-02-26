using System.Data.SqlTypes;
using System.Numerics;
using FluentValidation.Validators;
using Microsoft.Extensions.Options;

namespace HaKafkaNet;

public static class StateChangeHelperExtensions
{
    public static bool TurnedOn<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change, bool allowOldNull = true)
    {
        return Turned(change, OnOff.On, allowOldNull);
    }

    public static bool TurnedOff<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change, bool allowOldNull = true)
    {
        return Turned(change, OnOff.Off, allowOldNull);
    }

    public static bool Turned<_>(this HaEntityStateChange<HaEntityState<OnOff, _>> change, OnOff val, bool allowOldNull = true)
    {
        return (change.Old?.State ?? (allowOldNull ? (OnOff)(-1) : val)) != val && change.New.State == val;
    }

    public static bool CameHome<_>(this HaEntityStateChange<HaEntityState<string, _>> change, bool allowOldNull = true) where _ : TrackerModelBase
    {
        return (!change.Old?.IsHome() ?? allowOldNull) && change.New.IsHome();
    }

    public static bool LeftHome<_>(this HaEntityStateChange<HaEntityState<string, _>> change, bool allowOldNull = true) where _ : TrackerModelBase
    {
        return (change.Old?.IsHome() ?? allowOldNull) && !change.New.IsHome();
    }

    public static bool BecameGreaterThan<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old <= val) && change.New.State > val;
    }

    public static bool BecameGreaterThanOrEqual<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old < val) && change.New.State >= val;
    }

    public static bool BecameLessThan<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old >= val) && change.New.State < val;
    }

    public static bool BecameLessThanOrEqual<T, _>(this HaEntityStateChange<HaEntityState<T?,_>> change, T val, bool allowOldNull = true) where T : struct, IComparisonOperators<T, T, bool>
    {
        var old = change.Old?.State;
        return ((old is null && allowOldNull) || old > val) && change.New.State <= val;
    }
}
