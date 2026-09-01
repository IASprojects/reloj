namespace ChronosFlip.Core.Clocks;

public interface IClock
{
    DateTimeOffset GetNow();
}