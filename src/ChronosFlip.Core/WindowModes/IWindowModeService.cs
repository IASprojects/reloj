namespace ChronosFlip.Core.WindowModes;

/// <summary>
/// Abstraction over native window windowing so fullscreen / topmost behavior can
/// be unit-tested without a WinUI window (AGENTS.md, NFR-05). Implemented by the
/// WinUI app via <c>WinUIWindowModeService</c>.
/// </summary>
public interface IWindowModeService
{
    /// <summary>True while the window is in fullscreen "Desktop Clock" mode.</summary>
    bool IsFullScreen { get; }

    /// <summary>Expands the window to cover the screen without blocking the OS.</summary>
    void EnterFullScreen();

    /// <summary>Restores the widget bounds and standard window chrome.</summary>
    void ExitFullScreen();

    /// <summary>Pins (topmost) or unpins the window above other apps.</summary>
    void SetTopmost(bool pin);
}