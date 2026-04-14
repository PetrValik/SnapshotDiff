# SnapshotDiff

Cross-platform desktop file cleaner built with **.NET 10**, **Blazor** and **Tailwind CSS**.

Scan directories, filter files by age/size/extension, move to trash or delete permanently — with full undo via a built-in trash system backed by SQLite.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Hybrid-512BD4?logo=blazor)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-06B6D4?logo=tailwindcss)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **Directory scanning** — recursive scan with parallel I/O, cancellation support, real-time progress
- **Smart filtering** — by file age (stale > X days, new < X days), minimum size, extension, name search
- **Trash system** — move files to trash (atomic: DB insert → file move → rollback on failure), 30-day retention, restore or empty
- **Export** — scan results to CSV or JSON (CSV fields properly quoted against injection)
- **Exclusion rules** — glob patterns, exact paths, system-protected directories auto-detected
- **Multi-platform** — Windows & macOS (MAUI), Android (MAUI), Linux (Photino.NET)
- **Localization** — English + Czech, instant switching without page reload
- **Theming** — dark (default) / light / system, CSS custom properties

## Architecture

**Vertical Slice** — each feature is self-contained with its own Application, Domain, Infrastructure and UI layers.

```
SnapshotDiff/
├── SnapshotDiff.Core/          Razor Class Library (shared UI + business logic)
│   ├── Features/
│   │   ├── Config/             App settings, watched directories
│   │   ├── ExclusionRules/     Glob/exact ignore patterns
│   │   ├── Export/             CSV + JSON export
│   │   ├── Help/               In-app documentation
│   │   ├── Scanner/            Directory scan, filtering, sorting
│   │   └── Trash/              Move-to-trash, restore, purge (SQLite metadata)
│   ├── Infrastructure/         Cross-cutting: DI, hashing, localization, theme, file I/O
│   ├── Components/             Shared layout, toast notifications, dialogs
│   └── Styles/                 Tailwind CSS input + config
│
├── SnapshotDiff.MAUI/          MAUI host (Windows, macOS, Android, iOS)
├── SnapshotDiff.Linux/         Photino.NET host (Linux native)
├── SnapshotDiff/               ASP.NET Core Blazor Server host (dev/testing)
├── SnapshotDiff.Tests/         xUnit + NSubstitute + FluentAssertions (134 tests)
│
├── .github/workflows/
│   ├── build.yml               CI: build + test on push/PR
│   └── release.yml             CD: GitHub Release on tag v*.*.*
├── build-release.ps1           Local release build script
└── plan.md                     Project specification
```

### Data storage

| What | Where | Format |
|---|---|---|
| App config | `{AppData}/config.json` | JSON (atomic writes via tmp+move) |
| Trash metadata | `{AppData}/trash.db` | SQLite (`Microsoft.Data.Sqlite`) |
| Trash files | `{AppData}/trash/files/{guid}` | Original file, renamed |
| Scan results | In-memory only | Not persisted |

### Key design decisions

- **Thread safety** — `SemaphoreSlim` for config file access, `lock` for toast list, atomic DB+file operations in trash
- **Security** — path traversal guards, ReDoS-safe wildcard matching (iterative two-pointer), CSV injection prevention, pattern validation (260 chars, 10 wildcards max)
- **Symlink handling** — scanner detects `ReparsePoint` attribute and skips symlinks

## Platforms

| Platform | Host | Runtime |
|---|---|---|
| **Windows** | MAUI + BlazorWebView | `net10.0-windows10.0.19041.0` |
| **macOS** | MAUI + BlazorWebView | `net10.0-maccatalyst` |
| **Android** | MAUI + BlazorWebView | `net10.0-android` (API 26+) |
| **Linux** | Photino.NET | `net10.0` (linux-x64) |
| **Web** | Blazor Server | `net10.0` (dev/testing only) |

> iOS is excluded — Apple sandbox prevents free filesystem access.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (for Tailwind CSS compilation)
- **Windows:** MAUI workload — `dotnet workload install maui-windows`
- **Linux:** GTK3 and WebKit2GTK (for Photino) — `sudo apt install libgtk-3-dev libwebkit2gtk-4.1-dev`

### Build & run

```bash
# 1. Install Tailwind and compile CSS
cd SnapshotDiff.Core
npm install
npm run build:css

# 2a. Run the Web host (easiest for development)
cd ../SnapshotDiff
dotnet run

# 2b. Run the MAUI Windows app
cd ../SnapshotDiff.MAUI
dotnet build -f net10.0-windows10.0.19041.0

# 2c. Run the Linux app
cd ../SnapshotDiff.Linux
dotnet run
```

### Run tests

```bash
dotnet test SnapshotDiff.Tests
```

134 tests covering scanner, trash, export, exclusion rules, pattern matching and filtering.

### Watch CSS during development

```bash
cd SnapshotDiff.Core
npm run watch:css
```

## Building releases

### Local (PowerShell)

```powershell
# All platforms
.\build-release.ps1 -Version 1.0.0

# Windows only
.\build-release.ps1 -Version 1.0.0 -Target windows

# Linux only
.\build-release.ps1 -Version 1.0.0 -Target linux
```

Output in `./dist/`:
- `SnapshotDiff-{VERSION}-windows-x64.zip`
- `SnapshotDiff-{VERSION}-linux-x64.tar.gz`

### CI/CD (GitHub Actions)

| Workflow | Trigger | What it does |
|---|---|---|
| `build.yml` | Push/PR to `main` | Build all projects, run tests, upload results |
| `release.yml` | Tag `v*.*.*` | Build Windows + Linux, create GitHub Release |

```bash
# Tag a release
git tag v1.0.0
git push origin v1.0.0
```

## Styling

**Tailwind CSS 3.4** with CSS custom properties for theming.

- Input: `SnapshotDiff.Core/Styles/app.input.css`
- Config: `SnapshotDiff.Core/tailwind.config.js`
- Output: `SnapshotDiff.Core/wwwroot/app.css` (minified, ~20KB)

Dark theme is default (`:root`), light theme via `.light` class on `<html>`.

Component classes use BEM naming (`app-btn`, `app-btn--primary`, `app-select`, `filter-label`, `file-row`, `dialog-backdrop`, etc.) defined in `@layer components`.

## Localization

Supported languages: **English** (default), **Czech**.

Resource files use `IStringLocalizer<T>` with `.resx` files per feature. Culture switching is instant — the shared `CultureState` singleton notifies all `Routes.razor` components to re-key the `<Router>`, destroying and recreating the entire component tree with the new strings.

## Tech stack

| Layer | Technology |
|---|---|
| UI framework | Blazor (Razor Class Library) |
| Styling | Tailwind CSS 3.4 + CSS custom properties |
| Desktop hosts | .NET MAUI, Photino.NET |
| Database | SQLite via `Microsoft.Data.Sqlite` |
| Logging | Serilog (Web), Debug logger (MAUI) |
| Testing | xUnit 2.9.3, NSubstitute 5.3.0, FluentAssertions 6.12.2 |
| CI/CD | GitHub Actions |

## License

MIT
