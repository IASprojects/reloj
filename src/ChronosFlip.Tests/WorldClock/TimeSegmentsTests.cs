using ChronosFlip.Core.Clocks;

namespace ChronosFlip.Tests.WorldClock;

public sealed class TimeSegmentsTests
{
    [Fact]
    public void Of_ZeroPadsAllSegments()
    {
        var segments = TimeSegments.Of(new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("00", segments.Hours);
        Assert.Equal("00", segments.Minutes);
        Assert.Equal("00", segments.Seconds);
    }

    [Fact]
    public void Of_SplitsTimeIntoSegments()
    {
        var segments = TimeSegments.Of(new DateTimeOffset(2026, 9, 1, 13, 5, 9, TimeSpan.Zero));

        Assert.Equal("13", segments.Hours);
        Assert.Equal("05", segments.Minutes);
        Assert.Equal("09", segments.Seconds);
    }
}