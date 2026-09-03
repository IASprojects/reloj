using ChronosFlip.Core.Alarms;

namespace ChronosFlip.Tests.Alarms;

public sealed class AlarmTests
{
    private static readonly DateTimeOffset Base = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_RequiresZoneId()
    {
        Assert.Throws<ArgumentException>(() => new Alarm("  ", Base));
    }

    [Fact]
    public void Fire_LatchesHasFired_AndRings()
    {
        var alarm = new Alarm("Tokyo Standard Time", Base);

        alarm.Fire();
        alarm.Fire();

        Assert.True(alarm.IsRinging);
        Assert.True(alarm.HasFired);
    }

    [Fact]
    public void Dismiss_StopsRinging_AndDisablesAlarm()
    {
        var alarm = new Alarm("Tokyo Standard Time", Base);
        alarm.Fire();

        alarm.Dismiss();

        Assert.False(alarm.IsRinging);
        Assert.False(alarm.Enabled);
    }

    [Fact]
    public void DefaultLabel_FallsBackToZoneId()
    {
        var alarm = new Alarm("Tokyo Standard Time", Base);

        Assert.Equal("Tokyo Standard Time", alarm.Label);
    }
}