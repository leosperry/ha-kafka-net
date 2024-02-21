using System.Text.Json;

namespace HaKafkaNet;

public record OnOffEnity : HaEntityState<OnOff, JsonElement>{ }
public record OnOffEnity<T> : HaEntityState<OnOff, T>{ }

public record IntegerEnity : HaEntityState<int, JsonElement>{ }
public record IntegerEnity<T> : HaEntityState<int, T>{ }

public record DoubleEnity : HaEntityState<double, JsonElement>{ }
public record DoubleEnity<T> : HaEntityState<double, T>{ }

public record DateTimeEnity : HaEntityState<DateTime, JsonElement>{ }
public record DateTimeEnity<T> : HaEntityState<DateTime, T>{ }

public record BatteryStateEntity : HaEntityState<BatteryState, JsonElement>{ }
public record BatteryStateEntity<T> : HaEntityState<BatteryState, T>{ }
