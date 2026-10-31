  namespace APKDesktopBridge.Models;

public class APKInfo
{
    public string Name { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DateTime InstalledDate { get; set; } = DateTime.UtcNow;
}
