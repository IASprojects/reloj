using ChronosFlip.Core.Settings;
using ChronosFlip.Core.WorldClock;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.ViewModels;

/// <summary>
/// View-facing wrapper over <see cref="ChronosFlipSettings"/>. Keep neon state
/// in this VM (string hex, no WinUI dep) so the View can bind through a
/// value converter.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsStore _store;

    [ObservableProperty]
    private bool _neonEnabled;

    [ObservableProperty]
    private string _neonHexColor = SettingsDefaults.NeonHexColor;

    [ObservableProperty]
    private bool _pinToTop;

    /// <summary>Current world-clock tray zones in order (persisted on save).</summary>
    public IReadOnlyList<ClockZone> Zones { get; private set; } = [];

    public SettingsViewModel(SettingsStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public ChronosFlipSettings Load()
    {
        var loaded = _store.Load();
        Apply(loaded);
        return loaded;
    }

    public void Save()
    {
        var settings = new ChronosFlipSettings
        {
            NeonEnabled = NeonEnabled,
            NeonHexColor = NeonHexColor,
            PinToTop = PinToTop,
            Zones = Zones.Select(ClockZoneRef.FromClockZone).ToList(),
            Window = _store.Load().Window,
        };
        _store.Save(settings);
    }

    public void Apply(ChronosFlipSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        NeonEnabled = settings.NeonEnabled;
        NeonHexColor = string.IsNullOrWhiteSpace(settings.NeonHexColor)
            ? SettingsDefaults.NeonHexColor
            : settings.NeonHexColor;
        PinToTop = settings.PinToTop;
        Zones = settings.Zones?
            .Where(zone => zone is not null)
            .Select(zone => zone!.ToClockZone())
            .Where(zone => zone is not null)
            .Cast<ClockZone>()
            .ToList() ?? [];
        OnPropertyChanged(nameof(Zones));
    }

    /// <summary>Installs the current world-clock tray zones (persisted on next save).</summary>
    public void SetZones(IEnumerable<ClockZone> zones)
    {
        ArgumentNullException.ThrowIfNull(zones);
        Zones = zones.ToList();
        OnPropertyChanged(nameof(Zones));
    }
}
