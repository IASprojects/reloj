# Window Modes (Topmost / Fullscreen)

- **Status:** `Planned`
- **Requirements:** FR-40, FR-41, FR-42, FR-43, FR-50, FR-51, FR-52 (+ NFR-05)
- **Depends on:** 02 (shell), 03 (pin-state persistence)
- **Stage:** 4

## Goal

Make the widget pinnable (always-on-top) and expandable to a fullscreen
dedicated-clock mode that does **not** block the OS.

## Scope

- Pin/unpin toggle (topmost) — state persisted.
- Fullscreen "Desktop Clock" mode: whole-screen, non-blocking (no focus lock /
  no exclusive input); local/selected zone time in display-clock type.
- Exit via Esc or the same toggle.
- Neon preference applies in both modes.
- Keep window draggable while pinned.

## Out of scope

- Kiosk/blocking lock, multi-monitor span, tray integration.

## Acceptance Criteria

- Pinned window stays above other apps and persists across restart.
- Fullscreen covers the screen yet the OS stays interactive.
- Esc returns to widget mode.
- Works on 10/11 x64; idle CPU stays low.

## Verification

Manual on Windows: pin → move → fullscreen → interact with another app → Esc.
`dotnet build` green; no unit-test dependency on WinUI windowing (isolate logic
behind `WindowService` if needed).