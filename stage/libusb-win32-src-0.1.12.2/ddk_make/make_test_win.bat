@echo off
set TESTS_DIR=..\tests
set SRC_DIR=..\src

copy sources_test_win sources
copy %TESTS_DIR%\testlibusb_win.c .
copy %TESTS_DIR%\testlibusb_win_rc.rc .
copy %SRC_DIR%\usb.h .
copy %SRC_DIR%\*.rc .
copy ..\manifest.txt
