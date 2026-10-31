  using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Views;

public partial class LibraryView : UserControl
{
    private readonly ADBService _adb = new();

    public LibraryView()
    {
        InitializeComponent();
        RefreshList();
    }

    private void RefreshList()
    {
        var settings = SettingsService.Load();
        LstInstalled.ItemsSource = settings.InstalledApps;
    }

    private async void OnLaunchClick(object sender, RoutedEventArgs e)
    {
        if (LstInstalled.SelectedItem is not APKInfo app) return;
        await _adb.SetLandscapeModeAsync();
        await _adb.LaunchPackageAsync(app.PackageName);
    }

    private async void OnUninstallClick(object sender, RoutedEventArgs e)
    {
        if (LstInstalled.SelectedItem is not APKInfo app) return;
        if (await _adb.UninstallPackageAsync(app.PackageName))
        {
            var settings = SettingsService.Load();
            settings.InstalledApps.RemoveAll(a => a.PackageName == app.PackageName);
            SettingsService.Save(settings);
            RefreshList();
        }
    }
}
