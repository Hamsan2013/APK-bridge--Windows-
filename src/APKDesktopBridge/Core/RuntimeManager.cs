using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class RuntimeManager
{
    private const string CmdLineToolsUrl = "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
    
    // Configured handler to handle TLS/SSL certificate revocation hiccups gracefully
    private static readonly HttpClient Http = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    })
    {
        Timeout = TimeSpan.FromMinutes(30)
    };

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
                "Java is required by Android SDK Tools. Ensure 'java' is working on your system PATH.");
        }

        // Step 1: Download & Extract cmdline-tools
        if (!File.Exists(SdkManagerBat))
        {
            await DownloadAndExtractCmdLineToolsAsync(status, ct);
        }

        // Step 2: Accept licenses
        status?.Report("Accepting Android SDK licenses...");
        await AcceptLicensesAsync(ct);

        // Step 3: Install platform-tools, emulator, system image
        status?.Report($"Installing platform-tools, emulator, and {config.SystemImage}...");
        string sdkArgs = $"\"platform-tools\" \"emulator\" \"platforms;android-34\" \"{config.SystemImage}\"";
        var (sdkExit, sdkOut) = await RunSdkBatchAsync(SdkManagerBat, sdkArgs, autoFeedYes: true, status, ct);
        if (sdkExit != 0)
        {
            throw new InvalidOperationException($"sdkmanager failed (Exit {sdkExit}): {sdkOut}");
        }

        // Step 4: Create AVD
        status?.Report($"Creating Virtual Device '{config.AvdName}'...");
        await CreateAvdAsync(config, status, ct);

        // Step 5: Configure Landscape Hardware Profile
        status?.Report("Applying Landscape Tablet hardware profile...");
        ConfigureAvdHardware(config);

        status?.Report("Android Runtime and Virtual Device are ready!");
    }

    private async Task DownloadAndExtractCmdLineToolsAsync(IProgress<string>? status, CancellationToken ct)
    {
        status?.Report("Downloading Android Command Line Tools...");
        string zipPath = Path.Combine(SettingsService.DownloadsDir, "cmdline-tools.zip");

        // Try primary download via HttpClient with fallback to Windows curl
        bool downloaded = false;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, CmdLineToolsUrl);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            
            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs, ct);
            downloaded = true;
        }
        catch (Exception ex)
        {
            status?.Report($"HttpClient SSL issue encountered: {ex.Message}. Falling back to Windows curl...");
        }

        // Fallback: Use native Windows curl.exe if SSL error persists
        if (!downloaded || !File.Exists(zipPath) || new FileInfo(zipPath).Length < 1000)
        {
            var curlPsi = new ProcessStartInfo
            {
                FileName = "curl.exe",
                Arguments = $"-k -L -o \"{zipPath}\" \"{CmdLineToolsUrl}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var curlProc = Process.Start(curlPsi);
            if (curlProc != null)
            {
                await curlProc.WaitForExitAsync(ct);
            }
        }

        if (!File.Exists(zipPath) || new FileInfo(zipPath).Length < 1000)
        {
            throw new FileNotFoundException("Failed to download Android command-line tools archive.");
        }

        status?.Report("Extracting Command Line Tools...");
        string tempExtract = Path.Combine(SettingsService.DownloadsDir, "cmdline-extract");
        if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true);
        ZipFile.ExtractToDirectory(zipPath, tempExtract);

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
            await process.StandardInput.WriteLineAsync("no");
            process.StandardInput.Close();
        }

        await process.WaitForExitAsync(ct);
        return (process.ExitCode, outputBuilder.ToString());
    }
}
