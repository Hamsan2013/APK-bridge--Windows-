using System.Windows;
using System.Windows.Threading;
using APKDesktopBridge.Core;
using APKDesktopBridge.Views;

namespace APKDesktopBridge;

public partial class MainWindow : Window
{
    private readonly ADBService _adbService = new();
    private readonly DispatcherTimer _adbTimer = new();

    public MainWindow()
    {
        InitializeComponent();

        // If user double-clicked an APK in Windows Explorer, jump straight to Local Installer
        if (!string.IsNullOrEmpty(App.StartupApkPath))
        {
            MainContent.Content = new LocalInstallerView(App.StartupApkPath);
        }
        else
        {
            MainContent.Content = new HomeView(this);
        }

        // Poll ADB connection every 3 seconds so sidebar stays accurate
        _adbTimer.Interval = TimeSpan.FromSeconds(3);
        _adbTimer.Tick += async (s, e) => await UpdateStatusAsync();
        _adbTimer.Start();

        _ = UpdateStatusAsync();
    }

    public async Task UpdateStatusAsync()
    {
        bool connected = await _adbService.IsDeviceConnectedAsync();
        TxtAdbStatus.Text = connected ? "ADB: Connected" : "ADB: No Emulator Running";
        TxtAdbStatus.Foreground = connected
            ? System.Windows.Media.Brushes.MediumSpringGreen
            : System.Windows.Media.Brushes.Orange;
    }

    public void NavigateToLocal() => MainContent.Content = new LocalInstallerView();
    public void NavigateToFileTransfer() => MainContent.Content = new FileTransferView();
    public void NavigateToGitHub() => MainContent.Content = new GitHubView();
    public void NavigateToDirect() => MainContent.Content = new DirectURLView();

    private void NavHome_Click(object sender, RoutedEventArgs e) => MainContent.Content = new HomeView(this);
    private void NavLocal_Click(object sender, RoutedEventArgs e) => NavigateToLocal();
    private void NavFileTransfer_Click(object sender, RoutedEventArgs e) => NavigateToFileTransfer();
    private void NavGitHub_Click(object sender, RoutedEventArgs e) => NavigateToGitHub();
    private void NavDirect_Click(object sender, RoutedEventArgs e) => NavigateToDirect();
    private void NavLibrary_Click(object sender, RoutedEventArgs e) => MainContent.Content = new LibraryView();
    private void NavRuntime_Click(object sender, RoutedEventArgs e) => MainContent.Content = new RuntimeView();
    private void NavSettings_Click(object sender, RoutedEventArgs e) => MainContent.Content = new SettingsView();
}
