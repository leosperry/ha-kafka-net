#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HaKafkaNet;

/// <summary>
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Utility-classes#light-models
/// </summary>
/// <param name="Red"></param>
/// <param name="Green"></param>
/// <param name="Blue"></param>
public record RgbTuple(byte Red,byte Green, byte Blue)
{
    public static implicit operator RgbTuple((byte red, byte green, byte blue) tuple)
    {
        return new RgbTuple(tuple.red, tuple.green, tuple.blue);
    }
}

/// <summary>
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Utility-classes#light-models
/// </summary>
/// <param name="Red"></param>
/// <param name="Green"></param>
/// <param name="Blue"></param>
/// <param name="White"></param>
public record RgbwTuple(byte Red,byte Green, byte Blue, byte White)
{
    public static implicit operator RgbwTuple((byte red, byte green, byte blue, byte white) tuple)
    {
        return new RgbwTuple(tuple.red, tuple.green, tuple.blue, tuple.white);
    }
}

/// <summary>
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Utility-classes#light-models
/// </summary>
/// <param name="Red"></param>
/// <param name="Green"></param>
/// <param name="Blue"></param>
/// <param name="White"></param>
/// <param name="WarmWhite"></param>
public record RgbwwTuple(byte Red,byte Green, byte Blue, byte White, byte WarmWhite)
{
    public static implicit operator RgbwwTuple((byte red, byte green, byte blue, byte white, byte warmWhite) tuple)
    {
        return new RgbwwTuple(tuple.red, tuple.green, tuple.blue, tuple.white, tuple.warmWhite);
    }
}

/// <summary>
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Utility-classes#light-models
/// </summary>
/// <param name="X"></param>
/// <param name="Y"></param>
public record XyColor(float X, float Y)
{
    public static implicit operator XyColor((float x, float y) tuple)
    {
        return new XyColor(tuple.x, tuple.y);
    }
}

/// <summary>
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Utility-classes#light-models
/// </summary>
/// <param name="Hue"></param>
/// <param name="Saturation"></param>
public record HsColor(float Hue, float Saturation)
{
    public static implicit operator HsColor((float hue, float saturation) tuple)
    {
        return new HsColor(tuple.hue, tuple.saturation);
    }
}
