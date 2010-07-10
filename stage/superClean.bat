@ECHO OFF
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
REM //////////////////////////////////////////////////////////////////////////
REM LibUsbDotNet SuperClean batch file utility
REM Removes all files not included in a 'SVN Commit..'
REM
REM Cleans all directories ending in:
REM	  bin\debug
REM	  bin\release
REM   _UpgradeReport_Files
REM
REM Cleans all files ending in:
REM   UpgradeLog.XML
REM   .sln.cache
REM   .Userprefs
REM //////////////////////////////////////////////////////////////////////////
call clean.bat
.\Utility\RegexClean -d -r -m="(?i)\\(bin\\debug|bin\\release|_UpgradeReport_Files)$"
.\Utility\RegexClean -d -r -m="(?i)\\(bin)$"
.\Utility\RegexClean -r -m="(\\(UpgradeLog\.XML)|(\.sln\.cache)|\.Userprefs)$"

REM If the packager argument is present, the superCLean.bat
REM is being called from the make.bat utility during a build
REM proccess, so don't execute the following lines.
IF /I "%~1" EQU "packager" GOTO :EOF
IF EXIST ".\Docs\Help\" rmdir /S /Q .\Docs\Help
IF EXIST ".\build\PackageTemp\" rmdir /S /Q .\build\PackageTemp
IF EXIST .\build\CheckVars.txt del .\build\CheckVars.txt

