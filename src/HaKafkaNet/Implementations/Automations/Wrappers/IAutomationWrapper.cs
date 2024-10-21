using System;

namespace HaKafkaNet;

internal interface IAutomationWrapperBase : IInitializeOnStartup, IAutomationMeta
{
    IAutomationBase WrappedAutomation { get; }

    Task IInitializeOnStartup.Initialize() => (WrappedAutomation as IInitializeOnStartup)?.Initialize() ?? Task.CompletedTask;
    AutomationMetaData IAutomationMeta.GetMetaData() => (WrappedAutomation as IAutomationMeta)?.GetMetaData() ?? AutomationMetaData.Create(this.WrappedAutomation);
}

internal interface IAutomationWrapper : IAutomation, IAutomationWrapperBase;

