using System.Security.Cryptography;
using System.Text;

namespace WebDesk;

/// <summary>Scans the user's designated library folder for usable wallpapers.</summary>
internal sealed class LocalLibraryService
{
    private static readonly Dictionary<string, WallpaperType> Extensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".html"] = WallpaperType.Html,
            [".htm"] = WallpaperType.Html,
            [".mp4"] = WallpaperType.Video,
            [".webm"] = WallpaperType.Video,
            [".png"] = WallpaperType.Image,
            [".jpg"] = WallpaperType.Image,
            [".jpeg"] = WallpaperType.Image,
            [".gif"] = WallpaperType.Image,
            [".webp"] = WallpaperType.Image,
            [".bmp"] = WallpaperType.Image,
        };

    public List<Wallpaper> Scan(string? folder)
    {
        var list = new List<Wallpaper>();
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            return list;

        foreach (var path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
        {
            if (!Extensions.TryGetValue(Path.GetExtension(path), out var type))
                continue;

            var rel = Path.GetRelativePath(folder, path).Replace('\\', '/');
            var encoded = string.Join('/', rel.Split('/').Select(Uri.EscapeDataString));

            list.Add(new Wallpaper
            {
                Id = "local:" + Hash(path),
                Title = Path.GetFileNameWithoutExtension(path),
                Type = type,
                Source = path,
                Origin = "local",
                // Images can be previewed via the library virtual host; others get a placeholder.
                Thumbnail = type == WallpaperType.Image ? $"https://library.local/{encoded}" : null,
            });
        }

        return list;
    }

    private static string Hash(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..12].ToLowerInvariant();
    }
}
