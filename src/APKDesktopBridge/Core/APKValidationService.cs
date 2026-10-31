  using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using APKDesktopBridge.Models;

namespace APKDesktopBridge.Core;

public class APKValidationService
{
    public bool IsValidApkFormat(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fs.Length < 4) return false;

            byte[] buffer = new byte[4];
            int read = fs.Read(buffer, 0, 4);
            // ZIP magic bytes: PK\x03\x04 (50 4B 03 04)
            return read == 4 && buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ComputeSha256Async(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        byte[] hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public Task<APKMetadata> ExtractMetadataAsync(string apkPath)
    {
        var metadata = new APKMetadata
        {
            Label = Path.GetFileNameWithoutExtension(apkPath),
            PackageName = Path.GetFileNameWithoutExtension(apkPath).ToLowerInvariant().Replace(" ", "."),
            SupportsX86_64 = true
        };

        try
        {
            using ZipArchive archive = ZipFile.OpenRead(apkPath);
            bool hasManifest = archive.Entries.Any(e => e.FullName.Equals("AndroidManifest.xml", StringComparison.OrdinalIgnoreCase));
            if (!hasManifest)
            {
                metadata.PackageName = "Invalid APK (Missing AndroidManifest.xml)";
                return Task.FromResult(metadata);
            }

            bool hasArm64 = archive.Entries.Any(e => e.FullName.StartsWith("lib/arm64-v8a/", StringComparison.OrdinalIgnoreCase));
            bool hasX86_64 = archive.Entries.Any(e => e.FullName.StartsWith("lib/x86_64/", StringComparison.OrdinalIgnoreCase));
            bool hasAnyNativeLibs = archive.Entries.Any(e => e.FullName.StartsWith("lib/", StringComparison.OrdinalIgnoreCase));

            metadata.SupportsArm64 = !hasAnyNativeLibs || hasArm64;
            metadata.SupportsX86_64 = !hasAnyNativeLibs || hasX86_64;
        }
        catch
        {
            // Fallback if archive inspection fails
        }

        return Task.FromResult(metadata);
    }
}
