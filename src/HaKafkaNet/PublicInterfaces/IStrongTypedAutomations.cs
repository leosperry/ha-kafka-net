using System;
using System.Text.Json;

namespace HaKafkaNet;

public interface IAutomation_SceneController : IAutomation<DateTime?, SceneControllerEvent>;
public interface IAutomation_ColorLight : IAutomation<OnOff, ColorLightModel>;
public interface IAutomation_DimmableLight : IAutomation<OnOff, LightModel>;
public interface IAutomation_Button: IAutomation<DateTime?>;