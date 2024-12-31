#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace HaKafkaNet;

public record BadEntityState(string EntityId, HaEntityState? State = null);

