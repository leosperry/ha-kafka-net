namespace HaKafkaNet;

public record RgbTuple(byte Red,byte Green, byte Blue)
{
    public static implicit operator RgbTuple((byte red, byte green, byte blue) tuple)
    {
        return new RgbTuple(tuple.red, tuple.green, tuple.blue);
    }
}

public record RgbwTuple(byte Red,byte Green, byte Blue, byte White)
{
    public static implicit operator RgbwTuple((byte red, byte green, byte blue, byte white) tuple)
    {
        return new RgbwTuple(tuple.red, tuple.green, tuple.blue, tuple.white);
    }
}

public record RgbwwTuple(byte Red,byte Green, byte Blue, byte White, byte WarmWhite)
{
    public static implicit operator RgbwwTuple((byte red, byte green, byte blue, byte white, byte warmWhite) tuple)
    {
        return new RgbwwTuple(tuple.red, tuple.green, tuple.blue, tuple.white, tuple.warmWhite);
    }
}

public record XyColor(float X, float Y)
{
    public static implicit operator XyColor((float x, float y) tuple)
    {
        return new XyColor(tuple.x, tuple.y);
    }
}

public record HsColor(float Hue, float Saturation)
{
    public static implicit operator HsColor((float hue, float saturation) tuple)
    {
        return new HsColor(tuple.hue, tuple.saturation);
    }
}
