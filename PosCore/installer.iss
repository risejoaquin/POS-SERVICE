[Setup]
AppName=Super POS Express
AppVersion=1.0.0
Publisher=Tu Empresa
DefaultDirName={autopf}\SuperPOS
DefaultGroupName=Super POS Express
OutputDir=.\Output
OutputBaseFilename=SuperPOS_Setup_v1.0
Compression=lzma
SolidCompression=yes
; Instalar para el usuario actual (no requiere admin)
PrivilegesRequired=lowest
DisableProgramGroupPage=yes

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Super POS Express"; Filename: "{app}\PosCore.exe"
Name: "{autodesktop}\Super POS Express"; Filename: "{app}\PosCore.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear un icono en el escritorio"; GroupDescription: "Accesos directos:"

[Run]
Filename: "{app}\PosCore.exe"; Description: "Ejecutar Super POS Express"; Flags: nowait postinstall skipifsilent
