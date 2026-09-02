using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests.WorldClock;

internal sealed class FakeZoneResolver : IZoneResolver
{
    private readonly TimeZoneInfo[] _zones;

    public FakeZoneResolver(TimeZoneInfo local, params TimeZoneInfo[] zones)
    {
        Local = local;
        _zones = zones;
    }

    public TimeZoneInfo Local { get; }
    public IEnumerable<TimeZoneInfo> AvailableZones => _zones;
    public TimeZoneInfo? Resolve(string timeZoneId) => _zones.FirstOrDefault(z => z.Id == timeZoneId);
}