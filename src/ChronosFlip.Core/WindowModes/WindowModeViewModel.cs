using System.ComponentModel;
using ChronosFlip.Core.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.WindowModes;

/// <summary>
/// View-facing state for window modes (FR-40–43, FR-50–52). Fullscreen state is
/// guarded (re-enter/re-exit) and resilient to <see cref="IWindowModeService"/>
/// failures; pin state is a write-through to <see cref="SettingsViewModel.PinToTop"/>
/// so persistence rides the existing debounced save.
/// </summary>
public partial class WindowModeViewModel : ObservableObject
{
    private readonly IWindowModeService _service;
    private readonly SettingsViewModel _settings;

    [ObservableProperty]
    private bool _isFullScreen;

    public WindowModeViewModel(IWindowModeService service, SettingsViewModel settings)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    /// <summary>Pin state read/written through to the settings so it persists (FR-51).</summary>
    public bool IsPinActive
    {
        get => _settings.PinToTop;
        set
        {
            if (_settings.PinToTop == value)
            {
                return;
            }

            TogglePin();
            OnPropertyChanged(nameof(IsPinActive));
        }
    }

    /// <summary>Enters fullscreen; no-op when already fullscreen.</summary>
    public void EnterFullScreen()
    {
        if (IsFullScreen)
        {
            return;
        }

        try
        {
            _service.EnterFullScreen();
            IsFullScreen = true;
        }
        catch
        {
        }
    }

    /// <summary>Exits fullscreen; no-op unless fullscreen.</summary>
    public void ExitFullScreen()
    {
        if (!IsFullScreen)
        {
            return;
        }

        try
        {
            _service.ExitFullScreen();
            IsFullScreen = false;
            _service.SetTopmost(_settings.PinToTop);
        }
        catch
        {
        }
    }

    /// <summary>Shared enter/exit toggle for the same UI button (FR-40/42).</summary>
    public void ToggleFullScreen()
    {
        if (IsFullScreen)
        {
            ExitFullScreen();
        }
        else
        {
            EnterFullScreen();
        }
    }

    /// <summary>Esc hook: exits fullscreen; no-op in widget mode (FR-42).</summary>
    public void RequestExit() => ExitFullScreen();

    /// <summary>Applies the pin to the OS window and persists via settings (FR-50/51).</summary>
    public void TogglePin()
    {
        var target = !IsPinActive;
        try
        {
            _service.SetTopmost(target);
            _settings.PinToTop = target;
        }
        catch
        {
        }
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.PinToTop))
        {
            OnPropertyChanged(nameof(IsPinActive));
        }
    }
}