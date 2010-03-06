@echo off
set TESTS_DIR=..\tests
set SRC_DIR=..\src

copy sources_test sources %~1
copy %TESTS_DIR%\testlibusb.c . %~1
copy %SRC_DIR%\usb.h . %~1
copy %SRC_DIR%\*.rc . %~1
