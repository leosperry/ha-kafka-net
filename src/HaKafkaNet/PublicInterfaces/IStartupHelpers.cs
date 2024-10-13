namespace HaKafkaNet;

public interface IStartupHelpers
{
    public IAutomationBuilder Builder { get; }
    public IAutomationFactory Factory { get; }
    public IUpdatingEntityProvider UpdatingEntityProvider { get; }
}
