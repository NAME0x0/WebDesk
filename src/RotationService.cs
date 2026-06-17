using System.Windows.Forms;

namespace WebDesk;

/// <summary>Cycles the playlist on an interval (0 minutes = off).</summary>
internal sealed class RotationService : IDisposable
{
    private readonly WallpaperController _controller;
    private readonly SettingsStore _settings;
    private readonly System.Windows.Forms.Timer _timer = new();

    public RotationService(WallpaperController controller, SettingsStore settings)
    {
        _controller = controller;
        _settings = settings;
        _timer.Tick += (_, _) => _controller.ApplyPlaylistNext();
    }

    public void Reconfigure()
    {
        var minutes = _settings.Value.RotateMinutes;
        _timer.Stop();
        if (minutes > 0)
        {
            _timer.Interval = minutes * 60_000;
            _timer.Start();
        }
    }

    public void Dispose() => _timer.Dispose();
}
