  using System.IO;
using Microsoft.Win32;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class LocalAPKService
{
    private readonly APKValidationService _validator = new();

    public string? PromptForLocalApkFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Android APK File",
            Filter = "Android Package (*.apk)|*.apk",
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public async Task<(bool IsValid, string ErrorMessage, APKInfo? Info, APKMetadata? Meta)> InspectLocalApkAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return (false, "File does not exist.", null, null);

        if (!_validator.IsValidApkFormat(filePath))
            return (false, "Invalid APK archive header (failed ZIP magic byte check).", null, null);

        var metadata = await _validator.ExtractMetadataAsync(filePath);
        string sha256 = await _validator.ComputeSha256Async(filePath);
        var fileInfo = new FileInfo(filePath);

        var apkInfo = new APKInfo
        {
            Name = metadata.Label ?? Path.GetFileNameWithoutExtension(filePath),
            PackageName = metadata.PackageName ?? "unknown.package",
            LocalPath = filePath,
            SizeBytes = fileInfo.Length,
            Sha256 = sha256
        };

        return (true, string.Empty, apkInfo, metadata);
    }
}
