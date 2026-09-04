# Visual Clock Layout — Implementation Steps

Feature spec: `features/08-visual-clock-layout/spec.md` · Requirements: FR-01,
FR-04 (+ NFR-02/04) · Depends on: 02, 03, 05, 06, 07

Each step ends green: `dotnet build && dotnet test` in `src/`. This feature is a
**visual/layout restructure**: navigation state lives in Core and is unit-tested;
the WinUI canvas, nav rail, and view stacking are never unit-tested
(AGENTS.md) — isolated behind declarative bindings and code-behind that only
forwards. No new timers anywhere; the shared 1s tick, `WorldClock`,
`AlarmService`, and `CountdownTimer` update paths stay untouched (NFR-02).

## Design notes

- **One canvas.** `RootGrid` stays the window's top-level visual surface and
  still hosts the two neon shells switched by window mode — `NeonShell`
  (widget) and `NeonShellFullScreen` (fullscreen) — unchanged from features
  02/05 and `ApplyShellMode`. What this feature restructures is the **widget
  canvas**: the `Grid` inside `NeonShell.InnerContent`, laid out as a header
  strip (window tool buttons + settings) and a body (nav rail + `<Grid>` content
  region). Per-view titles move into each view's shared `ViewHeader`, so the
  strip carries no brand text. Everything — navigation, settings, the
  window-controls overlay, active view — derives its bounds from this one
  canvas, so nothing can resize or reposition the frame (AC).
- **Navigation state is a property, not code.** `MainNavigationViewModel` with
  `MainNavigationPage { Clock, Alarm, Timer }`; `SelectedPage` defaults to
  `Clock` (requirement 3) and `Select(page)` raises INPC for the page flags and
  buttons. Re-selecting the current page is a no-op.
- **Zero-jump view switching (key rule).** The Clock tray, `AlarmView`, and
  `TimerView` are all placed in the **same** content-region `Grid` cell with the
  same `Margin`/`Padding`, and switched via `Visibility` from the nav state.
  Identical measures guarantee identical bounds (AC) — no `ContentControl`
  swap, no relayout on change. This is what "alarm and timer fill the same area
  as the clock screen" means structurally.
- **Hidden views do no work.** All three views stay mounted (same instances,
  no state duplication) and switch purely by `Visibility`: a hidden view runs
  no timer and renders nothing. The single shared 1s tick keeps updating
  `WorldClock`/`Alarms`/`Timer` regardless of which view is visible (unchanged
  `MainWindow` wiring), so alarms still ring while the Clock view is shown.
  Hidden panels add no per-frame work.
- **Consistent per-view header.** Each view gets the same `ViewHeader`
  template at the same position (top-left of the content region, uniform title
  baseline + optional action slot), so titles align across Clock/Alarm/Timer.
  Pure XAML resources — no per-view code.
- **Window controls = the caption buttons** (minimize/maximize/close,
  today in `CaptionButtons` inside `HeaderHost`). They move into a top-right
  overlay of the content region so they share the canvas and are inset from the
  glow band that `NeonGlowBorder` reserves (12px shell padding + outward-band
  margins). Keep the drag/passthrough regions (`UpdateTitleBarRegions`) in sync
  so the title bar keeps working after the move.
- **Nav rail inset.** The current sidebar is flush to the shell edge, so its
  border sits under the outer glow band. Give it a uniform X inset
  (`LayoutGlowInset`, a dimension token derived from the glow zone) so the rail
  sits inside the canvas and never protrudes over the neon border
  (requirement 7). Visual style/behavior of the rail itself is unchanged.
- **No new visual language.** Only dimension/spacing adjustments — obsidian
  surfaces, existing type/color tokens, radii, and neon treatment are all
  reused (requirement 8). No new fonts, colors, or decorations.
- **Settings stays reachable** from the header (`SettingsButton` flyout,
  unchanged); the Alarm/Timer header flyout buttons are dropped because
  `AlarmView`/`TimerView` become the nav-hosted views (a second instance would
  duplicate state). The zone-picker flyout keeps working.

## Stage A — Core (RED-GREEN)

- [x] **A1. Navigation ViewModel**
  - `ChronosFlip.Core/Navigation/MainNavigationViewModel.cs`
    (`ObservableObject`): enum `MainNavigationPage { Clock, Alarm, Timer }`,
    `[ObservableProperty] SelectedPage` **defaulted to `Clock`**,
    `[NotifyPropertyChangedFor]`-derived `IsClockSelected`/`IsAlarmSelected`/
    `IsTimerSelected`, `Select(MainNavigationPage)` (same-page no-op), and
    `SelectCommand : RelayCommand<MainNavigationPage>` wired to `Select` so the
    WinUI buttons bind one command + `CommandParameter`. No timers, no events,
    pure state.
  - Tests: `Tests/Navigation/MainNavigationViewModelTests.cs` — default is
    Clock (AC), select switches page + flags + INPC, re-selecting same page
    keeps state and raises nothing, each page transition round-trips,
    `SelectCommand` executes `Select` per `CommandParameter` (same-page no-op).

## Stage B — WinUI app

- [x] **B1. Layout canvas (MainWindow.xaml)**
  - Restructure the **widget canvas** (`NeonShell.InnerContent`) into: header
    strip (tool buttons + settings) and body (`NavRail` + content region). The
    content region is one fixed `Grid` cell that hosts every view; the
    `WORLD CLOCK` label moves from the strip into the Clock view's `ViewHeader`.
  - New **dimension token(s) only**: `LayoutGlowInset` (+ spacing measures the
    inset composes two of the existing 4px rhythm units) — no colors/type.

- [x] **B2. Nav rail: Clock/Alarm/Timer**
  - Reuse the sidebar rail; repurpose the currently-disabled "World Clock"
    button into an enabled **Clock** nav button alongside Alarm and Timer.
  - The nav label is **Clock** and maps to the existing world-clock tray
    (`WorldClockViewModel` + `WorldClockCardControl` — no code renames):
    "Clock" stays the UI/nav term (spec), `WorldClock` stays the code/VM term.
  - Apply `LayoutGlowInset` so the rail is moved slightly right, inside the
    canvas, aligned with content (requirement 7). Visual style unchanged.
  - Buttons bound to a single `SelectCommand`
    (`RelayCommand<MainNavigationPage>`) with `CommandParameter` per button —
    no code-behind logic; the `EnumEqualsConverter` maps `SelectedPage` to the
    `IsClockSelected`/`IsAlarmSelected`/`IsTimerSelected` flags.
  - Selected nav item uses a shared style that highlights with the existing
    `NeonAccentBrush` (echo of the rail status dot) — existing tokens only, no
    new visual language.

- [x] **B3. Content region: stacked views**
  - Same `Grid` cell (identical `Margin`/`Padding`) hosts: the Clock view
    (existing world-clock card tray, includes the local card + `OtherCards`
    logic untouched), `AlarmView` (`AlarmPanel`), and `TimerView`
    (`TimerPanel`).
  - Replace each view's current flyout root (`StackPanel Width="360"` in
    `AlarmView`/`TimerView`) with a stretch root that fills the whole shared
    cell (`HorizontalAlignment="Stretch"` + the same `Margin`/`Padding` as the
    Clock view); inner content follows the shared `ViewHeader`/content rules so
    alarm and timer occupy the same bounds as the clock (AC).
  - Visibility driven purely by `SelectedPage` + converter; `DataContext`
    stays on the existing `WorldClock` / `Alarms` / `Timer` VMs — no new view
    instances, no state duplication, no relayout when switching.
  - Drop the Alarm/Timer header flyout usage; keep `SettingsButton` flyout and
    `ZonePicker`.

- [x] **B4. Window controls inset (requirement 6)**
  - Move `CaptionButtons` from the header strip into the content region's
    top-right overlay slot, inset from the upper-right neon border by
    `LayoutGlowInset` so they never sit on the glow or card border.
  - Update `UpdateTitleBarRegions` drag rectangle + passthrough coordinates
    for the new location; window-mode enter/exit keeps working.

- [x] **B5. Consistent view header**
  - Add a shared **XAML resource** `ViewHeader`: a `Style` (with
    `ControlTemplate`) targeting `ContentControl`, defined in the app resource
    dictionary — no code-behind ("pure XAML resources"). The template renders
    the title text and a trailing **optional action slot**; each view hosts the
    `ContentControl` with its title and, when present, one action element, all
    at the same y-origin and x-origin, so titles and content align
    (requirement 5) and the spaces previously owned by flyout headers vanish
    cleanly.

## Shell wiring (MainWindow.xaml.cs)

- [x] **B6.** Create `MainNavigationViewModel` (default Clock); bind nav
      buttons; keep `RootGrid.DataContext = WorldClock` for the Clock view;
      assign `AlarmPanel.ViewModel`/`TimerPanel.ViewModel` as today.
      `ClockTicker` wiring (`WorldClock.Attach`, `Alarms.Evaluate`,
      `Timer.Evaluate`, chime handlers) **unchanged**.

## Close-out

- [x] `dotnet build && dotnet test` green from `src/` (182 tests).
- [ ] Manual WinUI verification: default Clock on launch; switching
      Clock/Alarm/Timer replaces only the content region; all three views at
      identical bounds with aligned titles/content; alarm and timer fill the
      same area as the clock; window/caption buttons inset from the upper-right
      neon border; nav rail does not protrude over the border; window controls,
      settings, clock updates, alarms, timer, and neon all still work; stable on
      resize and at supported DPI scales; no per-view polling timers.
- [ ] Set `features/08-visual-clock-layout/spec.md` status `Done`; update
      `features/README.md` row 08 (`Planned` → `Done`).