
using System.Reflection.Metadata.Ecma335;

namespace HaKafkaNet.ExampleApp;

/// <summary>
/// This class has 6 different implementations of the same automation 
/// created expressly for the purpose of demonstrating the options
/// you have for structuring your code
/// </summary>
public class LightOnRegistry : IAutomationRegistry
{
    private readonly IHaServices _services;
    private readonly IAutomationBuilder _builder;
    private readonly IAutomationFactory _factory;

    public const string OFFICE_MOTION = "binary_sensor.office_motion";
    public const string OFFICE_LIGHT = "light.office_light";

    public LightOnRegistry(IHaServices services, IAutomationBuilder builder, IAutomationFactory factory)
    {
        _services = services;
        _builder = builder;
        _factory = factory;
    }

    public IEnumerable<IAutomation> Register()
    {
        //prebuilt for your convenience
        //you could write your own extension methods if you like
        yield return _factory.LightOnMotion(OFFICE_MOTION, OFFICE_LIGHT)
            //meta data is completely optional
            .WithMeta(new AutomationMetaData(){
                Name = "Office Light On Motion",        //defaults to GetType().Name
                Description = "from factory prebuilt",  //defaults to GetType().FullName
                Enabled = false                         //defaults to true
            });

        //use the factory to create any automation
        //you could put logic like this in an extension method
        yield return _factory.SimpleAutomation(
            [OFFICE_MOTION],
            async (stateChange, ct)=> {
                if (stateChange.New.State == "on")
                {
                    await _services.Api.LightTurnOn(OFFICE_LIGHT, ct);
                }
            }).WithMeta(new AutomationMetaData(){
                Name = "Office Light On Motion",
                Description = "from factory manual",
                Enabled = false
            });
        
        //create your own automations for reuse
        //you could also put a manual construction like this in a facotry extension method
        yield return new LightOnCustomAutomation(_services.Api, OFFICE_MOTION, OFFICE_LIGHT, 200, "Office Light On Motion", "custom built");
        
        //the builder offers a more descriptive way create automations
        yield return _builder.CreateSimple(false) // false is for enabled at startup which defaults to true if not specified
            .WithName("Office Light On Motion")
            .WithDescription("from builder without services")
            .WithTriggers(OFFICE_MOTION)
            .WithExecution(async (stateChange, ct) => {
                if (stateChange.New.State == "on")
                {
                    //services reference comes from this class
                    await _services.Api.LightTurnOn(OFFICE_LIGHT, ct);
                }
            })
            .Build();
        
        // you can rely on the builder to pass in home assistant services
        yield return _builder.CreateSimpleWithServices(false)
            .WithName("Office Light On Motion")
            .WithDescription("from builder with services")
            .WithTriggers(OFFICE_MOTION)
            .WithExecution(async (svc, stateChange, ct) =>{
                if (stateChange.New.State == "on")
                {
                    //services reference injected into callback
                    await svc.Api.LightTurnOn(OFFICE_LIGHT, ct);
                }
            })
            .Build();
    }

    public IEnumerable<IConditionalAutomation> RegisterContitionals()
    {
        //the conditional automation us typically used to run an automation on a delay
        //by not specifying the 'For" it defaults to TimeSpan.Zero and runs immediately
        //this allows you to clean up some of the if statements in the examples above
        yield return _builder.CreateConditional(false)
            .WithName("Office Light On Motion")
            .WithDescription("from builder using conditional")
            .WithTriggers(OFFICE_MOTION)
            .When(sc => sc.New.State == "on") // there is an asynchronous overload for this method should you need to call services
            .Then(ct => _services.Api.LightTurnOn(OFFICE_LIGHT, ct))
            .Build();
    }
}
