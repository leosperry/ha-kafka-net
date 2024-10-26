namespace HaKafkaNet.ExampleApp;

public class LightOffRegistry : IAutomationRegistry
{
    private readonly IHaServices _services;
    private readonly IAutomationFactory _facory;
    private readonly IAutomationBuilder _builder;

    const string OFFICE_MOTION = "binary_sensor.office_motion";
    const string OFFICE_LIGHT = "light.office_light";

    public LightOffRegistry(IHaServices services, IAutomationFactory automationFactory, IAutomationBuilder builder)
    {
        this._services = services;
        _facory = automationFactory;
        _builder = builder;
    }

    public void Register(IRegistrar reg)
    {
        reg.TryRegister(RegisterContitionals().ToArray());
    }

    public IEnumerable<IAutomation> Register()
    {
        return Enumerable.Empty<IAutomation>();
    }

    public IEnumerable<IConditionalAutomation> RegisterContitionals()
    {
        yield return _facory.LightOffOnNoMotion(OFFICE_MOTION, OFFICE_LIGHT, TimeSpan.FromMinutes(5))
            .WithMeta("Office Light Off When No Motion", "from factory prebuilt", false);

        yield return _facory.ConditionalAutomation(
            [OFFICE_MOTION], 
            (stateChange, ct)=> Task.FromResult(stateChange.New.State == "off"),
            TimeSpan.FromMinutes(5),
            ct => _services.Api.TurnOff(OFFICE_LIGHT, ct))
            .WithMeta("Office Light Off When No Motion", "from factory manual", false);

        yield return _builder.CreateConditional(false)
            .WithName("Office Light Off When No Motion")
            .WithDescription("from builder")
            .WithTriggers(OFFICE_MOTION)
            .WithAdditionalEntitiesToTrack(OFFICE_LIGHT)
            .When(stateChange => stateChange.New.State == "off")
            .ForMinutes(5)
            .Then(ct => _services.Api.TurnOff(OFFICE_LIGHT, ct))
            .Build();    
    }
}
