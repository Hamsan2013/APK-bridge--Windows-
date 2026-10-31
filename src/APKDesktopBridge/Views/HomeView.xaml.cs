  using System.Windows;
using System.Windows.Controls;

namespace APKDesktopBridge.Views;

public partial class HomeView : UserControl
{
    private readonly MainWindow _mainWindow;

    public HomeView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private void BtnOpenLocal_Click(object sender, RoutedEventArgs e) => _mainWindow.NavigateToLocal();
    private void BtnOpenGitHub_Click(object sender, RoutedEventArgs e) => _mainWindow.NavigateToGitHub();
    private void BtnOpenDirect_Click(object sender, RoutedEventArgs e) => _mainWindow.NavigateToDirect();
}
