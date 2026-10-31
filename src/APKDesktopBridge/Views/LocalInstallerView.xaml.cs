using System.IO;
using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Views;

public partial class LocalInstallerView : UserControl
{
    private readonly LocalAPKService _localApkService = new();
    private readonly ADBService _adbService = new();
    private readonly EmulatorService _emulatorService = new();
    private APKInfo? _currentApk;

    public LocalInstallerView(string? initialApkPath = null)
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(initialApkPath))
        {
            _ = LoadFileAsync(initialApkPath);
        }
    }

    private async void OnBrowseFilesClick(object sender, RoutedEventArgs e)
    {
        string? file = _localApkService.PromptForLocalApkFile();
        if (!string.IsNullOrEmpty(file))
        {
            await LoadFileAsync(file);
        }
    }

    private void OnApkDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void OnApkFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Path.GetExtension(files[0]).Equals(".apk", StringComparison.OrdinalIgnoreCase))
            {
                await LoadFileAsync(files[0]);
            }
            else
            {
                MessageBox.Show("Please drop a valid .apk file (use 'Emulator File Transfer' in the sidebar for non-APK files).", "Not an APK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private async Task LoadFileAsync(string path)
    {
        var (isValid, error, info, meta) = await _localApkService.InspectLocalApkAsync(path);
        if (!isValid || info == null || meta == null)
        {
            MessageBox.Show(error, "Invalid APK", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _currentApk = info;
        TxtAppName.Text = $"{info.Name} ({info.SizeBytes / (1024.0 * 1024.0):F2} MB)";
        TxtFilePath.Text = $"Local Path: {info.LocalPath}";
        TxtSha256.Text = $"SHA-256: {info.Sha256}";
        TxtArchWarning.Text = meta.SupportsX86_64
            ? "Architecture: Compatible with x86_64 Emulator"
            : "Warning: APK contains ARM64 native libraries only.";

        MetadataCard.Visibility = Visibility.Visible;
        TxtInstallStatus.Text = "Ready to install.";
    }

    private async void OnInstallLocalApkClick(object sender, RoutedEventArgs e)
    {
        if (_currentApk == null) return;

        // If emulator is not running, automatically boot it for the user!
        if (!await _adbService.IsDeviceConnectedAsync())
        {
            var settings = SettingsService.Load();
            TxtInstallStatus.Text = "Starting Android Emulator automatically...";
            if (!_emulatorService.LaunchEmulator(settings.Runtime))
            {
                MessageBox.Show("Emulator is not provisioned yet. Please run 'Auto-Provision SDK' in Runtime & Display first.", "Runtime Needed");
                return;
            }

            // Wait up to 45 seconds for ADB connection
            for (int i = 0; i < 15; i++)
            {
                await Task.Delay(3000);
                if (await _adbService.IsDeviceConnectedAsync()) break;
            }
        }

        TxtInstallStatus.Text = "Installing APK via ADB...";
        var (success, message) = await _adbService.InstallApkAsync(_currentApk.LocalPath);
        TxtInstallStatus.Text = message;

        if (success)
        {
            var settings = SettingsService.Load();
            settings.InstalledApps.RemoveAll(a => a.Name == _currentApk.Name);
            settings.InstalledApps.Add(_currentApk);
            SettingsService.Save(settings);

            await _adbService.SetLandscapeModeAsync();
        }
    }
}
