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
Name: {group}\LibUsbDotNet Help; Filename: {app}\LibUsbHelp.chm; WorkingDir: {app}\Docs; Flags: createonlyiffileexists
Name: {group}\USB InfWizard; Filename: {app}\InfWizard.exe; WorkingDir: {app}; Flags: createonlyiffileexists
Name: {group}\Test Info; Filename: {app}\Test_Info.exe; WorkingDir: {app}; Flags: createonlyiffileexists
Name: {group}\Test Device Notify; Filename: {app}\Test_DeviceNotify.exe; WorkingDir: {app}; Flags: createonlyiffileexists
Name: {group}\Uninstall LibUsbDotNet; Filename: {uninstallexe}; WorkingDir: {app}; Flags: createonlyiffileexists

[Files]

; Runtime Files
Source: .\bin\*; DestDir: {app}; Flags: recursesubdirs createallsubdirs ignoreversion;

; Source and Sample Code
Source: .\src\*; DestDir: {app}\Src; Flags: ignoreversion recursesubdirs createallsubdirs;

