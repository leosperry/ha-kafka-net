namespace HaKafkaNet;

public class HaKafkaNetConfig
{
    HomeAssistantConnectionInfo _haConnection = new();

    public string[] KafkaBrokerAddresses { get; set; } = ["localhost:9094"];
    public string TransofrmedTopic { get; set; } = "home_assistant_states";
    public bool ExposeKafkaFlowDashboard { get; set; } = true;
    public bool UseDashboard { get; set; } = false;

    [Obsolete("Please rename your Api element to HaConnectionInfo. This will be deleted in Version 3", false)]
    public HomeAssistantConnectionInfo? Api 
    { 
        get => null; 
        set  
        {
            if (value is not null)
            {
                var foreground = Console.ForegroundColor;
                var background = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("*****************************************************************************");
                Console.WriteLine("Please rename your 'Api' element in your HaKafkaNetConfig to HaConnectionInfo");
                Console.WriteLine("*****************************************************************************");
                Console.BackgroundColor = background;
                Console.ForegroundColor = foreground;
                _haConnection = value!;
            }
        } 
    }

    public HomeAssistantConnectionInfo HaConnectionInfo 
    { 
        get => _haConnection; set => _haConnection = value; 
    }

    public StateHandlerConfig StateHandler { get; set; } = new();
    public TransformerConfig Transformer { get; set; } = new();

    public EntityTrackerConfig EntityTracker { get; set; } = new();
}

public class HomeAssistantConnectionInfo
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

public class EntityTrackerConfig
{
    public bool Enabled { get; set; } = false;
    public int IntervalMinutes { get; set; } = 60;
    public int MaxEntityNonresponsiveHours { get; set; } = 12;
}

