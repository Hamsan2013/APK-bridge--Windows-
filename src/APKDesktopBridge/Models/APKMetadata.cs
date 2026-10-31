  namespace APKDesktopBridge.Models;

public class APKMetadata
{
    public string? Label { get; set; }
    public string? PackageName { get; set; }
    public string? VersionName { get; set; }
    public string? LaunchableActivity { get; set; }
    public bool SupportsArm64 { get; set; }
    public bool SupportsX86_64 { get; set; }
}
