using System.Text.Json;

namespace HaKafkaNet;

public static class EntityStateProviderExtensions
{
    public static async Task<IHaEntity<Tstate, TAttributes>?> GetEntity<Tstate, TAttributes>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<Tstate, TAttributes>>(entityId, cancellationToken);
    public static async Task<IHaEntity<T, JsonElement>?> GetStateTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) 
        => await provider.GetEntity<HaEntityState<T, JsonElement>>(entityId, cancellationToken);

    public static async Task<IHaEntity<string, T>?> GetAttributeTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<string, T>>(entityId, cancellationToken);
    
    public static async Task<IHaEntity<int?, JsonElement>?> GetIntegerEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<int?, JsonElement>>(entityId, cancellationToken);
    public static async Task<IHaEntity<int?, Tatt>?> GetIntegerEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<int?, Tatt>>(entityId, cancellationToken);
    
    public static IHaEntity<int?, JsonElement> GetIntegerEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<int?, JsonElement>(entityId);
    public static IHaEntity<int?, Tatt> GetIntegerEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<int?, Tatt>(entityId);

    public static async Task<IHaEntity<float?, JsonElement>?> GetFloatEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<float?, JsonElement>>(entityId, cancellationToken);
    public static async Task<IHaEntity<float?, Tatt>?> GetFloatEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<float?, Tatt>>(entityId, cancellationToken);

    public static IHaEntity<float?, JsonElement> GetFloatEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<float?, JsonElement>(entityId);
    public static IHaEntity<float?, Tatt> GetFloatEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<float?, Tatt>(entityId);
        
    public static async Task<IHaEntity<double?, JsonElement>?> GetDoubleEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<double?, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<double?, Tatt>?> GetDoubleEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<double?, Tatt>>(entityId, cancellationToken);

    public static IHaEntity<double?, JsonElement> GetDoubleEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<double?, JsonElement>(entityId);
    public static IHaEntity<double?, Tatt> GetDoubleEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<double?, Tatt>(entityId);
        
    public static async Task<IHaEntity<DateTime?, JsonElement>?> GetDateTimeEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<DateTime?, JsonElement>>(entityId, cancellationToken);
    public static async Task<IHaEntity<DateTime?, Tatt>?> GetDateTimeEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<DateTime?, Tatt>>(entityId, cancellationToken);

    public static IHaEntity<DateTime?, JsonElement> GetDateTimeEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<DateTime?, JsonElement>(entityId);
    public static IHaEntity<DateTime?, Tatt> GetDateTimeEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<DateTime?, Tatt>(entityId);
    
    public static async Task<IHaEntity<OnOff, JsonElement>?> GetOnOffEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<OnOff, JsonElement>>(entityId, cancellationToken);
    public static async Task<IHaEntity<OnOff, Tatt>?> GetOnOffEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<OnOff, Tatt>>(entityId, cancellationToken);

    public static IHaEntity<OnOff, JsonElement> GetOnOffEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<OnOff, JsonElement>(entityId);
    public static IHaEntity<OnOff, Tatt> GetOnOffEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<OnOff, Tatt>(entityId);

    public static async Task<IHaEntity<BatteryState, JsonElement>?> GetBatteryStateEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<BatteryState, JsonElement>>(entityId, cancellationToken);
    public static async Task<IHaEntity<BatteryState, Tatt>?> GetBatteryStateEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        await provider.GetEntity<HaEntityState<BatteryState, Tatt>>(entityId, cancellationToken);

    public static IHaEntity<BatteryState, JsonElement> GetBatteryStateEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<BatteryState, JsonElement>(entityId);
    public static IHaEntity<BatteryState, Tatt> GetBatteryStateEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<BatteryState, Tatt>(entityId);
    
    public static async Task<IHaEntity<SunState, SunAttributes>?> GetSun(this IEntityStateProvider provider, CancellationToken cancellationToken = default) 
        => await provider.GetEntity<SunModel>("sun.sun", cancellationToken);

    public static IHaEntity<SunState, SunAttributes> GetSun(this IUpdatingEntityProvider provider) => provider.GetEntity<SunState, SunAttributes>("sun.sun");

    public static async Task<IHaEntity<OnOff, LightModel>?> GetLightEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<OnOff, LightModel>>(entityId, cancellationToken);
    public static async Task<IHaEntity<OnOff, ColorLightModel>?> GetColorLightEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<OnOff, ColorLightModel>>(entityId, cancellationToken);

    public static IHaEntity<OnOff, LightModel> GetLightEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<OnOff, LightModel>(entityId);
    public static IHaEntity<OnOff, ColorLightModel> GetColorLightEntity(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<OnOff, ColorLightModel>(entityId);

    public static async Task<IHaEntity<string, PersonModel>?> GetPersonEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<string, PersonModel>>(entityId, cancellationToken);

    public static IHaEntity<string, PersonModel> GetPersonEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<string, PersonModel>(entityId);

    public static async Task<IHaEntity<string, DeviceTrackerModel>?> GetDeviceTrackerEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<string, DeviceTrackerModel>>(entityId, cancellationToken);

    public static IHaEntity<string, DeviceTrackerModel> GetDeviceTrackerEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<string, DeviceTrackerModel>(entityId);

    public static async Task<IHaEntity<int?, ZoneModel>?> GetZoneEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<int?, ZoneModel>>(entityId, cancellationToken);

    public static IHaEntity<int?, ZoneModel> GetZoneEntity<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<int?, ZoneModel>(entityId);

    public static async Task<IHaEntity<OnOff, CalendarModel>?> GetCalendar(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<OnOff, CalendarModel>>(entityId, cancellationToken);

    public static IHaEntity<OnOff, CalendarModel> GetCalendar<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<OnOff, CalendarModel>(entityId);
    
    public static async Task<IHaEntity<MediaPlayerState, JsonElement>?> GetMediaPlayer(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<MediaPlayerState, JsonElement>>(entityId, cancellationToken);
        
    public static async Task<IHaEntity<MediaPlayerState, Tatt>?> GetMediaPlayer<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) =>
        await provider.GetEntity<HaEntityState<MediaPlayerState, Tatt>>(entityId, cancellationToken);

    public static IHaEntity<MediaPlayerState, JsonElement> GetMediaPlayer(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<MediaPlayerState, JsonElement>(entityId);
    public static IHaEntity<MediaPlayerState, Tatt> GetMediaPlayer<Tatt>(this IUpdatingEntityProvider provider, string entityId) => provider.GetEntity<MediaPlayerState, Tatt>(entityId);
    
        
}
