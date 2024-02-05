using HaKafkaNet;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

HaKafkaNetConfig config = new HaKafkaNetConfig();
builder.Configuration.GetSection("HaKafkaNet").Bind(config);
services.AddHaKafkaNet(config);

// provide an IDistributedCache implementation
services.AddStackExchangeRedisCache(options => 
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConStr");
    /* optionally prefix keys */
    //options.InstanceName = "HaKafkaNet";
});


// services.AddHaKafkaNet(options =>{
//     //minimum amount of config
//     options.KafkaBrokerAddresses = ["your kafka instance"];
//     options.HaConnectionInfo.AccessToken = "your access token";
//     options.HaConnectionInfo.BaseUri = "your Home Assistant location";
//     //set additional options
// });

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("dashboard.html"));

await app.StartHaKafkaNet();

app.Run();

