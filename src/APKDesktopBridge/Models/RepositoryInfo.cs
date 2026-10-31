  namespace APKDesktopBridge.Models;

public class RepositoryInfo
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
}

public class GitHubAssetItem
{
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long Size { get; set; }
    public string SourceType { get; set; } = "Release"; // Release or Tree
}
