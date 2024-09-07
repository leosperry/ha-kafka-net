using FastEndpoints;

namespace HaKafkaNet;

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
