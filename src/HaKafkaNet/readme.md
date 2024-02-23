# HaKafkaNet
Integration that uses Home Assistant Kafka integration for creating home automations in .NET
It was created with the following goals:
* Create Home Assistant automations in .NET
* Expose a simple way to track states of all entities in Home Asstant
* Expose a simple way to respond to Home Assistant state changes
* Provide a means to call Home Assistant RESTful services
* Enable all automation code to be fully unit and/or component testable

## Why HaKafkaNet ?
* Kafka allows you to replay events. Therefore, when your application starts, it can quickly load the states of all your Home Assistant entities.
* It gives you a UI to manage your automations and inspect Kafka consumers.
* You have an easy way to respond to events during start up which means you are guarenteed to see/handle all events at least once, even if your application has been down. This means that you can create durable automations that will survive restarts.
* Strong typing of entity states
* Monitoring capabilities via system monitor including a global exception handler and ability to be alerted when entities become unresponsive.
* Extensible framework
* Automtion Factory and Automation builder with fluent syntax for quickly creating automations.
* Full unit testability
* MIT license

See [Documentation](https://github.com/leosperry/ha-kafka-net/wiki) for full details.