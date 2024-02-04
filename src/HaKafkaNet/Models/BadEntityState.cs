namespace HaKafkaNet;

public record BadEntityState(string EntityId, HaEntityState? State = null);

