using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace WebDesk;

/// <summary>
/// Application root: owns the services, one wallpaper surface per monitor, the
/// management window, the performance guard, and the tray icon.
/// </summary>
internal sealed class WebDeskContext : ApplicationContext
{
    private readonly SettingsStore _settings = new();
    private readonly LibraryStore _library = new();
    private readonly CatalogService _catalog = new();
    private readonly LocalLibraryService _local = new();
    private readonly AudioCapture _audio = new();

    private readonly List<WallpaperSurface> _surfaces;
    private readonly WallpaperController _controller;
    private readonly RotationService _rotation;
    private readonly MainWindow _main;
    private readonly PerfGuard _perf;
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _recentMenu = new("Recent");
    private HotkeyService? _hotkeys;

    private readonly bool _startInTray =
        Environment.GetCommandLineArgs().Any(a => a.Equals("--tray", StringComparison.OrdinalIgnoreCase));

    public WebDeskContext()
    {
        _surfaces = Screen.AllScreens.Select(s => new WallpaperSurface(s)).ToList();
        _controller = new WallpaperController(_surfaces, _settings, _audio);
        _rotation = new RotationService(_controller, _settings);
        _main = new MainWindow(_settings, _library, _catalog, _local, _controller, _rotation);
        _perf = new PerfGuard(_controller, _settings);

        _pauseItem = new ToolStripMenuItem(PauseLabel(), null, (_, _) => TogglePause());
        _tray = CreateTray();

        foreach (var surface in _surfaces) surface.Show();
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        try
        {
            AssetExtractor.ExtractUi();

            var options = new CoreWebView2EnvironmentOptions
            {
                AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required",
            };
            var env = await CoreWebView2Environment.CreateAsync(null, AppPaths.WebViewWallpaper, options);

            var host = DesktopHost.Resolve();
            foreach (var surface in _surfaces) surface.Attach(host);
            await Task.WhenAll(_surfaces.Select(s => s.InitializeAsync(env)));

            _controller.RestoreOnStartup();
            _rotation.Reconfigure();
            _perf.Start();
            RegisterHotkeys();

            await _main.InitializeAsync();

            var args = Environment.GetCommandLineArgs();
            var page = args.FirstOrDefault(a => a.StartsWith("--page=", StringComparison.OrdinalIgnoreCase))?["--page=".Length..];
            var open = args.FirstOrDefault(a => a.StartsWith("--open=", StringComparison.OrdinalIgnoreCase))?["--open=".Length..];
            var grab = args.FirstOrDefault(a => a.StartsWith("--grab=", StringComparison.OrdinalIgnoreCase))?["--grab=".Length..];

            if (!_startInTray) _main.ShowApp(page);

            if (!string.IsNullOrEmpty(open))
            {
                await _main.EnsureBrowserAsync();
                await Task.Delay(1500); // let the SPA position the pane
                _main.NavigateBrowser(open);

                // --grab: open a page and set its dominant media (or the page) as wallpaper.
                if (!string.IsNullOrEmpty(grab))
                {
                    await Task.Delay(3500); // let the page load
                    if (await _main.CaptureAsync(grab) is Wallpaper wp)
                        _controller.Apply(wp, "all");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "WebView2 failed to start. Install the Microsoft Edge WebView2 Runtime and try again.\n\n" + ex.Message,
                AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExitApp();
        }
    }

    private NotifyIcon CreateTray()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open WebDesk", null, (_, _) => _main.ShowApp());
        menu.Items.Add(_pauseItem);
        menu.Items.Add(_recentMenu);
        menu.Items.Add("Settings", null, (_, _) => _main.ShowApp("settings"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());
        menu.Opening += (_, _) => RebuildRecents();

        var tray = new NotifyIcon
        {
            Icon = IconLoader.AppIcon(),
            Text = AppInfo.Name,
            Visible = true,
            ContextMenuStrip = menu,
        };
        tray.DoubleClick += (_, _) => _main.ShowApp();
        return tray;
    }

    private void RebuildRecents()
    {
        _recentMenu.DropDownItems.Clear();
        var recents = _controller.Recents;
        if (recents.Count == 0)
        {
            _recentMenu.DropDownItems.Add(new ToolStripMenuItem("(none yet)") { Enabled = false });
            return;
        }
        foreach (var w in recents)
        {
            var item = w;
            _recentMenu.DropDownItems.Add(new ToolStripMenuItem(w.Title, null, (_, _) => _controller.Apply(item, "all")));
        }
    }

    private void RegisterHotkeys()
    {
        const uint mods = NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT;
        _hotkeys = new HotkeyService();
        _hotkeys.Register(mods, 0x27, () => _controller.ApplyPlaylistNext());            // Ctrl+Alt+Right → next
        _hotkeys.Register(mods, 0x50, () => { TogglePause(); });                          // Ctrl+Alt+P → pause
        _hotkeys.Register(mods, 0x4D, () => _controller.ToggleMute());                    // Ctrl+Alt+M → mute
    }

    private void TogglePause()
    {
        _controller.TogglePause();
        _pauseItem.Text = PauseLabel();
    }

    private string PauseLabel() => _controller.Paused ? "Resume wallpaper" : "Pause wallpaper";

    private void ExitApp()
    {
        _tray.Visible = false;
        _tray.Dispose();
        _hotkeys?.Dispose();
        _rotation.Dispose();
        _perf.Dispose();
        _audio.Dispose();
        _main.AllowClose();
        _main.Dispose();
        foreach (var surface in _surfaces) surface.Dispose();
        ExitThread();
    }
}
