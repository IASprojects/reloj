using System.ComponentModel;
using ChronosFlip.Core.Timers;

namespace ChronosFlip.Tests.Timers;

public sealed class TimerFormattingTests
{
    private static readonly DateTimeOffset Base = new(2026, 9, 3, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RemainingText_FormatsAsZeroPaddedMmSs()
    {
        var timer = new CountdownTimer(TimeSpan.FromSeconds(5));
        Assert.Equal("00:05", timer.RemainingText);

        timer.SetDuration(TimeSpan.FromMinutes(5));
        Assert.Equal("05:00", timer.RemainingText);

        timer.SetDuration(TimeSpan.FromMinutes(65));
        Assert.Equal("65:00", timer.RemainingText);
        Assert.Equal("65", timer.RemainingMinutes);
        Assert.Equal("00", timer.RemainingSeconds);
    }

    [Fact]
    public void RemainingText_ClampsAtMaxWidth()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(99));

        Assert.Equal("99:00", timer.RemainingText);
    }

    [Fact]
    public void RemainingText_AfterExpiry_IsZeroZero()
    {
        var timer = new CountdownTimer(TimeSpan.FromSeconds(2));
        timer.Start();
        timer.Evaluate(Base);
        timer.Evaluate(Base.AddSeconds(3));

        Assert.Equal(TimerState.Expired, timer.State);
        Assert.Equal("00:00", timer.RemainingText);
        Assert.Equal("00", timer.RemainingMinutes);
        Assert.Equal("00", timer.RemainingSeconds);
    }

    [Fact]
    public void Remaining_WhileRunning_RaisesPropertyChanged_ForFormattedParts()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));
        timer.Start();
        timer.Evaluate(Base);
        timer.Evaluate(Base.AddSeconds(1));

        var changed = new List<string?>();
        ((INotifyPropertyChanged)timer).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        timer.Evaluate(Base.AddSeconds(65)); // 1:05 elapsed, remaining 3:55

        Assert.Contains(nameof(CountdownTimer.RemainingMinutes), changed);
        Assert.Contains(nameof(CountdownTimer.RemainingSeconds), changed);
        Assert.Contains(nameof(CountdownTimer.RemainingText), changed);
        Assert.Equal("03:55", timer.RemainingText);
    }
}