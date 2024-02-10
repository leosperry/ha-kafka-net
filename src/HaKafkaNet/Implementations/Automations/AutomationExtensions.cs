namespace HaKafkaNet;

public static class SimpleAutomationExtensions
{
    public static T WithMeta<T>(this T auto, string name, string? description = null, bool enabledAtStartup = true, Guid? id = null) 
        where T: SimpleAutomationBase
    {
        AutomationMetaData meta = new AutomationMetaData()
        {
            Name = name,
            Description = description,
            Enabled = enabledAtStartup,
            Id = id ?? Guid.NewGuid(),
        };
        auto.SetMeta(meta);
        return auto;
    }
    
    public static T WithMeta<T>(this T auto, AutomationMetaData meta) 
        where T: SimpleAutomationBase
    {
        auto.SetMeta(meta);
        return auto;
    }

}

public static class ContitionalAutomationExtensions
{
    public static T WithMeta<T>(this T auto, string name, string? description = null, bool enabledAtStartup = true, Guid? id = null) 
        where T : ConditionalAutomationBase
    {
        AutomationMetaData meta = new AutomationMetaData()
        {
            Name = name,
            Description = description,
            Enabled = enabledAtStartup,
            Id = id ?? Guid.NewGuid(),
        };
        auto.SetMeta(meta);
        return auto;
    }

    public static T WithMeta<T>(this T auto, AutomationMetaData meta) 
        where T: ConditionalAutomationBase
    {
        auto.SetMeta(meta);
        return auto;
    }}
