#ifndef MyAppVersion
  #define MyAppVersion "1.0.0.0"
#endif

#define MyAppName "Compras S.I.G."
#define MyAppPublisher "Cipolatti, Inc."
#define MyAppURL "https://www.cipolatti.com.br"
#define MyAppExeName "Compras.exe"
#define DotNetRuntimeInstaller "redist\windowsdesktop-runtime-10.0-win-x64.exe"

[Setup]
AppId={{E54B9DBA-85FD-43EC-AB25-048E9338BF13}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\SIG\Compras S.I.G
DisableDirPage=yes
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
PrivilegesRequired=lowest
OutputDir=artifacts\installer
OutputBaseFilename=ComprasSetup-{#MyAppVersion}
SetupIconFile=Compras\Icones\logo-pink.ico
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
#ifexist DotNetRuntimeInstaller
Source: "{#DotNetRuntimeInstaller}"; DestDir: "{tmp}"; Flags: deleteafterinstall
#endif

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
#ifexist DotNetRuntimeInstaller
Filename: "{tmp}\windowsdesktop-runtime-10.0-win-x64.exe"; Parameters: "/install /quiet /norestart"; StatusMsg: "Instalando .NET Desktop Runtime 10..."; Check: not IsDotNetDesktopRuntime10Installed
#endif
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNetDesktopRuntime10Installed: Boolean;
begin
  Result := RegKeyExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\10.0');
end;
