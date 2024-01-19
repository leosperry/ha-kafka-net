namespace HaKafkaNet;

public interface IAutomationRegistry
{
    IEnumerable<IAutomation> Register(IAutomationFactory automationFactory);
    IEnumerable<IConditionalAutomation> RegisterContitionals(IAutomationFactory automationFactory);
}
