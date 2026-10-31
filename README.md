  # APK Desktop Bridge

Windows 10/11 desktop manager for downloading, sideloading, and running Android Local AI APK applications in landscape via an integrated Android Emulator runtime.

## Features
- **Local File Sideload:** Browse via Windows File Explorer or Drag & Drop `.apk` files directly into the app.
- **GitHub Release Browser:** Discover and install APKs directly from GitHub repositories.
- **Direct HTTPS Installer:** Download APK links with SHA-256 verification.
- **Integrated Runtime Manager:** Provisions the official Android SDK and launches a 1280×720 Landscape tablet AVD.

## Local Build Instructions
```bash
dotnet restore APKDesktopBridge.sln
dotnet build APKDesktopBridge.sln -c Release
dotnet test APKDesktopBridge.sln -c Release
