using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class DisplayManager
{
    private readonly ADBService _adb = new();

    public async Task ApplyPresetAsync(RuntimeConfig config, string preset)
    {
        switch (preset)
        {
            case "1080p":
                config.Width = 1920;
                config.Height = 1080;
                config.Dpi = 240;
                break;
            default:
                config.Width = 1280;
                config.Height = 720;
                config.Dpi = 160;
                break;
        }

        if (config.Landscape)
        {
            await _adb.SetLandscapeModeAsync();
        }
    }
}
