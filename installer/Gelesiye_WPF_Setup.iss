; ==============================================================================
; Gelesiye WPF - Inno Setup 6 Script (.NET 10 Win64)
; ==============================================================================

#ifndef MyAppName
  #define MyAppName "Gelesiye WPF"
#endif

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0.15"
#endif

#ifndef AppVerSuffix
  #define AppVerSuffix ""
#endif

#ifndef OutputBaseFilename
  #define OutputBaseFilename "Gelesiye_WPF_Setup_x64"
#endif

#ifndef SourceDir
  #define SourceDir "..\bin\Release\net10.0-windows"
#endif

#define MyAppPublisher "Cihad Kocaaşahan"
#define MyAppURL "https://github.com/cihadkocasahan/Gelesiye-WPF"
#define MyAppExeName "GelesiyeWPF.exe"

[Setup]
; App Identity
AppId={{E5F6A7B8-C9D0-1E2F-3A4B-5C6D7E8F90A1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}{#AppVerSuffix}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Sürüm Takibi
VersionInfoVersion={#MyAppVersion}
VersionInfoTextVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} — Basit Veresiye Takip Kurulum Paketi
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

; Target Directory (64-bit Program Files)
DefaultDirName={autopf64}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
DisableProgramGroupPage=no

; 64-bit Architecture Configuration
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Output
OutputDir=..\releases
OutputBaseFilename={#OutputBaseFilename}
SetupIconFile=..\Assets\gelesiye.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

; Uninstaller
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} v{#MyAppVersion}{#AppVerSuffix}

PrivilegesRequired=lowest
UsedUserAreasWarning=no

CloseApplications=yes
CloseApplicationsFilter=GelesiyeWPF.exe
RestartApplications=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
turkish.CreateDesktopIcon=Masaüstü simgesi oluşturulsun
english.CreateDesktopIcon=Create a desktop shortcut
turkish.LaunchProgram=Gelesiye WPF — Basit Veresiye Takip Programı'nı Başlat
english.LaunchProgram=Launch Gelesiye WPF — Simple Credit Manager

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Dirs]
Name: "{app}\data"; Permissions: users-modify
Name: "{app}\backups"; Permissions: users-modify
Name: "{app}\logs"; Permissions: users-modify

[Files]
; Main Binaries & Dependencies
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.db,*.db-shm,*.db-wal,*.log,backups\*,logs\*"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram}"; Flags: nowait postinstall skipifsilent
