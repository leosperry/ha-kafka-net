using System;
using HaKafkaNet;

namespace HaKafkaNet.ExampleApp.Automations;

public class TemplateRegistry : IAutomationRegistry, IInitializeOnStartup
{
    readonly IStartupHelpers _helpers;
    readonly IHaServices _services;
    readonly ILogger<TemplateRegistry> _logger;

    public TemplateRegistry(IStartupHelpers startupHelpers, IHaServices service, ILogger<TemplateRegistry> logger)
    {
        this._helpers = startupHelpers;
        this._services = service;
        this._logger = logger;
    }

    public Task Initialize()
    {
        return Task.CompletedTask;
    }

    public void Register(IRegistrar reg)
    {
        reg.Register(
            Simple1(),
            Simple2()
        );

        reg.RegisterDelayed(
            Delay1()
        );
    }

    IAutomation Simple1()
    {
        return _helpers.Builder.CreateSimple()
            // fill in automation
            .WithExecution(async (sc, ct) => await Task.CompletedTask)
            .Build();
    }

    IAutomation Simple2()
    {
        return _helpers.Factory.LightOnMotion("motionid", "lightid");
    }

    IDelayableAutomation Delay1()
    {
        return _helpers.Builder.CreateConditional()
            .When(sc => false)
            .WithExecution(async ct => await Task.CompletedTask)
            .Build();
    }
}
