# Alarms

- **Status:** `Planned`
- **Requirements:** FR-20, FR-21, FR-22, FR-23 (+ FR-24 P2)
- **Depends on:** 02 (clock/monotonic tick), 03 (persistence), 04 (per-zone)
- **Stage:** 5

## Goal

Per-zone, single-occurrence alarms with audible + visual notification and full
persistence.

## Scope

- `Alarm` model (zone, `DateTimeOffset`, enabled) and `AlarmService`
  (scheduling, firing, dismissal).
- Enable/disable per zone; on-card status indicator.
- Ringing state: sound + visual highlight, dismissed by user action.
- Re-arm correctly after reboot (persist next-firing schedule).
- (P2) Recurring alarms (weekly / workdays).

## Out of scope

- Multiple simultaneous alarm UI polish, ringtone picker, snooze.

## Acceptance Criteria

- Alarm fires within tolerance of the target instant (testable with injected
  clock).
- Disabled alarms never fire.
- Added/disabled alarms persist and re-arm across restart.
- Ringing is clearly visible and dismissible.

## Verification

`dotnet test` on `AlarmService` scheduling (injected `DateTimeOffset`, no
sleep); manual set + wait on Windows.