; WeldAdmin Pro Installer - installs run_weldadmin_gui.bat + script files
[Setup]
AppName=WeldAdmin Pro
AppVersion=1.0
DefaultDirName={pf}\WeldAdmin Pro
DefaultGroupName=WeldAdmin Pro
OutputBaseFilename=WeldAdminProInstaller
OutputDir=build
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
; Install the launcher .bat and the GUI/script files
Source: "run_weldadmin_gui.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "weldadmin_gui.py"; DestDir: "{app}"; Flags: ignoreversion
Source: "weldadmin_auto_map.py"; DestDir: "{app}"; Flags: ignoreversion
Source: "weldadmin_import_to_db.py"; DestDir: "{app}"; Flags: ignoreversion
Source: "parser_weldadmin.py"; DestDir: "{app}"; Flags: ignoreversion
Source: "ocr.py"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.txt"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Dirs]
Name: "{app}\uploads"
Name: "{app}\icons"

[Icons]
Name: "{group}\WeldAdmin Pro"; Filename: "{app}\run_weldadmin_gui.bat"
Name: "{commondesktop}\WeldAdmin Pro"; Filename: "{app}\run_weldadmin_gui.bat"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional Icons:";

[Run]
Filename: "{app}\run_weldadmin_gui.bat"; Description: "Launch WeldAdmin Pro"; Flags: nowait postinstall skipifsilent
