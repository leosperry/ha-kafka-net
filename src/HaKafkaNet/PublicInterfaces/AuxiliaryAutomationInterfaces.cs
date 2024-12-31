using System;

namespace HaKafkaNet;

/// <summary>
/// tells the framework additional logic should be called at startup
/// </summary>
public interface IInitializeOnStartup
{
    /// <summary>
    /// called before automations begin running
    /// </summary>
    /// <returns></returns>
    Task Initialize();
}

/// <summary>
/// Tells the framework that the use will supply metadata 
/// </summary>
public interface IAutomationMeta
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    AutomationMetaData GetMetaData();
}

/// <summary>
/// 
/// </summary>
public interface ISetAutomationMeta
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="meta"></param>
    void SetMeta(AutomationMetaData meta);
}
