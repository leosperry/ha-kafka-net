using FastEndpoints;

namespace HaKafkaNet;

public class PostNotificationRequest
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? UpdateType { get; set; }
}

internal class NotificationEndpoint : Endpoint<HaNotification, EmptyResponse>
{
    readonly ISystemObserver _observer;

    public NotificationEndpoint(ISystemObserver observer)
    {
        _observer = observer;
    }

    public override void Configure()
    {
        Post("api/notification");
        AllowAnonymous();
    }

    public override  Task<EmptyResponse> ExecuteAsync(HaNotification req, CancellationToken ct)
    {
        _observer.OnHaNotification(req, ct);

        return Task.FromResult(Response);
    }
}
