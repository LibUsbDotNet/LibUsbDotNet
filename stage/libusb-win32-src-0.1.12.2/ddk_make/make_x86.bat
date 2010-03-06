@ECHO OFF
call CMD /C make.bat "arch=x86" "app=all" "outdir=.\x86" "quiet=%~1" "warn=%~2"

