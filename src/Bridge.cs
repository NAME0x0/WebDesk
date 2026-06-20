using System.Diagnostics;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace WebDesk;

/// <summary>
/// JSON-RPC bridge between the HTML SPA and the C# backend. The SPA posts
/// { id, method, params }; this replies { id, ok, result, error }.
/// All handlers run on the UI thread (WebView2 marshals the event).
/// </summary>
internal sealed class Bridge
{
    private readonly SettingsStore _settings;
    private readonly LibraryStore _library;
    private readonly CatalogService _catalog;
    private readonly LocalLibraryService _local;
    private readonly WallpaperController _controller;
    private readonly MainWindow _window;
    private readonly RotationService _rotation;
    private readonly ProviderService _providers = new();
    private CoreWebView2? _web;

    public Bridge(SettingsStore settings, LibraryStore library, CatalogService catalog,
        LocalLibraryService local, WallpaperController controller, RotationService rotation, MainWindow window)
    {
        _settings = settings;
        _library = library;
        _catalog = catalog;
        _local = local;
        _controller = controller;
        _rotation = rotation;
        _window = window;
    }

    public void Attach(CoreWebView2 web)
    {
        _web = web;
        web.WebMessageReceived += OnMessage;
    }

    private async void OnMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var id = 0;
        try
        {
            using var doc = JsonDocument.Parse(e.TryGetWebMessageAsString());
            var root = doc.RootElement;
            id = root.GetProperty("id").GetInt32();
            var method = root.GetProperty("method").GetString()!;
            var p = root.TryGetProperty("params", out var pe) ? pe : default;

            var result = await Dispatch(method, p);
            Reply(id, ok: true, result, error: null);
        }
        catch (Exception ex)
        {
            Reply(id, ok: false, result: null, error: ex.Message);
        }
    }

    private async Task<object?> Dispatch(string method, JsonElement p) => method switch
    {
        "getState" => new
        {
            settings = _settings.Value,
            current = _controller.Current,
            paused = _controller.Paused,
            version = AppInfo.Version,
            startupEnabled = StartupRegistration.IsEnabled(),
            monitors = Monitors(),
        },
        "getMonitors" => Monitors(),
        "setMonitorDisabled" => SetMonitorDisabled(p),
        "getCatalog" => await _catalog.GetAsync(p.ValueKind == JsonValueKind.Object
            && p.TryGetProperty("refresh", out var r) && r.GetBoolean()),
        "getLibrary" => _library.All,
        "getLocal" => _local.Scan(_settings.Value.LibraryFolder),
        "apply" => Apply(p),
        "save" => Save(p),
        "remove" => Remove(p),
        "toggleStar" => new { starred = _library.ToggleStar(Str(p, "id")) },
        "getSettings" => _settings.Value,
        "setSettings" => ApplySettings(p.GetProperty("settings")),
        "pickFolder" => new { folder = _window.PickFolder() },
        "pauseToggle" => new { paused = _controller.TogglePause() },
        "checkUpdate" => new { update = await Updater.CheckAsync(AppInfo.Repo, AppInfo.Version) },
        "openExternal" => OpenExternal(p),
        "submitWallpaper" => SubmitWallpaper(p.GetProperty("wallpaper")),
        "searchProvider" => await SearchProvider(p),
        "getPlaylist" => _settings.Value.Playlist,
        "playlistAdd" => PlaylistAdd(p),
        "playlistRemove" => PlaylistRemove(p),
        "setRotation" => SetRotation(p),
        "browserShow" => await BrowserShow(p),
        "browserMove" => SetBrowserBounds(p),
        "browserHide" => Act(_window.HideBrowser),
        "browserNavigate" => await BrowserNavigate(p),
        "browserBack" => Act(_window.BrowserBack),
        "browserForward" => Act(_window.BrowserForward),
        "browserReload" => Act(_window.BrowserReload),
        "browserCapture" => await _window.CaptureAsync(
            p.TryGetProperty("mode", out var m) ? m.GetString() ?? "media" : "media") ?? new { source = (string?)null },
        _ => throw new InvalidOperationException($"Unknown method: {method}"),
    };

    private object PlaylistAdd(JsonElement p)
    {
        var w = p.GetProperty("wallpaper").Deserialize<Wallpaper>(Json.Web)!;
        var playlist = _settings.Value.Playlist;
        if (!playlist.Any(x => x.Id == w.Id)) { playlist.Add(w); _settings.Save(); }
        return playlist;
    }

    private object PlaylistRemove(JsonElement p)
    {
        _settings.Value.Playlist.RemoveAll(x => x.Id == Str(p, "id"));
        _settings.Save();
        return _settings.Value.Playlist;
    }

    private object SetRotation(JsonElement p)
    {
        _settings.Value.RotateMinutes = p.GetProperty("minutes").GetInt32();
        _settings.Value.Shuffle = p.TryGetProperty("shuffle", out var s) && s.GetBoolean();
        _settings.Save();
        _rotation.Reconfigure();
        return new { ok = true };
    }

    private async Task<object> SearchProvider(JsonElement p)
    {
        var provider = p.GetProperty("provider").GetString()!;
        var query = p.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "";
        var page = p.TryGetProperty("page", out var pg) ? pg.GetInt32() : 1;
        var (items, lastPage) = await _providers.SearchAsync(provider, query, page);
        return new { wallpapers = items, lastPage, page };
    }

    private async Task<object> BrowserShow(JsonElement p)
    {
        await _window.EnsureBrowserAsync();
        ApplyBounds(p);
        _window.ShowBrowser();
        return new { ok = true };
    }

    private async Task<object> BrowserNavigate(JsonElement p)
    {
        await _window.EnsureBrowserAsync();
        _window.NavigateBrowser(p.GetProperty("url").GetString()!);
        return new { ok = true };
    }

    private object SetBrowserBounds(JsonElement p) { ApplyBounds(p); return new { ok = true }; }

    private void ApplyBounds(JsonElement p) => _window.SetBrowserBounds(
        p.GetProperty("x").GetInt32(), p.GetProperty("y").GetInt32(),
        p.GetProperty("w").GetInt32(), p.GetProperty("h").GetInt32());

    private static object Act(Action action) { action(); return new { ok = true }; }

    private object[] Monitors() => _controller.Surfaces
        .Select(s => (object)new
        {
            id = s.DeviceName,
            label = s.Label,
            primary = s.IsPrimary,
            disabled = _settings.Value.DisabledMonitors.Contains(s.DeviceName),
        })
        .ToArray();

    private object SetMonitorDisabled(JsonElement p)
    {
        var id = p.GetProperty("id").GetString()!;
        var disabled = p.TryGetProperty("disabled", out var d) && d.GetBoolean();
        _controller.SetMonitorDisabled(id, disabled);
        return new { ok = true, disabled };
    }

    private object Apply(JsonElement p)
    {
        var wallpaper = p.GetProperty("wallpaper").Deserialize<Wallpaper>(Json.Web)!;
        var target = p.TryGetProperty("target", out var t) ? t.GetString() : "all";
        _controller.Apply(wallpaper, target);
        return new { ok = true };
    }

    private object SubmitWallpaper(JsonElement el)
    {
        var w = el.Deserialize<Wallpaper>(Json.Web)!;
        var entry = JsonSerializer.Serialize(new
        {
            id = w.Id, title = w.Title, type = w.Type, source = w.Source,
            thumbnail = w.Thumbnail, tags = w.Tags, author = w.Author,
        }, Json.Web);

        try { Clipboard.SetText(entry); } catch { /* clipboard busy */ }

        var body = Uri.EscapeDataString(
            "Proposed catalog entry (also copied to your clipboard):\n\n```json\n" + entry + "\n```\n");
        var title = Uri.EscapeDataString("Wallpaper submission: " + w.Title);
        Process.Start(new ProcessStartInfo(
            $"https://github.com/{AppInfo.Repo}/issues/new?title={title}&body={body}") { UseShellExecute = true });

        return new { submitted = true };
    }

    private object Save(JsonElement p)
    {
        _library.Add(p.GetProperty("wallpaper").Deserialize<Wallpaper>(Json.Web)!);
        return new { saved = true };
    }

    private object Remove(JsonElement p)
    {
        _library.Remove(Str(p, "id"));
        return new { removed = true };
    }

    private object ApplySettings(JsonElement el)
    {
        var incoming = el.Deserialize<AppSettings>(Json.Web)!;
        var current = _settings.Value;

        var folderChanged = current.LibraryFolder != incoming.LibraryFolder;
        var muteChanged = current.MuteAudio != incoming.MuteAudio;
        var startupChanged = current.RunAtStartup != incoming.RunAtStartup;

        // Preserve runtime state the Settings page doesn't own.
        incoming.Current = current.Current;
        incoming.Paused = current.Paused;
        _settings.Replace(incoming);

        if (startupChanged) StartupRegistration.Set(incoming.RunAtStartup);
        if (folderChanged) _window.MapLibrary(incoming.LibraryFolder);
        if (muteChanged) _controller.Reapply();

        return _settings.Value;
    }

    private static object OpenExternal(JsonElement p)
    {
        Process.Start(new ProcessStartInfo(Str(p, "url")) { UseShellExecute = true });
        return new { ok = true };
    }

    private static string Str(JsonElement p, string name) => p.GetProperty(name).GetString()!;

    private void Reply(int id, bool ok, object? result, string? error)
        => _web?.PostWebMessageAsString(
            JsonSerializer.Serialize(new { id, ok, result, error }, Json.Web));
}
