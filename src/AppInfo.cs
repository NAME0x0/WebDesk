namespace WebDesk;

/// <summary>Static app identity and remote endpoints.</summary>
internal static class AppInfo
{
    public const string Name = "WebDesk";
    public const string Version = "2.1.0";
    public const string Repo = "NAME0x0/WebDesk";

    // Free, server-less Discover catalog: a JSON file in the repo. Community
    // adds wallpapers via pull request; the app fetches it at runtime.
    public const string CatalogUrl =
        "https://raw.githubusercontent.com/NAME0x0/WebDesk/main/catalog/catalog.json";
}
