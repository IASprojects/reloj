# Visual Clock Layout

- **Status:** `In Progress`
- **Requirements:** FR-01, FR-04 (+ NFR-02, NFR-04)
- **Preserves:** FR-40/42 (fullscreen mode + exit), FR-50–52 (always-on-top),
  and the custom title bar drag/passthrough (feature 05) — layout moves must
  keep them working, no logic changes.
- **Depends on:** 02 (app shell + 1s tick), 03 (settings access),
  05 (window modes: shell states, title-bar drag/passthrough),
  06 (alarm engine/view), 07 (timer engine/view)
- **Stage:** 7

## Goal

Establish a consistent clock dashboard layout where the navigation bar, window
controls, settings access, and the main views (Clock, Alarm, Timer) share one
stable visual canvas. This is the visual and layout foundation that must be
completed before the per-card neon (09) and polish/hardening (10) work.

## Scope

- A main layout/canvas as parent visual surface containing: navigation bar,
  window-control buttons, settings access, and a fixed content region for the
  selected view.
- Navigation bar with at least **Clock**, **Alarm**, and **Timer**; **Clock
  selected by default** on launch.
- Each option renders its own reusable view inside the shared region: Clock
  (the world-clock content), Alarm (the alarm configuration view), Timer (the
  timer view).
- One set of content rules for every view — consistent title position, content
  origin, margins, padding, and available height — so no view resizes or
  repositions the shell and switching never causes visible layout jumps.
- Window controls / caption buttons (minimize/maximize/close) placed in the
  upper-right of the clock content, inset from the neon border; navigation bar
  kept on its current visual style but moved inside the canvas so it does not
  protrude over the neon border.
- Consistency with the Nocturne Utility design system: dark obsidian surfaces,
  existing typography tokens, 4px spacing rhythm, existing radii and neon
  treatment. No new colors, fonts, decorative elements, or features.
- Architecture stays WinUI 3 + MVVM: navigation state lives in a ViewModel,
  views are declarative and reusable, code-behind is wiring/forwarding only,
  no independent view timers, shared clock/timer update model unchanged.

## Out of scope

- Per-card neon (feature 09), polish & hardening (feature 10), installer &
  publishing (feature 11).
- New design tokens beyond layout dimension/spacing measures needed to inset
  content from the glow zone; new features; changes to alarm/timer behavior
  beyond where their views are hosted.

## Acceptance Criteria

- The new layout is the parent visual canvas for navigation, settings, and
  window controls.
- Clock is selected by default.
- Selecting Clock, Alarm, or Timer replaces only the shared content region.
- All three views occupy identical bounds and keep consistent title and content
  alignment; the alarm and timer screens fill the same area as the clock
  screen.
- Window/caption buttons are visibly inset from the upper-right neon border.
- The navigation bar no longer overlaps or protrudes over the neon border and
  stays aligned with the shared content canvas.
- Existing window controls, settings access, clock updates, alarms, timer
  behavior, and neon behavior continue working.
- The layout remains stable when the window is resized and at supported DPI
  scales.
- No per-view polling timers or unnecessary recurring work.

## Verification

- `dotnet test` covering the navigation ViewModel: default Clock selection and
  state transitions.
- `dotnet build && dotnet test` in `src/`.
- Manual WinUI pass for: default Clock selection, navigation between
  Clock/Alarm/Timer, equal content bounds, title and element alignment, window
  resizing, high-DPI rendering, neon border spacing, and window/caption /
  navigation-bar placement.