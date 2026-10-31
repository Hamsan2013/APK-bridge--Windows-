  [Setup]
AppName=APK Desktop Bridge
AppVersion=1.0.0
DefaultDirName={autopf}\APK Desktop Bridge
DefaultGroupName=APK Desktop Bridge
OutputDir=output
OutputBaseFilename=APKDesktopBridge-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Dirs]
Name: "{localappdata}\APK Desktop Bridge\Downloads"
Name: "{localappdata}\APK Desktop Bridge\AndroidSDK"
Name: "{localappdata}\APK Desktop Bridge\VirtualDevices"
Name: "{localappdata}\APK Desktop Bridge\Logs"
Name: "{localappdata}\APK Desktop Bridge\Settings"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\APK Desktop Bridge"; Filename: "{app}\APKDesktopBridge.exe"
Name: "{autodesktop}\APK Desktop Bridge"; Filename: "{app}\APKDesktopBridge.exe"

[Run]
Filename: "{app}\APKDesktopBridge.exe"; Description: "Launch APK Desktop Bridge"; Flags: nowait postinstall skipifsilent
