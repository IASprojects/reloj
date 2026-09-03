using ChronosFlip.Core.Alarms;

namespace ChronosFlip.Tests.Alarms;

public sealed class AlarmViewModelTests
{
    private static readonly DateTimeOffset Base = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    private static (AlarmViewModel Vm, AlarmService Service) Create()
    {
        var service = new AlarmService();
        return (new AlarmViewModel(service), service);
    }

    [Fact]
    public void AddAlarm_SyncsBothStores()
    {
        var (vm, service) = Create();

        vm.AddAlarm("Tokyo Standard Time", Base.AddDays(2), "Tokyo");

        Assert.Single(vm.Alarms);
        Assert.Single(service.Alarms);
        Assert.Equal(AlarmBadge.Armed, vm.BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void RemoveAlarm_DropsFromBothStores()
    {
        var (vm, _) = Create();
        vm.AddAlarm("Tokyo Standard Time", Base.AddDays(2), "Tokyo");

        vm.RemoveAlarm(vm.Alarms[0].Id);

        Assert.Empty(vm.Alarms);
        Assert.Equal(AlarmBadge.None, vm.BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void SetEnabled_ReflectsInBadge()
    {
        var (vm, _) = Create();
        vm.AddAlarm("Tokyo Standard Time", Base.AddDays(2), "Tokyo");
        var id = vm.Alarms[0].Id;

        vm.SetEnabled(id, false);

        Assert.Equal(AlarmBadge.None, vm.BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void Evaluate_RaisesAlarmRang_ButNotChanged_WhenAlarmRings()
    {
        var (vm, _) = Create();
        var changed = 0;
        var rang = 0;
        vm.Changed += (_, _) => changed++;
        vm.AlarmRang += (_, _) => rang++;
        vm.AddAlarm("Tokyo Standard Time", Base.AddMinutes(1), "Tokyo");

        vm.Evaluate(Base.AddMinutes(1));

        Assert.Equal(AlarmBadge.Ringing, vm.BadgeFor("Tokyo Standard Time"));
        Assert.Equal(1, vm.RingingCount);
        Assert.Equal(1, rang);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void Evaluate_BeforeDue_DoesNotRing()
    {
        var (vm, _) = Create();
        vm.AddAlarm("Tokyo Standard Time", Base.AddMinutes(1), "Tokyo");

        vm.Evaluate(Base);

        Assert.Equal(AlarmBadge.Armed, vm.BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void Dismiss_StopsRinging()
    {
        var (vm, _) = Create();
        vm.AddAlarm("Tokyo Standard Time", Base, "Tokyo");
        vm.Evaluate(Base);
        var id = vm.Alarms[0].Id;

        vm.Dismiss(id);

        Assert.Equal(0, vm.RingingCount);
        Assert.Equal(AlarmBadge.None, vm.BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void AlarmRang_ForwardsFromService()
    {
        var (vm, _) = Create();
        Alarm? rang = null;
        vm.AlarmRang += (_, alarm) => rang = alarm;
        vm.AddAlarm("Tokyo Standard Time", Base, "Tokyo");

        vm.Evaluate(Base);

        Assert.NotNull(rang);
        Assert.Equal("Tokyo Standard Time", rang!.ZoneId);
    }

    [Fact]
    public void AddAlarm_ComputesZoneTimeText()
    {
        var (vm, _) = Create();

        vm.AddAlarm("Tokyo Standard Time", Base, "Tokyo");

        Assert.False(string.IsNullOrWhiteSpace(vm.Alarms[0].ZoneTimeText));
        Assert.Contains("Sep 10", vm.Alarms[0].ZoneTimeText, StringComparison.Ordinal);
    }

    [Fact]
    public void AddAlarmAt_UnresolvableZone_ReturnsFalse()
    {
        var (vm, _) = Create();

        var ok = vm.AddAlarmAt("No/Such/Zone", new DateTime(2026, 9, 12, 7, 30, 0));

        Assert.False(ok);
        Assert.Empty(vm.Alarms);
    }

    [Fact]
    public void AddAlarmAt_ResolvableZone_AddsUnalignedInstant()
    {
        var (vm, _) = Create();

        var ok = vm.AddAlarmAt("Tokyo Standard Time", new DateTime(2026, 9, 12, 7, 30, 0), "Tokyo");

        Assert.True(ok);
        Assert.Single(vm.Alarms);
        Assert.Equal("Tokyo", vm.Alarms[0].Label);
        Assert.Contains("Sep 12", vm.Alarms[0].ZoneTimeText, StringComparison.Ordinal);
        Assert.Equal(AlarmBadge.Armed, vm.BadgeFor("Tokyo Standard Time"));
    }

    [Fact]
    public void DismissRingingForZone_StopsOnlyThatZonesRing()
    {
        var (vm, _) = Create();
        vm.AddAlarm("Tokyo Standard Time", Base, "Tokyo");
        vm.AddAlarm("Paris", Base.AddSeconds(1), "Paris");
        vm.Evaluate(Base.AddSeconds(2));

        vm.DismissRingingForZone("Tokyo Standard Time");

        Assert.Equal(1, vm.RingingCount);
        Assert.Equal(AlarmBadge.None, vm.BadgeFor("Tokyo Standard Time"));
        Assert.Equal(AlarmBadge.Ringing, vm.BadgeFor("Paris"));
    }

    [Fact]
    public void RemoveAlarmsForZone_RemovesOnlyThatZonesAlarms()
    {
        var (vm, _) = Create();
        vm.AddAlarm("Tokyo Standard Time", Base.AddDays(1), "Tokyo");
        vm.AddAlarm("Paris", Base.AddDays(2), "Paris");

        vm.RemoveAlarmsForZone("Tokyo Standard Time");

        Assert.Single(vm.Alarms);
        Assert.Equal("Paris", vm.Alarms[0].ZoneId);
        Assert.Equal(AlarmBadge.None, vm.BadgeFor("Tokyo Standard Time"));
    }
}