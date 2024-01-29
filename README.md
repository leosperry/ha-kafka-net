# ha-kafka-net
Integration that uses Home Assistant Kafka integration for creating home automations in .NET
It was created with the following goals:
* Create Home Assistant automations in .NET
* Expose a simple way to track states of all entities in Home Assistant
* Expose a simple way to respond to Home Assistant state changes
* Provide a means to call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

Nuget package can be found [here](https://www.nuget.org/packages/HaKafkaNet/).

Full documentation [here](https://github.com/leosperry/ha-kafka-net/wiki)

## Why ha-kafka-net ?
* Kafka allows you to replay events. Therefore, when your application starts, it can quickly load the states of all your Home Assistant entities.
* It gives you a easy-to-spin up infrastructure with management features out of the box. This includes docker images for both managing kafka and seeing the state of your consumers based on open source projects
* You have an easy way to respond to events during start up which means you are guarenteed to see/handle all events at least once, even if your application has been down.
* Full unit testability
* MIT license

## How it works
* Events are streamed from Home Assistant to the `home_assistant` topic. Unfortunately, the key is not utilizied by the provided home assistant kafka integration. Please upvote [this feature request](https://community.home-assistant.io/t/set-key-in-kafka-topic/671757/2)
* The transformer reads the messages and then adds them to the `home_assistant_states` topic with the entity id set as a key.
  - This allows us to compact the topic and make some assurances about order.
* A second consumer called the state handler reads from `home_assistant_states` topic and caches all state changes exposed by home assistant to Redis.
  - This allows for faster retrieval later and minimizes our application memory footprint. It also allows us to have some knowledge about which events were not handled between restarts and which ones were. The framework will tell your automation about such timings to allow you to handle messages appropriately.
* It then looks for automations which want to be notified.
  - If the entity id of the state change matches any of the `TriggerEntityIds` exposed by your automation, and the timing of the event matches your specified timings, then the `Execute` method of your automation will be called with a new `Task`.
  - It is up to the consumer to handle any errors. The framework prioritizes handling new messages speedily over tracking the state of individual automations. If your automation errors it will only write an ILogger message indicating the error.

## Current steps for Example App set up:
1. Follow instructions [here](https://github.com/leosperry/ha-kafka-net/wiki/Setup-Instructions) for setting up your environment.
2. In your HomeAssistant UI, create helper buttons named:
   - `Test Button`
   - `Test Button 2`
   - `Test Button 3`
2. Look through the provided examples for ID's of lights to set to match your environment.
3. Click your test buttons both while your application is up and while it is down to see different behaviors at starup.

## Coming soon
* More pre-built automations.
* builder for creating automations 
* More Documentation
* More enhanced HA API functionality
* V2 is in development and will offer a UI to list out all your executing automations as well as the ability to disable/enable automations at run time. Check out the V2 branch for details.

## Tips
* You can optionally add this repository as a submodule to your own instead of using the nuget package.
* During start up, it can take a minute or two for it to churn though thousands of events. In the output, you can see which kafka offsets have been handled. You can then compare that to the current offset which you can discover from your kafka-ui instance
* ILogger support has been added. When your automation is called, the name of your automation, information about the automation will be added to the scope.
* You can run the transformer seperately from the state manager and your automations. This allows you to constantly have the transformers work up to date if your automations are shut down for development or other reasons.

## Features recently added
* Some common API calls
* More API calls
* Test helper methods
* Sun model

## More examples
I have decided to make [my personal repository](https://github.com/leosperry/MyHome) public so that users can see working examples of some moderately complex automations.

If you have some examples you would like to share, please start a [discussion](https://github.com/leosperry/ha-kafka-net/discussions). I'd be happy to link it here.

Happy Automating!
