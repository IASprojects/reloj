namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Abstracts timezone discovery so world-clock logic stays independent of the
/// Windows timezone data source and is testable with custom zones (NFR-06).
/// </summary>
public interface IZoneResolver
{
    TimeZoneInfo Local { get; }
    TimeZoneInfo? Resolve(string timeZoneId);
    IEnumerable<TimeZoneInfo> AvailableZones { get; }
}