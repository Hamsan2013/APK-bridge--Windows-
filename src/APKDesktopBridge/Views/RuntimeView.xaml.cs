  using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;

namespace APKDesktopBridge.Views;

public partial class RuntimeView : UserControl
{
    private readonly RuntimeManager _runtime = new();
    private readonly EmulatorService _emulator = new();

    public RuntimeView()
    {
        InitializeComponent();
        TxtRuntimeState.Text = _runtime.IsPlatformToolsInstalled()
            ? "Android Platform-Tools (ADB) detected in LocalAppData."
            : "Android SDK not yet installed. Click 'Download Android SDK Tools' to provision.";
    }

    private async void OnSetupSdkClick(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "Do you accept the Android SDK License Terms to download the Android Command Line Tools?",
            "Android SDK License Acceptance",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (confirm != MessageBoxResult.Yes) return;

        var progress = new Progress<string>(msg => TxtRuntimeState.Text = msg);
        await _runtime.DownloadCommandLineToolsAsync(progress);
    }

    private void OnLaunchEmulatorClick(object sender, RoutedEventArgs e)
    {
        var settings = SettingsService.Load();
        bool started = _emulator.LaunchEmulator(settings.Runtime);
        if (!started)
        {
            MessageBox.Show("Emulator binary not found yet. Finish SDK provisioning first.", "Emulator Not Found");
        }
    }
}
