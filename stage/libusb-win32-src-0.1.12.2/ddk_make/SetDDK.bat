@ECHO OFF
IF "%_CURDDKMODE%"=="%1%2%3" GOTO WinDDKAllreadySet
SET _CURDDKMODE="%1%2%3"
Echo Setting WinDDK..
SET __CD=%CD%
call Z:\WinDDK\7600.16385.0\bin\setenv.bat Z:\WinDDK\7600.16385.0\ %1 %2 %3
CD %__CD%
goto SetDDKDone
:WinDDKAllreadySet
ECHO WinDDK Allready Set to %1 %2 %3
pause
:SetDDKDone

