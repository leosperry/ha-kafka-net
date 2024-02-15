﻿namespace HaKafkaNet;

public interface IAutomationRegistry
{
    // [Obsolete("Please use Register(IRegistrar reg) instead",false)]
    // IEnumerable<IAutomation> Register() => Enumerable.Empty<IAutomation>();

    // [Obsolete("Please use Register(IRegistrar reg) instead",false)]
    // IEnumerable<IConditionalAutomation> RegisterContitionals() => Enumerable.Empty<IConditionalAutomation>();

    void Register(IRegistrar reg);
}

public interface IRegistrar
{
    
    void Register(IAutomation automation);
    void Register(IDelayableAutomation automation);
    void Register<T>(T automation, DelayEvaluator<T> delayEvaluator)
        where T : IDelayableAutomation;

    void RegisterMultiple(IEnumerable<IAutomation> automations);
    void RegisterMultiple(IEnumerable<IDelayableAutomation> automations);

    void RegisterMultiple(params IAutomation[] automations);
    void RegisterMultiple(params IDelayableAutomation[] automations);
    
    void RegisterMultiple<T>(IEnumerable<T> automations, DelayEvaluator<T> delayEvaluator)
        where T : IDelayableAutomation;
}

internal interface IInternalRegistrar : IRegistrar
{
    IEnumerable<IAutomationWrapper> Registered { get; }
}

public delegate TimeSpan DelayEvaluator<in T>(T automation) where T : IDelayableAutomation;