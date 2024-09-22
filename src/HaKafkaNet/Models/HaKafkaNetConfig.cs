namespace HaKafkaNet;

public class HaKafkaNetConfig
{
    HomeAssistantConnectionInfo _haConnection = new();

    public string[] KafkaBrokerAddresses { get; set; } = [];
    

    [Obsolete("Please rename your TransofrmedTopic element to KafkaTopic. This will be deleted in Version 9", false)]
    public string TransofrmedTopic 
    { 
        get => KafkaTopic; 
        set 
        {
            if (value is not null)
            {
                var foreground = Console.ForegroundColor;
                var background = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("************************************************************************************");
                Console.WriteLine("Please rename your 'TransofrmedTopic' element in your HaKafkaNetConfig to KafkaTopic");
                Console.WriteLine("************************************************************************************");
                Console.BackgroundColor = background;
                Console.ForegroundColor = foreground;
                KafkaTopic = value;
            }
        } 
    } 

    public string KafkaTopic { get; set; } = "home_assistant_states";    public bool ExposeKafkaFlowDashboard { get; set; } = true;

    public bool UseDashboard { get; set; } = false;

    public HomeAssistantConnectionInfo HaConnectionInfo 
    { 
        get => _haConnection; set => _haConnection = value; 
    }

    public StateHandlerConfig StateHandler { get; set; } = new();
}

public class HomeAssistantConnectionInfo
{
    public string BaseUri { get; set; } = "http://localhost:8123";
    public string AccessToken { get; set; } = "<ACCESS TOKEN>";
}

public class StateHandlerConfig
{
    public string GroupId { get; set; }= "hakafkanet-consumer";
    public int BufferSize { get; set; } = 5;
    public int WorkerCount { get; set; } = 5;
}


