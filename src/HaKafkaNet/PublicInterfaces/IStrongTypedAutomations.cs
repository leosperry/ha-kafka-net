using System;
using System.Text.Json;

namespace HaKafkaNet;

/// <summary>
/// useful for scene controllers 
/// see: https://github.com/leosperry/ha-kafka-net/wiki/Scene-Controllers
/// </summary>
public interface IAutomation_SceneController : IAutomation<DateTime?, SceneControllerEvent>;

/// <summary>
/// Great for creating virtual lights
/// </summary>
public interface IAutomation_ColorLight : IAutomation<OnOff, ColorLightModel>;

/// <summary>
/// great for light groups
/// </summary>
public interface IAutomation_DimmableLight : IAutomation<OnOff, LightModel>;

/// <summary>
/// becase the state of a button is non-obvious
/// </summary>
public interface IAutomation_Button: IAutomation<DateTime?>;