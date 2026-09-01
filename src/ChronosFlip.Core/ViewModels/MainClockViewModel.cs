using System.Globalization;
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

    public string Hours => Now.ToString("HH", CultureInfo.InvariantCulture);

    public string Minutes => Now.ToString("mm", CultureInfo.InvariantCulture);

    public string Seconds => Now.ToString("ss", CultureInfo.InvariantCulture);

    public void Attach(ClockTicker ticker)
    {
        Now = ticker.Now;
        ticker.Tick += (_, now) => Now = now;
    }
}