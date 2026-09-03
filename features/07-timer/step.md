# Timer — Implementation Steps

Feature spec: `features/07-timer/spec.md` · Requirements: FR-30–33

Each step ends green: `dotnet build && dotnet test` in `src/`. Timer logic is a
pure engine in Core driven by the shared 1s tick from an injected `IClock`; no
private timers, no sleeps (NFR-02), fully unit-testable with `FakeClock`. WinUI
digits, flyout, and chime are never unit-tested (AGENTS.md) — isolated behind
bindable properties and the existing chime service.

## Design notes

- `TimerService` is a pure countdown state machine
  `Idle → Running ⇄ Paused → Expired →(Reset)→ Idle`, driven by
  `Evaluate(now)` called from the shared 1s tick. It holds **no timer** — the
  tick is the sole driver (NFR-02).
- **Drift-free accuracy (AC)**: on `Start` we capture `Duration` and record
  `EndsAt = now + Duration`. Each `Evaluate(now)` recomputes `Remaining =
  EndsAt − now` from the absolute instant, so throttled/missed ticks or a long
  `Paused` period can never accumulate error. `Remaining` clamps at zero.
  Pause stores `Remaining` (freeze) and just stops driving; resume re-derives
  `EndsAt = now + Remaining`.
- **Expiry fires exactly once**: the first `Evaluate` where `Remaining <= 0`
  latches an `Expired` state and raises `Expired` (single-shot flag, same
  pattern as `Alarm.HasFired`); subsequent ticks no-op. Per user decision the
  timer **stays sticky in `Expired`** showing `00:00` with the looping chime
  until the user presses RESET/STOP (alarm-style), instead of auto-resetting.
  The single-shot latch keeps AC "exactly once" satisfiable and testable.
- **`SetDuration`** is only honored in `Idle` (and validated
  `1s ≤ Duration ≤ 99:59` so it always fits the MM:SS flip digits); changing
  duration mid-run is a reset-first action.
- **FR-33**: persist last set duration as `TimerPresetSeconds` (int) via the
  existing `SettingsStore`; restored on launch into an `Idle` timer at that
  preset. Running state is **not** resumed after restart — but it does survive
  minimize naturally (process stays alive), satisfying FR-33 P1.
- Completion sound reuses `ChronosFlip/Services/AlarmChime.cs` unchanged
  (looping PCM WAV, no assets, no per-ring timers) — it loops, which is exactly
  the sticky-EXPIRED UX; RESET stops it. `Expired` drives the chime + digits
  through `TimerViewModel.Expired`, **not** through `Changed`, so persistence
  is only triggered on real mutations (mirrors the alarm `AlarmRang` rule).

## Stage A — Core (RED-GREEN)

- [x] **A1. TimerService**
  - `ChronosFlip.Core/Timers/Timer.cs` (`CountdownTimer` class, `ObservableObject`): `Duration`,
    `Remaining` (computed), enum `TimerState { Idle, Running, Paused,
    Expired }`, `EndsAt`/`HasExpired` latch, `Start()` (also resumes from
    `Paused`)/`Pause()`/`Reset()`,
    `SetDuration(TimeSpan)`, `Evaluate(DateTimeOffset now)` (no-op in
    `Idle`/`Expired`), single-shot `Expired` event. Constructor takes
    `TimeSpan duration`; the engine is clock-free (`Evaluate(now)` style, mirroring
    `AlarmService`).
  - Tests: `Tests/Timers/TimerTests.cs` with fixed timestamps — start→running,
    running→paused→running keeps accuracy, `Remaining` = `EndsAt − now` after a
    clock jump (no drift), expiry triggers exactly once (second evaluate no-op),
    expired is sticky until reset, reset from running/paused/expired → idle,
    `SetDuration` rejected outside idle / out of range.
- [x] **A2. Countdown display formatting**
  - On the timer model: `RemainingMinutes` / `RemainingSeconds`
    (zero-padded via string math) and `RemainingText`
    (`"MM:SS"`, clamps to `"00:00"` at expiry), all
    `[NotifyPropertyChangedFor]`d off `Remaining`.
  - Tests: `Tests/Timers/TimerFormattingTests.cs` — formatting edges — 0:05 → `"00:05"`, 5:00 → `"05:00"`, 99:59 max,
    clamp at 0, INPC raised while running.
- [x] **A3. TimerViewModel**
  - `ChronosFlip.Core/Timers/TimerViewModel.cs`: observable duration inputs
    (minutes/seconds ints, clamped + applied only while Idle), state-driven
    `CanStart`/`CanPause`/`CanReset`/`CanEditDuration` /
    `IsExpired`, plain `Start()`/`Pause()`/`Reset()` methods, `Expired` event
    passthrough, `Evaluate(now)` reusing the shared tick, and
    `RestoreDuration(int seconds)` for FR-33 launch restore.
  - Tests: `Tests/Timers/TimerViewModelTests.cs` — state flags per transition,
    commands disabled when not applicable, duration editing → `SetDuration`,
    expiry raises once, restore clamps invalid persisted value.

## Stage A2 — Persistence (FR-33/FR-71/FR-72)

- [x] **A4. TimerPresetSeconds**
  - `ChronosFlipSettings.TimerPresetSeconds` (int, default 300 via
    `SettingsDefaults.TimerPresetSeconds`).
  - `SettingsViewModel.Apply`/`Save` round-trip the preset (write-through on
    change).
  - `SettingsStore.Sanitize`: out-of-range/absent → 300; valid keeps `[1, 5999]`.
  - `SaveWindowBounds` re-persists `TimerPresetSeconds` so close/exit-fullscreen
    never drops it (pattern from alarms, FR-23).
  - Tests: `SettingsStoreTests` + `SettingsViewModelTests` — round-trip, missing→default,
    out-of-range (0/-5/6000) → default.

## Stage B — WinUI app

- [x] **B1. TimerView flyout**
  - `Views/TimerView.xaml` + code-behind: header "TIMER" (Space Mono, matches
    AlarmView title), two compact `TimerDigitCard` flip-card cells (`MM : SS`,
    new `TimerDigit*` dimension tokens only — all existing color/type tokens), a
    duration row with minute/second `NumberBox`es (1–99 / 0–59), disabled
    outside `CanEditDuration`, and START / PAUSE / RESET buttons with
    state-driven visibility.
  - Code-behind is pure forwarding (AlarmView pattern): button clicks ⇄ VM
    methods, no logic.

- [x] **B2. Shell wiring (MainWindow)**
  - Build `TimerViewModel` with `TimerPresetSeconds` from `ViewModel.Load()`;
    restore via `RestoreDuration`; `Evaluate` attached to the shared
    `ClockTicker`.
  - Timer button in the header (`Button.Flyout` hosting `TimerView`) and the
    previously-disabled sidebar Timer button (both `E917`, FR-30); both open the
    same `TimerPanel`.
  - `Ticker.Tick → Timer.Evaluate`; `Timer.Expired → _alarmChime.Start()`;
    duration-edit → persist preset (`ViewModel.TimerPresetSeconds` + `Save`);
    reset-to-idle stops the chime unless an alarm is still ringing
    (`Alarms.RingingCount == 0`).
  - `OnClosed` keeps saving via `SaveWindowBounds` (re-persists the preset).

## Close-out

- [x] `dotnet build && dotnet test` green from `src/` (171/171).
- [ ] Manual: set 0:05 → START → counts down; pause 10s → resume shows only
      5s elapsed (no drift); EXPIRED keeps `00:00` + looping chime → STOP
      silences and resets to Idle at the same duration; minimize while running
      → still correct when restored; restart → duration preserved, timer Idle.
- [ ] Set `features/07-timer/spec.md` status `Done`; update
      `features/README.md` row 07 (`Planned` → `Done`).