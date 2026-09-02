namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// A user-selected clock zone for the world-clock card tray. Carries only the
/// stable label and the Windows timezone id; the UTC offset is always derived
/// live from the current instant via <see cref="TimeZoneConverter"/> so it stays
/// DST-correct (see spec FR-13).
/// </summary>
public sealed record ClockZone
{
    public required string Label { get; init; }
    public required string TimeZoneId { get; init; }
}