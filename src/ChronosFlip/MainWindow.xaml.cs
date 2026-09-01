using System.Runtime.InteropServices;
using ChronosFlip.Core.Clocks;
using ChronosFlip.Core.Settings;
using ChronosFlip.Core.ViewModels;
using ChronosFlip.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using WinRT.Interop;

namespace ChronosFlip;

public sealed partial class MainWindow : Window
{
    private readonly ClockService _clock;
    private readonly SettingsStore _settingsStore;
    private readonly IntPtr _hwnd;

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
        var clockViewModel = new MainClockViewModel();
        clockViewModel.Attach(ticker);

        _clock = new ClockService(DispatcherQueue, ticker);

        RootGrid.DataContext = clockViewModel;
        _clock.Start();

        SettingsPanel.ViewModel = ViewModel;

        Title = "Chronos Flip";
        RestoreWindowBounds(loaded);
        ApplyPinToTop(loaded.PinToTop);

        Closed += OnClosed;
    }

    public SettingsViewModel ViewModel { get; }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        SaveWindowBounds();
        _clock.Dispose();
    }

    private void ApplyNeonAccent(string hex)
    {
        if (Application.Current?.Resources is null)
        {
            return;
        }
        if (!Application.Current.Resources.TryGetValue("NeonAccentBrush", out var resource) ||
            resource is not SolidColorBrush brush)
        {
            return;
        }
        if (ChronosFlip.Converters.HexToColorConverter.TryParse(hex, out var color))
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

    private void ApplyPinToTop(bool pin)
    {
        SetWindowPos(_hwnd,
            pin ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    private void SaveWindowBounds()
    {
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

    private const int HWND_TOPMOST = -1;
    private const int HWND_NOTOPMOST = -2;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

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
