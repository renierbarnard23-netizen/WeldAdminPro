; WeldAdminPro Inno Setup script (template)
#define MyAppVersion "1.0.0"

[Setup]
AppName=WeldAdminPro
AppVersion={#MyAppVersion}
DefaultDirName={commonpf}\WeldAdminPro
DefaultGroupName=WeldAdminPro
UninstallDisplayName=WeldAdminPro {#MyAppVersion}
OutputBaseFilename=WeldAdminPro_Installer_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
DisableDirPage=no
DisableProgramGroupPage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; The installer will package everything under the SourcePublish folder.
; This placeholder will be replaced when building on the runner.
Source: "C:\\users\\renie\\documents\\weldadminpro\\installer\\source\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\WeldAdminPro"; Filename: "{app}\WeldAdminPro.UI.exe"

[Run]
Filename: "{app}\WeldAdminPro.UI.exe"; Description: "Launch WeldAdminPro"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

