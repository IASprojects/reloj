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

    /// <summary>
    /// Converts a zone-local wall time (Kind.Unspecified, e.g. from a picker)
    /// to the equivalent UTC instant. For single-occurrence alarms the
    /// ambiguity gap is decided by <see cref="TimeZoneInfo"/>'s own offset
    /// rules for the local wall time; the result is then normalized to UTC.
    /// </summary>
    public static DateTimeOffset FromZoneTime(DateTime localWallTime, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        if (localWallTime.Kind != DateTimeKind.Unspecified)
        {
            localWallTime = DateTime.SpecifyKind(localWallTime, DateTimeKind.Unspecified);
        }

        var offset = zone.GetUtcOffset(localWallTime);
        var dateTimeOffset = new DateTimeOffset(localWallTime, offset);
        return dateTimeOffset.ToUniversalTime();
    }
}