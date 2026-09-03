# Window Modes — Implementation Steps

Feature spec: `features/05-window-modes/spec.md` · Requirements: FR-40–43, FR-50–52, NFR-05

Each step ends green: `dotnet build && dotnet test` in `src/`. WinUI windowing is
never unit-tested (AGENTS.md); window-mode logic lives behind an injectable
service and is covered by Core tests.

## Stage A — Core (RED-GREEN)

- [x] **A1. Mode service contract**
  - `ChronosFlip.Core/WindowModes/IWindowModeService.cs`:
    `bool IsFullScreen { get; }`, `void EnterFullScreen()`, `void ExitFullScreen()`,
    `void SetTopmost(bool pin)`.
  - Tests: `Tests/WindowModes/FakeWindowModeService.cs` (records calls).

- [x] **A2. WindowModeViewModel**
  - `ChronosFlip.Core/WindowModes/WindowModeViewModel.cs` (`ObservableObject`):
    - `IsFullScreen` + `ToggleFullScreen()` — guards re-enter/re-exit, catches
      service failure.
    - `IsPinActive` — write-through to `SettingsViewModel.PinToTop` so
      persistence stays with the existing debounced save.
    - `TogglePin()` → `service.SetTopmost` + persist.
    - `RequestExit()` — Esc hook; no-op unless fullscreen.
  - Tests `Tests/WindowModes/WindowModeViewModelTests.cs`:
    enter/exit toggling, exit-when-not-fullscreen no-op, Esc handling,
    pin→topmost+persisted, guard rails.

- [x] **A3. Fullscreen card surface**
  - `WorldClockCardViewModel`: add `TimeHMS` (`HH:MM:SS`) + `DateText`
    (e.g. `Tue, Sep 2`) as `[NotifyPropertyChangedFor]` on `Now`.
  - Tests: `TimeHMS`/`DateText` formatting + INPC raised on tick.

## Stage B — WinUI app

- [x] **B1. WinUIWindowModeService**
  - `ChronosFlip/Services/WinUIWindowModeService.cs`:
    - Enter: capture widget bounds (`AppWindow.Position`/`Size`) → restore target;
      `DisplayArea.GetFromWindowId(AppWindow.Id, Fallback)?.OuterBounds` → resize;
      borderless via `ExtendsContentIntoTitleBar=true` + `SetTitleBar(null)`;
      non-resizable presenter + `IsAlwaysOnTop=true`.
    - Exit: restore widget bounds + standard chrome; topmost per `IsPinActive`.
    - SetTopmost delegates to existing `SetWindowPos`.

- [x] **B2. MainWindow.xaml**
  - Header: Pin toggle (icon `E7C1`, bound to `IsPinActive`) + Fullscreen button
    (`E740`, bound to `IsFullScreen`), alongside Add/Settings.
  - Two content states switched on `IsFullScreen` (VisualState / Visibility):
    - Fullscreen: centered big `LocalCard.TimeHMS` (Space Mono) + `DateText` /
      local label, inside existing `NeonShell` (FR-43); bottom strip of the other
      `WorldClock.Cards` (`Label` + `Time`) via `ItemsWrapGrid`.
    - Widget: existing tray.
  - New dimension tokens only (font sizes/spacing; no new colors/typefaces).

- [x] **B3. MainWindow.xaml.cs**
  - Wire service + `WindowModeViewModel`; swap content on `IsFullScreen`;
    Esc via `RootGrid.KeyDown` when focused (focus set on fullscreen enter).
  - `SaveWindowBounds`: write last-known widget bounds when closing fullscreen.

## Close-out

- [x] `dotnet build` + `dotnet test` green from `src/` (96/96).
- [ ] Manual verification: pin → move → restart (pin persists) → fullscreen →
      other app still usable → Esc → widget + bounds restored.
- [ ] Set `features/05-window-modes/spec.md` status `Done`; update
      `features/README.md` row.