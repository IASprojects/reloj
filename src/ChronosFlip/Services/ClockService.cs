using ChronosFlip.Core.Clocks;
using Microsoft.UI.Dispatching;

namespace ChronosFlip.Services;

/// <summary>
/// Owns the single 1s UI tick (NFR-02). Only WinUI-touching clock component;
/// the pure <see cref="ClockTicker"/> it pumps lives in ChronosFlip.Core.
/// Must be created and used on the UI thread.
/// </summary>
public sealed class ClockService : IDisposable
{
    private readonly DispatcherQueueTimer _timer;
    private readonly ClockTicker _ticker;

    public ClockService(DispatcherQueue queue, ClockTicker ticker)
    {
        _ticker = ticker;
        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    private void OnTimerTick(DispatcherQueueTimer sender, object args) => _ticker.Pump();

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
    }
}