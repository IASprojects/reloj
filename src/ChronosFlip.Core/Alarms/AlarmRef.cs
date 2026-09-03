using System.Globalization;

namespace ChronosFlip.Core.Alarms;

/// <summary>
/// Persisted, serializable shape of an alarm: the absolute firing instant in
/// UTC plus the owning zone id and display label. All properties are nullable
/// so a partially-broken settings file degrades gracefully instead of being
/// quarantined wholesale (same contract as <c>ClockZoneRef</c>).
/// </summary>
public sealed class AlarmRef
{
    public string? Id { get; set; }

    public string? ZoneId { get; set; }

    public string? Label { get; set; }

    /// <summary>ISO-8601 UTC instant; never a calendar-less time.</summary>
    public string? FireAtUtc { get; set; }

    public bool IsEnabled { get; set; }

    public static AlarmRef FromAlarm(Alarm alarm) => new()
    {
        Id = alarm.Id,
        ZoneId = alarm.ZoneId,
        Label = alarm.Label,
        FireAtUtc = alarm.FireAtUtc.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
        IsEnabled = alarm.Enabled,
    };

    /// <summary>
    /// Converts back to an <see cref="Alarm"/>; returns null when required
    /// fields are blank or <c>FireAtUtc</c> is not parseable.
    /// </summary>
    public Alarm? ToAlarm()
    {
        if (string.IsNullOrWhiteSpace(Id) ||
            string.IsNullOrWhiteSpace(ZoneId) ||
            string.IsNullOrWhiteSpace(FireAtUtc) ||
            !DateTimeOffset.TryParse(FireAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var fireAt))
        {
            return null;
        }

        var alarm = Alarm.Restore(Id!, ZoneId!, fireAt, Label);
        alarm.Enabled = IsEnabled;
        return alarm;
    }
}