  using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Views;

public partial class GitHubView : UserControl
{
    private readonly GitHubService _gitHub = new();
    private readonly APKDownloadService _downloader = new();
    private readonly ADBService _adb = new();

    public GitHubView()
    {
        InitializeComponent();
    }

    private async void OnSearchGitHubClick(object sender, RoutedEventArgs e)
    {
        TxtStatus.Text = "Querying GitHub Releases API...";
        var assets = await _gitHub.FindApkAssetsAsync(TxtOwner.Text.Trim(), TxtRepo.Text.Trim());
        LstAssets.ItemsSource = assets;
        TxtStatus.Text = $"Found {assets.Count} APK asset(s).";
    }

    private async void OnDownloadAssetClick(object sender, RoutedEventArgs e)
    {
        if (LstAssets.SelectedItem is not GitHubAssetItem asset) return;

        try
        {
            TxtStatus.Text = $"Downloading {asset.FileName}...";
            string localPath = await _downloader.DownloadApkAsync(asset.DownloadUrl);
            TxtStatus.Text = "Installing APK via ADB...";
            var (success, msg) = await _adb.InstallApkAsync(localPath);
            TxtStatus.Text = msg;
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"Error: {ex.Message}";
        }
    }
}
