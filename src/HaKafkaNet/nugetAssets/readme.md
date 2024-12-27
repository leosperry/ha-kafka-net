# HaKafkaNet
A framework for creating Home Assistant automations in .NET and C#.

Kafka ensures automations are durable and state is restored between restarts.

It was created with the following goals:
* Create Home Assistant automations in .NET / C# with abilities to:
  * track/retrieve states of all entities in Home Assistant
  * respond to Home Assistant state changes
  * call Home Assistant RESTful services
* Enable all automation code to be fully testable with automated tests

## Why ha-kafka-net ? 
* Strongly typed [access to entities](https://github.com/leosperry/ha-kafka-net/wiki/State-Extension-Methods)
* Strongly typed [automations](https://github.com/leosperry/ha-kafka-net/wiki/Automation-Types#generic-automations)
* Durable - Automations that [survive restarts](https://github.com/leosperry/ha-kafka-net/wiki/Durable-Automations). See also [Event Timings](https://github.com/leosperry/ha-kafka-net/wiki/Event-Timings)
* Fast - Automations run in parallel and asynchronously.
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
* Full unit testability for custom automations and integration testing for all registered integrations.
* MIT license

## Example
Example of multiple durable automations. See [Tutorial](https://github.com/leosperry/ha-kafka-net/wiki/Tutorial:-Creating-Automations) for more examples.
```csharp
registrar.TryRegister(
    _factory.SunRiseAutomation(
        cancelToken => _api.TurnOff("light.night_light", cancelToken)),
    _factory.SunSetAutomation(
        cancelToken => _api.TurnOn("light.night_light", cancelToken),
        TimeSpan.FromMinutes(-10))
);
```

See [Documentation](https://github.com/leosperry/ha-kafka-net/wiki) for full details.