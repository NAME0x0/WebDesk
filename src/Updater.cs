using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebDesk;

internal sealed record UpdateInfo(string Version, string DownloadUrl, string Changelog);

/// <summary>
/// GitHub release self-updater. Unlike the Python original (which tried to
/// overwrite a running exe and never restarted cleanly), this renames the
/// running binary, swaps in the new one, and relaunches.
/// </summary>
internal static class Updater
{
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WebDesk", "1.0"));
        c.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return c;
    }

    public static async Task<UpdateInfo?> CheckAsync(string repo, string currentVersion)
    {
        try
        {
            var url = $"https://api.github.com/repos/{repo}/releases/latest";
            using var resp = await Http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            var tag = root.GetProperty("tag_name").GetString() ?? "";
            var body = root.TryGetProperty("body", out var b) ? b.GetString() ?? "" : "";

            string? asset = null;
            if (root.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
                asset = assets[0].GetProperty("browser_download_url").GetString();
            if (asset is null) return null;

            return IsNewer(tag, currentVersion) ? new UpdateInfo(tag, asset, body) : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsNewer(string tag, string current)
    {
        static Version Parse(string s)
        {
            s = s.TrimStart('v', 'V').Trim();
            return Version.TryParse(s, out var v) ? v : new Version(0, 0);
        }

        return Parse(tag) > Parse(current);
    }

    public static async Task ApplyAsync(UpdateInfo update)
    {
        var exe = Environment.ProcessPath
                  ?? throw new InvalidOperationException("Cannot locate the current executable.");
        var newExe = exe + ".new";
        var oldExe = exe + ".old";

        using (var resp = await Http.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            resp.EnsureSuccessStatusCode();
            await using var fs = File.Create(newExe);
            await resp.Content.CopyToAsync(fs);
        }

        // Windows won't overwrite a running exe, but it will rename it.
        if (File.Exists(oldExe)) File.Delete(oldExe);
        File.Move(exe, oldExe);
        File.Move(newExe, exe);

        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
    }

    public static void CleanupOldVersion()
    {
        try
        {
            if (Environment.ProcessPath is not { } exe) return;
            var oldExe = exe + ".old";
            if (File.Exists(oldExe)) File.Delete(oldExe);
        }
        catch
        {
            // The previous process may still hold a lock; it'll clear next run.
        }
    }
}
