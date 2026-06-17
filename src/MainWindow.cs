using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebDesk;

/// <summary>
/// The management window: a WebView2 hosting the HTML/CSS/JS SPA
/// (Home / Discover / Library / Settings), wired to the backend via <see cref="Bridge"/>.
/// </summary>
internal sealed class MainWindow : Form
{
    private const string UiHost = "webdesk.app";
    private const string LibraryHost = "library.local";

    private readonly WebView2 _web;
    private readonly SettingsStore _settings;
    private readonly Bridge _bridge;
    private bool _allowClose;

    // Native browsing pane, overlaid on the SPA's Browser page.
    private WebView2? _browser;

    public MainWindow(SettingsStore settings, LibraryStore library, CatalogService catalog,
        LocalLibraryService local, WallpaperController controller, RotationService rotation)
    {
        _settings = settings;

        Text = AppInfo.Name;
        Icon = IconLoader.AppIcon();
        MinimumSize = new Size(900, 600);
        Size = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(15, 16, 20);

        _web = new WebView2 { Dock = DockStyle.Fill };
        Controls.Add(_web);

        _bridge = new Bridge(settings, library, catalog, local, controller, rotation, this);
    }

    public async Task InitializeAsync()
    {
        var env = await CoreWebView2Environment.CreateAsync(null, AppPaths.WebViewUi);
        await _web.EnsureCoreWebView2Async(env);

        var ui = AssetExtractor.ExtractUi();
        _web.CoreWebView2.SetVirtualHostNameToFolderMapping(
            UiHost, ui, CoreWebView2HostResourceAccessKind.Allow);
        MapLibrary(_settings.Value.LibraryFolder);

        _bridge.Attach(_web.CoreWebView2);

        var s = _web.CoreWebView2.Settings;
        s.IsStatusBarEnabled = false;
        s.AreDefaultContextMenusEnabled = false;

        _web.CoreWebView2.Navigate($"https://{UiHost}/index.html");
    }

    /// <summary>Map (or remap) the user's library folder so the SPA can preview local files.</summary>
    public void MapLibrary(string? folder)
    {
        if (_web.CoreWebView2 is not { } core) return;

        try { core.ClearVirtualHostNameToFolderMapping(LibraryHost); }
        catch { /* not mapped yet */ }

        if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            core.SetVirtualHostNameToFolderMapping(
                LibraryHost, folder, CoreWebView2HostResourceAccessKind.Allow);
    }

    public string? PickFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select your wallpaper library folder",
            UseDescriptionForTitle = true,
        };
        if (!string.IsNullOrEmpty(_settings.Value.LibraryFolder))
            dialog.SelectedPath = _settings.Value.LibraryFolder;

        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.SelectedPath : null;
    }

    public void ShowApp(string? route = null)
    {
        Show();
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();

        if (route is not null && _web.CoreWebView2 is not null)
            _ = _web.CoreWebView2.ExecuteScriptAsync($"location.hash='{route}'");
    }

    // ---------- in-app browser pane ----------

    private const string CaptureJs = """
        (function () {
          function abs(u){ try { return new URL(u, location.href).href; } catch (e) { return u; } }
          var vids = [...document.querySelectorAll('video')]
            .map(v => ({ area: v.clientWidth * v.clientHeight, src: v.currentSrc || v.src }))
            .filter(v => v.src && v.area > 0).sort((a, b) => b.area - a.area);
          if (vids.length) return { kind: 'video', src: abs(vids[0].src), title: document.title };
          var imgs = [...document.querySelectorAll('img')]
            .map(i => ({ area: i.clientWidth * i.clientHeight, src: i.currentSrc || i.src }))
            .filter(i => i.src && i.area > 1000).sort((a, b) => b.area - a.area);
          if (imgs.length) return { kind: 'image', src: abs(imgs[0].src), title: document.title };
          return { kind: 'page', src: location.href, title: document.title };
        })()
        """;

    public async Task EnsureBrowserAsync()
    {
        if (_browser is not null) return;

        _browser = new WebView2 { Visible = false };
        Controls.Add(_browser);
        _browser.BringToFront();

        var env = await CoreWebView2Environment.CreateAsync(null, AppPaths.WebViewBrowser);
        await _browser.EnsureCoreWebView2Async(env);
        _browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
        _browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "webdesk.app", AppPaths.UiFolder, CoreWebView2HostResourceAccessKind.Allow);
        _browser.CoreWebView2.Navigate("https://webdesk.app/browser-home.html");
    }

    public void SetBrowserBounds(int x, int y, int w, int h)
    {
        if (_browser is null) return;
        var scale = DeviceDpi / 96.0;
        _browser.Bounds = new Rectangle(
            (int)(x * scale), (int)(y * scale), (int)(w * scale), (int)(h * scale));
    }

    public void ShowBrowser() { if (_browser is not null) { _browser.Visible = true; _browser.BringToFront(); } }
    public void HideBrowser() { if (_browser is not null) _browser.Visible = false; }
    public void NavigateBrowser(string url) => _browser?.CoreWebView2?.Navigate(url);
    public void BrowserBack() { if (_browser?.CoreWebView2 is { CanGoBack: true } c) c.GoBack(); }
    public void BrowserForward() { if (_browser?.CoreWebView2 is { CanGoForward: true } c) c.GoForward(); }
    public void BrowserReload() => _browser?.CoreWebView2?.Reload();

    /// <summary>Capture the dominant media (or the live page) from the browser pane.</summary>
    public async Task<object?> CaptureAsync(string mode)
    {
        if (_browser?.CoreWebView2 is not { } core) return null;

        if (mode == "page")
        {
            var url = core.Source;
            return new Wallpaper { Id = "capture:" + url, Title = Host(url), Type = WallpaperType.Url, Source = url, Origin = "capture" };
        }

        var json = await core.ExecuteScriptAsync(CaptureJs);
        if (string.IsNullOrEmpty(json) || json == "null") return null;

        using var doc = JsonDocument.Parse(json);
        var r = doc.RootElement;
        var kind = r.GetProperty("kind").GetString();
        var src = r.GetProperty("src").GetString() ?? "";
        var title = r.TryGetProperty("title", out var t) ? t.GetString() : null;
        if (string.IsNullOrEmpty(src)) return null;

        // Streamed (blob:) media can't be used outside the page — fall back to the live page.
        if (src.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            return new Wallpaper { Id = "capture:" + core.Source, Title = Host(core.Source), Type = WallpaperType.Url, Source = core.Source, Origin = "capture" };

        var type = kind == "video" ? WallpaperType.Video : kind == "image" ? WallpaperType.Image : WallpaperType.Url;
        return new Wallpaper
        {
            Id = "capture:" + src,
            Title = string.IsNullOrWhiteSpace(title) ? Host(src) : title!,
            Type = type,
            Source = src,
            Thumbnail = type == WallpaperType.Image ? src : null,
            Origin = "capture",
        };
    }

    private static string Host(string url)
    {
        try { return new Uri(url).Host; } catch { return url; }
    }

    public void AllowClose() => _allowClose = true;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Closing the window hides to tray; real exit goes through the tray menu.
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnFormClosing(e);
    }
}
