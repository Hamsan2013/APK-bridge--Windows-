  using System.Diagnostics;
using System.IO;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class EmulatorService
{
    public string EmulatorExePath => Path.Combine(SettingsService.SdkDir, "emulator", "emulator.exe");

    public bool LaunchEmulator(RuntimeConfig config)
    {
        if (!File.Exists(EmulatorExePath)) return false;

        string skinArgs = $"{config.Width}x{config.Height}";
        string gpuArgs = "-gpu auto";
        string arguments = $"-avd {config.AvdName} -skin {skinArgs} {gpuArgs}";

        var psi = new ProcessStartInfo
        {
            FileName = EmulatorExePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        psi.EnvironmentVariables["ANDROID_AVD_HOME"] = SettingsService.AvdDir;
        psi.EnvironmentVariables["ANDROID_SDK_ROOT"] = SettingsService.SdkDir;

        Process.Start(psi);
        return true;
    }
}
