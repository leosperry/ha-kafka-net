using System;
using System.Net;
using System.Text.Json;

namespace HaKafkaNet;

public static class HaApiExtensions
{
    /// <summary>
    /// Sometimes and entity is non-responsive, but HA does not report an error.
    /// This method turns on an entity then verifies it turned on
    /// </summary>
    /// <param name="api"></param>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true if the entity reports on after being told to turn on</returns>
    public static async Task<bool> TurnOnAndVerify(this IHaApiProvider api, string entityId, CancellationToken cancellationToken)
    {
        await api.TurnOn(entityId, cancellationToken);
        var apiResponse = await api.GetEntity<HaEntityState<OnOff, JsonElement>>(entityId, cancellationToken);
        return !apiResponse.entityState.Bad() && apiResponse.entityState?.State == OnOff.On;
    }

    /// <summary>
    /// Sometimes and entity is non-responsive, but HA does not report an error.
    /// This method turns off an entity then verifies it turned off
    /// </summary>
    /// <param name="api"></param>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true if the entity reports off after being told to turn off</returns>
    public static async Task<bool> TurnOffAndVerify(this IHaApiProvider api, string entityId, CancellationToken cancellationToken)
    {
        await api.TurnOff(entityId, cancellationToken);
        var apiResponse = await api.GetEntity<HaEntityState<OnOff, JsonElement>>(entityId, cancellationToken);
        return !apiResponse.entityState.Bad() && apiResponse.entityState?.State == OnOff.Off;
    }
}
