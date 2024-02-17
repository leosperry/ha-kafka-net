using System.Text.Json;

namespace HaKafkaNet;

public static class EntityStateProviderExtensions
{
    public static Task<HaEntityState<string, T>?> GetAttributeTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<string, T>>(entityId, cancellationToken);
    public static Task<DateTimeEnity?> GetDateTimeEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<DateTimeEnity>(entityId, cancellationToken);
    public static Task<DateTimeEnity<Tatt>?> GetDateTimeEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<DateTimeEnity<Tatt>>(entityId, cancellationToken);
    public static Task<DoubleEnity?> GetDoubleEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<DoubleEnity>(entityId, cancellationToken);
    public static Task<DoubleEnity<Tatt>?> GetDoubleEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<DoubleEnity<Tatt>>(entityId, cancellationToken);
    public static Task<HaEntityState<Tstate, TAttributes>?> GetEntity<Tstate, TAttributes>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<HaEntityState<Tstate, TAttributes>>(entityId, cancellationToken);
    public static Task<IntegerEnity?> GetIntegerEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<IntegerEnity>(entityId, cancellationToken);
    public static Task<IntegerEnity<Tatt>?> GetIntegerEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<IntegerEnity<Tatt>>(entityId, cancellationToken);
    public static Task<OnOffEnity?> GetOnOffEntity(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<OnOffEnity>(entityId, cancellationToken);
    public static Task<OnOffEnity<Tatt>?> GetOnOffEntity<Tatt>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) => 
        provider.GetEntity<OnOffEnity<Tatt>>(entityId, cancellationToken);
    public static Task<HaEntityState<T, JsonElement>?> GetStateTypedEntity<T>(this IEntityStateProvider provider, string entityId, CancellationToken cancellationToken = default) 
        => provider.GetEntity<HaEntityState<T, JsonElement>>(entityId, cancellationToken);
    public static Task<SunModel?> GetSun(this IEntityStateProvider provider, CancellationToken cancellationToken = default) 
        => provider.GetEntity<SunModel>("sun.sun", cancellationToken);
}
