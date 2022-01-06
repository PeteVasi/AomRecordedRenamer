; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AppName=AOMRecordedRenamer
AppVerName=AOMRecordedRenamer v1.3
AppPublisher=magneticpole.com
AppPublisherURL=http://software.magneticpole.com/rcxren/
AppSupportURL=http://software.magneticpole.com/rcxren/
AppUpdatesURL=http://software.magneticpole.com/rcxren/
DefaultDirName={pf}\AOMRecordedRenamer
DefaultGroupName=AOMRecordedRenamer
LicenseFile=C:\Personal\code\cs\AOMRecordedRenamer\installer\license.txt
Compression=lzma
SolidCompression=yes

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Personal\code\cs\AOMRecordedRenamer\bin\Release\AOMRecordedRenamer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Personal\code\cs\AOMRecordedRenamer\ICSharpCode.SharpZipLib.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[INI]
Filename: "{app}\AOMRecordedRenamer.url"; Section: "InternetShortcut"; Key: "URL"; String: "http://software.magneticpole.com/rcxren/"

[Icons]
Name: "{group}\AOMRecordedRenamer"; Filename: "{app}\AOMRecordedRenamer.exe"
Name: "{group}\{cm:ProgramOnTheWeb,AOMRecordedRenamer}"; Filename: "{app}\AOMRecordedRenamer.url"
Name: "{userdesktop}\AOMRecordedRenamer"; Filename: "{app}\AOMRecordedRenamer.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\AOMRecordedRenamer.exe"; Description: "{cm:LaunchProgram,AOMRecordedRenamer}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\AOMRecordedRenamer.url"

