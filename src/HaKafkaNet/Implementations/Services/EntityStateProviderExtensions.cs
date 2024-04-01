using System.Text.Json;

namespace HaKafkaNet;

public static class EntityStateProviderExtensions
{
    public static Task<HaEntityState<Tstate, TAttributes>?> GetEntity<Tstate, TAttributes>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<Tstate, TAttributes>>(entityId, cancellationToken);
    public static Task<HaEntityState<T, JsonElement>?> GetStateTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) 
        => provider.GetEntity<HaEntityState<T, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<string, T>?> GetAttributeTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<string, T>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<int?, JsonElement>?> GetIntegerEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<int?, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<int?, Tatt>?> GetIntegerEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<int?, Tatt>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<float?, JsonElement>?> GetFloatEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<float?, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<float?, Tatt>?> GetFloatEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<float?, Tatt>>(entityId, cancellationToken);
        
    public static Task<HaEntityState<double?, JsonElement>?> GetDoubleEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<double?, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<double?, Tatt>?> GetDoubleEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<double?, Tatt>>(entityId, cancellationToken);
        
    public static Task<HaEntityState<DateTime?, JsonElement>?> GetDateTimeEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<DateTime?, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<DateTime?, Tatt>?> GetDateTimeEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<DateTime?, Tatt>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<OnOff, JsonElement>?> GetOnOffEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<OnOff, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<OnOff, Tatt>?> GetOnOffEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<OnOff, Tatt>>(entityId, cancellationToken);

    public static Task<HaEntityState<BatteryState, JsonElement>?> GetBatteryStateEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<BatteryState, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<BatteryState, Tatt>?> GetBatteryStateEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<BatteryState, Tatt>>(entityId, cancellationToken);
    
    public static Task<SunModel?> GetSun(this IEntityStateProvider provider, CancellationToken cancellationToken = default) 
        => provider.GetEntity<SunModel>("sun.sun", cancellationToken);

    public static Task<HaEntityState<OnOff, LightModel>?> GetLightEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        provider.GetEntity<HaEntityState<OnOff, LightModel>>(entityId, cancellationToken);
    public static Task<HaEntityState<OnOff, ColorLightModel>?> GetColorLightEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        provider.GetEntity<HaEntityState<OnOff, ColorLightModel>>(entityId, cancellationToken);


    public static Task<HaEntityState<string, PersonModel>?> GetPersonEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        provider.GetEntity<HaEntityState<string, PersonModel>>(entityId, cancellationToken);

    public static Task<HaEntityState<string, DeviceTrackerModel>?> GetDeviceTrackerEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        provider.GetEntity<HaEntityState<string, DeviceTrackerModel>>(entityId, cancellationToken);

    public static Task<HaEntityState<int?, ZoneModel>?> GetZoneEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        provider.GetEntity<HaEntityState<int?, ZoneModel>>(entityId, cancellationToken);

    public static Task<HaEntityState<OnOff, CalendarModel>?> GetCalendar(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken) =>
        provider.GetEntity<HaEntityState<OnOff, CalendarModel>>(entityId, cancellationToken);

}
