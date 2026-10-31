  using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class GitHubService
{
    private static readonly HttpClient Http = new();

    public GitHubService()
    {
        if (!Http.DefaultRequestHeaders.UserAgent.Any())
        {
            Http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("APKDesktopBridge", "1.0.0"));
        }
    }

    public async Task<List<GitHubAssetItem>> FindApkAssetsAsync(string owner, string repo, string? token = null)
    {
        var results = new List<GitHubAssetItem>();
        string releasesUrl = $"https://api.github.com/repos/{owner}/{repo}/releases";

        using var request = new HttpRequestMessage(HttpMethod.Get, releasesUrl);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await Http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return results;

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        foreach (var release in doc.RootElement.EnumerateArray())
        {
            if (!release.TryGetProperty("assets", out var assets)) continue;
            foreach (var asset in assets.EnumerateArray())
            {
                string name = asset.GetProperty("name").GetString() ?? "";
                if (name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new GitHubAssetItem
                    {
                        FileName = name,
                        DownloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "",
                        Size = asset.GetProperty("size").GetInt64(),
                        SourceType = "GitHub Release"
                    });
                }
            }
        }

        return results;
    }
}
