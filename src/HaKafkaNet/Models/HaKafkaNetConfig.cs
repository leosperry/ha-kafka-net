namespace HaKafkaNet;

/// <summary>
/// configuration information for HaKafkNet
/// </summary>
public class HaKafkaNetConfig
{
    HomeAssistantConnectionInfo _haConnection = new();

    /// <summary>
    /// broker addresses sent to KafkaFlow
    /// </summary>
    public string[] KafkaBrokerAddresses { get; set; } = [];

    /// <summary>
    /// Topic to be used to read states
    /// </summary>
    public string KafkaTopic { get; set; } = "home_assistant_states";  
    
    /// <summary>
    /// Enables the KafkaFlow dashboard
    /// </summary>
    public bool ExposeKafkaFlowDashboard { get; set; } = true;

    /// <summary>
    /// Enables the HaKafkaNet dashboard
    /// </summary>
    public bool UseDashboard { get; set; } = false;

    /// <summary>
    /// required connection info for Home Assistant
    /// </summary>
    public HomeAssistantConnectionInfo HaConnectionInfo 
    { 
        get => _haConnection; set => _haConnection = value; 
    }

    /// <summary>
    /// contains information for configuring Kafka consumer
    /// </summary>
    public StateHandlerConfig StateHandler { get; set; } = new();
}

/// <summary>
/// your Home Assistant connection information
/// </summary>
public class HomeAssistantConnectionInfo
{
    /// <summary>
    /// Location of your Home Assistant instance
    /// </summary>
    public string BaseUri { get; set; } = "http://localhost:8123";

    /// <summary>
    /// user defined long lived access token for Home Assistant
    /// </summary>
    public string AccessToken { get; set; } = "<ACCESS TOKEN>";
}

/// <summary>
/// see: https://farfetch.github.io/kafkaflow/docs/guides/consumers/
/// </summary>
public class StateHandlerConfig
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public string GroupId { get; set; }= "hakafkanet-consumer";
    public int BufferSize { get; set; } = 5;
    public int WorkerCount { get; set; } = 5;
}


