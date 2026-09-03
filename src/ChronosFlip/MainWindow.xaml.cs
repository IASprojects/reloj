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
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics;
using Windows.System;
using Windows.UI;
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
        ConfigureCustomTitleBar();

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

    private void ConfigureCustomTitleBar()
    {
        // Code-only (a XAML setter throws at runtime).
        ExtendsContentIntoTitleBar = true;

        var titleBar = AppWindow.TitleBar;
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            // Button backgrounds honor alpha; colors map 1:1 to Nocturne tokens:
            // 0x2A2A2A = CardSurfaceBrush, 0x3A3A3A = CardBorderBrush, 0xF5F5F5 = TextOnCardBrush.
            var clear = Color.FromArgb(0, 0, 0, 0);
            titleBar.ButtonBackgroundColor = clear;
            titleBar.ButtonInactiveBackgroundColor = clear;
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 0x2A, 0x2A, 0x2A);
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 0x3A, 0x3A, 0x3A);
            titleBar.ButtonForegroundColor = Color.FromArgb(255, 0xF5, 0xF5, 0xF5);
            titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0xF5, 0xF5, 0xF5);
            titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 0x3A, 0x3A, 0x3A);

            // Collapse the native caption strip so the OS buttons disappear entirely;
            // dragging and button click-through are handled via InputNonClientPointerSource.
            // Some Windows 10 builds reject Collapsed: keep the default chrome there.
            try
            {
                titleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
            }
            catch
            {
            }
        }

        AppWindow.Changed += OnAppWindowChanged;
        HeaderHost.Loaded += (_, _) =>
        {
            if (HeaderHost.XamlRoot is not null)
            {
                HeaderHost.XamlRoot.Changed += (_, _) => UpdateTitleBarRegions();
            }
            UpdateTitleBarRegions();
        };
        HeaderHost.SizeChanged += (_, _) => UpdateTitleBarRegions();
        UpdateMaxRestoreGlyph(AppWindow.Presenter as OverlappedPresenter);
    }

    private void UpdateTitleBarRegions()
    {
        if (!ExtendsContentIntoTitleBar || (WindowMode?.IsFullScreen ?? true))
        {
            return;
        }

        if (HeaderHost.ActualWidth <= 0 || HeaderHost.ActualHeight <= 0)
        {
            return;
        }

        var scale = HeaderHost.XamlRoot?.RasterizationScale ?? 1.0;

        try
        {
            HeaderRightPad.Width = new GridLength(AppWindow.TitleBar.RightInset / scale);
        }
        catch
        {
        }
        try
        {
            HeaderLeftPad.Width = new GridLength(AppWindow.TitleBar.LeftInset / scale);
        }
        catch
        {
        }

        var source = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);

#pragma warning disable CS0612, CS0618
        var drag = ToPhysicalRect(HeaderHost, scale);
        if (drag.Width > 0 && drag.Height > 0)
        {
            AppWindow.TitleBar.SetDragRectangles([drag]);
        }
#pragma warning restore CS0612, CS0618

        var passthrough = new List<RectInt32>();
        AddPassthrough(HeaderTools, scale, passthrough);
        AddPassthrough(CaptionButtons, scale, passthrough);
        source.SetRegionRects(NonClientRegionKind.Passthrough, passthrough.ToArray());
    }

    private static RectInt32 ToPhysicalRect(FrameworkElement element, double scale)
    {
        var bounds = element.TransformToVisual(null)
            .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        return new RectInt32(
            (int)Math.Round(bounds.X * scale),
            (int)Math.Round(bounds.Y * scale),
            (int)Math.Round(bounds.Width * scale),
            (int)Math.Round(bounds.Height * scale));
    }

    private void AddPassthrough(FrameworkElement element, double scale, List<RectInt32> results)
    {
        if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
        {
            return;
        }

        results.Add(ToPhysicalRect(element, scale));
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs e)
    {
        if (e.DidPresenterChange)
        {
            UpdateMaxRestoreGlyph(sender.Presenter as OverlappedPresenter);
        }
    }

    private void UpdateMaxRestoreGlyph(OverlappedPresenter? presenter)
    {
        if (presenter is null)
        {
            return;
        }

        var maximized = presenter.State == OverlappedPresenterState.Maximized;
        MaxRestoreIcon.Glyph = maximized ? "\uE924" : "\uE923";
        ToolTipService.SetToolTip(MaximizeRestoreButton, maximized ? "Restore" : "Maximize");
    }

    private void OnMinimizeClicked(object sender, RoutedEventArgs e)
    {
        (AppWindow.Presenter as OverlappedPresenter)?.Minimize();
    }

    private void OnMaxRestoreClicked(object sender, RoutedEventArgs e)
    {
        var presenter = AppWindow.Presenter as OverlappedPresenter;
        if (presenter is null)
        {
            return;
        }

        if (presenter.State == OverlappedPresenterState.Maximized)
        {
            presenter.Restore();
        }
        else
        {
            presenter.Maximize();
        }
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();

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

        var source = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
        if (fullScreen)
        {
            // Drop drag + passthrough so the exit button stays clickable (no stale caption strip).
            source.SetRegionRects(NonClientRegionKind.Passthrough, []);
#pragma warning disable CS0612, CS0618
            try
            {
                AppWindow.TitleBar.SetDragRectangles([]);
            }
            catch
            {
            }
#pragma warning restore CS0612, CS0618
            return;
        }

        UpdateTitleBarRegions();
        UpdateMaxRestoreGlyph(AppWindow.Presenter as OverlappedPresenter);
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
            var placement = new WINDOWPLACEMENT { length = Marshal.SizeOf<WINDOWPLACEMENT>() };
            if (!GetWindowPlacement(_hwnd, ref placement))
            {
                return;
            }

            var rect = placement.rcNormalPosition;
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

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}