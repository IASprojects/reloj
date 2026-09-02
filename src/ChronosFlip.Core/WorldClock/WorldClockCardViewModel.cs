using ChronosFlip.Core.Clocks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Presentable model of a single world-clock card. Renders the zone's wall time
/// (segmented into HH/MM/SS) plus a human-readable UTC offset. The current
/// instant is applied externally via <see cref="SetPresent"/>; this type never
/// owns a timer (NFR-02).
/// </summary>
public partial class WorldClockCardViewModel : ObservableObject
{
    private readonly TimeZoneInfo _zone;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Hours))]
    [NotifyPropertyChangedFor(nameof(Minutes))]
    [NotifyPropertyChangedFor(nameof(Seconds))]
    [NotifyPropertyChangedFor(nameof(OffsetText))]
    [NotifyPropertyChangedFor(nameof(Time))]
    private DateTimeOffset _now = DateTimeOffset.MinValue;

    public WorldClockCardViewModel(string label, string timeZoneId, TimeZoneInfo zone)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        TimeZoneId = timeZoneId ?? throw new ArgumentNullException(nameof(timeZoneId));
        _zone = zone ?? throw new ArgumentNullException(nameof(zone));
    }

    public string Label { get; }

    public string TimeZoneId { get; }

    /// <summary>Zone wall-clock hours, zero-padded.</summary>
    public string Hours => TimeSegments.Of(Now).Hours;

    /// <summary>Zone wall-clock minutes, zero-padded.</summary>
    public string Minutes => TimeSegments.Of(Now).Minutes;

    /// <summary>Zone wall-clock seconds, zero-padded.</summary>
    public string Seconds => TimeSegments.Of(Now).Seconds;

    /// <summary>Zone wall-clock "HH:MM" for compact tray cards.</summary>
    public string Time => $"{Hours}:{Minutes}";

    /// <summary>True when the card may be removed from the tray (the local card cannot).</summary>
    public bool IsRemovable { get; set; } = true;

    /// <summary>Live UTC offset label for the current instant (e.g. "UTC+05:30").</summary>
    public string OffsetText => FormatOffset(Now.Offset);

    /// <summary>
    /// Applies the same instant to this card, converted to its own zone with a
    /// DST-safe offset (FR-11, NFR-06).
    /// </summary>
    public void SetPresent(DateTimeOffset instant) => Now = TimeZoneConverter.ToZoneTime(instant, _zone);

    /// <summary>Formats a timezone offset as a label; zero offset is "UTC".</summary>
    public static string FormatOffset(TimeSpan offset)
    {
        var totalMinutes = (int)offset.TotalMinutes;
        if (totalMinutes == 0)
        {
            return "UTC";
        }

        var sign = totalMinutes < 0 ? "-" : "+";
        var abs = Math.Abs(totalMinutes);
        return $"UTC{sign}{abs / 60:00}:{abs % 60:00}";
    }
}