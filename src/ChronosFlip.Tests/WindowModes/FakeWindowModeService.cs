using ChronosFlip.Core.WindowModes;

namespace ChronosFlip.Tests.WindowModes;

/// <summary>Records <see cref="IWindowModeService"/> calls for ViewModel tests.</summary>
public sealed class FakeWindowModeService : IWindowModeService
{
    public bool IsFullScreen { get; private set; }

    /// <summary>Chronological record of invoked members (e.g. "EnterFullScreen", "SetTopmost(True)").</summary>
    public List<string> Calls { get; } = [];

    /// <summary>Last <see cref="SetTopmost"/> value; null until first pin call.</summary>
    public bool? LastTopmost { get; private set; }

    public bool ThrowOnEnter { get; set; }

    public bool ThrowOnExit { get; set; }

    public bool ThrowOnTopmost { get; set; }

    public void EnterFullScreen()
    {
        Calls.Add(nameof(EnterFullScreen));
        if (ThrowOnEnter)
        {
            throw new InvalidOperationException("enter failed");
        }

        IsFullScreen = true;
    }

    public void ExitFullScreen()
    {
        Calls.Add(nameof(ExitFullScreen));
        if (ThrowOnExit)
        {
            throw new InvalidOperationException("exit failed");
        }

        IsFullScreen = false;
    }

    public void SetTopmost(bool pin)
    {
        Calls.Add($"SetTopmost({pin})");
        if (ThrowOnTopmost)
        {
            throw new InvalidOperationException("topmost failed");
        }

        LastTopmost = pin;
    }
}