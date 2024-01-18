namespace HaKafkaNet;

public interface IAutomationRegistry
{
    IEnumerable<IAutomation> Register(IAutomationFactory automationFactory);
}
