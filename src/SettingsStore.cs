using System.Text.Json;

namespace WebDesk;

internal sealed class AppSettings
{
    public string? LibraryFolder { get; set; }
    public bool RunAtStartup { get; set; }
    public bool AutoCheckUpdates { get; set; } = true;
    public bool MuteAudio { get; set; } = true;
    public string Theme { get; set; } = "dark";
    public string Accent { get; set; } = "#6c8cff";

    // Playlist / rotation
    public List<Wallpaper> Playlist { get; set; } = new();
    public int RotateMinutes { get; set; } // 0 = off
    public bool Shuffle { get; set; }
    public List<Wallpaper> Recents { get; set; } = new();

    // Performance
    public bool PauseOnFullscreen { get; set; } = true;
    public bool PauseOnBattery { get; set; }
    public int FpsCap { get; set; } // 0 = unlimited; applies to shader wallpapers

    // Not user-editable from the Settings page; managed by the controller.
    public bool Paused { get; set; }
    public Wallpaper? Current { get; set; }

    /// <summary>Per-monitor overrides, keyed by display device name.</summary>
    public Dictionary<string, Wallpaper> Monitors { get; set; } = new();

    /// <summary>Displays left untouched (real desktop + icons stay visible).</summary>
    public List<string> DisabledMonitors { get; set; } = new();
}

/// <summary>Loads/saves <see cref="AppSettings"/> as JSON.</summary>
internal sealed class SettingsStore
{
    public AppSettings Value { get; private set; }

    public SettingsStore() => Value = Load();

    private static AppSettings Load()
    {
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(
                File.ReadAllText(AppPaths.SettingsFile), Json.Web) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Replace(AppSettings settings)
    {
        Value = settings;
        Save();
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(AppPaths.SettingsFile, JsonSerializer.Serialize(Value, Json.Web));
        }
        catch
        {
            // Ignore transient write failures.
        }
    }
}
