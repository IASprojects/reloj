using System.Runtime.InteropServices;
using ChronosFlip.Core.WindowModes;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

namespace ChronosFlip.Services;

/// <summary>
/// Native window-mode implementation (NFR-05). Expands the window to the
/// display's outer bounds without a focus lock, using an <see cref="OverlappedPresenter"/>
/// in borderless, non-resizable, always-on-top form; restores the pre-fullscreen
/// widget bounds and chrome on exit. Topmost pinning delegates to HWND
/// <c>SetWindowPos</c>.
/// </summary>
public sealed class WinUIWindowModeService : IWindowModeService
{
    private readonly AppWindow _appWindow;
    private readonly IntPtr _hwnd;
    private RectInt32? _widgetBounds;

    public WinUIWindowModeService(Window window, IntPtr hwnd)
    {
        _hwnd = hwnd;
        _appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hwnd));
    }

    public bool IsFullScreen => _widgetBounds is not null;

    public void EnterFullScreen()
    {
        if (IsFullScreen)
        {
            return;
        }

        _widgetBounds = new RectInt32(
            _appWindow.Position.X, _appWindow.Position.Y,
            _appWindow.Size.Width, _appWindow.Size.Height);

        var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Nearest)
            ?? DisplayArea.Primary;

        var presenter = _appWindow.Presenter as OverlappedPresenter;
        presenter?.SetBorderAndTitleBar(false, false);
        if (presenter is not null)
        {
            presenter.IsResizable = false;
            presenter.IsAlwaysOnTop = true;
        }

        _appWindow.MoveAndResize(displayArea.OuterBounds);
    }

    public void ExitFullScreen()
    {
        var bounds = _widgetBounds;
        _widgetBounds = null;
        if (bounds is null)
        {
            return;
        }

        var presenter = _appWindow.Presenter as OverlappedPresenter;
        presenter?.SetBorderAndTitleBar(true, true);
        if (presenter is not null)
        {
            presenter.IsResizable = true;
            presenter.IsAlwaysOnTop = false;
        }

        if (bounds.Value.Width > 0 && bounds.Value.Height > 0)
        {
            _appWindow.MoveAndResize(bounds.Value);
        }
    }

    public void SetTopmost(bool pin)
    {
        SetWindowPos(_hwnd,
            pin ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    private const int HWND_TOPMOST = -1;
    private const int HWND_NOTOPMOST = -2;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}