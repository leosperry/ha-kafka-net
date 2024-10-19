namespace HaKafkaNet;

public static class AutomationExtensions
{
    public static T WithMeta<T>(this T auto, string name, string? description = null, bool enabledAtStartup = true, Guid? id = null) 
        where T: ISetAutomationMeta
    {
        AutomationMetaData meta = new AutomationMetaData()
        {
            Name = name,
            Description = description,
            Enabled = enabledAtStartup,
        };
        auto.SetMeta(meta);
        return auto;
    }
    
    public static T WithMeta<T>(this T auto, AutomationMetaData meta) 
        where T: ISetAutomationMeta
    {
        auto.SetMeta(meta);
        return auto;
    }
    
}

