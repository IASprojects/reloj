using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Zone picker state: a searchable list of zone options that can exclude ids
/// already shown on the tray. Purely a selection surface — the world-clock tray
/// add/remove is driven by these ids.
/// </summary>
public partial class ZonePickerViewModel : ObservableObject
{
    private readonly HashSet<string> _excludedIds = new(StringComparer.Ordinal);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Zones))]
    private string _searchText = string.Empty;

    public ZonePickerViewModel(ClockZoneFactory zoneFactory)
    {
        ArgumentNullException.ThrowIfNull(zoneFactory);
        _allZones = zoneFactory.AllAvailable().ToList();
    }

    private readonly List<ClockZone> _allZones;

    public IEnumerable<ClockZone> Zones
    {
        get
        {
            var filter = SearchText.Trim();
            return _allZones.Where(zone =>
                !_excludedIds.Contains(zone.TimeZoneId) &&
                (filter.Length == 0 ||
                 zone.Label.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                 zone.TimeZoneId.Contains(filter, StringComparison.OrdinalIgnoreCase)));
        }
    }

    /// <summary>Hides a zone id already shown on the tray.</summary>
    public void Exclude(string timeZoneId)
    {
        if (timeZoneId is null || !_excludedIds.Add(timeZoneId))
        {
            return;
        }

        OnPropertyChanged(nameof(Zones));
    }

    /// <summary>Restores a zone id previously excluded.</summary>
    public void Include(string timeZoneId)
    {
        if (timeZoneId is null || !_excludedIds.Remove(timeZoneId))
        {
            return;
        }

        OnPropertyChanged(nameof(Zones));
    }
}