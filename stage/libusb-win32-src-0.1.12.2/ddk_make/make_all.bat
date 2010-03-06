@ECHO OFF
CALL make_x86.bat %1 %2
CALL make_x64.bat %1 %2
CALL make_i64.bat %1 %2
