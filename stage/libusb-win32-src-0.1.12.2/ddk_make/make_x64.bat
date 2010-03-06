@echo off
call CMD /C make.bat "arch=x64" "app=all" "outdir=.\x64" "quiet=%~1" "warn=%~2"
