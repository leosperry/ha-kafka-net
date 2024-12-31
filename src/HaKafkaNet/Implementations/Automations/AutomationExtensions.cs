namespace HaKafkaNet;

/// <summary>
/// 
/// </summary>
public static class AutomationExtensions
{
    /// <summary>
    /// Adds metadata to an automation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="auto"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="enabledAtStartup"></param>
    /// <returns></returns>
    public static T WithMeta<T>(this T auto, string name, string? description = null, bool enabledAtStartup = true) 
        where T: ISetAutomationMeta
    {
        AutomationMetaData meta = new()
        {
            Name = name,
            Description = description,
            Enabled = enabledAtStartup,
        };
        auto.SetMeta(meta);
        return auto;
    }

    /// <summary>
    /// Adds metadata to an automation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="auto"></param>
    /// <param name="meta"></param>
    /// <returns></returns>
    public static T WithMeta<T>(this T auto, AutomationMetaData meta) 
        where T: ISetAutomationMeta
    {
        auto.SetMeta(meta);
        return auto;
    }
    
}

