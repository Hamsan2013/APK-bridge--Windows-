# APK Desktop Bridge

**Android apps. Windows experience.**  
*GitHub-powered & local APK management with an integrated Android runtime.*

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11%20(x64)-0078D6?logo=windows)
![Framework](https://img.shields.io/badge/.NET-10.0%20WPF-512BD4?logo=dotnet)
![Android](https://img.shields.io/badge/Runtime-Android%2014%20(API%2034)%20x86__64-3DDC84?logo=android)

**APK Desktop Bridge** is a self-contained Windows 10/11 desktop application designed to download, validate, sideload, and launch Android applications (especially Android Local AI apps) in landscape mode without touching Android Studio or typing manual ADB commands.

---

## Key Features

* **Local File Sideloading:** Pick `.apk` files directly from Windows File Explorer or drag and drop them into the application window.
* **GitHub Release Browser:** Search public repositories and fetch `.apk` release assets directly via the GitHub REST API.
* **Direct HTTPS Installer:** Download APKs from direct HTTPS URLs with live progress tracking.
* **Pre-Install Security & Validation:**
  * Verifies ZIP archive magic bytes (`50 4B 03 04`) and `AndroidManifest.xml` presence.
  * Computes and displays **SHA-256** checksums before installation.
  * Inspects native libraries (`lib/x86_64` vs `lib/arm64-v8a`) to warn about CPU architecture compatibility.
* **One-Click Runtime Provisioning:** Automatically downloads official Android SDK Command Line Tools, Platform-Tools (`adb`), Emulator, and Android 14 `x86_64` system images into `%LOCALAPPDATA%`.
* **Landscape Tablet Experience:** Pre-configures a 1280×720 (720p) or 1920×1080 (1080p) tablet virtual device and forces landscape orientation via ADB.

---

## System Requirements

| Requirement | Minimum Specification |
| :--- | :--- |
| **OS** | Windows 10 or Windows 11 (64-bit) |
| **Virtualization** | Intel VT-x or AMD-V enabled in BIOS + Windows Hypervisor Platform (WHPX) |
| **Java Runtime** | OpenJDK 17+ on system `PATH` (required for initial Android SDK provisioning) |
| **RAM** | 8 GB minimum (16 GB recommended for Local AI APKs) |
| **Disk Space** | ~6 GB free in `%LOCALAPPDATA%` for Android SDK & Virtual Device |

---

## Quick Start (End Users)

1. **Install:** Download and run `APKDesktopBridge-Setup.exe` from the [Releases](../../releases) page.
2. **Setup Runtime:**
   * Launch **APK Desktop Bridge**.
   * Navigate to **Runtime & Display** on the sidebar.
   * Select your preferred display preset (`1280 × 720` or `1920 × 1080`) and click **Auto-Provision SDK & Create AVD**.
   * Accept the Android SDK license prompt and wait for the provisioning log to report `Android Runtime and Virtual Device are ready!`.
3. **Boot the Emulator:**
   * Click **Start Landscape Emulator** and wait for the Android tablet desktop to appear.
4. **Install an APK:**
   * Go to **Local File Sideload** (to browse your PC), **GitHub Browser**, or **Direct URL Installer**.
   * Select your APK, review the metadata card, and click **Install & Launch in Landscape**.

---

## Building from Source (Developers)

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Inno Setup 6](https://jrsoftware.org/isinfo.php) (optional, for compiling the `.exe` installer)

### Build, Test & Package Commands

```powershell
# 1. Restore dependencies
dotnet restore APKDesktopBridge.sln

# 2. Build in Release mode
dotnet build APKDesktopBridge.sln -c Release --no-restore

# 3. Run unit tests
dotnet test APKDesktopBridge.sln -c Release --no-build

# 4. Publish self-contained single-file Windows executable
dotnet publish src/APKDesktopBridge/APKDesktopBridge.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish

# 5. Compile Windows Setup.exe installer
iscc installer/APKDesktopBridge.iss
