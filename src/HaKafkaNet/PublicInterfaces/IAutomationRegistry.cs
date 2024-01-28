namespace HaKafkaNet;

public interface IAutomationRegistry
{
    IEnumerable<IAutomation> Register();
    IEnumerable<IConditionalAutomation> RegisterContitionals();
}
