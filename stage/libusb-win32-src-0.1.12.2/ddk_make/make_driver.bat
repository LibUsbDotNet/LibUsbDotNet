@echo off

set SRC_DIR=..\src\driver
copy sources_drv sources %~1

copy %SRC_DIR%\*.c . %~1
copy %SRC_DIR%\*.h . %~1
copy %SRC_DIR%\*.rc . %~1
copy %SRC_DIR%\..\*.rc . %~1
