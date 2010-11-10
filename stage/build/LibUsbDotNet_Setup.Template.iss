; LibUsbDotNet Setup Script File

[Setup]
AppName                 =$(BaseName)
AppVerName              =$(FriendlyName)
VersionInfoDescription  =$(BaseName) Setup
VersionInfoVersion      =$(Version)
AppPublisher        =Travis Robinson
AppPublisherURL     =http://sourceforge.net/projects/libusbdotnet/
AppSupportURL       =http://libusbdotnet.sourceforge.net/
AppUpdatesURL       =http://sourceforge.net/projects/libusbdotnet/
AppCopyright        =Copyright (C) 2010 Travis Robinson.  All rights reserved.
DefaultDirName      ={pf}\$(BaseName)
DefaultGroupName    =$(BaseName)
AllowNoIcons        =yes
LicenseFile         = copyright.txt
InfoBeforeFile      = welcome.txt
;InfoAfterFile       = final.txt
OutputBaseFilename  =$(BaseName)_Setup.$(FriendlyVersion)
Compression         =lzma
SolidCompression    =yes
; requires Win2k, or higher
MinVersion = 0, 5.0.2195
OutputManifestFile=Setup-Manifest.txt

; "ArchitecturesInstallIn64BitMode=x64 ia64" requests that the install
; be done in "64-bit mode" on x64 & Itanium, meaning it should use the
; native 64-bit Program Files directory and the 64-bit view of the
; registry. On all other architectures it will install in "32-bit mode".
ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Icons]
Name: "{group}\libUsb-win32\Filter Wizard"; Filename: {app}\libusb-win32\install-filter-win.exe; Flags: createonlyiffileexists;
Name: "{group}\libUsb-win32\Filter Console Help"; Filename: {app}\libusb-win32\install-filter-help.txt; Flags: createonlyiffileexists;
Name: "{group}\libUsb-win32\Class Filter\Install all class filters"; Filename: {app}\libusb-win32\install-filter-win.exe; Parameters:"i -ac -p -w"; Flags: createonlyiffileexists; Comment: "Installs all libusb-win32 class filters."
Name: "{group}\libUsb-win32\Class Filter\Remove all class filters";  Filename: {app}\libusb-win32\install-filter-win.exe; Parameters:"u -ac -w"; Flags: createonlyiffileexists; Comment: "Removes all libusb-win32 class filters."

Name: {group}\libUsb-win32\Inf-Wizard; Filename: {app}\libusb-win32\inf-wizard.exe; WorkingDir: {app}\libusb-win32; Flags: createonlyiffileexists
Name: {group}\LibUsbDotNet Help; Filename: {app}\LibUsbHelp.chm; WorkingDir: {app}\Docs; Flags: createonlyiffileexists
Name: {group}\USB InfWizard; Filename: {app}\InfWizard.exe; WorkingDir: {app}; Flags: createonlyiffileexists
Name: {group}\Test Info; Filename: {app}\Test_Info.exe; WorkingDir: {app}; Flags: createonlyiffileexists
Name: {group}\Test Device Notify; Filename: {app}\Test_DeviceNotify.exe; WorkingDir: {app}; Flags: createonlyiffileexists
Name: {group}\Uninstall LibUsbDotNet; Filename: {uninstallexe}; WorkingDir: {app}; Flags: createonlyiffileexists

[Code]
function IsX64: Boolean;
begin
  Result := Is64BitInstallMode and (ProcessorArchitecture = paX64);
end;

function IsI64: Boolean;
begin
  Result := Is64BitInstallMode and (ProcessorArchitecture = paIA64);
end;

function IsX86: Boolean;
begin
  Result := not IsX64 and not IsI64;
end;

[Types]
Name: full; Description: Full Installation
Name: custom; Description: Custom Installation; flags: iscustom

[Components]
Name: dll; Description: Runtime Files; Types: full custom; flags: exclusive
Name: source; Description: Source and Example Code; Types: full custom

[Tasks]
Name: tasklibusb;  Description: Install libusb-win32 with filter capabilities?; GroupDescription: libusb-win32; Components: dll

[Files]
; LibUsb-win32 x86
Source: "$(LIBUSB_WIN32_BIN)\x86\libusb0_x86.dll"; DestName: libusb0.dll; DestDir: {sys}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX86; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\x86\libusb0.sys"; DestDir: {sys}\drivers; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX86; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\x86\install-filter.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX86; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\x86\install-filter-win.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion; Check: IsX86; Tasks: tasklibusb

; LibUsb-win32 AMD 64bit
Source: "$(LIBUSB_WIN32_BIN)\x86\libusb0_x86.dll"; DestName: libusb0.dll; DestDir: {syswow64}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\amd64\libusb0.sys"; DestDir: {sys}\drivers; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\amd64\libusb0.dll"; DestDir: {sys}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\amd64\install-filter.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\amd64\install-filter-win.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion; Check: IsX64; Tasks: tasklibusb

; LibUsb-win32 Itanium 64bit
Source: "$(LIBUSB_WIN32_BIN)\x86\libusb0_x86.dll"; DestName: libusb0.dll; DestDir: {syswow64}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsI64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\ia64\libusb0.sys"; DestDir: {sys}\drivers; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsI64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\ia64\libusb0.dll"; DestDir: {sys}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsI64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\ia64\install-filter.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsI64; Tasks: tasklibusb
Source: "$(LIBUSB_WIN32_BIN)\ia64\install-filter-win.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion; Check: IsI64; Tasks: tasklibusb

; LibUsb-win32 Common
Source: "$(LIBUSB_WIN32_BIN)\inf-wizard.exe"; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Tasks: tasklibusb

; Runtime Files
Source: .\bin\*; DestDir: {app}; Flags: recursesubdirs createallsubdirs ignoreversion; Components: dll

; Source and Sample Code
Source: .\src\*; DestDir: {app}\Src; Flags: ignoreversion recursesubdirs createallsubdirs; Components: source

[Run]
Filename: "{app}\libusb-win32\install-filter-win.exe"; Description: "Launch filter installer wizard"; Flags: postinstall nowait runascurrentuser; Tasks: tasklibusb;


