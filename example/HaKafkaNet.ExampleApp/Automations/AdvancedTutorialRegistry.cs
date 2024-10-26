namespace HaKafkaNet.ExampleApp;

public class AdvancedTutorialRegistry : IAutomationRegistry
{
    readonly IAutomationBuilder _builder;
    readonly IHaServices _services;

    public AdvancedTutorialRegistry(IAutomationBuilder builder, IHaServices services)
    {
        _builder = builder;
        _services = services;
    }
    public void Register(IRegistrar reg)
    {
        reg.TryRegister(
            () => DoorAlert("binary_sensor.front_door_contact", "front door"),
            () => DoorAlert("binary_sensor.back_door_contact", "back door")
        );
    }

    IConditionalAutomation DoorAlert(string entityId, string friendlyName)
    {
        const int seconds = 10;

        return _builder.CreateConditional()
            .WithName($"{friendlyName} open alert")
            .WithDescription($"Notify when the {friendlyName} has been open for more than {seconds} seconds")
            .When((sc) => sc.ToOnOff().IsOn())
            .ForSeconds(seconds)
            .Then(ct => NotifyDoorOpen(entityId, friendlyName, TimeSpan.FromSeconds(seconds), ct))
            .Build();
    }

    private async Task NotifyDoorOpen(string entityId, string friendlyName, TimeSpan seconds, CancellationToken ct)
    {
        // if we get here, the door has been open for 10 seconds
        string message = $"{friendlyName} is open";
        bool doorOpen = true;
        int alertCount = 0;
        try
        {
            do
            {
                await _services.Api.Speak("tts.piper", "media_player.kitchen", message, cancellationToken: ct);

                await Task.Delay(seconds, ct); // <-- use the cancellation token

                var doorState = await _services.EntityProvider.GetOnOffEntity(entityId, ct);
                doorOpen = doorState!.IsOn();
            } while (doorOpen && ++alertCount < 12 && !ct.IsCancellationRequested);

            if (doorOpen)
            {
                await _services.Api.NotifyGroupOrDevice("mobile_app_my_phone", message, cancellationToken: ct);
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
        {
            // don't do anything
            // the door was closed or
            // the application is shutting down
        }
    }    
}
