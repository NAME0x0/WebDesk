using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace WebDesk;

/// <summary>
/// Fetches the Discover catalog from GitHub, caching the result. Falls back to
/// the cached copy, then to the catalog embedded in the build.
/// </summary>
internal sealed class CatalogService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(12) };

    public async Task<List<Wallpaper>> GetAsync(bool refresh)
    {
        if (!refresh && ReadCache() is { Count: > 0 } cached)
            return cached;

        try
        {
            var json = await Http.GetStringAsync(AppInfo.CatalogUrl);
            var file = JsonSerializer.Deserialize<CatalogFile>(json, Json.Web);
            if (file?.Wallpapers is { Count: > 0 } fresh)
            {
                File.WriteAllText(AppPaths.CatalogCache, json);
                return fresh;
            }
        }
        catch
        {
            // Offline or rate-limited: fall through to cache / embedded.
        }

        return ReadCache() ?? ReadEmbedded() ?? new List<Wallpaper>();
    }

    private static List<Wallpaper>? ReadCache()
    {
        try
        {
            return JsonSerializer.Deserialize<CatalogFile>(
                File.ReadAllText(AppPaths.CatalogCache), Json.Web)?.Wallpapers;
        }
        catch
        {
            return null;
        }
    }

    private static List<Wallpaper>? ReadEmbedded()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = Array.Find(asm.GetManifestResourceNames(),
                n => n.EndsWith("catalog.json", StringComparison.OrdinalIgnoreCase));
            if (name is null) return null;

            using var stream = asm.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            return JsonSerializer.Deserialize<CatalogFile>(reader.ReadToEnd(), Json.Web)?.Wallpapers;
        }
        catch
        {
            return null;
        }
    }
}
