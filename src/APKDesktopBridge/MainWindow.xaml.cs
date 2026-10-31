  using System.Windows;
using APKDesktopBridge.Core;
using APKDesktopBridge.Views;

namespace APKDesktopBridge;

public partial class MainWindow : Window
{
    private readonly ADBService _adbService = new();

    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new HomeView(this);
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
    public void NavigateToGitHub() => MainContent.Content = new GitHubView();
    public void NavigateToDirect() => MainContent.Content = new DirectURLView();

    private void NavHome_Click(object sender, RoutedEventArgs e) => MainContent.Content = new HomeView(this);
    private void NavLocal_Click(object sender, RoutedEventArgs e) => NavigateToLocal();
    private void NavGitHub_Click(object sender, RoutedEventArgs e) => NavigateToGitHub();
    private void NavDirect_Click(object sender, RoutedEventArgs e) => NavigateToDirect();
    private void NavLibrary_Click(object sender, RoutedEventArgs e) => MainContent.Content = new LibraryView();
    private void NavRuntime_Click(object sender, RoutedEventArgs e) => MainContent.Content = new RuntimeView();
    private void NavSettings_Click(object sender, RoutedEventArgs e) => MainContent.Content = new SettingsView();
}
