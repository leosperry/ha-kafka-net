namespace HaKafkaNet;

/// <summary>
/// Exception throw by HakafkaNet throughout the stack.
/// Basic exception so that you can explicitly catch framework exceptions
/// </summary>
public class HaKafkaNetException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public HaKafkaNetException(string message): base(message)
    {
        
    }
}
