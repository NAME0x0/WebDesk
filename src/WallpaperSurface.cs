using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebDesk;

/// <summary>
/// One borderless WebView2 window covering a single monitor, reparented under
/// the desktop so its content renders as that monitor's wallpaper.
/// Renders URL, local HTML, video, image, and WebGL shader wallpapers.
/// </summary>
internal sealed class WallpaperSurface : Form
{
    private const string MediaHost = "wallpaper.local";
    private const string UiHost = "webdesk.app";

    private readonly Screen _screen;
    private readonly WebView2 _webView;

    private (Wallpaper Wallpaper, bool Mute, int Fps)? _pending;
    private string? _mappedDir;
    private string? _currentId;
    private bool _currentMute;
    private bool _shown;
    private bool _isShader;
    private bool _audio;

    public WallpaperSurface(Screen screen)
    {
        _screen = screen;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Bounds = screen.Bounds;
        Text = "WebDesk Wallpaper";

        _webView = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.Black };
        Controls.Add(_webView);
    }

    public string DeviceName => _screen.DeviceName;
    public bool IsPrimary => _screen.Primary;
    public string Label =>
        $"{(_screen.Primary ? "Primary" : "Display")} · {_screen.Bounds.Width}×{_screen.Bounds.Height}";
    public bool IsAudioShaderShowing => _shown && _isShader && _audio;
    public bool Disabled { get; private set; }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= NativeMethods.WS_EX_NOACTIVATE;
            return cp;
        }
    }

    /// <summary>Reparent under the desktop host and position over this monitor.</summary>
    public void Attach(IntPtr host)
    {
        if (host == IntPtr.Zero) return;

        NativeMethods.SetParent(Handle, host);
        var style = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, style | NativeMethods.WS_EX_NOACTIVATE);

        // Position relative to the host's top-left (== virtual screen origin).
        var v = SystemInformation.VirtualScreen;
        Location = new Point(_screen.Bounds.X - v.X, _screen.Bounds.Y - v.Y);
        Size = _screen.Bounds.Size;
    }

    public async Task InitializeAsync(CoreWebView2Environment env)
    {
        await _webView.EnsureCoreWebView2Async(env);

        var s = _webView.CoreWebView2.Settings;
        s.AreDefaultContextMenusEnabled = false;
        s.AreDevToolsEnabled = false;
        s.IsStatusBarEnabled = false;
        s.AreBrowserAcceleratorKeysEnabled = false;

        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            UiHost, AppPaths.UiFolder, CoreWebView2HostResourceAccessKind.Allow);

        if (_pending is { } p)
        {
            _pending = null;
            Render(p.Wallpaper, p.Mute, p.Fps, force: true);
        }
    }

    public void Render(Wallpaper w, bool mute, int fpsCap, bool force)
    {
        if (_webView.CoreWebView2 is not { } core)
        {
            _pending = (w, mute, fpsCap);
            return;
        }

        if (!force && _shown && _currentId == w.Id && _currentMute == mute && Visible) return;

        // Re-cover this monitor if it had been left untouched.
        Disabled = false;
        if (!Visible) Visible = true;

        _currentId = w.Id;
        _currentMute = mute;
        _shown = true;
        _isShader = false;
        _audio = false;

        switch (w.Type)
        {
            case WallpaperType.Url:
                core.Navigate(NormalizeUrl(w.Source));
                break;
            case WallpaperType.Html:
                core.Navigate(File.Exists(w.Source) ? MapAndUrl(w.Source) : NormalizeUrl(w.Source));
                break;
            case WallpaperType.Video:
                core.NavigateToString(VideoShell(MediaUrl(w.Source), mute));
                break;
            case WallpaperType.Image:
                core.NavigateToString(ImageShell(MediaUrl(w.Source)));
                break;
            case WallpaperType.Shader:
                RenderShader(core, w, fpsCap);
                break;
        }
    }

    public void Clear()
    {
        _shown = false;
        _currentId = null;
        _isShader = false;
        _audio = false;
        Disabled = false;
        if (!Visible) Visible = true;
        if (_webView.CoreWebView2 is { } core) core.Navigate("about:blank");
        else _pending = null;
    }

    /// <summary>Leave this monitor untouched — hide the surface so the real desktop shows.</summary>
    public void Disable()
    {
        Disabled = true;
        _shown = false;
        _currentId = null;
        _isShader = false;
        _audio = false;
        _pending = null;
        Visible = false;
        if (_webView.CoreWebView2 is { } core) core.Navigate("about:blank");
    }

    public void PushAudio(string json)
    {
        if (_isShader && _audio && _webView.CoreWebView2 is { } core)
            core.PostWebMessageAsString(json);
    }

    private void RenderShader(CoreWebView2 core, Wallpaper w, int fpsCap)
    {
        _isShader = true;
        _audio = w.Audio;
        var shaderSource = w.Source;

        void OnNavigated(object? s, CoreWebView2NavigationCompletedEventArgs e)
        {
            core.NavigationCompleted -= OnNavigated;
            _ = core.ExecuteScriptAsync("window.setShader(" + JsonSerializer.Serialize(shaderSource) + ")");
        }

        core.NavigationCompleted += OnNavigated;
        core.Navigate($"https://{UiHost}/shader-runner.html?fps={fpsCap}&audio={(w.Audio ? 1 : 0)}");
    }

    private string MediaUrl(string source) => File.Exists(source) ? MapAndUrl(source) : source;

    private string MapAndUrl(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath)!;
        if (_mappedDir != dir)
        {
            if (_mappedDir is not null)
                _webView.CoreWebView2!.ClearVirtualHostNameToFolderMapping(MediaHost);
            _webView.CoreWebView2!.SetVirtualHostNameToFolderMapping(
                MediaHost, dir, CoreWebView2HostResourceAccessKind.Allow);
            _mappedDir = dir;
        }
        return $"https://{MediaHost}/{Uri.EscapeDataString(Path.GetFileName(filePath))}";
    }

    private static string NormalizeUrl(string url)
        => url == "about:blank" || url.Contains("://") ? url : "https://" + url;

    private static string VideoShell(string src, bool mute) =>
        $$"""
          <!doctype html><html><head><meta charset="utf-8"><style>
          html,body{margin:0;width:100%;height:100%;overflow:hidden;background:#000}
          video{position:fixed;inset:0;width:100%;height:100%;object-fit:cover}
          </style></head><body>
          <video src="{{src}}" autoplay loop playsinline {{(mute ? "muted" : "")}}></video>
          </body></html>
          """;

    private static string ImageShell(string src) =>
        $$"""
          <!doctype html><html><head><meta charset="utf-8"><style>
          html,body{margin:0;width:100%;height:100%;background:#000 url("{{src}}") center/cover no-repeat}
          </style></head><body></body></html>
          """;
}
