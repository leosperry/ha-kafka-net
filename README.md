
<img src="/images/hkn_128.png?raw=true" align="left" />

<h3>HaKafkaNet</h3> 
A library for easily creating Home Assistant automations in .NET and C#.

Kafka ensures automations are durable and state is restored between restarts.

***
Version 11, is released and uses .NET 9. It also introduces integration testing. See [special release statement](https://github.com/leosperry/ha-kafka-net/wiki/Updating-to-Version-11) for details.
***
Featured on an episode of [On .NET Live](https://www.youtube.com/live/rEY9Bi0jOiE) !

[![HaKafkaNet featured On .NET Live](http://img.youtube.com/vi/rEY9Bi0jOiE/hqdefault.jpg)](https://www.youtube.com/live/rEY9Bi0jOiE)

***

It was created with the following goals:
* Create Home Assistant automations in .NET / C# with abilities to:
  * track/retrieve states of all entities in Home Assistant
  * respond to Home Assistant state changes
  * call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

## Example
Example of multiple durable automations. See [Tutorial](https://github.com/leosperry/ha-kafka-net/wiki/Tutorial-%E2%80%90-Creating-Automations) for more examples.
```csharp
registrar.TryRegister(
    _factory.SunRiseAutomation(
        cancelToken => _api.TurnOff("light.night_light", cancelToken)),
    _factory.SunSetAutomation(
        cancelToken => _api.TurnOn("light.night_light", cancelToken),
        TimeSpan.FromMinutes(-10))
);
```

## Resources
* [Documentation](https://github.com/leosperry/ha-kafka-net/wiki)
* [Nuget package](https://www.nuget.org/packages/HaKafkaNet/)
* Join the [Discord Server](https://discord.gg/RaGu72RbCt)

## Why ha-kafka-net ? 
* [Strongly typed](https://github.com/leosperry/ha-kafka-net/wiki/State-Extension-Methods) access to entities
* Durable - Automations that [survive restarts](https://github.com/leosperry/ha-kafka-net/wiki/Durable-Automations). See also [Event Timings](https://github.com/leosperry/ha-kafka-net/wiki/Event-Timings)
* Fast - Automations run in [parallel](https://github.com/leosperry/ha-kafka-net/wiki/Parallelism-and-threads-in-automations) and asynchronously.
* [UI](https://github.com/leosperry/ha-kafka-net/wiki/UI) to manage your automations and inspect Kafka consumers. 
* Observability through
  * [ISystemMonitor](https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor)
  * [Tracing with log capturing](https://github.com/leosperry/ha-kafka-net/wiki/Tracing)
  * [Open Telemetry Instrumentation](https://github.com/leosperry/ha-kafka-net/wiki/Open-Telemetry-Instrumentation)
* [Pre-built automations](https://github.com/leosperry/ha-kafka-net/wiki/Factory-Automations)
* Extensible framework - [create your own reusable automations](https://github.com/leosperry/ha-kafka-net/wiki/Tutorial:-Creating-Automations)
  * Extend automation factory with extension methods
  * Create your own automations from scratch
* [Automation builder](https://github.com/leosperry/ha-kafka-net/wiki/Automation-Builder) with fluent syntax for quickly creating automations.
* Full unit testability for custom automations and integration testing for all registered automations.
* MIT license

## What others have said:
> All in all, I'm really happy HaKafkaNet is a thing! It's a really ergonomic way of writing automations that make sense.

> I converted the automations for 3 rooms from Home Assistant automations to C#, and those work good! So I'm really satisfied with the project.

### Dashboard
![Image of dashboard](https://raw.githubusercontent.com/leosperry/ha-kafka-net/main/images/UI%20Examples/Dashboard-V5_5.PNG)
This is an image of the dashboard from the example app. See [UI](https://github.com/leosperry/ha-kafka-net/wiki/UI) for additional details.

## How it works
* State changes are sent from Home Assistant to a Kafka topic
* HaKafkaNet reads all state changes
* States for every entity are cached allowing for faster retrieval later.
  - It also allows us to have some knowledge about which events were not handled between restarts and which ones were. The framework will tell your automation about such timings to allow you to handle messages appropriately.
* It then looks for automations which want to be notified.
  - If the entity id of the state change matches any of the `TriggerEntityIds` exposed by your automation, and the timing of the event matches your specified timings, then the `Execute` method of your automation will be called with a new `Task`.
  - All of your automations will be called asynchronously and in parallel. 

## More examples
I have made [my personal repository](https://github.com/leosperry/MyHome) public so that users can see working examples of some moderately complex automations.

If you have some examples you would like to share, please start a [discussion](https://github.com/leosperry/ha-kafka-net/discussions). I'd be happy to link it here.

Happy Automating!
