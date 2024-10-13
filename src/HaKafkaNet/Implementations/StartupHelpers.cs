namespace HaKafkaNet;

public class StartupHelpers(IAutomationBuilder builder, IAutomationFactory factory, IUpdatingEntityProvider updatingEntityProvider) : IStartupHelpers
{
    public IAutomationBuilder Builder { get => builder; } 

    public IAutomationFactory Factory { get => factory;}

    public IUpdatingEntityProvider UpdatingEntityProvider { get => updatingEntityProvider;}
}
