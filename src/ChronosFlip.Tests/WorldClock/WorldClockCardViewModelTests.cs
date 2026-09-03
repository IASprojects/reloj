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

    [Fact]
    public void Time_FormatsHoursAndMinutes()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);

        card.SetPresent(new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("02:00", card.Time);
    }

    [Fact]
    public void TimeHMS_FormatsHoursMinutesSeconds()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);

        card.SetPresent(new DateTimeOffset(2026, 7, 15, 3, 4, 5, TimeSpan.Zero));

        Assert.Equal("05:04:05", card.TimeHMS);
    }

    [Fact]
    public void DateText_FormatsAbbreviatedDayAndMonth()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);

        card.SetPresent(new DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("Wed, Sep 2", card.DateText);
    }

    [Fact]
    public void TimeHMS_And_DateText_RaisePropertyChanged_OnTick()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);
        var changed = new List<string?>();
        card.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        card.SetPresent(new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero));

        Assert.Contains(nameof(WorldClockCardViewModel.TimeHMS), changed);
        Assert.Contains(nameof(WorldClockCardViewModel.DateText), changed);
    }

    [Fact]
    public void IsRemovable_DefaultsTrue()
    {
        var card = new WorldClockCardViewModel("Test", "SpringFallTest", SpringFallZone);

        Assert.True(card.IsRemovable);
    }
}
