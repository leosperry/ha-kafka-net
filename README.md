# ha-kafka-net
***
Version 3 Released. See [Release](https://github.com/leosperry/ha-kafka-net/releases/tag/v3.0.0) for details. It has one significant breaking change. See release notes or the example app shows changes needed in [`AutomationRegistry.cs`](https://github.com/leosperry/ha-kafka-net/blob/main/example/HaKafkaNet.ExampleApp/Automations/AutomationRegistry.cs).
***
HaKafkaNet is an integration that uses Home Assistant Kafka integration for creating home automations in .NET
It was created with the following goals:
* Create Home Assistant automations in .NET with abilities to:
  * track/retrieve states of all entities in Home Assistant
  * respond to Home Assistant state changes
  * call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

#### Resources
* [Getting Started](https://github.com/leosperry/ha-kafka-net/wiki/Getting-Started)
* [Nuget package](https://www.nuget.org/packages/HaKafkaNet/)
* [Test Harness Nuget](https://www.nuget.org/packages/HaKafkaNet.TestHarness/)
* [Full Documentation](https://github.com/leosperry/ha-kafka-net/wiki)
* Join the new [Discord Server](https://discord.gg/RaGu72RbCt)

## Why ha-kafka-net ?
* Kafka allows you to replay events. Therefore, when your application starts, it can quickly load the states of all your Home Assistant entities, and even handle missed events based on your choosing.
* UI to manage your automations and inspect Kafka consumers.
* Monitoring capabilities through [`ISystemMonitor`](https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor)
  * Global Exception Handler
  * Be alerted of non-responsive entities
* Pre-built automations
  * Lights On/Off from Motion sensors
  * Sun rise/set/etc events including offset (e.g. Run 15 minutes before sunset)
* Extensible framework - create your own reusable automations
  * Extend automation factory with extension methods
  * Create your own automamtions from scratch
* Delayed event handling with multiple scheduling options
* Automation builder with fluent syntax for quickly creating automations.
* Full unit testability and componet level testing with Test Harness
* MIT license

### Dashboard
![Image of dashboard](/images/HaKafkaNetDashboard.png?raw=true)
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
 
## Getting started with the Example App:
1. Follow instructions for [Getting Started](https://github.com/leosperry/ha-kafka-net/wiki/Getting-Started).
2. In your HomeAssistant UI, create helper buttons named:
   - `Test Button`
   - `Test Button 2`
   - `Test Button 3`
2. Look through the provided examples for ID's of lights/sensors and set them to match your environment.
3. Click your test buttons both while your application is up and while it is down to see different behaviors at starup.

## Tips
* During start up, it can take a minute or two for it to churn though thousands of events. In the output, you can see which kafka offsets have been handled. You can then compare that to the current Kafka offset which you can discover from your kafka-ui instance.
* You can run the transformer seperately from the state manager and your automations. This allows you to constantly have the transformers work up to date and have your applications running your automations have less work to do at startup.
* If you are running a dev instance alongside your production instance, you can reuse the same kafka instance, but it is recommended to change the 'GroupId' in your appsettings.json. This will ensure your development instance does not steal events from your production instance.
* You can raise state change events by setting them manually in the developer tools of your Home Assisstant instance. This won't change the actual states of your devices, but it will send the events through Kafka.

## Features recently added
* [`ISystemMonitor`](https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor) for handling errors and monitoring non-responsive entities.
* A brand new [UI](https://github.com/leosperry/ha-kafka-net/wiki/UI)! Currently it lists all your automations, where they came from and an ability to enable/disable them at runtime
* Test helper methods and a [test harness](https://github.com/leosperry/ha-kafka-net/wiki/Automated-Testing) for component level testing of your registries.
  * Test Harness also supports `ISystemMonitor`
* [Automation Builder](https://github.com/leosperry/ha-kafka-net/wiki/Automation-Registry#iautomationbuilder-interface) with fluent syntax
* Sun model and Sun Based Automations
* Scheduled Automation framework

## More examples
I have decided to make [my personal repository](https://github.com/leosperry/MyHome) public so that users can see working examples of some moderately complex automations.

If you have some examples you would like to share, please start a [discussion](https://github.com/leosperry/ha-kafka-net/discussions). I'd be happy to link it here.

Happy Automating!
