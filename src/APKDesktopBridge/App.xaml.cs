  using System.Windows;
using APKDesktopBridge.Core;

namespace APKDesktopBridge;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SettingsService.EnsureDirectoriesExist();
    }
}
