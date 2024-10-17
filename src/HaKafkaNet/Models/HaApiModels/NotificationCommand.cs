namespace HaKafkaNet;

/// <summary>
/// https://companion.home-assistant.io/docs/notifications/notification-commands
/// </summary>
public enum AndroidCommand
{
    clear_notification,
    command_activity,
    command_app_lock,
    command_auto_screen_brightness,
    command_bluetooth,
    command_ble_transmitter,
    command_beacon_monitor,
    command_broadcast_intent,
    command_dnd,
    command_high_accuracy_mode,
    command_launch_app,
    command_media,
    command_ringer_mode,
    command_screen_brightness_level,
    command_screen_off_timeout,
    command_screen_on,
    command_stop_tts,
    command_persistent_connection,
    command_update_sensors,
    command_volume_level,
    command_webview,
    remove_channel,
    request_location_update
}
