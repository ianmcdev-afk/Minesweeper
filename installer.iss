; Inno Setup 6.3+ script. Run build-installer.bat (builds the exe first), or compile this
; file directly after running build.bat.

#define AppName "Minesweeper"
#define AppVersion "1.0.0"
#define AppExe "Minesweeper.exe"

[Setup]
AppId={{B3F5C2A1-6D4E-4A8B-9C71-2E5F0A7D3B19}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=kingtreelo inc
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=Minesweeper-Setup
SetupIconFile=Minesweeper.ico
UninstallDisplayIcon={app}\{#AppExe}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Installs per-user (no admin prompt) by default; the installer offers an all-users option.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
