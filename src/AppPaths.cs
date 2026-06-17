namespace WebDesk;

/// <summary>Centralized on-disk locations. Data in %APPDATA%, caches in %LOCALAPPDATA%.</summary>
internal static class AppPaths
{
    public static string DataRoot { get; }
    public static string LocalRoot { get; }

    public static string SettingsFile => Path.Combine(DataRoot, "settings.json");
    public static string LibraryFile => Path.Combine(DataRoot, "library.json");
    public static string CatalogCache => Path.Combine(DataRoot, "catalog-cache.json");

    public static string UiFolder => Path.Combine(LocalRoot, "ui");
    public static string WebViewUi => Path.Combine(LocalRoot, "WebView2-ui");
    public static string WebViewWallpaper => Path.Combine(LocalRoot, "WebView2-wp");
    public static string WebViewBrowser => Path.Combine(LocalRoot, "WebView2-browser");

    static AppPaths()
    {
        DataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebDesk");
        LocalRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebDesk");
        Directory.CreateDirectory(DataRoot);
        Directory.CreateDirectory(LocalRoot);
    }
}
