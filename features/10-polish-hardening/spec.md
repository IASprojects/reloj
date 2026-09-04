# Polish & Hardening

- **Status:** `Planned`
- **Requirements:** NFR-01, NFR-02, NFR-06, NFR-07
- **Depends on:** all feature stages complete
- **Stage:** 9

## Goal

Final quality pass: verify the whole app stays light, correct and stable across
sessions.

## Scope

- High-DPI rendering correctness (blurry-neon / mis-scaled cards cleanup).
- Animation quality: smooth flip transitions, flicker-free glow, no jitter.
- Idle profile: confirm low CPU/RAM per NFR-01/02.
- Time correctness audit across DST/timezone edge cases (NFR-06).
- Single-instance + crash/restore hardening (NFR-07).
- Full `dotnet build && dotnet test` suite green.

## Out of scope

- New features; anything not in the PRD.

## Acceptance Criteria

- Idle CPU < 1% and RAM budget per NFR-01.
- One shared 1s tick drives all clocks/timers (audit).
- No regressions: full test suite + manual pass over prior features.

## Verification

Profiler smoke test on Windows; `dotnet test` full run; reviewer pass over
`git diff` against PRD.