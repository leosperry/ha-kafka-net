namespace HaKafkaNet;

/// <summary>
/// Tells the discovery process at startup to ignore classes
/// decorated with this and not create singletons
/// </summary>
public sealed class ExcludeFromDiscoveryAttribute: Attribute
{

}
