  using System.IO;
using System.Text.Json;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public static class SettingsService
{
    public static string RootDataDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "APK Desktop Bridge");

    public static string DownloadsDir => Path.Combine(RootDataDir, "Downloads");
    public static string SdkDir => Path.Combine(RootDataDir, "AndroidSDK");
    public static string AvdDir => Path.Combine(RootDataDir, "VirtualDevices");
    public static string LogsDir => Path.Combine(RootDataDir, "Logs");
    public static string SettingsFilePath => Path.Combine(RootDataDir, "Settings", "appsettings.json");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(DownloadsDir);
        Directory.CreateDirectory(SdkDir);
        Directory.CreateDirectory(AvdDir);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
    }

    public static AppSettings Load()
    {
        EnsureDirectoriesExist();
        if (!File.Exists(SettingsFilePath))
        {
            var defaults = new AppSettings
            {
                BaseDataPath = RootDataDir,
                SdkPath = SdkDir,
                DownloadsPath = DownloadsDir
            };
            Save(defaults);
            return defaults;
        }

        string json = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        EnsureDirectoriesExist();
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings, options));
    }
}
