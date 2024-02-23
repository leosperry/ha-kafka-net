# HaKafkaNet
***
Version 4 Released! 
* [Durable automations](https://github.com/leosperry/ha-kafka-net/wiki/Durable-Automations) - Automations that will survive restarts.
* Strong typing entity states - This version brings several improvements to this area including methods for strong typing an entity's `State` and `Attributes` properties. See [State](https://github.com/leosperry/ha-kafka-net/wiki/State-Extension-Methods) and [Provider](https://github.com/leosperry/ha-kafka-net/wiki/Entity-Provider-Extension-Methods) extension methods.
* [Utility Classes](https://github.com/leosperry/ha-kafka-net/wiki/Utility-classes) - New classes and models for you to use

> Note: Several methods for retrieving entities have been marked deprecated, but should still function to allow time to upgrade. The reasons for these changes are to add consistency.  In most cases, you can simply change `Get()` or `GetEntityState()` to `GetEntity()`.

***
HaKafkaNet is an integration that uses Home Assistant Kafka integration for creating home automations in .NET
It was created with the following goals:
* Create Home Assistant automations in .NET with abilities to:
  * track/retrieve states of all entities in Home Assistant
  * respond to Home Assistant state changes
  * call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

#### Resources
* [Documentation](https://github.com/leosperry/ha-kafka-net/wiki)
* [Nuget package](https://www.nuget.org/packages/HaKafkaNet/)
* Join the new [Discord Server](https://discord.gg/RaGu72RbCt)

## Why ha-kafka-net ?
* Kafka allows you to replay events. Therefore, when your application starts, it can quickly load the states of all your Home Assistant entities, and even handle missed events based on your choosing. See [Event Timings](https://github.com/leosperry/ha-kafka-net/wiki/Event-Timings) for more details.
* Strongly typed access to entities
* UI to manage your automations and inspect Kafka consumers. 
* Monitoring capabilities through [`ISystemMonitor`](https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor)
  * Global Exception Handler
  * Be alerted of non-responsive entities
* Pre-built automations
* Extensible framework - create your own reusable automations
  * Extend automation factory with extension methods
  * Create your own automamtions from scratch
* Delayed event handling with multiple scheduling options
* Automation builder with fluent syntax for quickly creating automations.
* Full unit testability and componet level testing with Test Harness
* MIT license

### Dashboard
![Image of dashboard](/images/HaKafkaNetDashboardV4.png?raw=true)
This is an image of the dashboard from the example app.

## How it works
* Events are streamed from Home Assistant to the `home_assistant` topic. Unfortunately, the key is not utilizied by the provided home assistant kafka integration. Please upvote [this feature request](https://community.home-assistant.io/t/set-key-in-kafka-topic/671757/2)
* The transformer reads the messages and then adds them to the `home_assistant_states` topic with the entity id set as a key.
  - This allows us to compact the topic and make some assurances about order.
* A second consumer called the state handler reads from `home_assistant_states` topic and caches all state changes exposed by home assistant to Redis.
  - This allows for faster retrieval later and minimizes our application memory footprint. It also allows us to have some knowledge about which events were not handled between restarts and which ones were. The framework will tell your automation about such timings to allow you to handle messages appropriately.
* It then looks for automations which want to be notified.
  - If the entity id of the state change matches any of the `TriggerEntityIds` exposed by your automation, and the timing of the event matches your specified timings, then the `Execute` method of your automation will be called with a new `Task`.
  - It is up to the consumer to handle any errors. The framework prioritizes handling new messages speedily over tracking the state of individual automations. If your automation errors it will only write an ILogger message indicating the error.

## Features recently added
* [Strongly typed access to Entities](https://github.com/leosperry/ha-kafka-net/wiki/State-Extension-Methods).

## More examples
I have decided to make [my personal repository](https://github.com/leosperry/MyHome) public so that users can see working examples of some moderately complex automations.

If you have some examples you would like to share, please start a [discussion](https://github.com/leosperry/ha-kafka-net/discussions). I'd be happy to link it here.

Happy Automating!
