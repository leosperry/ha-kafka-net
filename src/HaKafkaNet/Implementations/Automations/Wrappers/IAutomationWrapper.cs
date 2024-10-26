using System;

namespace HaKafkaNet;

internal interface IAutomationWrapperBase : IInitializeOnStartup
{
    IAutomationBase WrappedAutomation { get; }

    Task IInitializeOnStartup.Initialize()
    {
        IAutomationBase target = GetRoot();
        return (target as IInitializeOnStartup)?.Initialize() ?? Task.CompletedTask;
    }

    IAutomationBase GetRoot()
    {
        IAutomationBase target = WrappedAutomation;
        while (target is IAutomationWrapperBase wrapped)
        {
            target = wrapped.WrappedAutomation;
        }
        return target;
    }
}

internal interface IAutomationWrapper : IAutomation, IAutomationWrapperBase, IAutomationMeta;

