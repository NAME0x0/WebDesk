using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WebDesk;

/// <summary>
/// Watches for conditions where the wallpaper should auto-pause to save power:
/// a fullscreen foreground app (game/video) or running on battery.
/// </summary>
internal sealed class PerfGuard : IDisposable
{
    private readonly WallpaperController _controller;
    private readonly SettingsStore _settings;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 2000 };

    public PerfGuard(WallpaperController controller, SettingsStore settings)
    {
        _controller = controller;
        _settings = settings;
        _timer.Tick += (_, _) => Check();
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    public void Start()
    {
        _timer.Start();
        Check();
    }

    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e) => Check();

    private void Check()
    {
        var s = _settings.Value;
        var suspend =
            (s.PauseOnFullscreen && IsForegroundFullscreen()) ||
            (s.PauseOnBattery && OnBattery());
        _controller.SetAutoSuspend(suspend);
    }

    private static bool OnBattery()
        => SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline;

    private static bool IsForegroundFullscreen()
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        if (hWnd == IntPtr.Zero) return false;

        var sb = new StringBuilder(64);
        NativeMethods.GetClassName(hWnd, sb, sb.Capacity);
        var cls = sb.ToString();
        if (cls is "Progman" or "WorkerW" or "Shell_TrayWnd") return false; // desktop / taskbar

        if (!NativeMethods.GetWindowRect(hWnd, out var r)) return false;

        var monitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
        var mi = new NativeMethods.MONITORINFO { cbSize = Marshal.SizeOf<NativeMethods.MONITORINFO>() };
        if (!NativeMethods.GetMonitorInfo(monitor, ref mi)) return false;

        return r.Left <= mi.rcMonitor.Left && r.Top <= mi.rcMonitor.Top &&
               r.Right >= mi.rcMonitor.Right && r.Bottom >= mi.rcMonitor.Bottom;
    }

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _timer.Dispose();
    }
}
