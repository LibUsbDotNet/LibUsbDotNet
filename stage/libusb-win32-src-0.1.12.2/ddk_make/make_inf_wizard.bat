@echo off
set TESTS_DIR=..\tests
set SRC_DIR=..\src

copy sources_inf_wizard sources %~1
copy %SRC_DIR%\*.c . %~1
copy %SRC_DIR%\*.h . %~1
copy %SRC_DIR%\*.rc . %~1
copy %SRC_DIR%\driver\driver_api.h . %~1
copy ..\manifest.txt . %~1
