@ECHO OFF
Echo PReparing to clean .svn directory.. (CTRL-C) to cancel.
pause
.\Utility\RegexClean -d -r -m="\\\.svn$"
pause
