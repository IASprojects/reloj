using ChronosFlip.Core.Navigation;

namespace ChronosFlip.Tests.Navigation;

public sealed class MainNavigationViewModelTests
{
    [Fact]
    public void Default_IsClock()
    {
        var vm = new MainNavigationViewModel();

        Assert.Equal(MainNavigationPage.Clock, vm.SelectedPage);
        Assert.True(vm.IsClockSelected);
        Assert.False(vm.IsAlarmSelected);
        Assert.False(vm.IsTimerSelected);
    }

    [Fact]
    public void Select_SwitchesPage_AndFlags()
    {
        var vm = new MainNavigationViewModel();

        vm.Select(MainNavigationPage.Alarm);

        Assert.Equal(MainNavigationPage.Alarm, vm.SelectedPage);
        Assert.False(vm.IsClockSelected);
        Assert.True(vm.IsAlarmSelected);
        Assert.False(vm.IsTimerSelected);
    }

    [Fact]
    public void Select_RaisesPropertyChanged_ForPageAndFlags()
    {
        var vm = new MainNavigationViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Select(MainNavigationPage.Timer);

        Assert.Equal(MainNavigationPage.Timer, vm.SelectedPage);
        Assert.Contains(nameof(MainNavigationViewModel.SelectedPage), changed);
        Assert.Contains(nameof(MainNavigationViewModel.IsClockSelected), changed);
        Assert.Contains(nameof(MainNavigationViewModel.IsAlarmSelected), changed);
        Assert.Contains(nameof(MainNavigationViewModel.IsTimerSelected), changed);
    }

    [Fact]
    public void Select_SamePage_IsNoOp_AndRaisesNothing()
    {
        var vm = new MainNavigationViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Select(MainNavigationPage.Clock);

        Assert.Equal(MainNavigationPage.Clock, vm.SelectedPage);
        Assert.Empty(changed);
    }

    [Fact]
    public void Select_EveryPage_RoundTrips()
    {
        var vm = new MainNavigationViewModel();

        foreach (var page in new[]
        {
            MainNavigationPage.Alarm,
            MainNavigationPage.Clock,
            MainNavigationPage.Timer,
            MainNavigationPage.Alarm,
        })
        {
            vm.Select(page);
            Assert.Equal(page, vm.SelectedPage);
        }
    }

    [Theory]
    [InlineData(MainNavigationPage.Clock, true, false, false)]
    [InlineData(MainNavigationPage.Alarm, false, true, false)]
    [InlineData(MainNavigationPage.Timer, false, false, true)]
    public void Flags_FollowSelectedPage(
        MainNavigationPage page, bool clock, bool alarm, bool timer)
    {
        var vm = new MainNavigationViewModel();

        vm.Select(page);

        Assert.Equal(clock, vm.IsClockSelected);
        Assert.Equal(alarm, vm.IsAlarmSelected);
        Assert.Equal(timer, vm.IsTimerSelected);
    }

    [Fact]
    public void SelectCommand_ExecutesSelect_PerParameter()
    {
        var vm = new MainNavigationViewModel();

        vm.SelectCommand.Execute(MainNavigationPage.Alarm);

        Assert.Equal(MainNavigationPage.Alarm, vm.SelectedPage);
    }

    [Fact]
    public void SelectCommand_SamePage_IsNoOp()
    {
        var vm = new MainNavigationViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.SelectCommand.Execute(MainNavigationPage.Clock);

        Assert.Empty(changed);
    }

    [Fact]
    public void PageConstants_MapToEnum()
    {
        var vm = new MainNavigationViewModel();

        Assert.Equal(MainNavigationPage.Clock, vm.ClockPage);
        Assert.Equal(MainNavigationPage.Alarm, vm.AlarmPage);
        Assert.Equal(MainNavigationPage.Timer, vm.TimerPage);
    }
}