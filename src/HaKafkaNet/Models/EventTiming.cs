namespace HaKafkaNet;

[Flags]
public enum EventTiming
{
    /// <summary>
    /// There is no corresponding cache entry for the state currently being handled
    /// </summary>
    PreStartupNotCached =           0b000001,
    /// <summary>
    /// State currently being handled happened before the most recent cach entry
    /// </summary>
    PreStartupPreLastCached =       0b000010,
    /// <summary>
    /// State currently being handled matches the most recent cache entry
    /// </summary>
    PreStartupSameAsLastCached =    0b000100,
    /// <summary>
    /// State currently being handled does not match the most recent cache entry, but occurred simultaneously with the cached entry
    /// (extreme edge case)
    /// </summary>
    PreStartupSameTimeLastCached =  0b001000,
    /// <summary>
    /// State currently being handled happened after the most recent cach entry
    /// </summary>
    PreStartupPostLastCached =      0b010000,
    /// <summary>
    /// Event happened after startup (may or may not be cached)
    /// </summary>
    PostStartup =                   0b100000,
    /// <summary>
    /// Primarily used for schedulable events that need to survive restarts 
    /// </summary>
    Durable =                       0b110101,
    /// <summary>
    /// When IAutomation.EventTiming is set to this value, all events will be passed to the IAutomation
    /// </summary>
    All =                           0b111111
}
