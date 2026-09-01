using ChronosFlip.Core.Clocks;

namespace ChronosFlip.Tests;

public sealed class ClockTickerTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 9, 1, 12, 30, 45, TimeSpan.FromHours(2));

    [Fact]
    public void Now_ReturnsClockNow()
    {
        var ticker = new ClockTicker(new FakeClock(FixedNow));

        Assert.Equal(FixedNow, ticker.Now);
    }

    [Fact]
    public void Pump_RaisesTick_WithClockNow()
    {
        var ticker = new ClockTicker(new FakeClock(FixedNow));
        DateTimeOffset? received = null;
        var raisedCount = 0;
        ticker.Tick += (_, now) =>
        {
            received = now;
            raisedCount++;
        };

        ticker.Pump();

        Assert.Equal(FixedNow, received);
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void Pump_ReadsFreshClockTime_EachCall()
    {
        var clock = new FakeClock(FixedNow);
        var ticker = new ClockTicker(clock);
        var received = new List<DateTimeOffset>();
        ticker.Tick += (_, now) => received.Add(now);

        ticker.Pump();
        clock.Now = FixedNow.AddSeconds(1);
        ticker.Pump();

        Assert.Equal(2, received.Count);
        Assert.Equal(FixedNow.AddSeconds(1), received[1]);
    }

    [Fact]
    public void Pump_WithoutSubscribers_DoesNotThrow()
    {
        var ticker = new ClockTicker(new FakeClock(FixedNow));

        var exception = Record.Exception(() => ticker.Pump());

        Assert.Null(exception);
    }
}