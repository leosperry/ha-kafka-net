using System;
using HaKafkaNet.ExampleApp.Models;

namespace HaKafkaNet.ExampleApp.TestRegistry;

public class TestRegistry : IAutomationRegistry
{
    private readonly IAutomationBuilder _builder;
    private readonly IHaApiProvider _api;
    private readonly TimeProvider _time;

    public TestRegistry(IStartupHelpers helpers, IHaServices services, TimeProvider time)
    {
        this._builder = helpers.Builder;
        this._api = services.Api;
        this._time = time;
    }

    public void Register(IRegistrar reg)
    {
        reg.TryRegister(
            Simple,
            SimpleTyped,
            Conditional,
            ConditionalTyped,
            Schedulable,
            SchedulableTyped
        );
    }

    IAutomationBase Simple()
    {
        return _builder.CreateSimple()
            .WithName($"{nameof(TestRegistry)}.{nameof(Simple)}")
            .WithTriggers(Binary_Sensor.MotionForSimple)
            .WithExecution(async (sc, ct) =>
            {
                await _api.ButtonPress(Input_Button.HelperButtonForSimple, ct);
            })
            .Build();
    }

    IAutomationBase SimpleTyped()
    {
        return _builder.CreateSimple<OnOff>()
            .WithName($"{nameof(TestRegistry)}.{nameof(SimpleTyped)}")
            .WithTriggers(Binary_Sensor.MotionForSimpleTyped)
            .WithExecution(async (sc, ct) =>
            {
                await _api.ButtonPress(Input_Button.HelperButtonForSimpleTyped, ct);
            })
            .Build();
    }

    IAutomationBase Conditional()
    {
        return _builder.CreateConditional()
            .WithName($"{nameof(TestRegistry)}.{nameof(Conditional)}")
            .WithTriggers(Binary_Sensor.MotionForConditional)
            .When(sc => true )
            .For(TimeSpan.Zero)
            .Then(async ct => {
                await _api.ButtonPress(Input_Button.HelperButtonForConditional, ct);
            })
            .Build();
    }

    IAutomationBase ConditionalTyped()
    {
        return _builder.CreateConditional<OnOff>()
            .WithName($"{nameof(TestRegistry)}.{nameof(ConditionalTyped)}")
            .WithTriggers(Binary_Sensor.MotionForConditionalTyped)
            .When((sc, ct) => Task.FromResult(true))
            .For(TimeSpan.Zero)
            .Then(async ct => {
                await _api.ButtonPress(Input_Button.HelperButtonForConditionalTyped, ct);
            })
            .Build();
    }

    IAutomationBase Schedulable()
    {
        return _builder.CreateSchedulable()
            .WithName($"{nameof(TestRegistry)}.{nameof(Schedulable)}")
            .WithTriggers(Binary_Sensor.MotionForSchedulable)
            .GetNextScheduled((sc, ct) => 
            { 
                return Task.FromResult<DateTimeOffset?>(_time.GetLocalNow().AddSeconds(1));
            })
            .WithExecution(ct =>
            {
                 return _api.ButtonPress(Input_Button.HelperButtonForSchedulable, ct);
            })
            .Build();
    }

    IAutomationBase SchedulableTyped()
    {
        return _builder.CreateSchedulable()
            .WithName($"{nameof(TestRegistry)}.{nameof(SchedulableTyped)}")
            .WithTriggers(Binary_Sensor.MotionForSchedulableTyped)
            .While(sc => true)
            .ForSeconds(1)
            .WithExecution(ct => _api.ButtonPress(Input_Button.HelperButtonForSchedulableTyped, ct))
            .Build();
    }
}
