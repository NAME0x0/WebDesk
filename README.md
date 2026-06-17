# WebDesk 🖥️

**Turn any website, video, image, or WebGL shader into a live Windows wallpaper.**
A free, open-source alternative to Wallpaper Engine and Lively — built on C#/.NET 8 + WebView2.

[![CI](https://github.com/NAME0x0/WebDesk/actions/workflows/ci.yml/badge.svg)](https://github.com/NAME0x0/WebDesk/actions/workflows/ci.yml)
[![Release](https://github.com/NAME0x0/WebDesk/actions/workflows/release.yml/badge.svg)](https://github.com/NAME0x0/WebDesk/actions/workflows/release.yml)
[![Latest release](https://img.shields.io/github/v/release/NAME0x0/WebDesk?sort=semver)](https://github.com/NAME0x0/WebDesk/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/NAME0x0/WebDesk/total)](https://github.com/NAME0x0/WebDesk/releases)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D6)
[![License](https://img.shields.io/github/license/NAME0x0/WebDesk)](LICENSE)

---

## 🚀 Quick start

1. **Download** `WebDesk.exe` from the [latest release](https://github.com/NAME0x0/WebDesk/releases/latest).
2. **Run** it — no installer, fully portable, self-contained (no .NET install required).
3. **Pick a wallpaper** from Discover, browse the web for one, or paste a URL.

> Closing the window hides WebDesk to the tray. Right-click the tray icon → **Exit** to quit.

## ✨ Features

- **Any wallpaper type** — live websites, local HTML, video (`.mp4`/`.webm`), images, and **WebGL shaders**.
- **In-app browser + capture** — browse Moewalls, MotionBGs, MyLiveWallpapers, 4KWallpapers, Unsplash, Pinterest, X, anywhere — then "Set image/video" or "Set live page" as your wallpaper. Works on any site, no scraping.
- **Discover** — a curated, community-updatable catalog plus the **Wallhaven** provider (keyless search). Submit your own wallpapers straight from the app.
- **Audio-reactive shaders** — shader uniforms (`iBass`/`iMid`/`iTreble`/`iLevel`) driven by your system audio via WASAPI loopback.
- **Per-monitor wallpapers** — one wallpaper everywhere, or a different one per display. Renders *behind* desktop icons (WorkerW).
- **Library & playlists** — save/star wallpapers, point at a local folder, and rotate a playlist on a timer (with shuffle).
- **Performance** — auto-pause on fullscreen apps or on battery, plus a shader frame-rate cap.
- **Global hotkeys** — `Ctrl`+`Alt`+`→` next · `Ctrl`+`Alt`+`P` pause/resume · `Ctrl`+`Alt`+`M` mute.
- **Polished UI** — light/dark theme with an accent color, tray recents, run-at-startup (to tray), and an in-app self-updater.

## ⌨️ Usage

| Surface | What it does |
|---|---|
| **Home** | Current wallpaper + featured picks |
| **Discover** | Catalog + Wallhaven search, filter by tag, Set / Save / ★ / ＋ playlist |
| **Browser** | Real in-app browser with the capture bar |
| **Library** | Saved & Starred · Local Folder · Playlist (rotation + shuffle) |
| **Settings** | Theme/accent, performance, startup, mute, library folder, updates |
| **Tray** | Open · Pause/Resume · Recent · Settings · Exit |

**Command-line flags** (also handy for shortcuts): `--tray` (start hidden), `--page=<home|discover|browser|library|settings>` (deep-link, sub-routes like `discover/wallhaven/nature`), `--open=<url>` (open in the browser pane), `--grab=<media|page>` (open a URL and set it as wallpaper headlessly).

## ❓ FAQ

**Where is my data?** Settings and library live in `%APPDATA%\WebDesk`; caches in `%LOCALAPPDATA%\WebDesk`. The exe stays portable — the only system change is the optional "run at startup" registry entry.

**Do I need anything installed?** No. The build is self-contained. The Microsoft Edge **WebView2 runtime** is preinstalled on Windows 11 and most Windows 10 machines; if missing, install the Evergreen Bootstrapper from Microsoft.

**How do I add wallpapers to Discover?** Open the in-app **Submit** form (Discover → "+ Submit a wallpaper"), or send a PR adding an entry to [`catalog/catalog.json`](catalog/catalog.json) (`id`, `title`, `type`, `source`, optional `thumbnail`/`tags`/`author`/`audio`).

## 🛠️ Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer) on Windows.

```bash
# Run locally
dotnet run --project WebDesk.csproj

# Build a portable, self-contained single-file exe -> publish/WebDesk.exe
dotnet publish WebDesk.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

# Regenerate the icon from the logo (optional; needs Python + Pillow)
python tools/make_icon.py
```

## 🧩 Architecture

Three WebView2 surfaces, one rendering path. A URL navigates directly; local HTML loads via a virtual-host folder mapping; video/image are wrapped in a generated fullscreen HTML shell; shaders run in a WebGL runner. No separate media engine.

```
WebDesk/
├── WebDesk.csproj            # Project + WebView2 / NAudio package refs
├── app.manifest              # Per-monitor DPI awareness + Win10/11 compat
├── catalog/catalog.json      # Discover catalog (community-editable)
├── tools/make_icon.py        # Regenerates Resources/app.ico from logo.svg
├── wwwroot/                   # Embedded management UI (SPA), served via virtual host
│   ├── index.html · styles.css · app.js · bridge.js
│   ├── browser-home.html      # Browser start page (site tiles)
│   ├── shader-runner.html     # WebGL shader host (audio uniforms, FPS cap)
│   └── logo.svg
└── src/
    ├── Program.cs · WebDeskContext.cs        # Entry + app root (services/tray/windows)
    ├── MainWindow.cs                          # App shell + in-app browser pane
    ├── WallpaperSurface.cs · DesktopHost.cs   # Per-monitor renderer, WorkerW reparent
    ├── WallpaperController.cs                 # Apply / pause / rotate / audio routing
    ├── Bridge.cs                              # JSON-RPC bridge (SPA ⇄ C#)
    ├── CatalogService.cs · ProviderService.cs # Discover catalog + Wallhaven
    ├── LibraryStore.cs · LocalLibraryService.cs
    ├── PerfGuard.cs · RotationService.cs · HotkeyService.cs · AudioCapture.cs
    ├── SettingsStore.cs · Models.cs · AppPaths.cs · AppInfo.cs
    ├── AssetExtractor.cs · IconLoader.cs · StartupRegistration.cs
    ├── Updater.cs · NativeMethods.cs
```

## 🔄 CI/CD

Self-healing GitHub Actions:

- **`ci.yml`** — builds and validates on every push/PR. NuGet cached, restore retried, stale runs auto-cancelled.
- **`release.yml`** — on a `v*` tag: publishes the single-file exe, writes a SHA-256 checksum, and creates a GitHub Release with generated notes.
- **`auto-rerun.yml`** — re-runs failed CI/Release jobs **once** automatically, so transient (network/NuGet/runner) failures recover without intervention.
- **`dependabot.yml`** — weekly updates for GitHub Actions and NuGet packages.

Cut a release:

```bash
git tag -a vX.Y.Z -m "WebDesk X.Y.Z"
git push origin vX.Y.Z
```

## 🤝 Contributing

Fork → branch → PR. Adding wallpapers? Just edit [`catalog/catalog.json`](catalog/catalog.json). Bugs/ideas? [Open an issue](https://github.com/NAME0x0/WebDesk/issues).

## 🔍 Troubleshooting

- **Wallpaper not showing / black screen** — ensure the WebView2 runtime is installed and graphics drivers are current; try Pause→Resume from the tray.
- **Video won't autoplay with sound** — disable "Mute audio" in Settings (muted is the default).
- **Captured video doesn't play** — some sites stream via `blob:` URLs; WebDesk falls back to setting the live page instead.

## 📜 License

MIT — see [LICENSE](LICENSE).

## 🙏 Acknowledgments

[Microsoft WebView2](https://learn.microsoft.com/microsoft-edge/webview2/) · [NAudio](https://github.com/naudio/NAudio) · [Wallhaven API](https://wallhaven.cc/help/api) · [.NET](https://dotnet.microsoft.com/)
