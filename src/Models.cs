using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebDesk;

internal enum WallpaperType { Url, Html, Video, Image, Shader }

/// <summary>A wallpaper from any source (catalog, local folder, or custom URL).</summary>
internal sealed class Wallpaper
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public WallpaperType Type { get; set; } = WallpaperType.Url;

    /// <summary>Remote URL, or absolute local file path for local wallpapers.</summary>
    public string Source { get; set; } = "";

    public string? Thumbnail { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? Author { get; set; }

    /// <summary>"catalog" | "local" | "custom".</summary>
    public string Origin { get; set; } = "catalog";

    public bool Starred { get; set; }

    /// <summary>For shader wallpapers: drive uniforms from system audio.</summary>
    public bool Audio { get; set; }
}

internal sealed class CatalogFile
{
    public int Version { get; set; }
    public List<Wallpaper> Wallpapers { get; set; } = new();
}

/// <summary>Shared JSON options: camelCase, enums as strings, skip nulls.</summary>
internal static class Json
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
