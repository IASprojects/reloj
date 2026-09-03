# Timer

- **Status:** `Done`
- **Requirements:** FR-30, FR-31, FR-32, FR-33
- **Depends on:** 02 (flip card + tick), optionally 03 (preferred last value)
- **Stage:** 6

## Goal

A fully operable countdown timer: set duration, start, pause, reset, with
visual count-down on flip-card digits and a completion notification.

## Scope

- `TimerService` (state machine: idle → running → paused → expired) with an
  injected clock for testability.
- Set minutes/seconds UI; start/pause/reset controls.
- Digits render on a flip card, updated from the shared 1s tick.
- Completion: looping notification + sticky `00:00` EXPIRED state, reset by the
  user (FR-31/FR-32).
- Optionally persist last duration (FR-33: survive minimize; restart optional).

## Out of scope

- Count-up stopwatch, laps, multiple concurrent timers.

## Acceptance Criteria

- Pause/resume keeps elapsed accuracy (no drift from lost ticks).
- Expiry triggers notification exactly once; the timer holds `00:00` until the
  user resets (sticky EXPIRED with looping chime, alarm-style UX).
- All transitions testable with a fake clock.

## Verification

`dotnet test` on `TimerService` transitions and drift; manual run on Windows.