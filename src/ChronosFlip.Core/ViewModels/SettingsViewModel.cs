using ChronosFlip.Core.Settings;
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
    }
}
