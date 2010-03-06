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
AppCopyright        =Copyright (C) 2009 Travis Robinson.  All rights reserved.
DefaultDirName      ={pf}\$(BaseName)
DefaultGroupName    =$(BaseName)
AllowNoIcons        =yes
LicenseFile         = copyright.txt
InfoBeforeFile      = welcome.txt
;InfoAfterFile       = final.txt
OutputBaseFilename  =$(BaseName)_Setup.$(FriendlyVersion)
Compression         =lzma
SolidCompression    =yes
; requires Win98SE, Win2k, or higher
MinVersion =4.1.2222, 5
PrivilegesRequired=admin
OutputManifestFile=Setup-Manifest.txt

; "ArchitecturesInstallIn64BitMode=x64 ia64" requests that the install
; be done in "64-bit mode" on x64 & Itanium, meaning it should use the
; native 64-bit Program Files directory and the 64-bit view of the
; registry. On all other architectures it will install in "32-bit mode".
ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Icons]
Name: {group}\libUsb-win32\Enable filter service; Filename: {app}\libusb-win32\install-filter.exe; Parameters: -i; WorkingDir: {app}\libusb-win32; Flags: createonlyiffileexists
Name: {group}\libUsb-win32\Disable filter service; Filename: {app}\libusb-win32\install-filter.exe; Parameters: -u; WorkingDir: {app}\libusb-win32; Flags: createonlyiffileexists
Name: {group}\LibUsbDotNet Help; Filename: {app}\LibUsbHelp.chm; WorkingDir: {app}\Docs; Flags: createonlyiffileexists
Name: {group}\USB Inf Creation Wizard; Filename: {app}\InfWizard.exe; WorkingDir: {app}\release; Flags: createonlyiffileexists
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
Name: tasklibusb;  Description: Install LibUsbDotNet-libusb-win32 Generic USB Driver?; GroupDescription: LibUsbDotNet-libusb-win32; Components: dll
Name: tasklibusb\insfilter; Flags:unchecked; Description: Enable libusb-win32 filter service? This allows access to USB devices without an INF file.  This can be loaded/unloaded at anytime from the LibUsbDotNet/libusb-win32 sub-menu.; GroupDescription: LibUsbDotNet-libusb-win32; Components: dll

[Files]
; LibUsb-win32 x86
Source: .\bin\libusb-win32\x86\libusb0.dll; DestDir: {sys}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX86; Tasks: tasklibusb
Source: .\bin\libusb-win32\x86\libusb0.sys; DestDir: {sys}\drivers; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX86; Tasks: tasklibusb
Source: .\bin\libusb-win32\x86\inf-wizard.exe; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX86; Tasks: tasklibusb
Source: .\bin\libusb-win32\x86\install-filter.exe; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX86; Tasks: tasklibusb

; LibUsb-win32 AMD 64bit
Source: .\bin\libusb-win32\x86\libusb0.dll; DestDir: {syswow64}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX64; Tasks: tasklibusb
Source: .\bin\libusb-win32\x64\libusb0.sys; DestDir: {sys}\drivers; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX64; Tasks: tasklibusb
Source: .\bin\libusb-win32\x64\libusb0.dll; DestDir: {sys}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsX64; Tasks: tasklibusb
Source: .\bin\libusb-win32\x64\inf-wizard.exe; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX64; Tasks: tasklibusb
;Source: .\bin\libusb-win32\x64\install-filter.exe; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsX64; Tasks: tasklibusb

; LibUsb-win32 Itanium 64bit
Source: .\bin\libusb-win32\x86\libusb0.dll; DestDir: {syswow64}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsI64; Tasks: tasklibusb
Source: .\bin\libusb-win32\I64\libusb0.sys; DestDir: {sys}\drivers; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsI64; Tasks: tasklibusb
Source: .\bin\libusb-win32\I64\libusb0.dll; DestDir: {sys}; Flags: uninsneveruninstall replacesameversion restartreplace promptifolder; Check: IsI64; Tasks: tasklibusb
Source: .\bin\libusb-win32\I64\inf-wizard.exe; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsI64; Tasks: tasklibusb
;Source: .\bin\libusb-win32\I64\install-filter.exe; DestDir: {app}\libusb-win32; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsI64; Tasks: tasklibusb

; Runtime Files
Source: .\bin\*; DestDir: {app}; Flags: recursesubdirs createallsubdirs ignoreversion; Components: dll

; Source and Sample Code
Source: .\src\*; DestDir: {app}\Src; Flags: ignoreversion recursesubdirs createallsubdirs; Components: source

[Run]
Filename: {app}\libusb-win32\install-filter.exe; Parameters: -i; StatusMsg: Creating kernel service (this may take a few seconds) ...; Tasks: tasklibusb\insfilter; Flags: skipifdoesntexist
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
