# App Shell & Local Clock

- **Status:** `Done`
- **Requirements:** FR-01, FR-05, FR-11 (+ NFR-07 single-instance)
- **Depends on:** —
- **Stage:** 1

## Goal

A bootable WinUI 3 app that renders the dark "Nocturne Utility" theme and a
single local-time flip card, ticking once per second.

## Scope

- Solution scaffold in `src/` (`ChronosFlip.slnx`, `ChronosFlip` app project).
- Packages: `Microsoft.WindowsAppSDK`, `CommunityToolkit.Mvvm`.
- `Themes/` ResourceDictionary mapping DEGING.md tokens (colors, typography,
  spacing) — no undeclared tokens.
- `ClockService` (injectable clock, single `DispatcherQueueTimer` 1s tick).
- `FlipCard` control with the horizontal bisect line and 1px border.
- Single instance enforcement (mutex).

## Out of scope

- Alarms, timer, world clock, window modes, full persistence.

## Acceptance Criteria

- `dotnet build` green from `src/`.
- App launches showing local `HH:MM:SS` in `Space Mono` on a flip card.
- Time updates every 1s driven by one shared tick.
- Design tokens map 1:1 to `DEGING.md`; no new colors.
- A second instance does not open a duplicate window.

## Verification

`dotnet build && dotnet test` in `src/`; manual launch on Windows.