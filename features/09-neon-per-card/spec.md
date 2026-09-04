# Neon Per Card (P2)

- **Status:** `Planned`
- **Requirements:** FR-64
- **Depends on:** 02 (neon on shell), 03 (per-card color persistence),
  08 (shared layout canvas)
- **Stage:** 8

## Goal

Extend neon customization from a whole-widget accent to a per-card selectable
neon color.

## Scope

- Allowing an optional neon color override per flip card.
- Color picker integration and persistence (structure extension of
  `SettingsStore`).
- Glow rendering per card, flicker-free, consistent with DEGING.md.

## Out of scope

- Anything above FR-64; performance must stay flat (no per-card timers).

## Acceptance Criteria

- Each card can carry an independent neon color (or inherit global).
- Per-card choice persists across restart.
- Idle CPU/RAM unchanged vs single-neon mode.

## Verification

Manual UI check; `dotnet test` for persistence model; `dotnet build` green.