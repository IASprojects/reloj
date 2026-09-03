using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ChronosFlip.Core.Clocks;
using ChronosFlip.Core.Settings;
using ChronosFlip.Core.ViewModels;
using ChronosFlip.Core.WindowModes;
using ChronosFlip.Core.WorldClock;
using ChronosFlip.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using Windows.System;
using WinRT.Interop;

namespace ChronosFlip;

public sealed partial class MainWindow : Window
{
    private readonly ClockService _clock;
    private readonly SettingsStore _settingsStore;
    private readonly ZonePickerViewModel _zonePicker;
    private readonly IntPtr _hwnd;
    private readonly WinUIWindowModeService _windowModeService;

    public MainWindow()
    {
        InitializeComponent();

        _hwnd = WindowNative.GetWindowHandle(this);

        _settingsStore = new SettingsStore(SettingsStore.DefaultDirectory());
        ViewModel = new SettingsViewModel(_settingsStore);
        var loaded = ViewModel.Load();
        ApplyNeonAccent(ViewModel.NeonHexColor);
        ViewModel.PropertyChanged += (_, _) => ApplyNeonAccent(ViewModel.NeonHexColor);

        var ticker = new ClockTicker(new SystemClock());

        WorldClock = new WorldClockViewModel(new SystemZoneResolver(), ViewModel.Zones);
        WorldClock.Cards.CollectionChanged += OnCardsChanged;
        WorldClock.Attach(ticker);
        RefreshOtherCards();

        _zonePicker = new ZonePickerViewModel(new ClockZoneFactory(new SystemZoneResolver()));
        _zonePicker.Reset(WorldClock.Cards.Select(card => card.TimeZoneId));

        _clock = new ClockService(DispatcherQueue, ticker);

        _windowModeService = new WinUIWindowModeService(this, _hwnd);
        WindowMode = new WindowModeViewModel(_windowModeService, ViewModel);
        WindowMode.PropertyChanged += OnWindowModePropertyChanged;
        ApplyShellMode(false);

        RootGrid.DataContext = WorldClock;
        _clock.Start();

        SettingsPanel.ViewModel = ViewModel;
        ZonePicker.ViewModel = _zonePicker;
        ZonePicker.SetTray(WorldClock.Cards);
        ZonePicker.AddRequested += OnZoneAddRequested;
        ZonePicker.RemoveRequested += OnZoneRemoveRequested;

        Title = "Chronos Flip";
        RestoreWindowBounds(loaded);
        _windowModeService.SetTopmost(loaded.PinToTop);

        Closed += OnClosed;
    }

    public SettingsViewModel ViewModel { get; }

    public WorldClockViewModel WorldClock { get; }

    public WindowModeViewModel WindowMode { get; }

    /// <summary>Non-local zone cards for the fullscreen bottom strip.</summary>
    public ObservableCollection<WorldClockCardViewModel> OtherCards { get; } = new();

    private void OnClosed(object sender, WindowEventArgs args)
    {
        SettingsPanel.CancelPendingSave();
        SaveWindowBounds();
        _clock.Dispose();
    }

    private void OnZoneAddRequested(object? sender, ClockZone zone)
    {
        WorldClock.AddZone(zone);
    }

    private void OnZoneRemoveRequested(object? sender, string zoneId)
    {
        WorldClock.RemoveZone(zoneId);
    }

    private void OnCardsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshOtherCards();
        _zonePicker.Reset(WorldClock.Cards.Select(card => card.TimeZoneId));
        ViewModel.SetZones(WorldClock.ZonesToPersist());
        ViewModel.Save();
    }

    private void RefreshOtherCards()
    {
        OtherCards.Clear();
        foreach (var card in WorldClock.Cards.Where(card => !ReferenceEquals(card, WorldClock.LocalCard)))
        {
            OtherCards.Add(card);
        }
    }

    private void OnWindowModePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(WindowModeViewModel.IsFullScreen))
        {
            return;
        }

        var fullScreen = WindowMode.IsFullScreen;
        ApplyShellMode(fullScreen);

        if (fullScreen)
        {
            ExitFullScreenButton.Focus(FocusState.Programmatic);
        }
        else
        {
            SaveWindowBounds();
        }
    }

    private void ApplyShellMode(bool fullScreen)
    {
        NeonShell.Visibility = fullScreen ? Visibility.Collapsed : Visibility.Visible;
        NeonShellFullScreen.Visibility = fullScreen ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnFullScreenClicked(object sender, RoutedEventArgs e)
    {
        WindowMode.ToggleFullScreen();
    }

    private void OnRootKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape && WindowMode.IsFullScreen)
        {
            e.Handled = true;
            WindowMode.RequestExit();
        }
    }

    private void ApplyNeonAccent(string hex)
    {
        if (!ChronosFlip.Converters.HexToColorConverter.TryParse(hex, out var color))
        {
            return;
        }

        NeonShell.AccentColor = color;
        NeonShellFullScreen.AccentColor = color;

        if (Application.Current?.Resources is null)
        {
            return;
        }
        if (Application.Current.Resources.TryGetValue("NeonAccentBrush", out var resource) &&
            resource is Microsoft.UI.Xaml.Media.SolidColorBrush brush)
        {
            brush.Color = color;
        }
    }

    private void RestoreWindowBounds(ChronosFlipSettings settings)
    {
        if (settings.Window is { } saved && saved.Width > 0 && saved.Height > 0)
        {
            try
            {
                AppWindow.MoveAndResize(new RectInt32(saved.X, saved.Y, saved.Width, saved.Height));
                return;
            }
            catch
            {
            }
        }
        AppWindow.Resize(new SizeInt32(SettingsDefaults.WindowWidth, SettingsDefaults.WindowHeight));
    }

    private void SaveWindowBounds()
    {
        if (WindowMode.IsFullScreen)
        {
            return;
        }

        try
        {
            if (!GetWindowRect(_hwnd, out var rect))
            {
                return;
            }

            var settings = new ChronosFlipSettings
            {
                NeonEnabled = ViewModel.NeonEnabled,
                NeonHexColor = ViewModel.NeonHexColor,
                PinToTop = ViewModel.PinToTop,
                Zones = ViewModel.Zones.Select(ClockZoneRef.FromClockZone).ToList(),
                Window = new WindowBounds
                {
                    X = rect.left,
                    Y = rect.top,
                    Width = rect.right - rect.left,
                    Height = rect.bottom - rect.top,
                },
            };
            _settingsStore.Save(settings);
        }
        catch
        {
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}