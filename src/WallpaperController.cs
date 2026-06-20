using System.Text.Json;

namespace WebDesk;

/// <summary>
/// Drives every monitor's wallpaper surface: applies wallpapers (globally or
/// per-monitor), honours user pause and the performance auto-suspend, and
/// routes system-audio levels to active shader wallpapers.
/// </summary>
internal sealed class WallpaperController
{
    private readonly IReadOnlyList<WallpaperSurface> _surfaces;
    private readonly SettingsStore _settings;
    private readonly AudioCapture _audio;
    private readonly SynchronizationContext _ui;

    private bool _autoSuspend;
    private int _playlistIndex = -1;
    private readonly Random _rng = new();

    public WallpaperController(
        IReadOnlyList<WallpaperSurface> surfaces, SettingsStore settings, AudioCapture audio)
    {
        _surfaces = surfaces;
        _settings = settings;
        _audio = audio;
        _ui = SynchronizationContext.Current ?? new SynchronizationContext();
        _audio.LevelsAvailable += OnLevels;
    }

    public Wallpaper? Current => _settings.Value.Current;
    public bool Paused => _settings.Value.Paused;
    public IReadOnlyList<WallpaperSurface> Surfaces => _surfaces;
    public IReadOnlyList<Wallpaper> Recents => _settings.Value.Recents;

    /// <param name="target">"all" / null for every monitor, or a monitor device name.</param>
    public void Apply(Wallpaper wallpaper, string? target)
    {
        var s = _settings.Value;
        if (string.IsNullOrEmpty(target) || target == "all")
        {
            s.Current = wallpaper;
            s.Monitors.Clear();
        }
        else
        {
            s.Monitors[target] = wallpaper;
        }

        s.Paused = false;
        PushRecent(wallpaper);
        _settings.Save();
        EvaluateAll(force: true);
    }

    public void RestoreOnStartup() => EvaluateAll(force: true);

    /// <summary>Apply the next playlist entry (rotation timer + "next" hotkey).</summary>
    public void ApplyPlaylistNext()
    {
        var playlist = _settings.Value.Playlist;
        if (playlist.Count == 0) return;

        _playlistIndex = _settings.Value.Shuffle
            ? _rng.Next(playlist.Count)
            : (_playlistIndex + 1) % playlist.Count;

        Apply(playlist[_playlistIndex], "all");
    }

    public void ToggleMute()
    {
        _settings.Value.MuteAudio = !_settings.Value.MuteAudio;
        _settings.Save();
        Reapply();
    }

    private void PushRecent(Wallpaper wallpaper)
    {
        var recents = _settings.Value.Recents;
        recents.RemoveAll(w => w.Id == wallpaper.Id);
        recents.Insert(0, wallpaper);
        if (recents.Count > 6) recents.RemoveRange(6, recents.Count - 6);
    }

    public bool TogglePause()
    {
        _settings.Value.Paused = !_settings.Value.Paused;
        _settings.Save();
        EvaluateAll(force: false);
        return _settings.Value.Paused;
    }

    public void SetAutoSuspend(bool value)
    {
        if (_autoSuspend == value) return;
        _autoSuspend = value;
        EvaluateAll(force: false);
    }

    /// <summary>Re-render (e.g. after a mute-audio or FPS-cap change).</summary>
    public void Reapply() => EvaluateAll(force: true);

    /// <summary>Leave a monitor untouched (or restore it).</summary>
    public void SetMonitorDisabled(string deviceName, bool disabled)
    {
        var set = _settings.Value.DisabledMonitors;
        if (disabled)
        {
            if (!set.Contains(deviceName)) set.Add(deviceName);
        }
        else
        {
            set.RemoveAll(d => d == deviceName);
        }
        _settings.Save();
        EvaluateAll(force: true);
    }

    private void EvaluateAll(bool force)
    {
        var s = _settings.Value;
        var active = !s.Paused && !_autoSuspend;

        foreach (var surface in _surfaces)
        {
            // "Leave untouched" wins over everything: hide so the real desktop shows.
            if (s.DisabledMonitors.Contains(surface.DeviceName))
            {
                surface.Disable();
                continue;
            }

            var wallpaper = s.Monitors.GetValueOrDefault(surface.DeviceName) ?? s.Current;
            if (active && wallpaper is not null)
                surface.Render(wallpaper, s.MuteAudio, s.FpsCap, force);
            else
                surface.Clear();
        }

        UpdateAudio();
    }

    private void UpdateAudio()
    {
        var want = _surfaces.Any(s => s.IsAudioShaderShowing);
        if (want && !_audio.IsRunning) _audio.Start();
        else if (!want && _audio.IsRunning) _audio.Stop();
    }

    private void OnLevels(AudioLevels levels)
    {
        // Marshal from the capture thread to the UI thread (WebView2 affinity).
        _ui.Post(_ =>
        {
            var json = JsonSerializer.Serialize(
                new { bass = levels.Bass, mid = levels.Mid, treble = levels.Treble, level = levels.Level },
                Json.Web);
            foreach (var surface in _surfaces) surface.PushAudio(json);
        }, null);
    }
}
