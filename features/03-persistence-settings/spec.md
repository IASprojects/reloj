# Persistence & Settings

- **Status:** `Done`
- **Requirements:** FR-70, FR-71, FR-72, FR-61, FR-62, FR-63
- **Depends on:** 02 (app shell must exist to host settings)
- **Stage:** 2

## Goal

Persist user preferences to JSON under `%APPDATA%\ChronosFlip` and surface the
neon customization (toggle + color picker) with full persistence.

## Scope

- `SettingsStore` service: load/save JSON, atomic writes, corrupt-file recovery
  (fall back to defaults).
- Persisted state: neon enabled, neon color, pin state, window position/size.
- `SettingsView` + `SettingsViewModel`: neon toggle and color picker.
- Neon border applied to the shell/cards when enabled.

## Out of scope

- Persisting alarms/zones (owned by those features), theme switching.

## Acceptance Criteria

- Settings survive app restart.
- Corrupt settings file is ignored and replaced by defaults.
- Toggling neon and picking a color persists correctly.
- Glow matches DEGING.md: 1.5px stroke + gaussian soft shadow.

## Verification

`dotnet test` covering `SettingsStore` (round-trip, atomic writes, corrupt
recovery); manual restart check.