using System.Text.Json;

namespace WebDesk;

/// <summary>The user's saved/starred wallpaper collection, persisted as JSON.</summary>
internal sealed class LibraryStore
{
    private readonly List<Wallpaper> _items;

    public LibraryStore() => _items = Load();

    private static List<Wallpaper> Load()
    {
        try
        {
            return JsonSerializer.Deserialize<List<Wallpaper>>(
                File.ReadAllText(AppPaths.LibraryFile), Json.Web) ?? new List<Wallpaper>();
        }
        catch
        {
            return new List<Wallpaper>();
        }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(AppPaths.LibraryFile, JsonSerializer.Serialize(_items, Json.Web));
        }
        catch
        {
            // Ignore transient write failures.
        }
    }

    public IReadOnlyList<Wallpaper> All => _items;

    public bool Contains(string id) => _items.Any(w => w.Id == id);

    public void Add(Wallpaper wallpaper)
    {
        if (Contains(wallpaper.Id)) return;
        _items.Add(wallpaper);
        Save();
    }

    public void Remove(string id)
    {
        _items.RemoveAll(w => w.Id == id);
        Save();
    }

    public bool ToggleStar(string id)
    {
        var item = _items.FirstOrDefault(w => w.Id == id);
        if (item is null) return false;
        item.Starred = !item.Starred;
        Save();
        return item.Starred;
    }
}
