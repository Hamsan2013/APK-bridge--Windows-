using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class RuntimeManager
{
    private const string CmdLineToolsUrl = "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(30) };

    public string SdkRoot => SettingsService.SdkDir;
    public string AvdRoot => SettingsService.AvdDir;

    public string SdkManagerBat => Path.Combine(SdkRoot, "cmdline-tools", "latest", "bin", "sdkmanager.bat");
    public string AvdManagerBat => Path.Combine(SdkRoot, "cmdline-tools", "latest", "bin", "avdmanager.bat");
    public string AdbExe => Path.Combine(SdkRoot, "platform-tools", "adb.exe");
    public string EmulatorExe => Path.Combine(SdkRoot, "emulator", "emulator.exe");

    public bool IsFullRuntimeReady(RuntimeConfig config)
    {
        string avdIni = Path.Combine(AvdRoot, $"{config.AvdName}.ini");
        return File.Exists(AdbExe) && File.Exists(EmulatorExe) && File.Exists(avdIni);
    }

    public bool IsJavaAvailable()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = "-version",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(5000);
            return proc?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task ProvisionCompleteRuntimeAsync(
        RuntimeConfig config,
        IProgress<string>? status = null,
        CancellationToken ct = default)
    {
        if (!IsJavaAvailable())
        {
            throw new InvalidOperationException(
                "Java (JDK 17+) is required by Android SDK Command Line Tools. Please install OpenJDK 17 or Android Studio JBR and ensure 'java' is on your PATH.");
        }

        // Step 1: Download & Normalize cmdline-tools directory structure
        if (!File.Exists(SdkManagerBat))
        {
            await DownloadAndExtractCmdLineToolsAsync(status, ct);
        }

        // Step 2: Accept all Android SDK licenses non-interactively
        status?.Report("Accepting Android SDK licenses...");
        await AcceptLicensesAsync(ct);

        // Step 3: Download platform-tools, emulator, platforms, and system-image
        status?.Report($"Installing platform-tools, emulator, and {config.SystemImage} (this may take several minutes)...");
        string sdkArgs = $"\"platform-tools\" \"emulator\" \"platforms;android-34\" \"{config.SystemImage}\"";
        var (sdkExit, sdkOut) = await RunSdkBatchAsync(SdkManagerBat, sdkArgs, autoFeedYes: true, status, ct);
        if (sdkExit != 0)
        {
            throw new InvalidOperationException($"sdkmanager failed (Exit {sdkExit}): {sdkOut}");
        }

        // Step 4: Create the Android Virtual Device (AVD)
        status?.Report($"Creating Virtual Device '{config.AvdName}'...");
        await CreateAvdAsync(config, status, ct);

        // Step 5: Configure hardware properties (1280x720 Landscape Tablet, RAM, GPU)
        status?.Report("Applying Landscape Tablet hardware profile...");
        ConfigureAvdHardware(config);

        status?.Report("Android Runtime and Virtual Device are ready!");
    }

    private async Task DownloadAndExtractCmdLineToolsAsync(IProgress<string>? status, CancellationToken ct)
    {
        status?.Report("Downloading Android Command Line Tools...");
        string zipPath = Path.Combine(SettingsService.DownloadsDir, "cmdline-tools.zip");

        using (var response = await Http.GetAsync(CmdLineToolsUrl, HttpCompletionOption.ResponseHeadersRead, ct))
        {
            response.EnsureSuccessStatusCode();
            await using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs, ct);
        }

        status?.Report("Extracting Command Line Tools...");
        string tempExtract = Path.Combine(SettingsService.DownloadsDir, "cmdline-extract");
        if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
        ZipFile.ExtractToDirectory(zipPath, tempExtract);

        // Google's zip contains a top-level "cmdline-tools" folder; sdkmanager requires ".../cmdline-tools/latest/bin"
        string latestDir = Path.Combine(SdkRoot, "cmdline-tools", "latest");
        if (Directory.Exists(latestDir)) Directory.Delete(latestDir, true);
        Directory.CreateDirectory(Path.GetDirectoryName(latestDir)!);

        string innerFolder = Path.Combine(tempExtract, "cmdline-tools");
        Directory.Move(innerFolder, latestDir);
        Directory.Delete(tempExtract, true);
    }

    private async Task AcceptLicensesAsync(CancellationToken ct)
    {
        await RunSdkBatchAsync(SdkManagerBat, "--licenses", autoFeedYes: true, null, ct);
    }

    private async Task CreateAvdAsync(RuntimeConfig config, IProgress<string>? status, CancellationToken ct)
    {
        string avdArgs = $"create avd --force --name \"{config.AvdName}\" --package \"{config.SystemImage}\" --abi \"google_apis/x86_64\"";
        var (exitCode, output) = await RunSdkBatchAsync(AvdManagerBat, avdArgs, autoFeedYes: false, status, ct, feedCustomProfileNo: true);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"avdmanager failed to create AVD: {output}");
        }
    }

    private void ConfigureAvdHardware(RuntimeConfig config)
    {
        string avdFolder = Path.Combine(AvdRoot, $"{config.AvdName}.avd");
        string configIniPath = Path.Combine(avdFolder, "config.ini");
        if (!File.Exists(configIniPath)) return;

        var lines = File.ReadAllLines(configIniPath).ToList();

        void SetIniValue(string key, string value)
        {
            int idx = lines.FindIndex(l => l.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) lines[idx] = $"{key}={value}";
            else lines.Add($"{key}={value}");
        }

        // Force landscape resolution (Width > Height) and hardware acceleration
        SetIniValue("hw.lcd.width", config.Width.ToString());
        SetIniValue("hw.lcd.height", config.Height.ToString());
        SetIniValue("hw.lcd.density", config.Dpi.ToString());
        SetIniValue("hw.initialOrientation", config.Landscape ? "landscape" : "portrait");
        SetIniValue("hw.ramSize", "4096");
        SetIniValue("hw.gpu.enabled", "yes");
        SetIniValue("hw.gpu.mode", "auto");
        SetIniValue("hw.keyboard", "yes");

        File.WriteAllLines(configIniPath, lines);
    }

    private async Task<(int ExitCode, string Output)> RunSdkBatchAsync(
        string batchFilePath,
        string arguments,
        bool autoFeedYes,
        IProgress<string>? status,
        CancellationToken ct,
        bool feedCustomProfileNo = false)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"\"{batchFilePath}\" {arguments}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.EnvironmentVariables["ANDROID_SDK_ROOT"] = SdkRoot;
        psi.EnvironmentVariables["ANDROID_HOME"] = SdkRoot;
        psi.EnvironmentVariables["ANDROID_AVD_HOME"] = AvdRoot;

        using var process = new Process { StartInfo = psi };
        var outputBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                status?.Report(e.Data.Trim());
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (autoFeedYes)
        {
            for (int i = 0; i < 30; i++)
            {
                await process.StandardInput.WriteLineAsync("y");
            }
            process.StandardInput.Close();
        }
        else if (feedCustomProfileNo)
        {
            // Answers "no" to "Do you wish to create a custom hardware profile? [no]"
            await process.StandardInput.WriteLineAsync("no");
            process.StandardInput.Close();
        }

        await process.WaitForExitAsync(ct);
        return (process.ExitCode, outputBuilder.ToString());
    }
}
