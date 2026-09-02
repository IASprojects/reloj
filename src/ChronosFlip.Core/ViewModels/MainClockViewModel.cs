using ChronosFlip.Core.Clocks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.ViewModels;

/// <summary>
/// Backing view model for the local clock. Segments the current time into
/// fixed-width HH / MM / SS strings for the flip-card digits.
/// </summary>
public partial class MainClockViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Hours))]
    [NotifyPropertyChangedFor(nameof(Minutes))]
    [NotifyPropertyChangedFor(nameof(Seconds))]
    private DateTimeOffset _now = DateTimeOffset.MinValue;

    public string Hours => TimeSegments.Of(Now).Hours;

    public string Minutes => TimeSegments.Of(Now).Minutes;

    public string Seconds => TimeSegments.Of(Now).Seconds;

    public void Attach(ClockTicker ticker)
    {
        Now = ticker.Now;
        ticker.Tick += (_, now) => Now = now;
    }
}