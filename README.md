
<img src="/images/hkn_128.png?raw=true" align="left" />

<h3>HaKafkaNet</h3> 
A library for easily creating Home Assistant automations in .NET and C#.

Kafka ensures automations are durable and state is restored between restarts.

***
Version 7 Released! 
* HaKafkaNet now natively supports [Open Telemetry](https://github.com/leosperry/ha-kafka-net/wiki/Open-Telemetry-Instrumentation)! Get detailed insights into how your system is operating.
***

It was created with the following goals:
* Create Home Assistant automations in .NET / C# with abilities to:
  * track/retrieve states of all entities in Home Assistant
  * respond to Home Assistant state changes
  * call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

## Example
Example of multiple durable automations. See [Tutorial](https://github.com/leosperry/ha-kafka-net/wiki/Tutorial:-Creating-Automations) for more examples.
```csharp
registrar.RegisterMultiple(
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
* Join the new [Discord Server](https://discord.gg/RaGu72RbCt)

## Why ha-kafka-net ? 
* [Strongly typed](https://github.com/leosperry/ha-kafka-net/wiki/State-Extension-Methods) access to entities
* Durable - Automations that [survive restarts](https://github.com/leosperry/ha-kafka-net/wiki/Durable-Automations). See also [Event Timings](https://github.com/leosperry/ha-kafka-net/wiki/Event-Timings)
* Fast - Automations run in parallel and asynchronously.
* [UI](https://github.com/leosperry/ha-kafka-net/wiki/UI) to manage your automations and inspect Kafka consumers. 
* Observability through
  * [ISystemMonitor](https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor)
  * [Tracing with log capturing](https://github.com/leosperry/ha-kafka-net/wiki/Tracing) 
* [Pre-built automations](https://github.com/leosperry/ha-kafka-net/wiki/Factory-Automations)
* Extensible framework - [create your own reusable automations](https://github.com/leosperry/ha-kafka-net/wiki/Tutorial:-Creating-Automations)
  * Extend automation factory with extension methods
  * Create your own automamtions from scratch
* [Automation builder](https://github.com/leosperry/ha-kafka-net/wiki/Automation-Registry#iautomationbuilder-interface) with fluent syntax for quickly creating automations.
* Full unit testability and componet level testing with [Test Harness](https://github.com/leosperry/ha-kafka-net/wiki/Automated-Testing)
* MIT license

### Dashboard
![Image of dashboard](https://raw.githubusercontent.com/leosperry/ha-kafka-net/main/images/UI%20Examples/Dashboard-V5_5.PNG)
This is an image of the dashboard from the example app. See [UI](https://github.com/leosperry/ha-kafka-net/wiki/UI) for additional details.

## How it works
* Events are streamed from Home Assistant to the `home_assistant` topic. Unfortunately, the key is not utilizied by the provided home assistant kafka integration. Please upvote [this feature request](https://community.home-assistant.io/t/set-key-in-kafka-topic/671757/2)
* The transformer reads the messages and then adds them to the `home_assistant_states` topic with the entity id set as a key.
  - This allows us to compact the topic and make some assurances about order.
* A second consumer called the state handler reads from `home_assistant_states` topic and caches all state changes exposed by home assistant to Redis.
  - This allows for faster retrieval later and minimizes our application memory footprint. It also allows us to have some knowledge about which events were not handled between restarts and which ones were. The framework will tell your automation about such timings to allow you to handle messages appropriately.
* It then looks for automations which want to be notified.
  - If the entity id of the state change matches any of the `TriggerEntityIds` exposed by your automation, and the timing of the event matches your specified timings, then the `Execute` method of your automation will be called with a new `Task`.
  - It is up to the consumer to handle any errors. The framework prioritizes handling new messages speedily over tracking the state of individual automations. If your automation errors it will only write an ILogger message indicating the error.

## More examples
I have made [my personal repository](https://github.com/leosperry/MyHome) public so that users can see working examples of some moderately complex automations.

If you have some examples you would like to share, please start a [discussion](https://github.com/leosperry/ha-kafka-net/discussions). I'd be happy to link it here.

Happy Automating!
