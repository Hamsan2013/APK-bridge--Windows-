  namespace APKDesktopBridge.Models;

public class RuntimeConfig
{
    public string AvdName { get; set; } = "APKBridge_Tablet_API34";
    public string SystemImage { get; set; } = "system-images;android-34;google_apis;x86_64";
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public int Dpi { get; set; } = 160;
    public bool Landscape { get; set; } = true;
    public bool Fullscreen { get; set; } = false;
}
