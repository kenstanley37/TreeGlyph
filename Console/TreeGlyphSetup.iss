[Setup]
AppName=TreeGlyph CLI
VersionInfoCompany=StanleySoft
VersionInfoDescription=TreeGlyph CLI Utility
VersionInfoVersion=1.0.0.0
AppVersion=1.0
DefaultDirName={pf}\TreeGlyph
DefaultGroupName=TreeGlyph
OutputBaseFilename=TreeGlyphSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\Release\net9.0\win-x64\publish\exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\TreeGlyph CLI"; Filename: "{app}\exe"

[Run]
Filename: "{app}\exe"; Description: "Run TreeGlyph Console"; Flags: postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\exe"

[Code]
function AddToPath(): Boolean;
var
  OldPath, NewPath: string;
begin
  RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', OldPath);
  if Pos(ExpandConstant('{app}'), OldPath) = 0 then begin
    if (OldPath = '') then
      NewPath := ExpandConstant('{app}')
    else
      NewPath := OldPath + ';' + ExpandConstant('{app}');
    RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', NewPath);
    Result := True;
  end else
    Result := False;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then begin
    if AddToPath() then
      MsgBox('TreeGlyph CLI path has been added to your system PATH. You may need to restart your terminal.', mbInformation, MB_OK);
  end;
end;