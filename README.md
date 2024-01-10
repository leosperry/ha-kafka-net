# ha-kafka-net
Integration that uses Home Assistant Kafka integration for creating home automations in .NET
It was created with the following goals:
* Create Home Assistant automations in .NET
* Expose a simple way to track states of all entities in Home Asstant
* Expose a simple way to respond to Home Assistant state changes
* Provide a means to call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

This project is still in an alpha state. No nuget pakage is yet created. More features are forthcoming.

## Why ha-kafka-net ?
* Kafka allows you to replay events. Therefore, when your application starts, it can quickly load the states of all your Home Assistant entities.
* It gives you a easy-to-spin up infrastructure with management features out of the box. This includes docker images for both managing kafka and seeing the state of your consumers based on open source projects
* You have an easy way to respond to events during start up which means you are guarenteed to see/handle all events at least once, even if your application has been down.
* Full unit testability
* MIT license

## How it works
* Events are streamed from Home Assistant to the `home_assistant` topic. Unfortunately, the key is not utilizied by the provided home assistant kafka integration. 
* The transformer reads the messages and then adds them to the `home_assistant_states` topic with the entity id set as a key.
  - This allows us to compact the topic and make some assurances about order.
* A second consumer called the state handler reads from `home_assistant_states` topic and caches all state changes exposed by home assistant to Redis.
  - This allows for faster retrieval later and minimizes our application memory footprint. It also allows us to have some knowledge about which events were not handled between restarts and which ones were. The framework will tell your automation about such timings to allow you to handle messages appropriately.
* It then looks for automations which want to be notified.
  - If the entity id of the state change matches any of the `TriggerEntityIds` exposed by your automation, and the timing of the event matches your specified timings, then the `Execute` method of your automation will be called with a new `Task`.
  - It is up to the consumer to handle any errors. The framework prioritizes handling new messages speedily over tracking the state of individual automations. If your automation erros it will only write a console message indicating the error(s).

## Current steps for Example App set up:
1. Edit the `~/infrastructure/docker-compose.yml` for your environment and run it.
   - you  need to edit the values for the external listner of `KAFKA_CFG_ADVERTISED_LISTENERS` and persistent storage.
2. Connect to your kafka-ui instance on port 8080 and create two topics.
   - home_assistant - This is where Home Assistant will deposit state changes. Set the retention to your liking.
   - home_assistant_states - This is where the transformer will add keys to the messages based on the Home Assistant EntityId. Set the clean up policy to `compact`. Optionally add a custom parameter named `max.compaction.lag.ms` to force compaction to run more often which should save space and prevent excessively handling of old messages at startup.
3. Launch your Home Assistant UI and edit your `configuration.yaml` to include kafka.
   - see [Apache Kafka integration documentation](https://www.home-assistant.io/integrations/apache_kafka/)
   - set the topic to `home_assistant`
   - set the port to `9094` if not running on the same machine.
   - It is recommended to set an `include_domains` filter, otherwise you will produce hundreds of thousands of events every week. You should include all domains for all entities that you plan to respond to or inspect in your automaions. For the included examples to run, you should, at a minimum, include:
     - `light`
     - `input_button`
4. Restart Home Assistant
   - At this point events should be streaming from Home Assistant into the `home_assistant` topic, which you can inspect via your kafka-ui instance.
5. In the `~/example/HakafkaNet.ExampleApp` directory, create an `appsettings.Development.json` file.
   - Copy/paste the contents of the `appsettings.json` file and modify appropriately.

At this point your environment is set up and ready for development. If you run the example app, you can watch the consumers via a dashboard provided at  `localhost:<port>/kafkaflow`. It is provided via [KafkaFlow](https://github.com/Farfetch/kafkaflow). You could also connect to redis to see events being cached. To see the example automations in action, continue with these steps:
1. In your HomeAssistant UI, create two helper buttons named:
   - `Test Button`
   - `Test Button 2`
2. Modify the `example/HaKafkaNet.ExampleApp/Automations/SimpleLightAutomation.cs` file and set `_idOfLightToDim` to an id of a light that exists in your Home Assistant instance
3. Click your test buttons both while your application is up and while it is down to see different behaviors at starup.

## TODO:
* CI/CD and Nuget package
* More automated tests
* Enhanced API functionality
