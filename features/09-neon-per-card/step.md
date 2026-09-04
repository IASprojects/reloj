# Neon Per Card — Implementation Steps

Feature spec: `features/09-neon-per-card/spec.md` · Requirements: FR-64 (P2, Stage 8)

Each step ends green: `dotnet build && dotnet test` in `src/`. Per-card neon is a
set-only visual preference: no per-card timers, no per-tick allocation, nothing
added to the 1s tick path (NFR-02). Glow rendering and the color-picker UI are
never unit-tested (AGENTS.md); override resolution and persistence are covered
by Core tests.

## Design notes

- **Override model**: every card carries an **optional** `NeonHexColor`
  (nullable string). `null` = **inherit global** accent. Effective glow color =
  `override ?? global` — resolved in one place so XAML stays declarative.
- **Persistence (structure extension of `SettingsStore`)**: tray-card overrides
  ride on the zone itself (`ClockZoneRef.NeonHexColor` → `ClockZone` → card VM),
  so add/remove/reorder of a zone naturally carries or drops its color. The
  always-present local card is **not** in `Zones`, so it gets a dedicated
  top-level `ChronosFlipSettings.LocalCardNeonHexColor`.
- **Render**: reuse the existing `NeonGlowBorder` (1.5px stroke + layered band
  glow per FR-63 / DEGING.md) — but now **one instance per card** around the
  card box, plus the existing whole-shell wrappers. No new colors/typefaces;
  only a compact `GlowPadding` dimension token (the shell's 12px padding is too
  much between tray cards).
- **Flicker-free (AC)**: glow is a static brush composition, no animation, no
  per-frame writes; color only ever changes on user action (rare, imperative —
  safe off the tick path, like existing `ApplyNeonAccent`).
- **Fullscreen (FR-43)**: the fullscreen `NeonShell` shows the local clock, so
  its accent follows the **local card's effective** color (override or global).
- **Neon toggle gate**: FR-61 stays global — when `NeonEnabled` is off, all
  card glows collapse regardless of override (card `IsNeonEnabled` derives from
  the global switch).
- **Ringing interaction**: an alarm ringing on a card keeps forcing its border
  neon (existing `OnIsAlarmRingingChanged`); the per-card glow simply renders in
  the card's configured effective color.

## Stage A — Core (RED-GREEN)

- [ ] **A1. Persisted shape for overrides**
  - `ClockZoneRef.NeonHexColor` (`string?`, nullable, null = inherit) +
    `FromClockZone`/`ToClockZone` null-safe round-trip; `ClockZone` gains the
    same nullable field.
  - `ChronosFlipSettings.LocalCardNeonHexColor` (`string?`) with
    `SettingsDefaults` untouched (null default).
  - `SettingsStore.Sanitize`: per-zone override and local override validated
    with a small Core hex validator (`Settings` helper, no WinUI dependency —
    mirror of `HexToColorConverter.TryParse`); invalid/blank → `null`
    (inherit), valid kept; dedupe-by-id keeps the first zone's color, as today.
  - Tests: `SettingsStoreTests` — round-trip override, missing→inherit,
    invalid hex (`"red"`, `"#GG0000"`, blank) → inherit, valid `#RRGGBB` kept,
    reorder preserves per-card colors, `SaveWindowBounds` re-persists them.

- [ ] **A2. Card override surface**
  - `WorldClockCardViewModel`: `[ObservableProperty] string? NeoHexColor`,
    `HasNeonOverride` (`NotNullOrWhiteSpace`), and an injected/assigned
    `EffectiveNeonHexColor` (nullable, shell-driven: override ?? global) so the
    UI binds one value. Set-only — never touched by `SetPresent`.
  - `WorldClockViewModel`: `AddZone`/constructor propagate override →
    `ClockZoneRef.ToClockZone()`; `ZonesToPersist` reads `card.NeonHexColor`
    back into `ClockZone`; local card created with `LocalCardNeonHexColor`.
  - Tests: `WorldClockCardViewModelTests`, `WorldClockViewModelTests` —
    override passthrough on add, persist round-trip, local override applied.

- [ ] **A3. ClockZone/SettingsVM wiring**
  - `SettingsViewModel`: `LocalCardNeonHexColor` observable property;
    `Apply`/`Save` round-trip it; `SetZones`/`Save` carry `ClockZone.NeonHexColor`.
  - Tests: `SettingsViewModelTests` — override survives Apply→Save, local
    override round-trip, invalid value sanitized to inherit on load.

## Stage B — WinUI app

- [ ] **B1. Compact per-card glow**
  - `NeonGlowBorder`: add `GlowPadding` DP (default 12, keeps shell behaviour);
    corner radii off padding so the layered band always hugs the card edge.
    New dimension token `CardGlowPadding` (≈6) only.
- [ ] **B2. Card glow inside WorldClockCardControl**
  - Wrap the card box (bordered `Grid` with the bisect line + digits) in a
    `NeonGlowBorder` bounded by `GlowPadding`; label/offset text stay below,
    outside the glow.
  - New DPs on the control: `IsNeonEnabled` (collapses glow, gate global)
    and `AccentColor` (bound through the existing `HexToColor` converter to
    `EffectiveNeonHexColor`). Keep ringing-badge border logic as is.
- [ ] **B3. Per-card picker**
  - Small color-dot affordance on the card (top-right, beside the alarm bell,
    icon `E7C1`-family) → `Flyout` with `ColorPicker`
    (`IsAlphaEnabled="False"`, same block as SettingsView) bound to the card
    VM's `NeonHexColor` (TwoWay + `HexToColor`) and an "INHERIT" reset
    (`NeonHexColor = null`). Code-behind pure forwarding, AlarmView pattern.
- [ ] **B4. Shell wiring (MainWindow)**
  - Restore overrides on launch: `WorldClock.AddZone(ClockZone)` per loaded
    zone (already carries color); local card via `LocalCardNeonHexColor`.
  - `UpdateCardGlows()`: set `EffectiveNeonHexColor` + `IsNeonEnabled` on every
    card; run on neon toggle, global accent change, and any card override change
    (never from the tick). New `Cards`/override ends in `ViewModel.Save()`
    (existing `OnCardsChanged` path) or `LocalCardNeonHexColor = …; Save()`.
  - Fullscreen: `ApplyShellMode` passes the **local card's** effective color to
    `NeonShellFullScreen.AccentColor`.
  - `SaveWindowBounds` re-persists all card overrides + local override
    (pattern from alarms, FR-23).

## Close-out

- [ ] `dotnet build && dotnet test` green from `src/`.
- [ ] Manual: set per-card colors → restart → each card keeps its own (non-
      overridden cards follow global); change global accent mid-session → cards
      without override follow; INHERIT on a card returns it to global; neon
      toggle off hides all glows; fullscreen local clock uses the local card's
      color; idle CPU/RAM unchanged vs single-neon mode; alarm ringing on an
      overridden card still highlights.
- [ ] Set `features/09-neon-per-card/spec.md` status `Done`; update
      `features/README.md` row 09 (`Planned` → `Done`).