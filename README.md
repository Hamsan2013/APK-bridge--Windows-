# 🚀 APK Desktop Bridge

**Android apps. Windows experience. Zero bloatware.**  
*A lightweight, open-source Android APK runner and file bridge for Windows 10 & 11.*

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11%20(x64)-0078D6?logo=windows)
![Framework](https://img.shields.io/badge/.NET-10.0%20WPF-512BD4?logo=dotnet)
![Android](https://img.shields.io/badge/Runtime-Android%2014%20(API%2034)%20x86__64-3DDC84?logo=android)
![License](https://img.shields.io/badge/License-MIT-10B981)

---

## 💡 Why I Built This

Hi! I'm **Hamsan**, a 14-year-old developer. I wanted a simple way to test Android `.apk` files and run **Android Local AI apps** on my Windows laptop. However, popular commercial emulators like BlueStacks and LDPlayer take up 5 GB to 10+ GB of storage, consume massive RAM, and are packed with ads and pre-installed bloatware games.

So, I built **APK Desktop Bridge**—a clean, open-source Windows desktop application powered by `.NET 10` and official Google Android virtualization tools. No ads, no background telemetry, and no manual command-line setup required.

---

## ⚡ How It Compares

| Feature | APK Desktop Bridge | Commercial Emulators (BlueStacks / LDPlayer) |
| :--- | :--- | :--- |
| **Ads & Sponsored Games** | **0% (100% Clean & Ad-Free)** | Heavy ads, popups, and pre-installed games |
| **Installer Size** | **~395 MB** (Includes offline ADB & Emulator) | 500 MB – 1+ GB installer + multi-GB bloat |
| **Base Disk Footprint** | **Lightweight & Modular** | 5 GB – 10+ GB |
| **Windows `.apk` Integration** | **Double-click any `.apk` in File Explorer** | Requires heavy launcher wrapper |
| **Source Code** | **100% Open Source (MIT)** | Closed source / proprietary |

---

## ✨ Key Features

* **🖱️ Double-Click Windows `.apk` Integration:** Associates `.apk` files across Windows with a custom icon. Double-click any `.apk` on your Desktop or File Explorer to open it directly in APK Desktop Bridge ready to install.
* **📂 Local Sideload & Drag-and-Drop:** Browse via Windows File Explorer or drag and drop `.apk` files directly into the dashboard.
* **📲 Emulator File Transfer:** Drag and drop *any* file from your laptop (such as `.gguf` AI models, ZIPs, images, or documents) directly into your Android Emulator's internal storage (`/sdcard/Download/`, `/sdcard/Documents/`, or any custom path).
* **🐙 GitHub Release Browser:** Enter any GitHub repository (`owner` / `repo`) to automatically discover, download, and install `.apk` release assets.
* **🔗 Direct HTTPS URL Installer:** Paste any direct `https://` APK link to download with a live progress bar and verify its **SHA-256** checksum before installing.
* **🛡️ Pre-Install Security & ABI Validation:** Inspects ZIP magic bytes (`50 4B 03 04`), verifies `AndroidManifest.xml`, calculates SHA-256 hashes, and checks native libraries (`x86_64` vs `arm64-v8a`).
* **🖥️ Landscape Tablet Mode:** Pre-configured for **1280 × 720 (720p)** and **1920 × 1080 (1080p)** landscape display with live 3-second ADB connection monitoring.
* **📦 Ultra-Compressed Offline Bundling:** The Windows installer comes pre-bundled with `adb.exe`, `cmdline-tools`, and the Android `emulator` engine out of the box.

---

## 📥 Quick Start (How to Install & Use)

1. **Download the Installer:**
   * Go to the [**Releases**](https://github.com/Hamsan2013/APK-bridge--Windows-/releases) page.
   * Expand **Assets** and download **`APKDesktopBridge-Setup.exe`**.
2. **Run Setup:**
   * Double-click `APKDesktopBridge-Setup.exe`. *(If Windows SmartScreen appears, click **More info** -> **Run anyway**).*
3. **Start the Landscape Emulator:**
   * Open **APK Desktop Bridge** and click **Runtime & Display** on the sidebar.
   * Click the green **Start Landscape Emulator** button.
   * *(First-Time Setup Note: If the Android 14 system image isn't created on your PC yet, click **Auto-Provision SDK & Create AVD** once to let the app automatically download the image and configure your virtual tablet).*
4. **Install Your First APK:**
   * Double-click any `.apk` file on your PC, **or** go to **Local File Sideload** and drag an `.apk` into the window.
   * Click **INSTALL & LAUNCH IN LANDSCAPE**!

---

## 💻 System Requirements

* **OS:** Windows 10 or Windows 11 (64-bit)
* **Virtualization:** Intel VT-x or AMD-V enabled in BIOS + **Windows Hypervisor Platform (WHPX)** enabled in Windows Features
* **Java:** OpenJDK 17+ (Required only when running initial AVD creation via `Auto-Provision SDK`)
* **RAM:** 8 GB minimum (16 GB recommended for running Local AI apps)

---

## 🛠️ Building from Source (For Developers)

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Inno Setup 6](https://jrsoftware.org/isinfo.php) (for compiling the Windows `.exe` installer)

### Commands

```powershell
# 1. Clone the repository
git clone https://github.com/Hamsan2013/APK-bridge--Windows-.git
cd APK-bridge--Windows-

# 2. Restore, build, and run automated unit tests
dotnet restore APKDesktopBridge.slnx
dotnet build APKDesktopBridge.slnx -c Release --no-restore
dotnet test APKDesktopBridge.slnx -c Release --no-build

# 3. Publish self-contained Windows x64 executable
dotnet publish src/APKDesktopBridge/APKDesktopBridge.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

---

## 🗂️ Architecture & Directory Structure

Application files and Android SDK data are cleanly separated so the runtime can update without requiring administrator privileges:

```text
C:\Program Files\APK Desktop Bridge\
├── APKDesktopBridge.exe          # Main WPF Desktop Application
└── app.ico                       # Application & Windows .apk File Icon

%LOCALAPPDATA%\APK Desktop Bridge\
├── Downloads\                    # Downloaded APKs & temporary cache
├── AndroidSDK\                   # Bundled platform-tools (adb), emulator, cmdline-tools
├── VirtualDevices\               # Custom Landscape Tablet AVD (.avd & config.ini)
├── Logs\                         # Diagnostic logs
└── Settings\
    └── appsettings.json          # User configuration & installed app library
```

---

## 🤝 Contributing & Reporting Bugs

This project is currently in **Pre-Release Beta**! If you find a bug, have a feature idea, or want to contribute:

1. Open the [**Issues**](https://github.com/Hamsan2013/APK-bridge--Windows-/issues) tab and create a new issue with your error details or screenshot.
2. Fork the repository, create a feature branch, and submit a Pull Request.

If you find **APK Desktop Bridge** useful, please click the **⭐ Star** button at the top right of this repository to help more people discover it!

---

## 📄 License

Distributed under the **MIT License**.  
*Android SDK tools and system images are governed by the Google Android Software Development Kit License Agreement.*
