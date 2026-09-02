using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Core.Settings;

public sealed class ChronosFlipSettings
{
    public int Version { get; set; } = SettingsVersions.Current;

    public bool NeonEnabled { get; set; } = false;

    public string NeonHexColor { get; set; } = SettingsDefaults.NeonHexColor;

    public bool PinToTop { get; set; } = false;

    public WindowBounds? Window { get; set; }

    /// <summary>Persisted world-clock zones (local card never included).</summary>
    public List<ClockZoneRef>? Zones { get; set; }
}

public sealed class WindowBounds
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public static class SettingsVersions
{
    public const int Current = 1;
}

public static class SettingsDefaults
{
    public const string NeonHexColor = "#00E5FF";
    public const int WindowWidth = 672;
    public const int WindowHeight = 340;
}
