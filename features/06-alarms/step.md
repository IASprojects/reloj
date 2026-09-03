# Alarms — Implementation Steps

Feature spec: `features/06-alarms/spec.md` · Requirements: FR-20–23 (FR-24 P2 out of scope)

Each step ends green: `dotnet build && dotnet test` in `src/`. Scheduling is
unit-tested in Core with injected `DateTimeOffset`; no sleeps. WinUI sound and
card visuals are never unit-tested (AGENTS.md) — isolated behind a thin chime
service + bindable properties.

## Design notes

- Alarms target an **absolute instant** (`FireAtUtc`, UTC-normalized) so
  re-arm after reboot is a pure `now` comparison — no calendar re-derivation.
- Persisted shape mirrors `ClockZoneRef`: `AlarmRef` in
  `ChronosFlip.Core/Alarms`, sanitized in `SettingsStore` (dedupe by id, drop
  blanks/unparseable).
- `AlarmService` is the pure engine: owns the set, a single
  `Evaluate(DateTimeOffset now)` called from the shared 1s tick, an
  `AlarmRang` event, disable/dismiss methods. No timers, no per-tick
  allocations (NFR-02).
- After an alarm rings it stays `IsRinging` until dismissed (FR-22). Firing is
  single-shot (`HasFired` latch); dismissing also disables it so the history
  entry cannot re-fire.
- Re-arm after reboot: future `FireAtUtc` re-arms untouched; an `Enabled` alarm
  whose instant already passed while the app was closed rings on the first
  tick (missed-alarm ring-on-launch) and is dismissed like any other.
- Ringing is surfaced via `AlarmRang` (Chime + card refresh), *not* `Changed`,
  so a ringing alarm never triggers a JSON save every second.

## Stage A — Core (RED-GREEN)

- [x] **A1. Alarm + AlarmRef model**
  - `ChronosFlip.Core/Alarms/Alarm.cs` (`ObservableObject`): `Id`, `ZoneId`,
    `Label`, `FireAtUtc` (UTC), `Enabled`, `IsRinging`, `ZoneTimeText`,
    `HasFired` latch, `Fire()`/`Dismiss()`/`Restore()`.
  - `ChronosFlip.Core/Alarms/AlarmRef.cs`: persisted shape (`Id`, `ZoneId`,
    `Label`, `FireAtUtc`, `IsEnabled`) + `FromAlarm`/`ToAlarm` (null-safe).
  - Tests: `AlarmTests`, `AlarmRefTests` — round-trip UTC-stable, blanks
    rejected.

- [x] **A2. AlarmService**
  - `ChronosFlip.Core/Alarms/AlarmService.cs`:
    - `Add` / `Remove` / `RemoveAllForZone` / `SetEnabled` / `Dismiss` /
      `DismissAll`.
    - `Evaluate(now)`: enabled + not-yet-fired + `FireAtUtc <= now` → `Fire()` +
      `AlarmRang`; latches so repeated ticks never re-raise.
    - `ActiveForZone`/`RingingForZone` for card badges — "future" compared
      against the last `Evaluate` instant (injectable, no real-clock reads).
  - Tests: `AlarmServiceTests` — fires exactly once, disabled never fires,
    dismiss stops, zone cascade, badge uses evaluated instant.

- [x] **A3. AlarmViewModel**
  - `ChronosFlip.Core/Alarms/AlarmViewModel.cs`: `ObservableCollection<Alarm>`
    in sync with the service; `AddAlarm`, `AddAlarmAt(zone, wallTime)`
    (zone-local → UTC via `TimeZoneConverter.FromZoneTime`), `RemoveAlarm`,
    `RemoveAlarmsForZone`, `SetEnabled`, `Dismiss`, `DismissRingingForZone`,
    `Evaluate` passthrough, `BadgeFor(zoneId)`.
  - `AlarmBadge` enum (None/Armed/Ringing).
  - Tests: `AlarmViewModelTests` — add/remove/toggle/dismiss, ringing forwards
    via `AlarmRang` only, zone cascade, badge transitions.

## Stage A2 — Persistence

- [x] **A4. Settings + store**
  - `ChronosFlipSettings.Alarms` (`List<AlarmRef>?`).
  - `SettingsViewModel.Apply`/`Save`/`SetAlarms` round-trip `Alarms` (FR-71).
  - `SettingsStore.Sanitize`: drop null/blank ids, unparseable `FireAtUtc`,
    duplicate ids (case-insensitive), keep order (FR-72).
  - `SaveWindowBounds` also re-persists `Alarms` so close/exit-fullscreen
    never drops them (FR-23).
  - Tests: `SettingsStoreTests` — round-trip, missing→empty, dedupe,
    case-insensitive, blank/bad rows dropped.

## Stage B — WinUI app

- [x] **B1. Ringing sound**
  - `ChronosFlip/Services/AlarmChime.cs`: `MediaPlayer` + in-memory PCM WAV
    dual-tone, `IsLoopingEnabled` (no assets, no per-ring timers); `Start()` /
    `Stop()`. Started on `AlarmRang`, stopped when `RingingCount` hits 0.

- [x] **B2. On-card status indicator (FR-21/22)**
  - `WorldClockCardViewModel`: `HasAlarm`, `IsAlarmRinging`,
    `DismissAlarmCommand` (assigned by shell).
  - `WorldClockCardControl`: bell badge when armed; when ringing the border
    turns neon + a STOP button appears (bound to `DismissAlarmCommand`).

- [x] **B3. Alarm panel**
  - `Views/AlarmView.xaml` + code-behind in a header `Flyout`: list of alarms
    (label, `ZoneTimeText`, enable ToggleSwitch routed through
    `AlarmViewModel.SetEnabled`, STOP when ringing, DELETE), plus a create row
    (zone selector from local+tray cards, DatePicker + TimePicker,
    `AddAlarmAt`).

- [x] **B4. Shell wiring (MainWindow)**
  - Restore alarms from settings → `AlarmService` → `AlarmViewModel`;
    `Evaluate` attached to the shared `ClockTicker`; `AlarmRang` →
    chime + badge refresh; `Changed` → persist (`ViewModel.SetAlarms`+`Save`)
    - badge refresh + stop chime when quiet; zone removal cascade-deletes the
    zone's alarms; card `DismissAlarmCommand` routes to the VM.

## Close-out

- [x] `dotnet build && dotnet test` green from `src/` (132/132).
- [ ] Manual: set future alarm → wait → rings + highlight → STOP/delete →
      restart → alarm still listed/re-armed → zone removal drops its alarms.
- [ ] Set `features/06-alarms/spec.md` status `Done`; update `features/README.md`.
