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

## Current steps for Example App set up:
1. run the `~/infrastructure/docker-compose.yml`
2. connect to your kafka-ui instance on port 8080
3. create two topics
   - home_assistant - This is where Home Assistant will deposit state changes
   - home_assistant_states - This is where the transformer will add keys to the messages based on the Home Assistant EntityId
4. edit the `home_assitatn_states` topic and set cleanup policy to compact.
   - Optionally add a custom parameter named `max.compaction.lag.ms` to force compaction to run more often
5. Launch your Home Assistant UI and edit your `configuration.yaml` to include kafka.
   - see [Apache Kafka integration documentation](https://www.home-assistant.io/integrations/apache_kafka/)
   - set the topic to `home_assistant`
   - set the port to `9094`
   - It is recommended to set an `include_domains` filter, otherwise you will produce hundreds of thousands of events every week. For the examples to run, you should include:
     - `light`
     - `input_button`
7. Restart Home Assistant
   - At this point events should be streaming from Home Assistant into the `home_assistant` topic
9. In your HomeAssistant UI, create two helper buttons named:
   - `Test Button`
   - `Test Button 2`
10. In the `~/example/HakafkaNet.ExampleApp` directory, create an `appsettings.Development.json` file.
11. copy/paste the contents of the `appsettings.json` file and modify appropriately.
12. Modify the `example/HaKafkaNet.ExampleApp/Automations/SimpleLightAutomation.cs` file and set `_idOfLightToDim` to an id of a light that exists in your Home Assistant instance
13. Run the example App
14. optionally, view your KafkaFlow dashboard at `localhost:<port>/kafkaflow`
15. Click your test buttons
16. Enjoy
