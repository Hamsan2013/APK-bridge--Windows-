  using System.Windows.Controls;
using APKDesktopBridge.Core;

namespace APKDesktopBridge.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        TxtRootDir.Text = SettingsService.RootDataDir;
        TxtSdkDir.Text = SettingsService.SdkDir;
        TxtDownloadsDir.Text = SettingsService.DownloadsDir;
    }
}
