using System.ComponentModel;
using ChronosFlip.Core.Clocks;
using ChronosFlip.Core.ViewModels;

namespace ChronosFlip.Tests;

public sealed class MainClockViewModelTests
{
    private static MainClockViewModel CreateViewModel(DateTimeOffset now)
    {
        var ticker = new ClockTicker(new FakeClock(now));
        var viewModel = new MainClockViewModel();
        viewModel.Attach(ticker);
        return viewModel;
    }

    [Fact]
    public void Tick_SegmentsLocalTime()
    {
        var now = new DateTimeOffset(2026, 9, 1, 13, 5, 9, TimeSpan.FromHours(2));
        var viewModel = CreateViewModel(now);

        Assert.Equal("13", viewModel.Hours);
        Assert.Equal("05", viewModel.Minutes);
        Assert.Equal("09", viewModel.Seconds);
    }

    [Fact]
    public void Tick_ZeroPadsValues()
    {
        var now = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.FromHours(2));
        var viewModel = CreateViewModel(now);

        Assert.Equal("00", viewModel.Hours);
        Assert.Equal("00", viewModel.Minutes);
        Assert.Equal("00", viewModel.Seconds);
    }

    [Fact]
    public void Tick_ReflectsNewTime_OnSubsequentPumps()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero));
        var ticker = new ClockTicker(clock);
        var viewModel = new MainClockViewModel();
        viewModel.Attach(ticker);

        ticker.Pump();
        Assert.Equal("10", viewModel.Hours);

        clock.Now = new DateTimeOffset(2026, 9, 1, 23, 59, 59, TimeSpan.Zero);
        ticker.Pump();

        Assert.Equal("23", viewModel.Hours);
        Assert.Equal("59", viewModel.Minutes);
        Assert.Equal("59", viewModel.Seconds);
    }

    [Fact]
    public void Tick_RaisesPropertyChanged_ForSegments()
    {
        var viewModel = CreateViewModel(new DateTimeOffset(2026, 9, 1, 8, 15, 30, TimeSpan.Zero));
        var changed = new List<string?>();
        viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        var clock = new FakeClock(new DateTimeOffset(2026, 9, 1, 8, 15, 31, TimeSpan.Zero));
        var ticker = new ClockTicker(clock);
        viewModel.Attach(ticker);
        ticker.Pump();

        Assert.Contains(nameof(MainClockViewModel.Hours), changed);
        Assert.Contains(nameof(MainClockViewModel.Minutes), changed);
        Assert.Contains(nameof(MainClockViewModel.Seconds), changed);
    }
}