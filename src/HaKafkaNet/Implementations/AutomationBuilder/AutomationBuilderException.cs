namespace HaKafkaNet;

/// <summary>
/// Thrown when a builder does not have enough information provided to construct an automation
/// </summary>
public class AutomationBuilderException : Exception
{
    internal AutomationBuilderException(string message): base(message){}
}
