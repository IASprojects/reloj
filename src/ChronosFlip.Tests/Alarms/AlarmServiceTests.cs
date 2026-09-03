using ChronosFlip.Core.Alarms;

namespace ChronosFlip.Tests.Alarms;

public sealed class AlarmServiceTests
{
    private static readonly DateTimeOffset Base = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    private static Alarm CreateAlarm(string zoneId = "Tokyo Standard Time", DateTimeOffset? fireAt = null, bool enabled = true) =>
        new(zoneId, fireAt ?? Base) { Enabled = enabled };

    [Fact]
    public void Evaluate_BeforeFireAt_DoesNotRing()
    {
        var service = new AlarmService();
        service.Add(CreateAlarm(fireAt: Base.AddMinutes(5)));
        var rang = 0;
        service.AlarmRang += (_, _) => rang++;

        service.Evaluate(Base);

        Assert.Equal(0, rang);
        Assert.Equal(0, service.RingingCount);
    }

    [Fact]
    public void Evaluate_AtFireAt_RingsExactlyOnce()
    {
        var service = new AlarmService();
        service.Add(CreateAlarm(fireAt: Base.AddMinutes(5)));
        var rang = 0;
        service.AlarmRang += (_, _) => rang++;

        service.Evaluate(Base.AddMinutes(5));
        service.Evaluate(Base.AddMinutes(6));

        Assert.Equal(1, rang);
        Assert.Equal(1, service.RingingCount);
        Assert.Equal(AlarmBadge.Ringing, new AlarmViewModel(service).BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void Evaluate_DisabledAlarm_NeverFires()
    {
        var service = new AlarmService();
        service.Add(CreateAlarm(enabled: false));
        var rang = 0;
        service.AlarmRang += (_, _) => rang++;

        service.Evaluate(Base.AddHours(1));

        Assert.Equal(0, rang);
        Assert.Equal(0, service.RingingCount);
    }

    [Fact]
    public void Dismiss_StopsRinging_AndDisablesAlarm()
    {
        var service = new AlarmService();
        var alarm = CreateAlarm(fireAt: Base);
        service.Add(alarm);
        service.Evaluate(Base);

        service.Dismiss(alarm.Id);

        Assert.False(alarm.IsRinging);
        Assert.False(alarm.Enabled);
        Assert.Equal(0, service.RingingCount);
    }

    [Fact]
    public void DismissAll_StopsEveryRingingAlarm()
    {
        var service = new AlarmService();
        service.Add(CreateAlarm("Paris", Base));
        service.Add(CreateAlarm("Tokyo Standard Time", Base.AddSeconds(1)));
        service.Evaluate(Base.AddSeconds(2));

        service.DismissAll();

        Assert.Equal(0, service.RingingCount);
        Assert.All(service.Alarms, alarm => Assert.False(alarm.IsRinging));
    }

    [Fact]
    public void Remove_DropsAlarm_FromService()
    {
        var service = new AlarmService();
        var alarm = CreateAlarm();
        service.Add(alarm);

        Assert.True(service.Remove(alarm.Id));
        Assert.Empty(service.Alarms);
    }

    [Fact]
    public void RemoveAllForZone_DropsOnlyThatZonesAlarms()
    {
        var service = new AlarmService();
        service.Add(CreateAlarm("Tokyo Standard Time", fireAt: Base.AddDays(1)));
        service.Add(CreateAlarm("Paris", fireAt: Base.AddDays(1)));

        service.RemoveAllForZone("Tokyo Standard Time");

        Assert.Single(service.Alarms);
        Assert.Equal("Paris", service.Alarms[0].ZoneId);
    }

    [Fact]
    public void ActiveForZone_UsesLastEvaluatedInstant()
    {
        var service = new AlarmService();
        var alarm = CreateAlarm(fireAt: Base.AddMinutes(5));
        service.Add(alarm);

        service.Evaluate(Base); // last evaluated = Base
        var before = service.ActiveForZone("Tokyo Standard Time");

        service.Evaluate(Base.AddMinutes(10)); // alarm now firing
        var after = service.ActiveForZone("Tokyo Standard Time");

        Assert.Single(before);
        Assert.Empty(after);
        Assert.True(alarm.IsRinging);
    }

    [Fact]
    public void ActiveForZone_ExcludesRingingAndDisabledAndPast()
    {
        var service = new AlarmService();
        var now = DateTimeOffset.UtcNow;
        var future = CreateAlarm(fireAt: now.AddDays(1));
        service.Add(future);
        service.Add(CreateAlarm("Paris", fireAt: now.AddDays(2)));
        var past = CreateAlarm("Paris", fireAt: now.AddDays(-1));
        service.Add(past);
        service.SetEnabled(past.Id, true);
        var disabled = CreateAlarm(fireAt: now.AddDays(3));
        disabled.Enabled = false;
        service.Add(disabled);

        var tokyo = service.ActiveForZone("Tokyo Standard Time");
        var paris = service.ActiveForZone("Paris");

        Assert.Single(tokyo);
        Assert.Equal(future.Id, tokyo[0].Id);
        Assert.DoesNotContain(paris, alarm => alarm.Id == past.Id);
    }

    [Fact]
    public void ReArmed_Alarm_FiresImmediately_WhenInstantAlreadyPassed()
    {
        var service = new AlarmService();
        var rearmed = Alarm.Restore("persisted-1", "Tokyo Standard Time", Base.AddMinutes(-5));
        rearmed.Enabled = true;
        service.Add(rearmed);
        var rang = 0;
        service.AlarmRang += (_, _) => rang++;

        service.Evaluate(Base);

        Assert.Equal(1, rang);
        Assert.True(rearmed.IsRinging);
    }
}