namespace HaKafkaNet;

public static class Bytes
{
    public const byte 
        Zero = 0,
        One = 1,
        _5pct = 12,
        _10pct = 25,
        _15pct = 38,
        _20pct = 51,
        _25pct = 63,
        _30pct = 76,
        _33pct = 84,
        _35pct = 89,
        _40pct = 102,
        _45pct = 114,
        _50pct = 127,
        _55pct = 140,
        _60pct = 153,
        _65pct = 165,
        _66pct = 168,
        _70pct = 178,
        _75pct = 191,
        _80pct = 204,
        _85pct = 216,
        _90pct = 229,
        _95pct = 242,
        _100pct = 255,
        Max = 255;
    
    /// <summary>
    /// Converts a percentage to a byte
    /// </summary>
    /// <param name="percent">an integer between 0 and 100 inclusive</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte PercentToByte(int percent)
    {
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "percent must be between 0 and 100 inclusive");
        }
        return (byte)(percent * 2.55f);
    }

    /// <summary>
    /// Converts a percentage to a byte
    /// </summary>
    /// <param name="percent">a float between 0 and 1 inclusive</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte PercentToByte(float percent)
    {
        if (percent < 0f || percent > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "percent must be between 0 and 1 inclusive");
        }
        return (byte)(percent * 255f);
    }

    /// <summary>
    /// Converts a percentage to a byte
    /// </summary>
    /// <param name="percent">a double between 0 and 1 inclusive</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte PercentToByte(double percent)
    {
        if (percent < 0d || percent > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), "percent must be between 0 and 1 inclusive");
        }
        return (byte)(percent * 255d);
    }
}
