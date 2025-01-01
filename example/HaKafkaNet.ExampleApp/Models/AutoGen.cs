using System;
using MyHome.Dev;

namespace HaKafkaNet.ExampleApp.Models;

/*
This file represents an example output when using the template found here:
 https://github.com/leosperry/ha-kafka-net/blob/main/infrastructure/hakafkanet.jinja

 It is used in the example app integration tests
*/

public class Binary_Sensor
{
    public const string MotionForSimple = "binary_sensor.motion_for_simple";
    public const string MotionForSimpleTyped = "binary_sensor.motion_for_simple_typed";
    public const string MotionForConditional = "binary_sensor.motion_for_conditional";
    public const string MotionForConditionalTyped = "binary_sensor.motion_for_conditional_typed";
    public const string MotionForSchedulable = "binary_sensor.motion_for_schedulable";
    public const string MotionForSchedulableTyped = "binary_sensor.motion_for_schedulable_typed";
    public const string TriggerForLongDelay = "binary_sensor.trigger_for_long_delay";
    
    
}

public class Input_Button
{
    public const string HelperButtonForSimple = "input_button.helper_button_for_simple"; 
    public const string HelperButtonForSimpleTyped = "input_button.helper_button_for_simple_typed"; 
    public const string HelperButtonForConditional = "input_button.helper_button_for_conditional"; 
    public const string HelperButtonForConditionalTyped = "input_button.helper_button_for_conditional_typed"; 
    public const string HelperButtonForSchedulable = "input_button.helper_button_for_schedulable"; 
    public const string HelperButtonForSchedulableTyped = "input_button.helper_button_for_schedulable_typed"; 
    public const string HelperButtonForLongDelay = "input_button.helper_button_for_long_delay"; 
    
}


