using ChronosFlip.Core.Timers;

namespace ChronosFlip.Tests.Timers;

public sealed class TimerTests
{
    private static readonly DateTimeOffset Base = new(2026, 9, 3, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_SetsDuration_AndIsIdle()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));

        Assert.Equal(TimerState.Idle, timer.State);
        Assert.Equal(TimeSpan.FromMinutes(5), timer.Duration);
        Assert.Equal(TimeSpan.FromMinutes(5), timer.Remaining);
    }

    [Fact]
    public void Start_FromIdle_EntersRunning()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));

        timer.Start();

        Assert.Equal(TimerState.Running, timer.State);
    }

    [Fact]
    public void Start_NotIdle_IsNoOp()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));
        timer.Start();
        var started = timer.State;

        timer.Start();

        Assert.Equal(TimerState.Running, timer.State);
        Assert.Equal(started, timer.State);
    }

    [Fact]
    public void Evaluate_Running_RemainingTracksEndsAtMinusNow()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));
        timer.Start();

        timer.Evaluate(Base);                          // endsAt = Base + 5:00
        timer.Evaluate(Base.AddSeconds(60));
        timer.Evaluate(Base.AddSeconds(120));

        Assert.Equal(TimeSpan.FromSeconds(180), timer.Remaining);
        Assert.Equal(TimerState.Running, timer.State);
    }

    [Fact]
    public void Pause_PreservesElapsed_ResumeDoesNotCountPausedWindow()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));
        timer.Start();
        timer.Evaluate(Base);
        timer.Evaluate(Base.AddSeconds(30));           // 30s elapsed

        timer.Pause();
        timer.Evaluate(Base.AddHours(1));              // paused window must not count

        Assert.Equal(TimeSpan.FromSeconds(270), timer.Remaining);
        Assert.Equal(TimerState.Paused, timer.State);

        timer.Start();
        timer.Evaluate(Base.AddHours(1).AddSeconds(30)); // endsAt = now + 4:30
        timer.Evaluate(Base.AddHours(1).AddSeconds(60)); // 30s more elapsed

        Assert.Equal(TimeSpan.FromSeconds(240), timer.Remaining);
    }

    [Fact]
    public void Pause_NotRunning_IsNoOp()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));

        timer.Pause();

        Assert.Equal(TimerState.Idle, timer.State);
    }

    [Fact]
    public void Evaluate_AtZero_RaisesExpiredExactlyOnce_AndStaysSticky()
    {
        var timer = new CountdownTimer(TimeSpan.FromSeconds(5));
        var expiredCount = 0;
        timer.Expired += (_, _) => expiredCount++;

        timer.Start();
        timer.Evaluate(Base);
        timer.Evaluate(Base.AddSeconds(6));            // crosses zero -> Expired

        Assert.Equal(1, expiredCount);
        Assert.Equal(TimerState.Expired, timer.State);
        Assert.Equal(TimeSpan.Zero, timer.Remaining);

        timer.Evaluate(Base.AddSeconds(60));           // sticky: no re-raise, no state change
        timer.Evaluate(Base.AddSeconds(120));
        Assert.Equal(1, expiredCount);
        Assert.Equal(TimerState.Expired, timer.State);
    }

    [Fact]
    public void Evaluate_Running_DoesNotRaiseExpired_BeforeZero()
    {
        var timer = new CountdownTimer(TimeSpan.FromSeconds(5));
        var expiredCount = 0;
        timer.Expired += (_, _) => expiredCount++;

        timer.Start();
        timer.Evaluate(Base);
        timer.Evaluate(Base.AddSeconds(4));

        Assert.Equal(0, expiredCount);
        Assert.Equal(TimeSpan.FromSeconds(1), timer.Remaining);
    }

    [Theory]
    [InlineData(TimerState.Running)]
    [InlineData(TimerState.Paused)]
    [InlineData(TimerState.Expired)]
    public void Reset_ReturnsToIdle_WithFullDuration(TimerState fromState)
    {
        var timer = new CountdownTimer(TimeSpan.FromSeconds(5));
        timer.Expired += (_, _) => { };
        if (fromState == TimerState.Expired)
        {
            timer.Start();
            timer.Evaluate(Base.AddSeconds(10));
        }
        else
        {
            timer.Start();
            timer.Evaluate(Base);
            if (fromState == TimerState.Paused)
            {
                timer.Pause();
            }
        }

        timer.Reset();

        Assert.Equal(TimerState.Idle, timer.State);
        Assert.Equal(TimeSpan.FromSeconds(5), timer.Remaining);
    }

    [Fact]
    public void Reset_FromExpired_AllowsReuse()
    {
        var timer = new CountdownTimer(TimeSpan.FromSeconds(1));
        var expiredCount = 0;
        timer.Expired += (_, _) => expiredCount++;
        timer.Start();
        timer.Evaluate(Base);                // endsAt = Base + 1s
        timer.Evaluate(Base.AddSeconds(2));  // expires #

        Assert.Equal(1, expiredCount);

        timer.Reset();
        Assert.Equal(TimerState.Idle, timer.State);

        timer.Start();
        timer.Evaluate(Base.AddSeconds(2));              // fresh endsAt = Base + 3s
        timer.Evaluate(Base.AddSeconds(4));              // expires again

        Assert.Equal(2, expiredCount);
        Assert.Equal(TimerState.Expired, timer.State);
    }

    [Fact]
    public void SetDuration_Valid_AppliesInIdle()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));

        Assert.True(timer.SetDuration(TimeSpan.FromSeconds(90)));
        Assert.Equal(TimeSpan.FromSeconds(90), timer.Duration);
        Assert.Equal(TimeSpan.FromSeconds(90), timer.Remaining);
    }

    [Fact]
    public void SetDuration_OutOfRange_Throws()
    {
        var timer = new CountdownTimer();

        Assert.Throws<ArgumentOutOfRangeException>(() => { timer.SetDuration(TimeSpan.Zero); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { timer.SetDuration(TimeSpan.FromMinutes(100)); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { timer.SetDuration(TimeSpan.FromMinutes(-1)); });
    }

    [Fact]
    public void SetDuration_WhenNotIdle_IsRejected()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));
        timer.Start();

        Assert.False(timer.SetDuration(TimeSpan.FromMinutes(1)));
        Assert.Equal(TimeSpan.FromMinutes(5), timer.Duration);
    }

    [Fact]
    public void Evaluate_Idle_IsNoOp()
    {
        var timer = new CountdownTimer(TimeSpan.FromMinutes(5));

        timer.Evaluate(Base.AddDays(1));

        Assert.Equal(TimerState.Idle, timer.State);
        Assert.Equal(TimeSpan.FromMinutes(5), timer.Remaining);
    }
}