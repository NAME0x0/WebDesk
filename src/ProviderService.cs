using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebDesk;

/// <summary>
/// Searchable wallpaper providers with official/free APIs. Calls run in C# to
/// avoid browser CORS. Wallhaven needs no API key for SFW content.
/// </summary>
internal sealed class ProviderService
{
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
        c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WebDesk", "2.0"));
        return c;
    }

    public Task<(List<Wallpaper> Items, int LastPage)> SearchAsync(string provider, string query, int page)
        => provider switch
        {
            "wallhaven" => Wallhaven(query, page),
            _ => Task.FromResult((new List<Wallpaper>(), 1)),
        };

    private static async Task<(List<Wallpaper>, int)> Wallhaven(string query, int page)
    {
        var items = new List<Wallpaper>();
        var lastPage = 1;
        var sorting = string.IsNullOrWhiteSpace(query) ? "toplist" : "relevance";
        var url = "https://wallhaven.cc/api/v1/search?q=" + Uri.EscapeDataString(query ?? "") +
                  $"&page={Math.Max(1, page)}&purity=100&categories=111&sorting={sorting}";

        try
        {
            using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));
            var root = doc.RootElement;

            if (root.TryGetProperty("meta", out var meta) && meta.TryGetProperty("last_page", out var lp))
                lastPage = lp.GetInt32();

            foreach (var d in root.GetProperty("data").EnumerateArray())
            {
                var id = d.GetProperty("id").GetString()!;
                var path = d.GetProperty("path").GetString()!;
                var thumb = d.GetProperty("thumbs").GetProperty("small").GetString();
                var res = d.TryGetProperty("resolution", out var r) ? r.GetString() : null;

                items.Add(new Wallpaper
                {
                    Id = "wallhaven:" + id,
                    Title = res ?? id,
                    Type = WallpaperType.Image,
                    Source = path,
                    Thumbnail = thumb,
                    Author = "Wallhaven",
                    Origin = "wallhaven",
                });
            }
        }
        catch
        {
            // Offline / rate-limited: return whatever we have.
        }

        return (items, lastPage);
    }
}
