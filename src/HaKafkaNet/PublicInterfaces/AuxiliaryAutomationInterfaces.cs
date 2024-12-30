using System;

namespace HaKafkaNet;

public interface IInitializeOnStartup
{
    Task Initialize();
}

public interface IAutomationMeta
{
    AutomationMetaData GetMetaData();
}

public interface ISetAutomationMeta
{
    void SetMeta(AutomationMetaData meta);
}
