# Chronos Flip

A native Windows desktop clock dashboard inspired by mid-century flip clocks.
WinUI 3 / .NET 10, dark "Nocturne Utility" theme, flip-card UI with a single
shared 1-second clock tick.

Current stage: **App Shell & Local Clock** (feature `02`). Shows the local time
as three flip cards (Hours / Minutes / Seconds) in a compact widget window.

## Requirements

- Windows 10/11 (x64)
- .NET 10 SDK (`dotnet --list-sdks`)
- Windows App SDK 2.4.0 is pulled automatically from NuGet

## Build & Run

```powershell
# from repo root
dotnet build src\ChronosFlip.slnx

# run the app (unpackaged, launch directly)
dotnet run --project src\ChronosFlip

# or open the built exe
src\ChronosFlip\bin\Debug\net10.0-windows10.0.19041.0\win-x64\ChronosFlip.exe
```

## Tests

```powershell
dotnet test src\ChronosFlip.slnx
```

Unit tests cover the pure `ChronosFlip.Core` services/ViewModels (clock ticker,
time segmentation) with xUnit. The WinUI 3 UI itself is not under test.

## Debugging

### Visual Studio 2022+

1. Open `src\ChronosFlip.slnx`.
2. Set **ChronosFlip** as the startup project.
3. F5 to debug. The app is unpackaged, so no MSIX install is needed.

> If WinUI Hot Reload / Live Visual Tree are unavailable, verify the workload
> "Desktop development with C#" is installed (Windows App SDK tooling).

### VS Code

1. Open the repo root in VS Code with the **C# Dev Kit** extension.
2. Add a launch profile in `.vscode/launch.json`:

   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": "ChronosFlip (Debug)",
         "type": "coreclr",
         "request": "launch",
         "cwd": "${workspaceFolder}",
         "program": "${workspaceFolder}/src/ChronosFlip/bin/Debug/net10.0-windows10.0.19041.0/win-x64/ChronosFlip.exe",
         "preLaunchTask": "build"
       }
     ]
   }
   ```

   And a build task in `.vscode/tasks.json`:

   ```json
   {
     "version": "2.0.0",
     "tasks": [
       {
         "label": "build",
         "type": "process",
         "command": "dotnet",
         "args": ["build", "${workspaceFolder}/src/ChronosFlip.slnx"],
         "group": "build"
       }
     ]
   }
   ```

3. F5 to build and launch under the debugger. XAML (XBF) compiles at build time,
   so breakpoints in `.xaml.cs` work normally.

### CLI (`dotnet` on the console)

Debugging from a plain terminal is limited to writing to output and inspecting
event logs; for breakpoints use VS Code (see above) or Visual Studio. You can
still capture diagnostics of a running instance:

```powershell
dotnet build src\ChronosFlip.slnx
src\ChronosFlip\bin\Debug\net10.0-windows10.0.19041.0\win-x64\ChronosFlip.exe
# crash/minidump analysis: enable LocalDumps via regedit, or use
# dotnet-counters / dotnet-dump (install: dotnet tool install -g dotnet-dump)
dotnet-dump collect --process-id <pid>
```

### Notes

- **Single instance**: only one `ChronosFlip` process may run. If a second launch
  silently exits, one is already running. Kill it with
  `Stop-Process -Name ChronosFlip` before launching again.
- **Crash on launch**: an immediate exit often means the unpackaged Windows App
  SDK runtime failed to initialize. Check the Windows Event Log
  (`Application` → source `Application Error` / `.NET Runtime`).

## Solution Layout

``` text
src/
├── ChronosFlip.slnx        # solution
├── ChronosFlip.Core/       # pure services/ViewModels (net10.0, no WinUI)
├── ChronosFlip/            # WinUI 3 app (Windows App SDK, unpackaged)
└── ChronosFlip.Tests/      # xUnit tests (Core only)
```

Design system: `identyvisual/DEGING.md`. Requirements/roadmap: `features/`.
