#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using FastEndpoints;

namespace HaKafkaNet;

internal class NotifyStartupShutdownEndpoint : Endpoint<StartUpShutDownEvent, EmptyResponse>
{
    ISystemObserver _observer;

    public NotifyStartupShutdownEndpoint(ISystemObserver observer)
    {
        _observer = observer;
    }

    public override void Configure()
    {
        Post("api/notifystartupshutdown");
        AllowAnonymous();
    }

    public override Task<EmptyResponse> ExecuteAsync(StartUpShutDownEvent req, CancellationToken ct)
    {
        _observer.OnHaStartUpShutdown(req, ct);
        return Task.FromResult(Response);
    }
}

public record StartUpShutDownEvent
{
    public string? Event { get; set; }
}