#define MyAppName "PC Remote"
#define MyAppExeName "PCRemote.exe"
#define MyServiceExeName "PCRemote.Service.exe"
#define MyServiceName "PCRemoteService"
#define MyAppVersion "1.0.1"

[Setup]
AppId={{C54B8E67-ACF2-4C5A-B7C7-2F9C9C45E000}
AppName={#MyAppName}
AppVersion={#MyAppVersion}

DefaultDirName={pf}\PC Remote
DefaultGroupName=PC Remote

OutputDir=output
OutputBaseFilename=PCRemoteSetup

Compression=lzma
SolidCompression=yes
WizardStyle=modern

ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

SetupIconFile=icon.ico
SetupLogging=yes


[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"


; =====================================================
; FILES
; =====================================================

[Files]

Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs


; =====================================================
; SHORTCUTS
; =====================================================

[Icons]

Name: "{group}\PC Remote"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\PC Remote"; Filename: "{app}\{#MyAppExeName}"


; =====================================================
; RUN
; =====================================================

[Run]

Filename: "{app}\{#MyAppExeName}"; Flags: nowait postinstall runascurrentuser


; =====================================================
; FIREWALL RULE
; =====================================================

Filename: "netsh"; \
Parameters: "advfirewall firewall add rule name=""PCRemote"" dir=in action=allow protocol=TCP localport=5055"; \
Flags: runhidden


; =====================================================
; UNINSTALL
; =====================================================

[UninstallRun]

Filename: "taskkill.exe"; Parameters: "/IM {#MyAppExeName} /F"; Flags: runhidden

Filename: "sc.exe"; Parameters: "stop {#MyServiceName}"; Flags: runhidden
Filename: "sc.exe"; Parameters: "delete {#MyServiceName}"; Flags: runhidden

Filename: "netsh"; Parameters: "advfirewall firewall delete rule name=""PCRemote"""; Flags: runhidden

; =====================================================
; Reestr
; =====================================================

[Registry]

Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
ValueName: "PCRemote"; Flags: uninsdeletevalue


[InstallDelete]

Type: files; Name: "{app}\PCRemote.Service.exe"

; =====================================================
; SERVICE MANAGEMENT
; =====================================================

[Code]

var
  VpnWarnPage: TWizardPage;
  VpnCheckBox: TNewCheckBox;


function ServiceExists(ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  Exec('sc.exe','query "' + ServiceName + '"','',SW_HIDE,ewWaitUntilTerminated,ResultCode);
  Result := (ResultCode = 0);
end;


procedure StopAndDeleteService(ServiceName: string);
var
  ResultCode: Integer;
begin

  Exec('sc.exe','stop "' + ServiceName + '"','',SW_HIDE,ewWaitUntilTerminated,ResultCode);
  Sleep(1000);

  Exec('sc.exe','delete "' + ServiceName + '"','',SW_HIDE,ewWaitUntilTerminated,ResultCode);

end;


procedure CreateAndStartService();
var
  ResultCode: Integer;
  SvcPath: string;
begin

  SvcPath := ExpandConstant('{app}\{#MyServiceExeName}');

  if ServiceExists('{#MyServiceName}') then
    StopAndDeleteService('{#MyServiceName}');

  Exec(
    'sc.exe',
    'create {#MyServiceName} binPath= "' + SvcPath + '" start= auto',
    '',
    SW_HIDE,
    ewWaitUntilTerminated,
    ResultCode
  );

  Exec(
    'sc.exe',
    'start {#MyServiceName}',
    '',
    SW_HIDE,
    ewWaitUntilTerminated,
    ResultCode
  );
  
  Exec(
  'sc.exe',
  'failure {#MyServiceName} reset= 0 actions= restart/5000/restart/5000/restart/5000',
  '',
  SW_HIDE,
  ewWaitUntilTerminated,
  ResultCode
);

end;


function InitializeSetup(): Boolean;
begin

  if ServiceExists('{#MyServiceName}') then
    StopAndDeleteService('{#MyServiceName}');

  Result := True;

end;


procedure CurStepChanged(CurStep: TSetupStep);
begin

  if CurStep = ssPostInstall then
    CreateAndStartService();

end;

procedure VpnCheckChanged(Sender: TObject);
begin
  WizardForm.NextButton.Enabled := VpnCheckBox.Checked;
end;


procedure CurPageChanged(CurPageID: Integer);
begin

  if Assigned(VpnWarnPage) and (CurPageID = VpnWarnPage.ID) then
    WizardForm.NextButton.Enabled := VpnCheckBox.Checked
  else
    WizardForm.NextButton.Enabled := True;

end;


procedure CreateVpnWarningPage;
var
  Text: TNewStaticText;
begin

  VpnWarnPage :=
    CreateCustomPage(
      wpWelcome,
      'Важно перед использованием',
      'VPN / Proxy могут мешать работе сервера'
    );

  Text := TNewStaticText.Create(VpnWarnPage);
  Text.Parent := VpnWarnPage.Surface;
  Text.Width := VpnWarnPage.SurfaceWidth;
  Text.Height := ScaleY(140);
  Text.WordWrap := True;

  Text.Caption :=
    'Использование VPN или Proxy может мешать подключению.' + #13#10 +
    'Это касается и мобильного клиента.' + #13#10#13#10 +
    'Перед использованием рекомендуется отключить VPN.';

  VpnCheckBox := TNewCheckBox.Create(VpnWarnPage);
  VpnCheckBox.Parent := VpnWarnPage.Surface;
  VpnCheckBox.Caption := 'Я понимаю возможные ограничения';
  VpnCheckBox.Width := VpnWarnPage.SurfaceWidth;
  VpnCheckBox.Top := Text.Top + Text.Height + ScaleY(10);
  VpnCheckBox.OnClick := @VpnCheckChanged;

end;


procedure InitializeWizard;
begin
  CreateVpnWarningPage;
end;