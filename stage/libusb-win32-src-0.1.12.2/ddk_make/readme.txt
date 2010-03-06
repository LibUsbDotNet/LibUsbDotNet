
This folder contains batch files to compile libusb-win32 with Microsoft's DDK.

Requirements:

1.) Windows Server 2003 DDK (older versions might work but have not
    been tested yet)

How to compile:

1.) install the DDK
2.) open a DDK build environment, either a "checked" or a "free" one
3.) launch one of the following batch files to compile the sources:
    
    make_driver.bat: builds the driver
    make_dll.bat: builds the DLL
    make_test.bat: builds the command line version of the test tool
    make_test_win.bat: builds the Windows version of the test tool
    make_all.bat: builds everything, driver, DLL, and the test tools