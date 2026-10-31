using System.IO;
using System.Windows;
using APKDesktopBridge.Core;

namespace APKDesktopBridge;

public partial class App : Application
{
    public static string? StartupApkPath { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SettingsService.EnsureDirectoriesExist();

        // Check if Windows launched the app by double-clicking an .apk file
        if (e.Args.Length > 0 && File.Exists(e.Args[0]) && e.Args[0].EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
        {
            StartupApkPath = e.Args[0];
        }
    }
}
