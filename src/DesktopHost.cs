namespace WebDesk;

internal readonly record struct DesktopHostHandles(
    IntPtr Progman,
    IntPtr WorkerW,
    IntPtr ShellView,
    bool IsRaisedDesktop);

/// <summary>
/// Resolves Explorer's desktop windows for both the legacy top-level WorkerW
/// layout and the newer raised-desktop layout used by Windows 11.
/// </summary>
internal static class DesktopHost
{
    public static DesktopHostHandles? Resolve()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        if (progman == IntPtr.Zero) return null;

        var progmanStyle = NativeMethods.GetWindowLong(progman, NativeMethods.GWL_EXSTYLE);
        var isRaisedDesktop =
            (progmanStyle & NativeMethods.WS_EX_NOREDIRECTIONBITMAP) != 0;

        // Ask Progman to spawn the WorkerW wallpaper layer.
        NativeMethods.SendMessageTimeout(
            progman,
            NativeMethods.WM_SPAWN_WORKERW,
            new IntPtr(0xD),
            new IntPtr(0x1),
            0,
            1000,
            out _);

        if (isRaisedDesktop)
        {
            // Newer Windows 11 builds keep both windows as Progman children:
            // SHELLDLL_DefView (icons) above WorkerW (system background).
            var raisedShellView = NativeMethods.FindWindowEx(
                progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            var raisedWorker = NativeMethods.FindWindowEx(
                progman, IntPtr.Zero, "WorkerW", null);

            return raisedShellView != IntPtr.Zero && raisedWorker != IntPtr.Zero
                ? new DesktopHostHandles(progman, raisedWorker, raisedShellView, true)
                : null;
        }

        var worker = IntPtr.Zero;
        var shellView = IntPtr.Zero;
        NativeMethods.EnumWindows((hWnd, _) =>
        {
            // The wallpaper WorkerW is the sibling that follows the WorkerW
            // hosting the desktop icon view (SHELLDLL_DefView).
            var candidateShellView = NativeMethods.FindWindowEx(
                hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (candidateShellView == IntPtr.Zero) return true;

            var candidateWorker = NativeMethods.FindWindowEx(
                IntPtr.Zero, hWnd, "WorkerW", null);
            if (candidateWorker == IntPtr.Zero) return true;

            shellView = candidateShellView;
            worker = candidateWorker;
            return false;
        }, IntPtr.Zero);

        // Never attach directly to Progman as a fallback: without explicit
        // z-ordering that can cover Explorer's icon and context-menu layer.
        return worker != IntPtr.Zero
            ? new DesktopHostHandles(progman, worker, shellView, false)
            : null;
    }
}
