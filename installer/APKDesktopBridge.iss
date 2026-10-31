#ifndef MyAppVersion
  #define MyAppVersion "1.0.0.0.2"
#endif

[Setup]
AppId={{8A2E5B1C-4F9D-4E2A-9B3C-1D2E3F4A5B6C}
AppName=APK Desktop Bridge
AppVersion={#MyAppVersion}
VersionInfoVersion=1.0.0.2
DefaultDirName={autopf}\APK Desktop Bridge
DefaultGroupName=APK Desktop Bridge
OutputDir=output
OutputBaseFilename=APKDesktopBridge-Setup-v{#MyAppVersion}
SetupIconFile=..\src\APKDesktopBridge\app.ico
UninstallDisplayIcon={app}\APKDesktopBridge.exe
ChangesAssociations=yes

Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

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
Source: "..\src\APKDesktopBridge\app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bundled_sdk\*"; DestDir: "{localappdata}\APK Desktop Bridge\AndroidSDK"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist

[Registry]
; Associate .apk files in Windows with APK Desktop Bridge and its custom icon
Root: HKA; Subkey: "Software\Classes\.apk"; ValueType: string; ValueName: ""; ValueData: "APKDesktopBridge.apk"; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\APKDesktopBridge.apk"; ValueType: string; ValueName: ""; ValueData: "Android Application Package"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\APKDesktopBridge.apk\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\app.ico,0"
Root: HKA; Subkey: "Software\Classes\APKDesktopBridge.apk\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\APKDesktopBridge.exe"" ""%1"""

[Icons]
Name: "{group}\APK Desktop Bridge"; Filename: "{app}\APKDesktopBridge.exe"; IconFilename: "{app}\app.ico"
Name: "{group}\Uninstall APK Desktop Bridge"; Filename: "{uninstallexe}"
Name: "{autodesktop}\APK Desktop Bridge"; Filename: "{app}\APKDesktopBridge.exe"; IconFilename: "{app}\app.ico"

[Run]
Filename: "{app}\APKDesktopBridge.exe"; Description: "Launch APK Desktop Bridge"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\APK Desktop Bridge"
Type: filesandordirs; Name: "{app}"
