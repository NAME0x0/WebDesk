using System.Reflection;

namespace WebDesk;

/// <summary>
/// Extracts the embedded SPA (wwwroot) to disk so WebView2 can serve it via a
/// virtual-host folder mapping. Keeps the build a single self-contained exe.
/// </summary>
internal static class AssetExtractor
{
    private const string Prefix = "WebDesk.wwwroot.";

    public static string ExtractUi()
    {
        var dir = AppPaths.UiFolder;
        Directory.CreateDirectory(dir);

        var asm = Assembly.GetExecutingAssembly();
        foreach (var name in asm.GetManifestResourceNames())
        {
            if (!name.StartsWith(Prefix, StringComparison.Ordinal)) continue;

            var fileName = name[Prefix.Length..]; // wwwroot is flat: "index.html", "app.js", …
            using var stream = asm.GetManifestResourceStream(name);
            if (stream is null) continue;

            using var file = File.Create(Path.Combine(dir, fileName));
            stream.CopyTo(file);
        }

        return dir;
    }
}
