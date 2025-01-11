using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class NotificationEndpoint : Endpoint<HaNotification, EmptyResponse>
{
    readonly ISystemObserver _observer;
    readonly ILogger<NotificationEndpoint> _logger;

    public NotificationEndpoint(ISystemObserver observer, ILogger<NotificationEndpoint> logger)
    {
        _observer = observer;
        _logger = logger;;
    }

    public override void Configure()
    {
        Post("api/notification");
        AllowAnonymous();
    }

    public override  Task<EmptyResponse> ExecuteAsync(HaNotification req, CancellationToken ct)
    {
        _logger.LogTrace("Received notification {notification_id} {op}", req.Id, req.UpdateType);
        _observer.OnHaNotification(req, ct);

        return Task.FromResult(Response);
    }
}
