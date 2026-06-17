using System.Windows.Forms;

namespace WebDesk;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Remove the leftover ".old" binary from a previous self-update.
        Updater.CleanupOldVersion();

        Application.Run(new WebDeskContext());
    }
}
