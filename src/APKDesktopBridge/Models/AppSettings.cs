  namespace APKDesktopBridge.Models;

public class AppSettings
{
    public string BaseDataPath { get; set; } = string.Empty;
    public string SdkPath { get; set; } = string.Empty;
    public string DownloadsPath { get; set; } = string.Empty;
    public RuntimeConfig Runtime { get; set; } = new();
    public List<APKInfo> InstalledApps { get; set; } = new();
}
