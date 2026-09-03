using System.Collections.ObjectModel;

namespace ChronosFlip.Core.Alarms;

/// <summary>
/// Pure alarm engine: owns the alarm set and decides when an alarm rings.
/// Driven by <see cref="Evaluate(DateTimeOffset)"/> fed from the shared 1s tick
/// (NFR-02) — no timers, no UI dependency, fully unit-testable. Firing is
/// single-shot per alarm: once <see cref="Alarm.Fire"/> latches, further
/// evaluates no-op (FR-22).
/// </summary>
public sealed class AlarmService
{
    private readonly Dictionary<string, Alarm> _alarmsById = new(StringComparer.Ordinal);

    public AlarmService(IEnumerable<Alarm>? alarms = null)
    {
        if (alarms is not null)
        {
            foreach (var alarm in alarms.Where(a => a is not null))
            {
                Add(alarm);
            }
        }
    }

    /// <summary>Most recent instant passed to <see cref="Evaluate"/>; drives "future" badge checks.</summary>
    private DateTimeOffset _lastEvaluatedAt = DateTimeOffset.MinValue;

    public event EventHandler<Alarm>? AlarmRang;

    public IReadOnlyList<Alarm> Alarms => _alarmsById.Values.OrderBy(AlarmSortKey).ToList();

    /// <summary>Number of alarms ringing right now (for shell-level badges).</summary>
    public int RingingCount => _alarmsById.Values.Count(alarm => alarm.IsRinging);

    public Alarm? Find(string id) =>
        string.IsNullOrWhiteSpace(id) ? null : _alarmsById.GetValueOrDefault(id);

    public void Add(Alarm alarm)
    {
        ArgumentNullException.ThrowIfNull(alarm);
        _alarmsById[alarm.Id] = alarm;
    }

    public bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _alarmsById.Remove(id);
    }

    /// <summary>Drops every alarm belonging to a zone (zone removed from tray).</summary>
    public void RemoveAllForZone(string zoneId)
    {
        foreach (var id in _alarmsById.Values
                     .Where(alarm => string.Equals(alarm.ZoneId, zoneId, StringComparison.Ordinal))
                     .Select(alarm => alarm.Id)
                     .ToList())
        {
            _alarmsById.Remove(id);
        }
    }

    public void SetEnabled(string id, bool enabled)
    {
        var alarm = Find(id);
        if (alarm is not null)
        {
            alarm.Enabled = enabled;
        }
    }

    /// <summary>
    /// Stops ringing for an alarm and disables it (single occurrence is spent).
    /// </summary>
    public void Dismiss(string id)
    {
        Find(id)?.Dismiss();
    }

    /// <summary>Dismisses every currently ringing alarm.</summary>
    public void DismissAll()
    {
        foreach (var alarm in _alarmsById.Values.Where(alarm => alarm.IsRinging).ToList())
        {
            alarm.Dismiss();
        }
    }

    /// <summary>
    /// Rings every enabled, not-yet-fired alarm whose instant has passed.
    /// Call from the shared tick with the current instant.
    /// </summary>
    public void Evaluate(DateTimeOffset now)
    {
        _lastEvaluatedAt = now;
        foreach (var alarm in _alarmsById.Values)
        {
            if (!alarm.Enabled || alarm.IsRinging || alarm.FireAtUtc > now)
            {
                continue;
            }

            alarm.Fire();
            AlarmRang?.Invoke(this, alarm);
        }
    }

    /// <summary>Active (enabled, future) alarms for the given zone.</summary>
    public IReadOnlyList<Alarm> ActiveForZone(string zoneId)
    {
        var now = _lastEvaluatedAt == DateTimeOffset.MinValue ? DateTimeOffset.UtcNow : _lastEvaluatedAt;
        return _alarmsById.Values
            .Where(alarm => string.Equals(alarm.ZoneId, zoneId, StringComparison.Ordinal) &&
                            alarm.Enabled &&
                            alarm.FireAtUtc > now)
            .OrderBy(AlarmSortKey)
            .ToList();
    }

    /// <summary>Ringing alarms for the given zone.</summary>
    public IReadOnlyList<Alarm> RingingForZone(string zoneId) =>
        _alarmsById.Values
            .Where(alarm => alarm.IsRinging &&
                            string.Equals(alarm.ZoneId, zoneId, StringComparison.Ordinal))
            .ToList();

    private static DateTimeOffset AlarmSortKey(Alarm alarm) => alarm.FireAtUtc;
}