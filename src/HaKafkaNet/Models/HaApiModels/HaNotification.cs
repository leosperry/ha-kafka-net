#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.ComponentModel;

namespace HaKafkaNet;

/// <summary>
/// https://www.home-assistant.io/integrations/persistent_notification/
/// </summary>
public record HaNotification
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? UpdateType { get; set; }
}

public static class HaNotificationExtensions
{
    public static HaNotificationType? GetNotificationType(this HaNotification notification)
    {
        if (notification.UpdateType is null)
        {
            return null;
        }
        return Enum.Parse<HaNotificationType>(notification.UpdateType, true);
    }
}

public enum HaNotificationType
{
    Added,
    Removed,
    Updated,
    Current
}
