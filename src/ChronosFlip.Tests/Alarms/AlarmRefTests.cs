using ChronosFlip.Core.Alarms;

namespace ChronosFlip.Tests.Alarms;

public sealed class AlarmRefTests
{
    [Fact]
    public void FromAlarm_RoundTripsAllFields()
    {
        var alarm = new Alarm("Tokyo Standard Time", new DateTimeOffset(2026, 9, 10, 2, 30, 0, TimeSpan.Zero), "Tokyo")
        {
            Enabled = false,
        };

        var roundTripped = AlarmRef.FromAlarm(alarm).ToAlarm();

        Assert.NotNull(roundTripped);
        Assert.Equal(alarm.Id, roundTripped!.Id);
        Assert.Equal(alarm.ZoneId, roundTripped.ZoneId);
        Assert.Equal("Tokyo", roundTripped.Label);
        Assert.Equal(alarm.FireAtUtc, roundTripped.FireAtUtc);
        Assert.False(roundTripped.Enabled);
    }

    [Fact]
    public void ToAlarm_ReturnsNull_WhenFieldsBlank()
    {
        Assert.Null(new AlarmRef().ToAlarm());
        Assert.Null(new AlarmRef { Id = "1", ZoneId = "Z", FireAtUtc = "not-a-date" }.ToAlarm());
        Assert.Null(new AlarmRef { Id = "1", ZoneId = "  ", FireAtUtc = "2026-09-10T00:00:00Z" }.ToAlarm());
    }

    [Fact]
    public void FireAtUtc_IsPersistedAsUtc()
    {
        var alarm = new Alarm("Paris", new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.FromHours(2)));

        var serialized = AlarmRef.FromAlarm(alarm).FireAtUtc!;

        Assert.StartsWith("2026-09-10T10:00:00", serialized, StringComparison.Ordinal);
    }
}