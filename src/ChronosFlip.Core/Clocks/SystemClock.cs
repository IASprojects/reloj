namespace ChronosFlip.Core.Clocks;

public sealed class SystemClock : IClock
{
    public DateTimeOffset GetNow() => DateTimeOffset.Now;
}