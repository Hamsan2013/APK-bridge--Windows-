  using System.IO;
using System.Net.Http;

namespace APKDesktopBridge.Core;

public class APKDownloadService
{
    private static readonly HttpClient Http = new();

    public async Task<string> DownloadApkAsync(string url, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("Only valid HTTPS URLs are permitted for security.");

        string fileName = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
            fileName = $"downloaded_{DateTime.UtcNow:yyyyMMdd_HHmmss}.apk";

        string targetPath = Path.Combine(SettingsService.DownloadsDir, fileName);

        using var response = await Http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        byte[] buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            totalRead += bytesRead;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                progress?.Report((double)totalRead / totalBytes.Value * 100.0);
            }
        }

        return targetPath;
    }
}
