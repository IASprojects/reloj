namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Production <see cref="IZoneResolver"/> over the Windows timezone database.
/// Resolve failures (unknown or corrupt ids) yield null rather than throwing.
/// </summary>
public sealed class SystemZoneResolver : IZoneResolver
{
    public TimeZoneInfo Local => TimeZoneInfo.Local;

    public IEnumerable<TimeZoneInfo> AvailableZones => TimeZoneInfo.GetSystemTimeZones();

    public TimeZoneInfo? Resolve(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return null;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }
}