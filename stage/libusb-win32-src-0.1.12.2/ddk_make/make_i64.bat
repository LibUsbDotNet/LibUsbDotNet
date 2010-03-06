@ECHO OFF
call CMD /C make.bat "arch=i64" "app=all" "outdir=.\i64" "quiet=%~1" "warn=%~2"
