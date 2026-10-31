  using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;

namespace APKDesktopBridge.Views;

public partial class DirectURLView : UserControl
{
    private readonly APKDownloadService _downloader = new();
    private readonly APKValidationService _validator = new();
    private readonly ADBService _adb = new();

    public DirectURLView()
    {
        InitializeComponent();
    }

    private async void OnDownloadDirectClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var progress = new Progress<double>(p => DownloadProgress.Value = p);
            TxtDirectStatus.Text = "Downloading APK...";
            string path = await _downloader.DownloadApkAsync(TxtUrl.Text.Trim(), progress);

            if (!_validator.IsValidApkFormat(path))
            {
                TxtDirectStatus.Text = "Downloaded file is not a valid APK package.";
                return;
            }

            string sha = await _validator.ComputeSha256Async(path);
            var confirm = MessageBox.Show($"SHA-256: {sha}\n\nProceed with ADB installation?", "Verify APK", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                var (_, msg) = await _adb.InstallApkAsync(path);
                TxtDirectStatus.Text = msg;
            }
        }
        catch (Exception ex)
        {
            TxtDirectStatus.Text = $"Download failed: {ex.Message}";
        }
    }
}
