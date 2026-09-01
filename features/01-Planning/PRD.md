# Product Requirements Document — Chronos Flip

Status: **Draft** · Module: `01-Planning` · Last updated: 2026-09-01

This document is the authoritative, detailed expansion of `draft.md`. Use it to
derive epics, stories, and implementation order. Design tokens referenced here
come from `identyvisual/DEGING.md`.

## 1. Product Summary

Chronos Flip is a native Windows 10/11 desktop clock dashboard. It presents
world-clock tiles as flip cards, supports per-zone alarms and a countdown
timer, and can behave either as a floating always-on-top widget or as a
non-blocking fullscreen clock.

## 2. Personas & Goals

- **Focused user**: wants a calm, legible time reference on the desktop while
  working; cares about low resource usage.
- **World-clock user**: tracks colleagues across time zones; wants glanceable
  tiles without launching a browser tab.
- **Distraction-free user**: uses the fullscreen mode as a dedicated clock
  (e.g., timeboxing) without locking the machine.

## 3. Functional Requirements

Priority legend: **P0** = MVP must, **P1** = should, **P2** = nice-to-have.

### 3.1 Flip Cards & Navigation (P0)

- FR-01 Main window is a tray/host of modular flip cards.
- FR-02 Each card shows: time zone label, current time (HH:MM), and an
  indicator of seconds/alarm state.
- FR-03 Clicking/tapping a card "flips" it (animated transition) to reveal
  zone details: full time, UTC offset, date, alarm status.
- FR-04 Cards stack in a compact grid suitable for a widget footprint; layout
  survives window resize.
- FR-05 Primary card always present: local time in fullscreen-mode size.
- FR-06 Add/remove time-zone cards through a zone picker (timezone-agnostic;
  uses `TimeZoneInfo`).

### 3.2 World Clock (P0)

- FR-10 Display current time for local time plus N user-selected zones.
- FR-11 All clocks update on a single 1-second tick (single UI timer).
- FR-12 Zone selection persists across sessions.
- FR-13 Cards label zones with city name and UTC offset.

### 3.3 Alarms (P0 for creation, P1 for rich UX)

- FR-20 Create a single-occurrence alarm for a given zone/time.
- FR-21 Enable/disable alarms per zone; visual on-card status indicator.
- FR-22 Ringing state: audible notification + visual highlight; dismissed by
  user action.
- FR-23 Alarms persist across sessions and re-arm correctly after reboot.
- FR-24 (P2) Recurring alarms (weekly/on-workdays).

### 3.4 Timer (P1)

- FR-30 Fully operable countdown: set minutes/seconds, start, pause, reset.
- FR-31 Visual indication while running (flip-card digits count down).
- FR-32 On completion: notification + state reset.
- FR-33 Timer state survives minimize but not necessarily app restart (P1).

### 3.5 Fullscreen Desktop Clock (P1)

- FR-40 Toggle button enters "Desktop Clock" mode: window covers entire screen
  but does **not** block the OS (input forwarding / no focus lock).
- FR-41 Displays local (or selected zone) time in display-clock typography.
- FR-42 ESC (or same toggle) exits back to widget mode.
- FR-43 Neon preference applies in fullscreen too.

### 3.6 Always-on-Top Widget (P0)

- FR-50 Pin/unpin toggle keeps window above other apps.
- FR-51 Pin state persists across sessions.
- FR-52 Window remains draggable/movable while pinned.

### 3.7 Neon Customization (P0 core, P1 palette)

- FR-60 Dark theme by default; no light theme required for MVP.
- FR-61 Neon toggle enables glowing border around the widget/cards.
- FR-62 Color picker chooses the neon accent (persisted).
- FR-63 Glow is a 1.5px outer stroke + soft shadow (10–20px gaussian),
  flicker-free, per DEGING.md.
- FR-64 (P2) Per-card neon color.

### 3.8 Settings & Persistence (P0)

- FR-70 Persist to JSON under `%APPDATA%` (`ChronosFlip/*.json`).
- FR-71 Persisted: zones, alarms, neon color, neon enabled, pin state,
  window position/size.
- FR-72 Atomic writes; corrupt-file recovery (fall back to defaults).

## 4. Non-Functional Requirements

| ID   | Requirement                                                                |
|------|----------------------------------------------------------------------------|
| NFR-01 | Idle CPU < 1% and low RAM footprint while running as widget.              |
| NFR-02 | Single 1s tick drives all updates; no per-card timers.                    |
| NFR-03 | WinUI 3 (Windows App SDK), .NET 8/9, C# latest LTS, MVVM.                |
| NFR-04 | Fluent UI with smooth flip/glow animations, no visual jitter.             |
| NFR-05 | Native Windows window control (topmost, fullscreen, HWND pinning).        |
| NFR-06 | Time computations are timezone-agnostic and DST-safe (`TimeZoneInfo`).    |
| NFR-07 | Double instance: prevent launching a second process.                      |

## 5. Out of Scope (MVP)

- Cross-platform support (WinUI 3 is Windows-only).
- Multi-user/profiles; OCR/mic; cloud sync.
- System tray popup menu (can be added as widget host later).

## 6. Proposed Milestones

1. **M1 — Shell & Local Clock**: solution scaffold in `src/`, WinUI 3 app
   booting, dark theme tokens, primary local-time flip card, 1s tick, neon
   toggle + fixed default neon. *(covers FR-01, 05, 11, 60–63, NFR-01/03/04)*
2. **M2 — Settings & Persistence**: JSON store, window/theme/neon persistence,
   settings UI (color picker). *(FR-70–72, NFR-02)*
3. **M3 — World Clock**: multi-zone cards, zone picker, offset calc, persistence
   of zones. *(FR-06, 10–13)*
4. **M4 — Window Modes**: always-on-top + fullscreen desktop clock (non-blocking).
   *(FR-40–43, 50–52, NFR-05)*
5. **M5 — Alarms**: create/enable/disable, ringing UX, persistence. *(FR-20–23)*
6. **M6 — Timer**: full countdown UX. *(FR-30–33)*
7. **M7 — Polish**: animations quality, high-DPI, single-instance, bug
   hardening. *(NFR-06/07)*

## 7. Open Questions

- Notification sound source for alarms (system sound vs bundled asset)?
- Fullscreen non-blocking: transparent overlay w/ click-through vs borderless
  window focus suppression?
- Default zones shown on first run.