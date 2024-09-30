using System;

namespace HaKafkaNet;

/// <summary>
/// Provides access to Entities that update in memory automatically. Use in moderation.
/// Entities will perssist in memory until the application exits.
/// Updates lock the entity.
/// </summary>
public interface IUpdatingEntityProvider
{
    IHaEntity<Tstate, Tatt> GetEntity<Tstate, Tatt>(string entityId);
}


