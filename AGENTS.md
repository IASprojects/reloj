# AGENTS.md

Opinionated working context for AI agents contributing to **Chronos Flip**.

This file is the single source of truth for project decisions, conventions, and
structure. Update it whenever a foundational decision changes.

## 1. Product Overview

Chronos Flip is a **native Windows desktop clock dashboard** inspired by
mid-century flip clocks. It behaves like a lightweight desktop widget that can
expand to a fullscreen clock view.

### Core modules

- **World clock**: view current time in multiple time zones from the tray of
  flip cards.
- **Flip cards**: the primary navigation surface. Cards can be expanded
  ("flipped") to reveal details/alarms for a given zone.
- **Alarms**: create, enable and disable per-zone alarms (single occurrence).
- **Timer**: a fully working countdown stopwatch.
- **Fullscreen widget mode**: expand the clock to cover the whole screen
  without blocking the OS (i.e., a dedicated-clock look, not a kiosk lock).
- **Always-on-top**: pin the window in front of other apps as a floating
  widget.
- **Neon customization**: dark theme by default; each card can render a neon
  glow border whose color is user-selectable.

> Source of functional requirements: `features/01-Planning/draft.md`.

## 2. Design System

Identified by the codename **Nocturne Utility** (see `identyvisual/DEGING.md`).

Design pillars: **Minimalism + Tactile Skeuomorphism**, "calm precision",
"Digital Tactility".

- **Colors**: "Obsidian" dark surfaces. Primary `#121212`, text `#F5F5F5`
  (on cards), card surface `#2A2A2A`, borders `#3A3A3A`. Neon accents are
  functional tokens (high saturation + gaussian blur 10–20px).
- **Typography**:
  - `Space Mono` — clock digits / headers (monospaced, prevents jitter).
  - `Geist` — descriptive text and settings.
  - `JetBrains Mono` — labels and metadata, uppercase, letterspaced.
- **Layout**: 4px spacing unit; cards are modular; 24px inner padding so neon
  glows don't crowd content.
- **Flip cards**: each card has a 1px `#3A3A3A` border and a horizontal 1px
  divider (`#121212`) bisecting the number to evoke the mechanical flip split.
- **Physics/animations**: smooth card-flip transitions, clean neon flicker-free
  glow animation (UI requirement).

## 3. Technical Stack (fixed)

| Concern          | Choice                                                   |
|------------------|----------------------------------------------------------|
| OS               | Windows 10/11 (x64), native                              |
| Language         | C#, .NET 10 (LTS)                                        |
| UI framework     | **WinUI 3 (Windows App SDK)**                            |
| UI architecture  | MVVM                                                     |
| State/persistence| `SettingsStore` in `ChronosFlip.Core/Settings` (JSON under `%APPDATA%\ChronosFlip\settings.json`, atomic writes, corrupt-file recovery) |
| Time zones       | `TimeZoneInfo` / IANA via Windows; keep app timezone-agnostic|
| Build            | .NET solution in `src/` + `dotnet` CLI                   |

Rationale: WinUI 3 chosen over MAUI for lighter resource usage, native window
control (always-on-top, fullscreen), and smoother desktop experience with
hardware acceleration.

## 4. Repository Structure

```text
reloj/
├── AGENTS.md              # this file (agent context)
├── features/              # planning & feature docs (ADR-style)
│   └── 01-Planning/
│       └── draft.md       # functional/non-functional requirements
├── identyvisual/          # design system + mockups
│   ├── DEGING.md
│   └── mockup.html        # high-fidelity HTML prototype
└── src/                   # .NET solution lives here
    └── (TODO: ChronosFlip.* projects)
```

## 5. Conventions

- **Language**: code, commits and docs in English. User-facing UI text also
  English (localization can be added later).
- **Docs**: markdown. Planning docs live in `features/`, numbered folders
  (`01-Planning`, `02-...`). Keep draft.md updated as truth source.
- **Code style**: idiomatic modern C# (nullable enabled, file-scoped
  namespaces, implicit usings). Follow existing conventions in the repo.
- **MVVM**: keep ViewModels `INotifyPropertyChanged`-based (CommunityToolkit).
  No code-behind logic beyond wiring views.
- **UI**: never introduce new colors/typography outside the tokens in
  `DEGING.md`. Map design tokens to WinUI `ResourceDictionary` resources.
- **Performance**: low CPU/RAM is a first-class requirement (the app is meant
  to sit idle as a widget). Avoid polling timers where unnecessary; use a
  single 1s tick for clock reads; reuse timers for UI updates.

## 6. Development Workflow

Solo-dev, trunk-based. Every task ends green: build + tests pass.

1. **Plan first** — reference `features/01-Planning/PRD.md` (FR/NFR ids).
   No `src/` changes before a requirement owns the change.
2. **Build/verify** — `dotnet build` (from `src/`); tests with `dotnet test`.
   `dotnet build && dotnet test` must pass in `src/` before closing work.
3. **Commit** only when explicitly asked; small, atomic conventional commits
   (`feat:`, `fix:`, `docs:`, `refactor:`).
4. **Tests** — test services/ViewModels only (`ChronosFlip.Tests`, xUnit);
   never the WinUI 3 UI. Follow RED-GREEN-REFACTOR. Inject clocks / accept
   `DateTimeOffset`; never sleep in tests.

## 7. Agents & Token Budget

The main agent runs the full model; subagents run `small_model` to save tokens.
Definition of done includes routing review/test/API work to subagents.

- **`reviewer`** — read-only critic. Reviews `git diff` against PRD +
  conventions before a task closes. Never edits. Outputs severity-tagged report.
- **`tester`** — RED-GREEN cycle. Writes tests, runs `dotnet test`/`dotnet build`,
  reports pass counts.
- **`winui-expert`** — consultative. WinUI 3 / Windows App SDK / .NET API
  answers with paste-ready snippets. Never edits.
- **`explore`** (built-in) — cheap codebase search/reading to avoid pulling
  files into the main context.

Practical rules:

- Batch reads; delegate exploration to subagents instead of loading files
  yourself when results are large.
- Keep tool output tight (`tool_output.max_lines/bytes`); disable `lsp` if
  noisy.
- Compaction is auto (`tail_turns: 15`); long sessions stay usable.
