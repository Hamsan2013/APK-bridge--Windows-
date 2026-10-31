  using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace APKDesktopBridge.Core;

public class RuntimeManager
{
    private const string CmdLineToolsUrl = "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
    private static readonly HttpClient Http = new();

    public bool IsPlatformToolsInstalled()
    {
        string adbPath = Path.Combine(SettingsService.SdkDir, "platform-tools", "adb.exe");
        return File.Exists(adbPath);
    }

    public async Task DownloadCommandLineToolsAsync(IProgress<string>? status = null)
    {
        status?.Report("Downloading official Android Command Line Tools...");
        string zipPath = Path.Combine(SettingsService.DownloadsDir, "cmdline-tools.zip");

        using var response = await Http.GetAsync(CmdLineToolsUrl);
        response.EnsureSuccessStatusCode();
        await using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await response.Content.CopyToAsync(fs);
        }

        status?.Report("Extracting Android SDK Command Line Tools...");
        string extractDir = Path.Combine(SettingsService.SdkDir, "cmdline-tools", "latest");
        Directory.CreateDirectory(extractDir);
        ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);

        status?.Report("Command line tools staged in LocalAppData.");
    }
}
