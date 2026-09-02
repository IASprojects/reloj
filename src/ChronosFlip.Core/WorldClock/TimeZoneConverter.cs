namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Converts an instant to a zone's local wall time with the UTC offset that is
/// valid at that instant. Offset is computed from the UTC moment, never from
/// local wall time, so results are DST-safe across transition boundaries.
/// </summary>
public static class TimeZoneConverter
{
    public static DateTimeOffset ToZoneTime(DateTimeOffset instant, TimeZoneInfo zone)
    {
        var utc = instant.ToUniversalTime();
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc.UtcDateTime, zone);
        return new DateTimeOffset(local, zone.GetUtcOffset(utc));
    }
}