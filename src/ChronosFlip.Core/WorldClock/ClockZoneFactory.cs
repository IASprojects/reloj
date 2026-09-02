namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Creates <see cref="ClockZone"/> instances from resolved timezones, deriving
/// a human-friendly label from the Windows display name (FR-13).
/// </summary>
public sealed class ClockZoneFactory
{
    private readonly IZoneResolver _resolver;

    public ClockZoneFactory(IZoneResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public bool TryCreate(string timeZoneId, string? label, out ClockZone? zone)
    {
        zone = null;

        var resolved = _resolver.Resolve(timeZoneId);
        if (resolved is null)
        {
            return false;
        }

        zone = new ClockZone
        {
            Label = string.IsNullOrWhiteSpace(label) ? DefaultLabel(resolved) : label,
            TimeZoneId = resolved.Id,
        };
        return true;
    }

    public IEnumerable<ClockZone> AllAvailable()
    {
        foreach (var resolved in _resolver.AvailableZones)
        {
            yield return new ClockZone
            {
                Label = DefaultLabel(resolved),
                TimeZoneId = resolved.Id,
            };
        }
    }

    public static string DefaultLabel(TimeZoneInfo zone)
    {
        var displayName = zone.DisplayName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return zone.Id;
        }

        if (displayName.StartsWith("(UTC", StringComparison.OrdinalIgnoreCase))
        {
            var closeParen = displayName.IndexOf(')');
            if (closeParen >= 0)
            {
                var trimmed = displayName[(closeParen + 1)..].Trim();
                return string.IsNullOrWhiteSpace(trimmed) ? zone.Id : trimmed;
            }
        }

        return displayName;
    }
}