namespace WebDesk;

/// <summary>
/// Resolves the window to parent wallpaper surfaces under. Prefers the WorkerW
/// layer (sits behind desktop icons and spans every monitor); falls back to
/// Progman when WorkerW can't be located.
/// </summary>
internal static class DesktopHost
{
    public static IntPtr Resolve()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        if (progman == IntPtr.Zero) return IntPtr.Zero;

        // Ask Progman to spawn the WorkerW wallpaper layer.
        NativeMethods.SendMessageTimeout(
            progman, NativeMethods.WM_SPAWN_WORKERW, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);

        var worker = IntPtr.Zero;
        NativeMethods.EnumWindows((hWnd, _) =>
        {
            // The wallpaper WorkerW is the sibling that follows the WorkerW
            // hosting the desktop icon view (SHELLDLL_DefView).
            if (NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
                worker = NativeMethods.FindWindowEx(IntPtr.Zero, hWnd, "WorkerW", null);
            return true;
        }, IntPtr.Zero);

        return worker != IntPtr.Zero ? worker : progman;
    }
}
