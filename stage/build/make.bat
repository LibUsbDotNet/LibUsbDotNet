@ECHO OFF
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
SET LUDN_ERRORLEVEL=0
SET LUDN_QUIET_MODE=
SET LUDN_ERROR=

SET LibUsbDotNet_Cfg=LibUsbDotNet.Cfg

:Begin
cls
CALL :SetColor 07
CALL :LoadConfig

IF /I "%~1" EQU "MergeChangeHistory" (
	CALL :MergeChangeHistory
	GOTO :EOF
)

IF /I "%~1" EQU "UpdateRevisionFromSvn" (
	CALL :UpdateRevisionFromSvn
	GOTO :EOF
)

IF /I "%~1" EQU "FinalizeRelease" (
	CALL :FinalizeRelease
	GOTO :EOF
)

IF /I "%~1" EQU "UpdateAssemblyInfo" (
	CALL :UpdateAssemblyInfoVersions "!CMDVAR_VERSION!" "!CMDVAR_BASEDIR!AssemblyInfo.cs" -rd
	CALL :UpdateXmlElement HelpTitle "!CMDVAR_FRIENDLYNAME!" "!CMDVAR_SHFBPROJECTDIR!!CMDVAR_SHFBPROJECTNAME!"
	CALL :UpdateXmlElement HelpFileVersion "!CMDVAR_VERSION!" "!CMDVAR_SHFBPROJECTDIR!!CMDVAR_SHFBPROJECTNAME!"
	GOTO :EOF
)

CALL :Status "Loading Config '!LibUsbDotNet_Cfg!'"
CALL :LoadConfig

ECHO !CMDVAR_BaseName! v!CMDVAR_Version! started at !TIME!..

:MoreParams
IF "%~1"=="" GOTO NoMoreParams
ECHO %~1|FINDSTR /R /C:"^[ ]*[a-z_][a-z0-9_]*=.*" >_ParamVal.tmp
FOR /F "eol=; tokens=1,2* usebackq delims==" %%I IN (_ParamVal.tmp) DO (
	SET CMDVAR_%%I=%%~J
)
SHIFT /1
GOTO MoreParams
:NoMoreParams
IF EXIST _ParamVal.tmp DEL _ParamVal.tmp

IF /I "!CMDVAR_WARN!" EQU "warn" SET CMDVAR_WARN=true
IF /I "!CMDVAR_QUIET!" EQU "quiet" SET CMDVAR_QUIET=true

SET LUDN_QUIET_MODE=""
SET LUDN_QUIET_MODE2=""
IF /I "!CMDVAR_QUIET!" EQU "true" SET LUDN_QUIET_MODE="2>NUL >NUL"
IF /I "!CMDVAR_QUIET!" EQU "true" SET LUDN_QUIET_MODE2=">NUL"


IF EXIST "CheckVars.txt" DEL "CheckVars.txt"
CALL :TagEnv "'CheckVars.txt.in' -o='CheckVars.txt'"

CALL :ReCreateDir "!CMDVAR_TEMPDIR!"
CALL :ToAbsoutePaths CMDVAR_TEMPDIR "!CMDVAR_TEMPDIR!"

CALL :Status "Building Solution '!CMDVAR_SLN!'"
CALL :BuildDotNetSln "!CMDVAR_SLN!" /rebuild

CALL :Status "Building SHFB Documentation"
CALL :BuildSHFB_Docs

CALL :Status "Packaging !CMDVAR_FULLNAME! Bin Files"
CALL :Package_Bin

CALL :Status "Cleaning"
CALL :SuperClean

CALL :Status "Packaging !CMDVAR_FULLNAME! Src Files"
CALL :Package_Src

CALL :Status "Packaging !CMDVAR_FULLNAME! Setup Files"
CALL :Package_Setup

CALL :Status "!CMDVAR_FULLNAME! Build Successful" title false
CALL :SetColor 07
EXIT

:LoadConfig
	IF NOT EXIST "!LibUsbDotNet_Cfg!" (
		SET LUDN_ERROR=Config file not found "!LibUsbDotNet_Cfg!".
		GOTO ErrorWaitExit
	)
	FOR /F "eol=; tokens=1,2* usebackq delims==" %%I IN (!LibUsbDotNet_Cfg!) DO (
	
		IF NOT "%%~I" EQU "" (
			SET _PNAME=%%~I
			SET _PNAME=!_PNAME: =!
			IF /I "!_PNAME!" EQU "DevEnv" (
				CALL :ToShortNames _PVALUE "%%~J"
				ECHO !_PVALUE!
			) ELSE (
				SET _PVALUE=%%J
			)
			SET CMDVAR_!_PNAME!=!_PVALUE!
		)
	)

	CALL :ToAbsoutePaths CMDVAR_SLN "!CMDVAR_SLN!"
	CALL :ToAbsoutePaths CMDVAR_TOOLPATH "!CMDVAR_TOOLPATH!"
	CALL :ToAbsoutePaths CMDVAR_BASEDIR "!CMDVAR_BASEDIR!"
	CALL :ToAbsoutePaths CMDVAR_DEVENV "!CMDVAR_DEVENV!"
	CALL :ToAbsoutePaths CMDVAR_SHFBPROJECTDIR "!CMDVAR_SHFBPROJECTDIR!"
	CALL :ToAbsoutePaths CMDVAR_SHFBHELPFILE "!CMDVAR_SHFBHELPFILE!"

	IF "!CMDVAR_BASENAME!" EQU "" (
		SET LUDN_ERROR=Could not open/read config file "!LibUsbDotNet_Cfg!".
		GOTO ErrorWaitExit
	)
	IF NOT EXIST "!CMDVAR_SLN!" (
		SET LUDN_ERROR=Solution "!CMDVAR_SLN!" not found.
		GOTO ErrorWaitExit
	)
	IF NOT EXIST "!CMDVAR_TOOLPATH!" (
		SET LUDN_ERROR=ToolPath "!CMDVAR_TOOLPATH!" not found.
		GOTO ErrorWaitExit
	)
	IF NOT EXIST "!CMDVAR_DEVENV!" (
		SET LUDN_ERROR=DevEnv "!CMDVAR_DEVENV!" not found.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:IncConfigValue
	CALL :Status "Incrementing %1 number in !LibUsbDotNet_Cfg!"
	"!CMDVAR_TOOLPATH!RegRep" !LibUsbDotNet_Cfg! -m="(?mi:(?<=^%1[ \\t]*=[ \\t]*)([0-9]+))" -r="\e{\01+1}" > NUL
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=IncConfigValue Failed.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:SetConfigValue
	CALL :Status "setting %1 in !LibUsbDotNet_Cfg!"
	"!CMDVAR_TOOLPATH!RegRep" !LibUsbDotNet_Cfg! -m="(?mi:(?<=^%1[ \\t]*=[ \\t]*)([0-9]+))" -r="%2" > NUL
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=SetConfigValue Failed.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:UpdateAssemblyInfoVersions
	"!CMDVAR_TOOLPATH!RegRep" -m="([ \t]*\[assembly[ \r\n\f\t]*?:[ \r\n\f\t]*?(?:AssemblyVersion|AssemblyFileVersion)[ \r\n\f\t]*?\([ \r\n\f\t]*?\x22)([0-9]+)\.([0-9]+)\.([0-9]+)\.([0-9*]+)(\x22[ \r\n\f\t]*?\)[ \r\n\f\t]*?\])" -r="\01%~1\06" %2 %3 %4 %5
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=UpdateAssemblyInfoVersions Failed.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:ToAbsoutePaths
	IF NOT "%~1" EQU "" (
		SET %~1=%~f2
		SHIFT /1
		SHIFT /1
		GOTO ToAbsoutePaths
	)
GOTO :EOF

:ToShortNames
	IF NOT "%~1" EQU "" (
		SET %~1=%~s2
		SHIFT /1
		SHIFT /1
		GOTO ToShortNames
	)
GOTO :EOF

:TagEnv
	IF NOT DEFINED _LTAG_ SET _LTAG_=\$\(
	IF NOT DEFINED _RTAG_ SET _RTAG_=\)
	FOR /F "tokens=1,* usebackq delims==" %%I IN (`SET CMDVAR_`) DO (
		SET _TagEnvTag_=%%I
		SET _TagEnvTag_=!_LTAG_!!_TagEnvTag_:~7!!_RTAG_!
		SET _TagEnvRegRep_=!_TagEnvRegRep_! -m="!_TagEnvTag_!" -r"%%~J"
	)
	"!CMDVAR_TOOLPATH!RegRep" !_TagEnvRegRep_! %~1 !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=TagEnv Failed "!CMDVAR_TOOLPATH!RegRep" !_TagEnvRegRep_! %~1.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:BuildDotNetSln
	IF "%~3" EQU "" (
		SET __BuildConfig__="Release|Any CPU"
	) ELSE (
		SET __BuildConfig__="%~3"
	)
	"!CMDVAR_DEVENV!" "%~1" %~2 !__BuildConfig__! !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Error Building Solution "%~1" /Build !__BuildConfig__!.
		GOTO ErrorWaitExit
	)	
GOTO :EOF

:GetPath
	SET %1=%~d2%~p2
GOTO :EOF

:UpdateRevisionFromSvn
	ECHO Getting revision info from svn repository..
	svn info !CMDVAR_SvnRepository! > _tempSvn.info.txt

	FOR /F "eol=; tokens=1,2* usebackq delims=:" %%I IN (_tempSvn.info.txt) DO (
		IF "%%I" EQU "Revision" SET /A SVN_REVISION=%%J+1
	)
	
	IF EXIST _tempSvn.info.txt DEL _tempSvn.info.txt
	
	IF "!SVN_REVISION!" EQU "" (
		ECHO Revision field not found in 'svn info' output.
		GOTO :EOF
	)
	:SVN_REVISION_FOUND
	CALL :SetConfigValue revision !SVN_REVISION!
GOTO :EOF

:MergeChangeHistory
	CALL :GetPath TEMPAML "!CMDVAR_ChangeHistoryTemplate!" 
	SET TEMPAML=!TEMPAML!LibUsbDotNet.Change.History.Temp.aml
	SET MAINAML=!CMDVAR_ChangeHistoryMaster!
	SET CURRAML=!CMDVAR_ChangeHistoryTemplate!
	
	CALL :TagEnv "'!CURRAML!' -o='!TEMPAML!'"
	
    "!CMDVAR_TOOLPATH!RegRep" "!MAINAML!" -m="(?s:[ \t]*<releaseNotes[ \t]+address=\x22!CMDVAR_BaseName!_Changes_v!CMDVAR_Major!\.!CMDVAR_Minor!\.!CMDVAR_Build!\x22>.+?</releaseNotes>[ \t]*[\f\r\n]+)" -r=""

    REM "!CMDVAR_TOOLPATH!RegRep" "!MAINAML!" -m="<\x21-- CurrentReleaseNotes -->" -f="!TEMPAML!"
    "!CMDVAR_TOOLPATH!RegRep" "!MAINAML!" -m="(?<=<\x21--[ \t]*CurrentReleaseNotes[ \t]*-->[ \t]*)(\r\n)" -f="!TEMPAML!"

    DEL "!TEMPAML!"
    
    CALL :MakeTextReleaseNotes
    
GOTO :EOF

:FinalizeRelease
	ECHO WARNING:
	ECHO Moves the current change history to previous change history.
	ECHO Updates the build number in the 'LibUsbDotNet.cfg' config file.
	SET /P CONTINUE_FinalizeRelease=Type 'yes' to continue:
	IF /I "!CONTINUE_FinalizeRelease!" NEQ "yes" GOTO :EOF
	
	CALL :GetPath TEMPAML "!CMDVAR_ChangeHistoryTemplate!" 
	SET TEMPAML=!TEMPAML!LibUsbDotNet.Change.History.Temp.aml
	SET MAINAML=!CMDVAR_ChangeHistoryMaster!

    "!CMDVAR_TOOLPATH!RegRep" "!MAINAML!" -mo="(?s:[ \t\r\n]*<releaseNotes[ \t]+address=\x22!CMDVAR_BaseName!_Changes_v!CMDVAR_Major!\.!CMDVAR_Minor!\.!CMDVAR_Build!\x22>.+?</releaseNotes>[ \t]*[\f\r\n]+)" -r="" -o="!TEMPAML!"

    "!CMDVAR_TOOLPATH!RegRep" "!MAINAML!" -m="(?s:[ \t]*<releaseNotes[ \t]+address=\x22!CMDVAR_BaseName!_Changes_v!CMDVAR_Major!\.!CMDVAR_Minor!\.!CMDVAR_Build!\x22>.+?</releaseNotes>[ \t]*[\f\r\n]+)" -r=""
    
    "!CMDVAR_TOOLPATH!RegRep" "!MAINAML!" -m="(?<=<\x21--[ \t]*Previous[ \t]*Release[ \t]*Notes[ \t]*-->[ \t]*)(\r\n)" -f="!TEMPAML!"

    DEL "!TEMPAML!"
    
    CALL :IncConfigValue Build
	pause
    
GOTO :EOF

:MakeTextReleaseNotes
	!CMDVAR_LudnReleaseNotes! "!CMDVAR_ChangeHistoryMaster!" "!CMDVAR_SHFBPROJECTDIR!Release_Notes.!CMDVAR_FriendlyVersion!.txt" "!CMDVAR_BASENAME! Version History"
GOTO :EOF

:BuildSHFB_Docs
	PUSHD "!CD!"
	CD "!CMDVAR_SHFBPROJECTDIR!"
	"!CMDVAR_SHFBCONSOLE!" /p:Configuration=Release "!CMDVAR_SHFBPROJECTNAME!" !LUDN_QUIET_MODE2:~1,-1!
	SET LUDN_ERRORLEVEL=!ERRORLEVEL!
	POPD
	
	IF !LUDN_ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Error Building SHFB documentation "!CMDVAR_SHFBPROJECTDIR!!CMDVAR_SHFBPROJECTNAME!".
		GOTO ErrorWaitExit
	)
	
	CALL :MakeTextReleaseNotes
	
GOTO :EOF

:Package_Bin
	SET _SRC_=!CMDVAR_BASEDIR!Bin\Release\
	SET _DST_=!CMDVAR_TEMPDIR!Bin\
	CALL :ReCreateDir "!_DST_!"
	
	CALL :CopyFile Benchmark.exe
	CALL :CopyFile BenchmarkCon.exe
	CALL :CopyFile InfWizard.exe
	CALL :CopyFile LibUsbDotNet.dll
	CALL :CopyFile LibUsbDotNet.xml
	CALL :CopyFile Test_Bulk.exe
	CALL :CopyFile Test_DeviceNotify.exe
	CALL :CopyFile Test_Info.exe
	
	CALL :Copy_ChmHelp "!_DST_!"
	CALL :Copy_License "!_DST_!"
	
	PUSHD "!CD!"
	CD /D "!CMDVAR_TEMPDIR!"
	"!CMDVAR_ZIP!" -tzip a -r "!CMDVAR_TEMPDIR!!CMDVAR_BASENAME!_Bin.!CMDVAR_FRIENDLYVERSION!.zip" ".\Bin\*" !LUDN_QUIET_MODE2:~1,-1!
	SET LUDN_ERRORLEVEL=!ERRORLEVEL!
	POPD
	
	IF !LUDN_ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Package_Bin Error.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:Package_Src
	CALL :SuperClean

	
	SET _SRC_=!CMDVAR_BASEDIR!
	SET _DST_=!CMDVAR_TEMPDIR!Src\
	CALL :ReCreateDir "!_DST_!"
	CALL :CopyDirs "!CMDVAR_BASEDIR!Benchmark\*" "!_DST_!Benchmark\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!BenchmarkCon\*" "!_DST_!BenchmarkCon\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!Test_Bulk\*" "!_DST_!Test_Bulk\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!Test_Info\*" "!_DST_!Test_Info\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!Examples\*" "!_DST_!Examples\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!InfWizard\*" "!_DST_!InfWizard\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!LibWinUsb\*" "!_DST_!LibWinUsb\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!MonoLibUsb\*" "!_DST_!MonoLibUsb\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!Utility\*" "!_DST_!Utility\"
	CALL :CopyDirs "!CMDVAR_BASEDIR!Test_DeviceNotify\*" "!_DST_!Test_DeviceNotify\"
	
	CALL :Copy_ChmHelp "!_DST_!"
	CALL :Copy_License "!_DST_!"
	
	SET _SRC_=!CMDVAR_BASEDIR!
	CALL :CopyFile LibUsbDotNet.sln
	CALL :CopyFile All.sln
	
	CALL :TagEnv "'!CMDVAR_BASENAME!_Setup.Template.iss' -o='!_DST_!!CMDVAR_BASENAME!_Setup.iss'"
	
	PUSHD "!CD!"
	CD /D "!CMDVAR_TEMPDIR!"
	"!CMDVAR_ZIP!" -tzip a -r "!CMDVAR_TEMPDIR!!CMDVAR_BASENAME!_Src.!CMDVAR_FRIENDLYVERSION!.zip" ".\Src\*" !LUDN_QUIET_MODE2:~1,-1!
	POPD
		
GOTO :EOF

:Package_Setup
	DEL /Q "!CMDVAR_TEMPDIR!Src\LibUsbHelp.chm"
	IF !ERRORLEVEL! NEQ 0 SET LUDN_ERRORLEVEL=1
	DEL /Q "!CMDVAR_TEMPDIR!Src\lgpl-3.0.txt"
	IF !ERRORLEVEL! NEQ 0 SET LUDN_ERRORLEVEL=1
	DEL /Q "!CMDVAR_TEMPDIR!Src\gpl-3.0.txt"
	IF !ERRORLEVEL! NEQ 0 SET LUDN_ERRORLEVEL=1
	IF !LUDN_ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Package_Setup failed. No Bin/src files.
		GOTO ErrorWaitExit
	)
	
	CALL :TagEnv "'!CMDVAR_BASENAME!_Setup.Template.iss' -o='!CMDVAR_TEMPDIR!!CMDVAR_BASENAME!_Setup.iss'"
	
	SET _SRC_=!CD!\
	SET _DST_=!CMDVAR_TEMPDIR!
	CALL :CopyFile welcome.txt
	CALL :CopyFile copyright.txt
	IF EXIST "!CMDVAR_BASEDIR!Docs\Release_Notes.!CMDVAR_FRIENDLYVERSION!.txt" COPY /Y "!CMDVAR_BASEDIR!Docs\Release_Notes.!CMDVAR_FRIENDLYVERSION!.txt" "!CMDVAR_TEMPDIR!Welcome.txt" !LUDN_QUIET_MODE2:~1,-1!
		
	PUSHD "!CD!"
	CD /D "!CMDVAR_TEMPDIR!"
	"!CMDVAR_ISCC!" "!CMDVAR_BASENAME!_Setup.iss" !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 SET LUDN_ERRORLEVEL=1
	POPD
	
	IF !LUDN_ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Package_Setup failed. "!CMDVAR_BASENAME!_Setup.iss" /q.
		GOTO ErrorWaitExit
	)
	
	PUSHD "!CD!"
	CD /D "!CMDVAR_TEMPDIR!"
	RMDIR /S /Q "!CMDVAR_TEMPDIR!Bin" !LUDN_QUIET_MODE2:~1,-1!
	RMDIR /S /Q "!CMDVAR_TEMPDIR!Src" !LUDN_QUIET_MODE2:~1,-1!
	
	SET _SRC_=!CMDVAR_TEMPDIR!Output\
	SET _DST_=!CMDVAR_TEMPDIR!
	CALL :CopyFile !CMDVAR_BaseName!_Setup.!CMDVAR_FriendlyVersion!.exe
	
	RMDIR /S /Q "!CMDVAR_TEMPDIR!Output" !LUDN_QUIET_MODE2:~1,-1!

	MKDIR "!CMDVAR_TEMPDIR!Help"
	CALL :CopyDirs "!CMDVAR_SHFBProjectDir!Help\*" "!CMDVAR_TEMPDIR!Help\"

	REN welcome.txt "Release_Notes.!CMDVAR_FRIENDLYVERSION!.txt" !LUDN_QUIET_MODE2:~1,-1!
	DEL /Q copyright.txt !LUDN_QUIET_MODE2:~1,-1!
	DEL /Q "!CMDVAR_BASENAME!_Setup.iss" !LUDN_QUIET_MODE2:~1,-1!
	
	POPD


GOTO :EOF

:Copy_License
	CALL :CopySingleFile "!CMDVAR_BASEDIR!lgpl-3.0.txt" %1
	CALL :CopySingleFile "!CMDVAR_BASEDIR!gpl-3.0.txt" %1
GOTO :EOF

:Copy_ChmHelp
	CALL :CopySingleFile "!CMDVAR_SHFBHELPFILE!" %1
GOTO :EOF

:ReCreateDir
	IF EXIST "%~1" RMDIR /S /Q "%~1"
	MKDIR "%~1"
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Failed re-creating directory "%~1".
		GOTO ErrorWaitExit
	)
GOTO :EOF

:CopyFile
	SET _COPYFILE_DEST_=!_DST_!
	IF "%~2" NEQ "" SET _COPYFILE_DEST_="%~2"
	COPY /Y "!_SRC_!%~1" "!_COPYFILE_DEST_!" !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Error copying file "!_SRC_!%~1" to "!_COPYFILE_DEST_!".
		GOTO ErrorWaitExit
	)
GOTO :EOF

:CopySingleFile
	COPY /Y %1 %2 %3 %4 %5 %6 %7 %8 %9 !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Error copying file %1 %2 %3 %4 %5 %6 %7 %8 %9.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:CopyDirs
	IF NOT EXIST "%~2" MKDIR "%~2"
	XCOPY /I /S /Y "%~1" "%~2" !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=Error copying "%~1" "%~2"
		GOTO ErrorWaitExit
	)
GOTO :EOF

:UpdateXmlElement
	"!CMDVAR_TOOLPATH!RegRep" -m="(?<=<%~1>)(.*?)(?=</%~1>)" -r="%~2" %3 %4 %5 %6 %7 %8 %9 !LUDN_QUIET_MODE2:~1,-1!
	IF !ERRORLEVEL! NEQ 0 (
		SET LUDN_ERROR=UpdateXmlElement "%~1" Failed.
		GOTO ErrorWaitExit
	)
GOTO :EOF

:SuperClean
	PUSHD "!CD!"
	CD "!CMDVAR_BASEDIR!"
	CALL superClean.bat packager !LUDN_QUIET_MODE2:~1,-1!
	POPD
GOTO :EOF

:SetColor
	COLOR %1
GOTO :EOF

:Status
	SET _LAST_STATUS_TEXT_=%~1
	IF "%~3" NEQ "false" SET _LAST_STATUS_TEXT_=!_LAST_STATUS_TEXT_!..
	IF /I "%~2" EQU ""		TITLE  !_LAST_STATUS_TEXT_!
	IF /I "%~2" EQU "title" TITLE  !_LAST_STATUS_TEXT_!
	ECHO !_LAST_STATUS_TEXT_!
GOTO :EOF

:ErrorWaitExit
	CALL :SetColor 0C
	ECHO BUILD FAILED.
	ECHO !LUDN_ERROR!
	pause
	CALL :SetColor 07
EXIT 1
