using System.Drawing;
using System.Reflection;

namespace WebDesk;

/// <summary>Loads the application icon for the tray and dialogs.</summary>
internal static class IconLoader
{
    public static Icon AppIcon()
    {
        // Prefer the embedded icon: works in `dotnet run` and single-file publish.
        var asm = Assembly.GetExecutingAssembly();
        var name = Array.Find(asm.GetManifestResourceNames(),
            n => n.EndsWith("app.ico", StringComparison.OrdinalIgnoreCase));
        if (name is not null)
        {
            using var stream = asm.GetManifestResourceStream(name);
            if (stream is not null) return new Icon(stream);
        }

        try
        {
            if (Environment.ProcessPath is { } p && Icon.ExtractAssociatedIcon(p) is { } ico)
                return ico;
        }
        catch
        {
            // fall through to the system default
        }

        return SystemIcons.Application;
    }
}
