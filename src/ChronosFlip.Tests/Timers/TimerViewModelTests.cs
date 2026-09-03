using ChronosFlip.Core.Timers;

namespace ChronosFlip.Tests.Timers;

public sealed class TimerViewModelTests
{
    private static readonly DateTimeOffset Base = new(2026, 9, 3, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_DefaultsToFiveMinutes()
    {
        var vm = new TimerViewModel();

        Assert.Equal(5, vm.InputMinutes);
        Assert.Equal(0, vm.InputSeconds);
        Assert.Equal(TimeSpan.FromMinutes(5), vm.Timer.Duration);
        Assert.True(vm.CanStart);
        Assert.True(vm.CanEditDuration);
        Assert.False(vm.CanPause);
        Assert.False(vm.CanReset);
    }

    [Fact]
    public void RestoreDuration_SetsInputsAndTimer()
    {
        var vm = new TimerViewModel();

        vm.RestoreDuration(90);

        Assert.Equal(1, vm.InputMinutes);
        Assert.Equal(30, vm.InputSeconds);
        Assert.Equal(TimeSpan.FromSeconds(90), vm.Timer.Duration);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(999999, 5999)]
    public void RestoreDuration_ClampsOutOfRange(int value, int expectedSeconds)
    {
        var vm = new TimerViewModel();

        vm.RestoreDuration(value);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), vm.Timer.Duration);
    }

    [Fact]
    public void Start_FlipsControls_ThenPauseAndReset()
    {
        var vm = new TimerViewModel();

        vm.Start();
        Assert.Equal(TimerState.Running, vm.Timer.State);
        Assert.False(vm.CanStart);
        Assert.True(vm.CanPause);
        Assert.True(vm.CanReset);
        Assert.False(vm.CanEditDuration);

        vm.Pause();
        Assert.Equal(TimerState.Paused, vm.Timer.State);
        Assert.True(vm.CanStart);
        Assert.False(vm.CanPause);
        Assert.True(vm.CanReset);

        vm.Reset();
        Assert.Equal(TimerState.Idle, vm.Timer.State);
        Assert.True(vm.CanStart);
        Assert.False(vm.CanReset);
    }

    [Fact]
    public void Start_WhileRunning_IsNoOp()
    {
        var vm = new TimerViewModel();
        vm.Start();
        var before = vm.Timer.State;

        vm.Start();

        Assert.Equal(before, vm.Timer.State);
    }

    [Fact]
    public void Resume_FromPaused_RestartsCountdown()
    {
        var vm = new TimerViewModel();
        vm.Start();
        vm.Evaluate(Base);
        vm.Evaluate(Base.AddSeconds(60));
        vm.Pause();
        vm.Evaluate(Base.AddHours(1));

        vm.Start();
        vm.Evaluate(Base.AddHours(1).AddSeconds(30)); // resume: endsAt = now + 4:00
        vm.Evaluate(Base.AddHours(1).AddSeconds(60)); // 30s more elapsed

        Assert.Equal(TimeSpan.FromSeconds(210), vm.Timer.Remaining);
    }

    [Fact]
    public void Expired_RaisesEventOnce_AndFlagsIsExpired()
    {
        var vm = new TimerViewModel();
        vm.RestoreDuration(2);
        var expiredCount = 0;
        vm.Expired += (_, _) => expiredCount++;

        vm.Start();
        vm.Evaluate(Base);
        vm.Evaluate(Base.AddSeconds(3));

        Assert.Equal(TimerState.Expired, vm.Timer.State);
        Assert.True(vm.IsExpired);
        Assert.False(vm.CanStart);
        Assert.False(vm.CanPause);
        Assert.True(vm.CanReset);

        vm.Evaluate(Base.AddSeconds(10));
        Assert.Equal(1, expiredCount);
        Assert.True(vm.IsExpired);
    }

    [Fact]
    public void EditingInputs_WhileIdle_AppliesToTimer()
    {
        var vm = new TimerViewModel();

        vm.InputMinutes = 2;
        vm.InputSeconds = 15;

        Assert.Equal(TimeSpan.FromSeconds(135), vm.Timer.Duration);
    }

    [Fact]
    public void EditingInputs_ClampsToRange()
    {
        var vm = new TimerViewModel();

        vm.InputMinutes = 150;
        vm.InputSeconds = 75;

        Assert.Equal(TimeSpan.FromSeconds(5999), vm.Timer.Duration);
    }

    [Fact]
    public void EditingInputs_WhileRunning_IsIgnored()
    {
        var vm = new TimerViewModel();
        vm.Start();

        vm.InputMinutes = 9;

        Assert.Equal(TimeSpan.FromMinutes(5), vm.Timer.Duration);
    }

    [Fact]
    public void Reset_FromExpired_ReturnsToIdle_WithEditableDuration()
    {
        var vm = new TimerViewModel();
        vm.RestoreDuration(2);
        vm.Start();
        vm.Evaluate(Base.AddSeconds(5));

        vm.Reset();

        Assert.Equal(TimerState.Idle, vm.Timer.State);
        Assert.Equal(TimeSpan.FromSeconds(2), vm.Timer.Remaining);
        Assert.True(vm.CanEditDuration);
        Assert.False(vm.IsExpired);
    }
}