@ECHO OFF
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
IF "%~1" NEQ "" GOTO Begin

ECHO ---------------------------------------------------------------------------
ECHO ////////////////////////LibUsb-Win32 Build Utility/////////////////////////
ECHO Example: CMD /C make.bat "arch=x86" "app=all" "quiet=true" "outdir=.\x86"
ECHO Where:
ECHO REQUIRED      arch=[x86/i64/x64]
ECHO REQUIRED      app=[all/dll/driver/inf_wizard/install_filter]
ECHO OPTIONAL      quiet=[true/false]
ECHO OPTIONAL      warn=[true/false]
ECHO OPTIONAL      outdir=Path to the final build output directory.
ECHO OPTIONAL      winddk=Full path to the windows DDK installation (see below)
ECHO ///////////////////////////////////////////////////////////////////////////
GOTO :EOF

:Begin
SET _WINDDK_DIR=Z:\WinDDK\7600.16385.0\

IF EXIST _ParamVal.tmp DEL _ParamVal.tmp
:MoreParams
IF "%~1"=="" GOTO NoMoreParams
ECHO %~1|FINDSTR /R /C:"^[ ]*[a-z_][a-z0-9_]*=.*" >_ParamVal.tmp
FOR /F "eol=; tokens=1,2* usebackq delims==" %%i IN (_ParamVal.tmp) DO (
	SET CMDVAR_%%i=%%j
)
SHIFT /1
GOTO MoreParams
:NoMoreParams
IF EXIST _ParamVal.tmp DEL _ParamVal.tmp

IF "!CMDVAR_WINDDK!" NEQ "" (
	SET _WINDDK_DIR=!CMDVAR_WINDDK!
)

IF /I "!CMDVAR_WARN!" EQU "warn" SET CMDVAR_WARN=true
IF /I "!CMDVAR_QUIET!" EQU "quiet" SET CMDVAR_QUIET=true

IF /I "!CMDVAR_QUIET!" EQU "true" (
	SET _QUIET_MODE="2>NUL>NUL"
) ELSE (
	SET _QUIET_MODE=
)

IF NOT EXIST "!_WINDDK_DIR!" (
	SET BUILD_ERROR_LEVEL=1
	ECHO Invalid WinDDK Directory!
	ECHO %_WINDDK_DIR%
	GOTO WaitForExit
)

SET _LIBUSB_OUTPUT_DIR=!CMDVAR_OUTDIR!
IF "%_LIBUSB_OUTPUT_DIR%" EQU "" SET _LIBUSB_OUTPUT_DIR=.\%_BUILDARCH%
IF EXIST "%_LIBUSB_OUTPUT_DIR%" RMDIR /Q /S "%_LIBUSB_OUTPUT_DIR%"
MKDIR "%_LIBUSB_OUTPUT_DIR%"
SET BUILD_ERRORLEVEL=%ERRORLEVEL%
IF NOT %BUILD_ERRORLEVEL%==0 (
	ECHO Failed creating output directory!
	ECHO %_LIBUSB_OUTPUT_DIR%  
	GOTO WaitForExit
)

IF        /I "!CMDVAR_ARCH!" EQU "x86" (
	CALL :SetDDK !_QUIET_MODE! fre WXP
) ELSE IF /I "!CMDVAR_ARCH!" EQU "x64" (
	CALL :SetDDK !_QUIET_MODE! fre x64 WNET
) ELSE IF /I "!CMDVAR_ARCH!" EQU "i64" (
	CALL :SetDDK !_QUIET_MODE! fre 64 WNET
) ELSE (
	ECHO Invalid argument!
	ECHO %1
	GOTO WaitForExit
)

call make_clean.bat %_QUIET_MODE%

IF /I "!CMDVAR_APP!" EQU "all" (
	SET _LIBUSB_APP=Dll
	CALL :Build %_QUIET_MODE%
	
	SET _LIBUSB_APP=Driver
	CALL :Build %_QUIET_MODE%
	
	SET _LIBUSB_APP=Inf_Wizard
	CALL :Build %_QUIET_MODE%
	
	SET _LIBUSB_APP=Install_Filter
	CALL :Build %_QUIET_MODE%
	
	CALL :CopyToOutOutDir %_QUIET_MODE%
) ELSE (
	SET _LIBUSB_APP=!CMDVAR_APP!
	CALL :Build %_QUIET_MODE%
	CALL :CopyToOutOutDir %_QUIET_MODE%
)

EXIT /B

:CopyToOutOutDir
	IF EXIST *.sys MOVE /Y *.sys "%_LIBUSB_OUTPUT_DIR%" %~1
	IF EXIST *.dll MOVE /Y *.dll "%_LIBUSB_OUTPUT_DIR%" %~1
	IF EXIST *.exe MOVE /Y *.exe "%_LIBUSB_OUTPUT_DIR%" %~1
	IF EXIST *.lib MOVE /Y *.lib "%_LIBUSB_OUTPUT_DIR%" %~1
GOTO :EOF

:Build
	CALL make_clean.bat %1
	CALL make_!_LIBUSB_APP!.bat %1
	IF NOT %ERRORLEVEL%==0 (
		SET BUILD_ERROR_LEVEL=1
		ECHO Application batch file not found or contains errors.
		ECHO make_!_LIBUSB_APP!.bat
		GOTO WaitForExit
	)
	SET _title=Building LibUsb !_LIBUSB_APP! (%_BUILDARCH% - %_BuildType%)
	title !_title!
	build %~1
	SET BUILD_ERRORLEVEL=%ERRORLEVEL%
	IF NOT %BUILD_ERRORLEVEL%==0 GOTO BuildError
	CALL make_clean.bat %1
	IF EXIST libusb0.lib move libusb0.lib libusb.lib %~1
	
	IF /I "!CMDVAR_WARN!" EQU "true" (
		CALL :SetColor 0e
		IF EXIST ".\build%BUILD_ALT_DIR%.wrn" TYPE ".\build%BUILD_ALT_DIR%.wrn"
		CALL :SetColor 07
	)
	CALL :SetColor 0b
	ECHO !_LIBUSB_APP! (%_BUILDARCH% - %_BuildType%) : Build Successful
	CALL :SetColor 07
GOTO :EOF

:SetDDK
	PUSHD %CD%
	CALL !_WINDDK_DIR!\bin\setenv.bat !_WINDDK_DIR! %2 %3 %4 %~1
	SET BUILD_ERRORLEVEL=%ERRORLEVEL%
	POPD
	IF NOT !BUILD_ERRORLEVEL!==0 (
		ECHO Failed setting DDK environment!
		GOTO WaitForExit
	)
GOTO :EOF

:SetColor
	IF EXIST "..\cc.exe" "..\cc.exe" %1
GOTO :EOF

:BuildError
	title Error !_title!
	CALL :SetColor 0c
	ECHO !_LIBUSB_APP! (%_BUILDARCH% - %_BuildType%) : Build Failed
	
	IF EXIST ".\build%BUILD_ALT_DIR%.err" (
		TYPE ".\build%BUILD_ALT_DIR%.err"
	) ELSE (
		ECHO Unable to find .\build%BUILD_ALT_DIR%.err!
	)
	CALL :SetColor 07
GOTO WaitForExit

:WaitForExit
	pause
	EXIT !BUILD_ERRORLEVEL!
