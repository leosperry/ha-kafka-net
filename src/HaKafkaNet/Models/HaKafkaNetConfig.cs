namespace HaKafkaNet;

public class HaKafkaNetConfig
{
    public string[] KafkaBrokerAddresses { get; set; } = ["localhost:9094"];
    public string TransofrmedTopic { get; set; } = "home_assistant_states";
    public bool ExposeKafkaFlowDashboard { get; set; } = true;
    public bool UseDashboard { get; set; } = false;
    public HaApiConfig Api { get; set; } = new();
    public StateHandlerConfig StateHandler { get; set; } = new();
    public TransformerConfig Transformer { get; set; } = new();
}

public class HaApiConfig
{
    public bool Enabled { get; set; } = true;
    public string BaseUri { get; set; } = "http://localhost:8123";
    public string AccessToken { get; set; } = "<ACCESS TOKEN>";
}

public class StateHandlerConfig
{
    public bool Enabled { get; set; } = true;
    public string GroupId { get; set; }= "hakafkanet-consumer";
    public int BufferSize { get; set; } = 5;
    public int WorkerCount { get; set; } = 5;
}

public class TransformerConfig
{
    public bool Enabled { get; set; } = true;
    public string GroupId { get; set; }= "hakafkanet-transformer";

    public int BufferSize { get; set; } = 5;
    public int WorkerCount { get; set; } = 5;
    public string HaRawTopic { get; set; } = "home_assistant";
}

