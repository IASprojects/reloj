using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests.WorldClock;

public sealed class WorldClockCardViewModelTests
{
    private static readonly TimeZoneInfo SpringFallZone = TestZones.CreateSpringFallZone();

    [Fact]
    public void Ctor_CapturesLabelAndZoneId()
    {
        var card = new WorldClockCardViewModel("Paris", "Western Europe", SpringFallZone);

        Assert.Equal("Paris", card.Label);
        Assert.Equal("Western Europe", card.TimeZoneId);
    }

    [Fact]
    public void SetPresent_ConvertsToZoneWallTime()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);

        card.SetPresent(new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("02", card.Hours);
        Assert.Equal("00", card.Minutes);
        Assert.Equal("00", card.Seconds);
    }

    [Fact]
    public void SetPresent_AtDstBoundary_SwitchesOffset()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);

        card.SetPresent(new DateTimeOffset(2026, 4, 5, 0, 59, 0, TimeSpan.Zero));
        Assert.Equal("UTC+01:00", card.OffsetText);

        card.SetPresent(new DateTimeOffset(2026, 4, 5, 2, 0, 0, TimeSpan.Zero));
        Assert.Equal("UTC+02:00", card.OffsetText);
    }

    [Theory]
    [InlineData(0, "UTC")]
    [InlineData(300, "UTC+05:00")]
    [InlineData(-210, "UTC-03:30")]
    [InlineData(345, "UTC+05:45")]
    public void FormatOffset_FormatsUtcAndDerivedOffsets(int totalMinutes, string expected)
    {
        Assert.Equal(expected, WorldClockCardViewModel.FormatOffset(TimeSpan.FromMinutes(totalMinutes)));
    }

    [Fact]
    public void SetPresent_RaisesPropertyChanged_ForChangedSegments()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);
        var changed = new List<string?>();
        card.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        card.SetPresent(new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero));

        Assert.Contains(nameof(WorldClockCardViewModel.Hours), changed);
        Assert.Contains(nameof(WorldClockCardViewModel.Minutes), changed);
        Assert.Contains(nameof(WorldClockCardViewModel.Seconds), changed);
        Assert.Contains(nameof(WorldClockCardViewModel.OffsetText), changed);
    }
}