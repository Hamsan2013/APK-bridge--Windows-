using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;

namespace APKDesktopBridge.Views;

public partial class RuntimeView : UserControl
{
    private readonly RuntimeManager _runtime = new();
    private readonly EmulatorService _emulator = new();
    private readonly DisplayManager _display = new();

    public RuntimeView()
    {
        InitializeComponent();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var settings = SettingsService.Load();
        bool ready = _runtime.IsFullRuntimeReady(settings.Runtime);
        TxtRuntimeState.Text = ready
            ? $"Runtime Ready: AVD '{settings.Runtime.AvdName}' configured ({settings.Runtime.Width}×{settings.Runtime.Height} Landscape)."
            : "Android Runtime or Virtual Device not yet provisioned.";
    }

    private async void OnSetupSdkClick(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            "This will download the official Android SDK, Platform-Tools, Emulator, and Android 14 x86_64 system image (~1.5 GB) into LocalAppData.\n\nDo you accept the Android SDK License Agreement?",
            "Accept Android SDK License",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        BtnProvision.IsEnabled = false;
        TxtConsoleLog.Clear();

        var settings = SettingsService.Load();
        string preset = CmbResolution.SelectedIndex == 1 ? "1080p" : "720p";
        await _display.ApplyPresetAsync(settings.Runtime, preset);
        SettingsService.Save(settings);

        var progress = new Progress<string>(line =>
        {
            TxtRuntimeState.Text = line;
            TxtConsoleLog.AppendText(line + Environment.NewLine);
            TxtConsoleLog.ScrollToEnd();
        });

        try
        {
            await _runtime.ProvisionCompleteRuntimeAsync(settings.Runtime, progress);
            RefreshStatus();
        }
        catch (Exception ex)
        {
            TxtConsoleLog.AppendText($"[ERROR] {ex.Message}{Environment.NewLine}");
            MessageBox.Show(ex.Message, "Provisioning Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BtnProvision.IsEnabled = true;
        }
    }

    private async void OnLaunchEmulatorClick(object sender, RoutedEventArgs e)
    {
        var settings = SettingsService.Load();
        string preset = CmbResolution.SelectedIndex == 1 ? "1080p" : "720p";
        await _display.ApplyPresetAsync(settings.Runtime, preset);
        SettingsService.Save(settings);

        bool started = _emulator.LaunchEmulator(settings.Runtime);
        if (!started)
        {
            MessageBox.Show(
                "Emulator executable not found. Click 'Auto-Provision SDK & Create AVD' first.",
                "Emulator Missing",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
