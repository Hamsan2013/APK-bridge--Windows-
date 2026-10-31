  using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using APKDesktopBridge.Core;

namespace APKDesktopBridge.Views;

public partial class FileTransferView : UserControl
{
    private readonly ADBService _adb = new();

    public FileTransferView()
    {
        InitializeComponent();
    }

    private void PresetDownloads_Click(object sender, RoutedEventArgs e) => TxtAndroidPath.Text = "/sdcard/Download/";
    private void PresetDocuments_Click(object sender, RoutedEventArgs e) => TxtAndroidPath.Text = "/sdcard/Documents/";

    private void OnAnyFileDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnAnyFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            await TransferFilesAsync(files);
        }
    }

    private async void OnBrowseFilesClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select File(s) to Send Inside Android Emulator",
            Filter = "All Files (*.*)|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            await TransferFilesAsync(dialog.FileNames);
        }
    }

    private async Task TransferFilesAsync(string[] files)
    {
        string targetFolder = TxtAndroidPath.Text.Trim();
        foreach (string file in files)
        {
            TxtTransferStatus.Text = $"Sending {System.IO.Path.GetFileName(file)} to {targetFolder}...";
            var (success, msg) = await _adb.PushFileToEmulatorAsync(file, targetFolder);
            TxtTransferStatus.Text = msg;
            LstTransferLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}");
        }
    }
}
