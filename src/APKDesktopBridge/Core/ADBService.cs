using System.Diagnostics;
using System.IO;

namespace APKDesktopBridge.Core;

public class ADBService
{
    public string AdbExecutablePath => Path.Combine(SettingsService.SdkDir, "platform-tools", "adb.exe");

    public async Task<bool> IsDeviceConnectedAsync()
    {
        if (!File.Exists(AdbExecutablePath)) return false;
        var (exitCode, output, _) = await RunAdbCommandAsync("devices");
        if (exitCode != 0) return false;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return lines.Skip(1).Any(l => l.Contains("\tdevice"));
    }

    public async Task<(bool Success, string Message)> InstallApkAsync(string apkPath, bool allowDowngrade = false)
    {
        if (!File.Exists(apkPath))
            return (false, "APK file not found on disk.");

        if (!File.Exists(AdbExecutablePath))
            return (false, $"ADB not found at {AdbExecutablePath}. Install the Android Runtime first.");

        string flags = allowDowngrade ? "-r -d -g" : "-r -g";
        var (exitCode, output, error) = await RunAdbCommandAsync($"install {flags} \"{apkPath}\"");

        bool success = exitCode == 0 && output.Contains("Success", StringComparison.OrdinalIgnoreCase);
        return (success, success ? "Installed successfully." : $"{output} {error}".Trim());
    }

    public async Task<(bool Success, string Message)> PushFileToEmulatorAsync(string localFilePath, string androidDestinationFolder)
    {
        if (!File.Exists(localFilePath))
            return (false, "Selected local file does not exist.");

        if (!File.Exists(AdbExecutablePath))
            return (false, "ADB is not installed yet.");

        if (!await IsDeviceConnectedAsync())
            return (false, "Emulator is not running. Start the emulator first.");

        // Ensure Android path uses forward slashes and ends cleanly
        string cleanAndroidDir = androidDestinationFolder.Replace("\\", "/").Trim();
        if (!cleanAndroidDir.StartsWith("/")) cleanAndroidDir = "/" + cleanAndroidDir;
        if (!cleanAndroidDir.EndsWith("/")) cleanAndroidDir += "/";

        // Create target directory inside Android emulator first
        await RunAdbCommandAsync($"shell mkdir -p \"{cleanAndroidDir}\"");

        string fileName = Path.GetFileName(localFilePath);
        string fullRemotePath = $"{cleanAndroidDir}{fileName}";

        var (exitCode, output, error) = await RunAdbCommandAsync($"push \"{localFilePath}\" \"{fullRemotePath}\"");
        bool success = exitCode == 0;
        return (success, success ? $"Saved to emulator: {fullRemotePath}" : $"Transfer failed: {output} {error}".Trim());
    }

    public async Task<bool> LaunchPackageAsync(string packageName)
    {
        if (!File.Exists(AdbExecutablePath)) return false;
        var (exitCode, _, _) = await RunAdbCommandAsync($"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1");
        return exitCode == 0;
    }

    public async Task<bool> UninstallPackageAsync(string packageName)
    {
        if (!File.Exists(AdbExecutablePath)) return false;
        var (exitCode, output, _) = await RunAdbCommandAsync($"uninstall {packageName}");
        return exitCode == 0 && output.Contains("Success", StringComparison.OrdinalIgnoreCase);
    }

    public async Task SetLandscapeModeAsync()
    {
        if (!File.Exists(AdbExecutablePath)) return;
        await RunAdbCommandAsync("shell settings put system accelerometer_rotation 0");
        await RunAdbCommandAsync("shell settings put system user_rotation 1");
    }

    private async Task<(int ExitCode, string Output, string Error)> RunAdbCommandAsync(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = AdbExecutablePath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, output, error);
    }
}
