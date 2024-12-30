using System;
using System.Text.Json;

namespace HaKafkaNet.ExampleApp.TestClasses;

public class ActiveRegistry : IAutomationRegistry
{
    private readonly IAutomationBuilder _builder;
    private readonly IHaApiProvider _api;

    public ActiveRegistry(IAutomationBuilder builder, IHaApiProvider services)
    {
        this._builder = builder;
        this._api = services;
    }

    public void Register(IRegistrar reg)
    {
        reg.TryRegister(SimpleActive);
    }

    IAutomationBase SimpleActive()
    {
        return _builder.CreateSimple()
            .MakeActive()
            .WithTriggers("my.button")
            .WithExecution((sc, ct) =>
            {
                _api.ButtonPress("my.button", default);
                return Task.CompletedTask;
            })
            .Build();
    }
}
