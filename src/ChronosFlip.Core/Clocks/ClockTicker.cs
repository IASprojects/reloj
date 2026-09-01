namespace ChronosFlip.Core.Clocks;

/// <summary>
/// Pure tick engine: reads the injected clock and raises <see cref="Tick"/>
/// once per <see cref="Pump"/>. Holds no timer and no UI dependency, so it is
/// fully unit-testable.
/// </summary>
public sealed class ClockTicker
{
    private readonly IClock _clock;

    public ClockTicker(IClock clock) => _clock = clock;

    public event EventHandler<DateTimeOffset>? Tick;

    public DateTimeOffset Now => _clock.GetNow();

    public void Pump()
    {
        var now = _clock.GetNow();
        Tick?.Invoke(this, now);
    }
}