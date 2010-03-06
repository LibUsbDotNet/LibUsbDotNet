@echo off

set OUTDIR=""

if exist .\output\i386 set OUTDIR=.\output\i386
if exist .\output\amd64 set OUTDIR=.\output\amd64
if exist .\output\ia64  set OUTDIR=.\output\ia64

if not OUTDIR=="" (
  if exist %OUTDIR%\*.exe copy /y %OUTDIR%\*.exe . %~1
  if exist %OUTDIR%\*.dll copy /y %OUTDIR%\*.dll . %~1
  if exist %OUTDIR%\*.lib copy /y %OUTDIR%\*.lib . %~1
  if exist %OUTDIR%\*.sys copy /y %OUTDIR%\*.sys . %~1
)

if exist .\output rmdir /s /q .\output
if exist .\objchk_wxp_x86 rmdir /s /q .\objchk_wxp_x86
if exist .\objchk_wnet_AMD64 rmdir /s /q .\objchk_wnet_AMD64
if exist .\objchk_wnet_IA64 rmdir /s /q .\objchk_wnet_IA64
if exist .\objfre_wxp_x86 rmdir /s /q .\objfre_wxp_x86
if exist .\objfre_wnet_AMD64 rmdir /s /q .\objfre_wnet_AMD64
if exist .\objfre_wnet_IA64 rmdir /s /q .\objfre_wnet_IA64
if exist .\objfre_wxp_ia64 rmdir /s /q .\objfre_wxp_ia64

if exist sources del /q sources 
if exist *.def del *.def 
if exist *.h del *.h 
if exist *.c del *.c 
if exist *.rc del *.rc 
if exist manifest.txt del manifest.txt

