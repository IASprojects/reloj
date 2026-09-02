namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Persisted, serializable shape of a world-clock zone: the stable Windows
/// timezone id plus the user-facing label. Never stores a calendar offset —
/// that is always derived live from the current instant so it stays DST-correct.
/// Uses nullable, non-required properties so a partially-broken settings file
/// degrades gracefully instead of being quarantined wholesale.
/// </summary>
public sealed class ClockZoneRef
{
    public string? Label { get; set; }

    public string? TimeZoneId { get; set; }

    /// <summary>Wraps an in-memory zone for persistence.</summary>
    public static ClockZoneRef FromClockZone(ClockZone zone) => new()
    {
        Label = zone.Label,
        TimeZoneId = zone.TimeZoneId,
    };

    /// <summary>
    /// Converts to a <see cref="ClockZone"/>; returns null when the id is blank.
    /// </summary>
    public ClockZone? ToClockZone()
    {
        if (string.IsNullOrWhiteSpace(TimeZoneId))
        {
            return null;
        }

        return new ClockZone
        {
            Label = string.IsNullOrWhiteSpace(Label) ? TimeZoneId : Label,
            TimeZoneId = TimeZoneId,
        };
    }
}