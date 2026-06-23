[Setup]
AppName=WeldAdminPro
AppVersion=1.0
DefaultDirName={pf}\WeldAdminPro
DefaultGroupName=WeldAdminPro
OutputDir=Installer
OutputBaseFilename=WeldAdminPro_Setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "release\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\WeldAdminPro"; Filename: "{app}\WeldAdminPro.UI.exe"
Name: "{commondesktop}\WeldAdminPro"; Filename: "{app}\WeldAdminPro.UI.exe"