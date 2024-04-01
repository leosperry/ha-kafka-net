using HaKafkaNet;
using NLog.Web;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog(); // enables log tracing

var services = builder.Services;

// // for local development of dashboard only
// services.AddCors(options => {
//     options.AddPolicy("hknDev", policy =>{
//         policy.WithOrigins("*");
//         policy.AllowAnyHeader();
//     });
// });

HaKafkaNetConfig config = new HaKafkaNetConfig();
builder.Configuration.GetSection("HaKafkaNet").Bind(config);
services.AddHaKafkaNet(config);

// provide an IDistributedCache implementation
var redisUri = builder.Configuration.GetConnectionString("RedisConStr");
services.AddStackExchangeRedisCache(options => 
{
    options.Configuration = redisUri;
    /* optionally prefix keys */
    options.InstanceName = "ExampleApp.";
});

// get rid of warning about AspNetCore protecting keys
var redis = ConnectionMultiplexer.Connect(redisUri!);
services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

// programatic example of configuration
// services.AddHaKafkaNet(options =>{
//     //minimum amount of config
//     options.KafkaBrokerAddresses = ["your kafka instance"];
//     options.HaConnectionInfo.AccessToken = "your access token";
//     options.HaConnectionInfo.BaseUri = "your Home Assistant location";
//     //set additional options
// });

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("hakafkanet"));

// // for local development of dashboard only
// app.UseCors("hknDev");

await app.StartHaKafkaNet();

app.Run();

