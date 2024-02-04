using HaKafkaNet;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

HaKafkaNetConfig config = new HaKafkaNetConfig();
builder.Configuration.GetSection("HaKafkaNet").Bind(config);

// provide an IDistributedCache implementation
services.AddStackExchangeRedisCache(options => 
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConStr");
    /* optionally prefix keys */
    //options.InstanceName = "HaKafkaNet";
});

//services.AddHaKafkaNet(config);
services.AddHaKafkaNet(options =>{
    var haConfif = options.HaConnectionInfo;
    haConfif.AccessToken = config.HaConnectionInfo.AccessToken;
    haConfif.BaseUri = config.HaConnectionInfo.BaseUri;

    options.KafkaBrokerAddresses = config.KafkaBrokerAddresses;

    options.Transformer.Enabled = false;
    options.StateHandler.Enabled = true;
    options.EntityTracker.Enabled = true;
    options.UseDashboard = false;
});

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("dashboard.html"));


await app.StartHaKafkaNet();

app.Run();

