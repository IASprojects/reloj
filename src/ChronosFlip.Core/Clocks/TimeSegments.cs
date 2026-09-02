using System.Globalization;

namespace ChronosFlip.Core.Clocks;

/// <summary>
/// Splits a <see cref="DateTimeOffset"/> into fixed-width HH / MM / SS strings
/// for the flip-card digits. Used by every clock card so all cards render from
/// a single segmentation point (maintains width stability on the 1s tick).
/// </summary>
public readonly record struct TimeSegments(string Hours, string Minutes, string Seconds)
{
    /// <summary>Splits <paramref name="value"/> into fixed-width HH / MM / SS strings.</summary>
    public static TimeSegments Of(DateTimeOffset value)
    {
        return new TimeSegments(
            value.ToString("HH", CultureInfo.InvariantCulture),
            value.ToString("mm", CultureInfo.InvariantCulture),
            value.ToString("ss", CultureInfo.InvariantCulture));
    }
}