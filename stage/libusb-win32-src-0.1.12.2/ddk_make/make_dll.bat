@echo off
set SRC_DIR=..\src
copy sources_dll sources %~1

copy %SRC_DIR%\*.c . %~1
copy ..\*.def . %~1
copy %SRC_DIR%\*.h . %~1
copy %SRC_DIR%\*.rc . %~1
copy %SRC_DIR%\driver\driver_api.h . %~1
