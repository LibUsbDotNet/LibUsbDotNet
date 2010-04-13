@ECHO OFF
REM //////////////////////////////////////////////////////////////////////////
REM LibUsbDotNet Clean batch file utility
REM
REM Cleans all directories containing:
REM	  \_ReSharper.
REM
REM Cleans all directories ending in:
REM	  obj
REM	  Debug
REM
REM Cleans all files ending in:
REM   cod, cof, mcs, mptags, err, resharper, user, tagsrc, hex, ncb, pdb, pidb,
REM   suo, vshost.exe
REM
REM //////////////////////////////////////////////////////////////////////////
.\Utility\RegexClean -d -r -m="\\(_ReSharper\.|obj$|Debug$)"
.\Utility\RegexClean -r -m="\.(cod|cof|mcs|mptags|err|resharper|user|tagsrc|hex|ncb|pdb|pidb|suo|vshost\.exe)$"
.\Utility\RegexClean -r -m="thumbs.db$"

