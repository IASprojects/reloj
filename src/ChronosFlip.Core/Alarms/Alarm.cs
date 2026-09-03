using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.Alarms;

/// <summary>
/// A single-occurrence alarm targeting an absolute instant. <see cref="FireAtUtc"/>
/// is stored in UTC so re-arming after a reboot is a pure comparison against
/// "now" — no calendar re-derivation (FR-23). Kept observable so card badges
/// and the alarm panel can react to ringing state without extra plumbing.
/// </summary>
public partial class Alarm : ObservableObject
{
    private static long s_nextId = DateTime.UtcNow.Ticks;

    private static string NextId() => (s_nextId++).ToString("x");

    [ObservableProperty]
    private bool _enabled = true;

    [ObservableProperty]
    private bool _isRinging;

    /// <summary>
    /// Zone-local display string (e.g. "Tue, Sep 12 · 14:30"). Computed by the
    /// view-model layer after restore so the panel can bind a plain string.
    /// </summary>
    [ObservableProperty]
    private string _zoneTimeText = string.Empty;

    public Alarm(string zoneId, DateTimeOffset fireAtUtc, string? label = null)
        : this(NextId(), zoneId, fireAtUtc, label)
    {
    }

    private Alarm(string id, string zoneId, DateTimeOffset fireAtUtc, string? label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId);
        Id = id;
        ZoneId = zoneId;
        FireAtUtc = fireAtUtc;
        Label = string.IsNullOrWhiteSpace(label) ? zoneId : label!;
    }

    public string Id { get; }

    /// <summary>Id of the world-clock zone the alarm belongs to (FR-20).</summary>
    public string ZoneId { get; }

    /// <summary>Display label (zone name) at creation time.</summary>
    public string Label { get; }

    /// <summary>Absolute firing instant, UTC-normalized.</summary>
    public DateTimeOffset FireAtUtc { get; }

    /// <summary>True once this alarm has already fired this session (single occurrence).</summary>
    public bool HasFired { get; private set; }

    public void Fire()
    {
        if (HasFired)
        {
            return;
        }

        HasFired = true;
        IsRinging = true;
    }

    /// <summary>Stops the ringing state; the alarm stays in the list as disabled.</summary>
    public void Dismiss()
    {
        IsRinging = false;
        Enabled = false;
    }

    /// <summary>Restores an alarm with a caller-provided id (persistence re-arm).</summary>
    public static Alarm Restore(string id, string zoneId, DateTimeOffset fireAtUtc, string? label = null) =>
        new(id, zoneId, fireAtUtc, label);
}