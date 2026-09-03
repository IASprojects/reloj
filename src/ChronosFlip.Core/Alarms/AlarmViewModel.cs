using System.Collections.ObjectModel;
using System.Globalization;

namespace ChronosFlip.Core.Alarms;

/// <summary>Per-zone alarm presence used to paint card badges (FR-21).</summary>
public enum AlarmBadge
{
    None,
    Armed,
    Ringing,
}

/// <summary>
/// View-facing layer over <see cref="AlarmService"/>: keeps an observable
/// collection in sync with the service, forwards <see cref="Evaluate"/> from the
/// shared tick, and exposes per-zone badge state for the world-clock cards.
/// Structural mutations (add/remove/toggle/dismiss) raise <see cref="Changed"/>
/// so the shell can persist (FR-23); ringing transitions surface through
/// <see cref="AlarmRang"/> instead, so a ringing alarm never triggers a save
/// every tick. Zone-local wall times are converted to absolute instants via the
/// injected resolver + <c>TimeZoneConverter</c> (FR-20).
/// </summary>
public sealed class AlarmViewModel
{
    private readonly AlarmService _service;
    private readonly Func<string, TimeZoneInfo?> _zoneResolver;

    public AlarmViewModel(AlarmService service, Func<string, TimeZoneInfo?>? zoneResolver = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _zoneResolver = zoneResolver ?? ResolveSystemZone;
        _service.AlarmRang += OnServiceAlarmRang;
        foreach (var alarm in service.Alarms)
        {
            RefreshZoneText(alarm);
            Alarms.Add(alarm);
        }
    }

    public event EventHandler? Changed;

    /// <summary>Forwarded from the service when an alarm starts ringing (FR-22).</summary>
    public event EventHandler<Alarm>? AlarmRang;

    public ObservableCollection<Alarm> Alarms { get; } = new();

    public AlarmService Service => _service;

    /// <summary>Number of alarms currently ringing.</summary>
    public int RingingCount => _service.RingingCount;

    /// <summary>Creates an enabled single-occurrence alarm and syncs both stores.</summary>
    public void AddAlarm(string zoneId, DateTimeOffset fireAtUtc, string? label = null)
    {
        var alarm = new Alarm(zoneId, fireAtUtc, label);
        RefreshZoneText(alarm);
        _service.Add(alarm);
        Alarms.Add(alarm);
        OnChanged();
    }

    /// <summary>
    /// Creates an alarm from a zone-local wall time (e.g. date+time pickers).
    /// Resolves the zone, converts to the absolute instant (FR-20). Returns
    /// false when the zone is unknown/unresolvable.
    /// </summary>
    public bool AddAlarmAt(string zoneId, DateTime localWallTime, string? label = null)
    {
        var zone = _zoneResolver(zoneId);
        if (zone is null)
        {
            return false;
        }

        var fireAt = ChronosFlip.Core.WorldClock.TimeZoneConverter.FromZoneTime(localWallTime, zone);
        AddAlarm(zoneId, fireAt, label);
        return true;
    }

    /// <summary>Dismisses the ringing alarm for one zone (single stop action).</summary>
    public void DismissRingingForZone(string zoneId)
    {
        var ringing = _service.RingingForZone(zoneId);
        if (ringing.Count == 0)
        {
            return;
        }

        foreach (var alarm in ringing)
        {
            alarm.Dismiss();
        }

        OnChanged();
    }

    /// <summary>Drops an alarm from both stores; ringing alarms stop.</summary>
    public bool RemoveAlarm(string id)
    {
        var alarm = Find(id);
        if (alarm is null || !_service.Remove(id))
        {
            return false;
        }

        Alarms.Remove(alarm);
        OnChanged();
        return true;
    }

    /// <summary>Removes every alarm for a zone (cascade on zone removal).</summary>
    public void RemoveAlarmsForZone(string zoneId)
    {
        var removed = Alarms.Where(alarm =>
            string.Equals(alarm.ZoneId, zoneId, StringComparison.Ordinal)).ToList();
        if (removed.Count == 0)
        {
            return;
        }

        foreach (var alarm in removed)
        {
            Alarms.Remove(alarm);
        }

        _service.RemoveAllForZone(zoneId);
        OnChanged();
    }

    public Alarm? Find(string id) => _service.Find(id);

    public void SetEnabled(string id, bool enabled)
    {
        _service.SetEnabled(id, enabled);
        OnChanged();
    }

    /// <summary>Dismisses one ringing alarm (single occurrence stays disabled).</summary>
    public void Dismiss(string id)
    {
        _service.Dismiss(id);
        OnChanged();
    }

    /// <summary>Dismisses every ringing alarm at once.</summary>
    public void DismissAll()
    {
        _service.DismissAll();
        OnChanged();
    }

    /// <summary>
    /// Drives scheduling from the shared tick. When an alarm crosses its instant
    /// it rings via <see cref="AlarmRang"/> exactly once; re-evaluation across
    /// subsequent ticks is a no-op until dismissed (FR-22). No per-tick mutation
    /// events fire, so the shell persists only on real changes.
    /// </summary>
    public void Evaluate(DateTimeOffset now) => _service.Evaluate(now);

    /// <summary>Badge state for a zone id, for the card indicator (FR-21).</summary>
    public AlarmBadge BadgeFor(string zoneId) =>
        _service.RingingForZone(zoneId).Count > 0
            ? AlarmBadge.Ringing
            : _service.ActiveForZone(zoneId).Count > 0
                ? AlarmBadge.Armed
                : AlarmBadge.None;

    private void OnServiceAlarmRang(object? sender, Alarm alarm) => AlarmRang?.Invoke(this, alarm);

    private void RefreshZoneText(Alarm alarm)
    {
        var zone = _zoneResolver(alarm.ZoneId);
        if (zone is null)
        {
            alarm.ZoneTimeText = alarm.FireAtUtc.ToString("ddd, MMM d · HH:mm", CultureInfo.InvariantCulture);
            return;
        }

        var zoneNow = TimeZoneInfo.ConvertTime(alarm.FireAtUtc, zone);
        alarm.ZoneTimeText = zoneNow.ToString("ddd, MMM d · HH:mm", CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo? ResolveSystemZone(string zoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}