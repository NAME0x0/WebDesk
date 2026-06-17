# WebDesk 🖥️

A lightweight desktop application that lets you set web pages as your Windows wallpaper.

![WebDesk Preview](docs/preview.png)

## 🚀 Quick Start

1. **Download**: Get the latest version from the [Releases](../../releases) page
2. **Run**: Just double-click `WebDesk.exe` - no installation needed!
3. **Use**: Right-click the tray icon to access settings and features

## ✨ Features

- **Any wallpaper type** — live websites, local HTML, video (`.mp4`/`.webm`), images, and **WebGL shaders**
- **Audio-reactive shaders** — shader uniforms driven by your system audio (WASAPI loopback)
- **Per-monitor wallpapers** — set one wallpaper for all displays, or a different one per monitor
- **Discover** — browse a curated, community-updatable catalog, and **submit your own** (no account, no server)
- **Library** — save & star wallpapers, or point WebDesk at a local folder
- **Performance** — auto-pause on fullscreen apps / on battery, plus a shader frame-rate cap
- **Settings + tray** — run at startup (to tray), mute audio, auto-update, pause/resume
- No installation required · renders behind your desktop icons · lightweight

## 🤔 Common Questions

### How do I use WebDesk?

1. Download `WebDesk.exe`
2. Run it
3. Right-click the tray icon (near the clock)
4. Choose your settings
5. Enjoy your live wallpaper!

### How do I update?

WebDesk checks for updates automatically. When an update is available, it will download and apply it automatically.

### Where are my settings saved?

Settings and your library live in `%APPDATA%\WebDesk`; caches in `%LOCALAPPDATA%\WebDesk`. The executable itself stays portable — no installer, no registry clutter (aside from the optional "run at startup" entry).

### How do I add wallpapers to Discover?

Discover reads [`catalog/catalog.json`](catalog/catalog.json) from this repo. Open a pull request adding an entry (`id`, `title`, `type`, `source`, optional `thumbnail`/`tags`/`author`) and it shows up for everyone.

## 🛠️ For Developers

WebDesk is a C# / .NET 8 (WinForms) app that renders a web page as the desktop
wallpaper using the Microsoft Edge **WebView2** runtime.

### Building from Source

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer).
2. Clone this repository.
3. Run it locally:
   ```bash
   dotnet run --project WebDesk.csproj
   ```
4. Build a portable, self-contained single-file `WebDesk.exe` (no .NET install
   needed on the target machine):
   ```bash
   dotnet publish WebDesk.csproj -c Release -r win-x64 --self-contained true \
     -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
   ```
   The exe lands in `publish/WebDesk.exe`.

> The WebView2 **runtime** is preinstalled on Windows 11 and most Windows 10
> machines. If missing, grab the Evergreen Bootstrapper from Microsoft.

### Project Structure
```
WebDesk/
├── WebDesk.csproj          # Project + WebView2 package reference
├── app.manifest            # Per-monitor DPI awareness + Win10/11 compat
├── wwwroot/                # The management UI (HTML/CSS/JS SPA), embedded
│   ├── index.html · styles.css · app.js · bridge.js · logo.svg
├── catalog/catalog.json    # Discover catalog (community-editable)
├── tools/make_icon.py      # Regenerates Resources/app.ico
└── src/
    ├── Program.cs          # Entry point
    ├── WebDeskContext.cs   # Services + tray + windows, wired together
    ├── MainWindow.cs       # App shell: WebView2 hosting the SPA
    ├── WallpaperForm.cs    # Wallpaper renderer, reparented under the desktop
    ├── Bridge.cs           # JSON-RPC bridge (SPA ⇄ C#)
    ├── WallpaperController.cs · SettingsStore.cs · LibraryStore.cs
    ├── CatalogService.cs · LocalLibraryService.cs · StartupRegistration.cs
    ├── Updater.cs · AssetExtractor.cs · AppPaths.cs · AppInfo.cs
    ├── Models.cs · NativeMethods.cs · IconLoader.cs
```

**How rendering works:** everything is WebView2. A URL navigates directly; local HTML loads via a virtual-host folder mapping; video/image are wrapped in a generated fullscreen HTML shell. One rendering path, no separate media engine.

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 🔍 Troubleshooting

### Common Issues

1. **Wallpaper not showing**
   - Make sure WebView2 Runtime is installed
   - Check if your HTML file path is correct
   - Verify the HTML file works in a browser

2. **Black screen**
   - Check if your graphics drivers are updated
   - Try running WebDesk as administrator

3. **Won't start with Windows**
   - Check Windows startup settings
   - Try reinstalling the application

### Getting Help

- [Open an Issue](https://github.com/your-username/WebDesk/issues)

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Microsoft WebView2](https://docs.microsoft.com/microsoft-edge/webview2/)
- [.NET Foundation](https://dotnetfoundation.org/)
