# HaKafkaNet
A library for easily creating Home Assistant automations in .NET and C#.

Kafka ensures automations are durable and state is restored between restarts.

It was created with the following goals:
* Create Home Assistant automations in .NET / C# with abilities to:
  * track/retrieve states of all entities in Home Assistant
  * respond to Home Assistant state changes
  * call Home Assistant RESTful services
* Enable all automation code to be fully unit testable

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

See [Documentation](https://github.com/leosperry/ha-kafka-net/wiki) for full details.