  using System.IO;
using APKDesktopBridge.Core;
using Xunit;

namespace APKDesktopBridge.Tests;

public class APKValidationTests
{
    [Fact]
    public void IsValidApkFormat_ReturnsTrue_ForValidZipMagicBytes()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00 });
            var validator = new APKValidationService();
            Assert.True(validator.IsValidApkFormat(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsValidApkFormat_ReturnsFalse_ForNonZipFile()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "Not an APK file");
            var validator = new APKValidationService();
            Assert.False(validator.IsValidApkFormat(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
