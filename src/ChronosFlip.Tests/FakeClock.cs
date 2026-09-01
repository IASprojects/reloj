using ChronosFlip.Core.Clocks;

namespace ChronosFlip.Tests;

public sealed class FakeClock : IClock
{
    public FakeClock(DateTimeOffset now) => Now = now;

    public DateTimeOffset Now { get; set; }

    public DateTimeOffset GetNow() => Now;
}