@ECHO OFF
REM Gets the revision number from the libusbdotnet SVN repository.
REM Updates the revision field in the 'LibUsbDotNet.cfg' config file.
CALL make.bat UpdateRevisionFromSvn