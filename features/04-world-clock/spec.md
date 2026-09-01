# World Clock

- **Status:** `Planned`
- **Requirements:** FR-06, FR-10, FR-12, FR-13
- **Depends on:** 02 (flip card + 1s tick), 03 (zone list persistence)
- **Stage:** 3

## Goal

Display local time plus N user-selected time zones as a tray of flip cards,
with a zone picker to add/remove them.

## Scope

- `ClockZone` model (label, `TimeZoneInfo` id, UTC offset) + factory.
- `WorldClockViewModel` rendering one card per zone off the single 1s tick.
- Zone picker UI, add/remove, byte-safe `TimeZoneInfo` conversion (DST-aware).
- Zone list persistence via `SettingsStore`.

## Out of scope

- Per-card alarms (see 06), per-card neon (see 08), fullscreen (see 05).

## Acceptance Criteria

- Cards show label + current time; all advance from one tick.
- Timestamps correct across DST boundaries (testable).
- Add/remove persists and restores across restart.
- Cards resize/flow in a compact widget grid.

## Verification

`dotnet test` on offset/DST logic; manual: add zones, restart, confirm restore.