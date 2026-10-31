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
    private APKInfo? _currentApk;

    public LocalInstallerView()
    {
        InitializeComponent();
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
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Path.GetExtension(files[0]).Equals(".apk", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnApkFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                await LoadFileAsync(files[0]);
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
            : "Warning: APK contains ARM64 native libraries only. Verify ABI translation support on your system image.";

        MetadataCard.Visibility = Visibility.Visible;
        TxtInstallStatus.Text = "Ready to install.";
    }

    private async void OnInstallLocalApkClick(object sender, RoutedEventArgs e)
    {
        if (_currentApk == null) return;

        var confirm = MessageBox.Show(
            $"Install '{_currentApk.Name}' onto the Android Emulator?",
            "Confirm APK Installation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        TxtInstallStatus.Text = "Installing via ADB...";
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
