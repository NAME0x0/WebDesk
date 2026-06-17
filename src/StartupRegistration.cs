using Microsoft.Win32;

namespace WebDesk;

/// <summary>Registers WebDesk to launch at login via the HKCU Run key.</summary>
internal static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static void Set(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key is null) return;

            if (enable && Environment.ProcessPath is { } exe)
                key.SetValue(AppInfo.Name, $"\"{exe}\" --tray"); // launch to tray at login
            else
                key.DeleteValue(AppInfo.Name, throwOnMissingValue: false);
        }
        catch
        {
            // Non-fatal: startup registration is best-effort.
        }
    }

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(AppInfo.Name) is not null;
        }
        catch
        {
            return false;
        }
    }
}
