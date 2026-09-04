# Installer & Publishing

- **Status:** `Planned`
- **Requirements:** NFR-05 (+ release/distribution concern; no dedicated FR in PRD yet)
- **Depends on:** all feature stages complete (02–10)
- **Stage:** 10

## Goal

Produce a distributable, installable build of Chronos Flip for Windows 10/11
x64: packaging, installation UX, and a repeatable release process.

## Scope

- **Packaging strategy**: choose packaged (MSIX) vs unpackaged — for a
  lightweight desktop widget prefer **unpackaged** + **self-contained Windows
  App SDK** (no runtime install burden, simpler always-on-top/windowing
  behavior), with an installer wrapper.
- **Installer**: MSI via **Inno Setup** (or MSIX if packaging is preferred);
  installs to `%LOCALAPPDATA%\ChronosFlip`, Start Menu shortcut, uninstall entry.
- **Signing**: optional Authenticode code-signing path (no cert at MVP; document
  the command for later `-SignToolPath`/pfx injection).
- **Release pipeline**: repeatable script (`scripts/publish.ps1`) that builds
  Release x64, runs the full `dotnet test` suite, stamps version, and outputs
  the installer artifact + `.sha256`/`.txt` checksums.
- **README/RELEASES**: short install/uninstall notes and a release checklist.

## Out of scope

- Auto-update mechanism, Microsoft Store submission, multi-arch (AnyCPU/ARM),
  portable zip builds (can be a later add-on).

## Acceptance Criteria

- Fresh Windows 10/11 (x64) installs the app without a preinstalled .NET/App SDK
  (self-contained), under a local user account (no admin required).
- Typical + silent install (`/VERYSILENT`), silent uninstall, Start Menu
  shortcut and Add/Remove Programs entry work.
- `publish.ps1` produces: installer, checksums, and a passing test run in CI.
- The installed app behaves identically to the dev build (widget, fullscreen,
  topmost, settings persist under `%APPDATA%\ChronosFlip`).

## Verification

Scripted smoke test: install on clean Windows VM, launch, exercise window modes,
uninstall and confirm cleanup. Verify checksums. `dotnet test` green before
every publish.