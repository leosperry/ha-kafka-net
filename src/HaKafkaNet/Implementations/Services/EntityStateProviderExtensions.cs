using System.Text.Json;

namespace HaKafkaNet;

public static class EntityStateProviderExtensions
{
    public static Task<HaEntityState<string, T>?> GetAttributeTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<string, T>>(entityId, cancellationToken);
    public static Task<HaEntityState<DateTime?, JsonElement>?> GetDateTimeEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<DateTime?, JsonElement>>(entityId, cancellationToken);
    public static Task<HaEntityState<DateTime?, Tatt>?> GetDateTimeEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<DateTime?, Tatt>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<double?, JsonElement>?> GetDoubleEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<double?, JsonElement>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<double?, Tatt>?> GetDoubleEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<double?, Tatt>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<Tstate, TAttributes>?> GetEntity<Tstate, TAttributes>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<Tstate, TAttributes>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<int?, JsonElement>?> GetIntegerEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<int?, JsonElement>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<int?, Tatt>?> GetIntegerEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<int?, Tatt>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<OnOff, JsonElement>?> GetOnOffEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<OnOff, JsonElement>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<OnOff, Tatt>?> GetOnOffEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<OnOff, Tatt>>(entityId, cancellationToken);
    
    public static Task<HaEntityState<T, JsonElement>?> GetStateTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) 
        => provider.GetEntity<HaEntityState<T, JsonElement>>(entityId, cancellationToken);
    
    public static Task<SunModel?> GetSun(this IEntityStateProvider provider, CancellationToken cancellationToken = default) 
        => provider.GetEntity<SunModel>("sun.sun", cancellationToken);
}
