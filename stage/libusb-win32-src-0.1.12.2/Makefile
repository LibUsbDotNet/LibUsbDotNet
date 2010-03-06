# LIBUSB-WIN32, Generic Windows USB Library
# Copyright (c) 2002-2005 Stephan Meyer <ste_meyer@web.de>
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
# 
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
# 
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA



# If you're cross-compiling and your mingw32 tools are called
# i586-mingw32msvc-gcc and so on, then you can compile libusb-win32
# by running
#    make host_prefix=i586-mingw32msvc all


ifdef host_prefix
	override host_prefix := $(host_prefix)-
endif

CC = $(host_prefix)gcc
LD = $(host_prefix)ld
WINDRES = $(host_prefix)windres
DLLTOOL = $(host_prefix)dlltool

MAKE = make
CP = cp
CD = cd
MV = mv
RM = -rm -fr
TAR = tar
ISCC = iscc
INSTALL = install
LIB = lib
IMPLIB = implib
UNIX2DOS = unix2dos

VERSION_MAJOR = 0
VERSION_MINOR = 1
VERSION_MICRO = 12
VERSION_NANO = 2

VERSION = $(VERSION_MAJOR).$(VERSION_MINOR).$(VERSION_MICRO).$(VERSION_NANO)
RC_VERSION = $(VERSION_MAJOR),$(VERSION_MINOR),$(VERSION_MICRO),$(VERSION_NANO)
RC_VERSION_STR = '\"$(VERSION)\"'
INST_VERSION = $(VERSION)

INF_DATE = $(shell date +"%m/%d/%Y")
DATE = $(shell date +"%Y%m%d")

DDK_MAKE_DIR = ./ddk_make

TARGET = libusb
DLL_TARGET = $(TARGET)$(VERSION_MAJOR)
LIB_TARGET = $(TARGET)
DRIVER_TARGET = $(TARGET)$(VERSION_MAJOR).sys

DLL_TARGET_X64 = $(TARGET)$(VERSION_MAJOR)_x64
DRIVER_TARGET_X64 = $(TARGET)$(VERSION_MAJOR)_x64.sys

INSTALL_DIR = /usr
DLL_OBJECTS = usb.o error.o descriptors.o windows.o resource.o install.o \
	registry.o 

DRIVER_OBJECTS = abort_endpoint.o claim_interface.o clear_feature.o \
	dispatch.o get_configuration.o \
	get_descriptor.o get_interface.o get_status.o \
	ioctl.o libusb_driver.o pnp.o release_interface.o reset_device.o \
	reset_endpoint.o set_configuration.o set_descriptor.o \
	set_feature.o set_interface.o transfer.o vendor_request.o \
	power.o driver_registry.o driver_debug.o libusb_driver_rc.o 

INSTALLER_NAME = $(TARGET)-win32-filter-bin-$(INST_VERSION).exe
SRC_DIST_DIR = $(TARGET)-win32-src-$(INST_VERSION)
BIN_DIST_DIR = $(TARGET)-win32-device-bin-$(INST_VERSION)


DIST_SOURCE_FILES = ./src
DIST_MISC_FILES = COPYING_LGPL.txt COPYING_GPL.txt AUTHORS.txt

SRC_DIR = ./src
DRIVER_SRC_DIR = $(SRC_DIR)/driver

VPATH = .:./src:./src/driver:./tests

INCLUDES = -I./src -I./src/driver -I.

CFLAGS = -O2 -Wall -mno-cygwin
WIN_CFLAGS = $(CFLAGS) -mwindows

CPPFLAGS = -DVERSION_MAJOR=$(VERSION_MAJOR) \
	-DVERSION_MINOR=$(VERSION_MINOR) \
	-DVERSION_MICRO=$(VERSION_MICRO) \
	-DVERSION_NANO=$(VERSION_NANO) \
	-DINF_DATE='$(INF_DATE)' \
	-DINF_VERSION='$(VERSION)' \
  -DDBG

WINDRES_FLAGS = -I./src -DRC_VERSION='$(RC_VERSION)' \
								-DRC_VERSION_STR=$(RC_VERSION_STR)

LDFLAGS = -s -mno-cygwin -L. -lusb -lgdi32 -luser32 -lcfgmgr32 \
	 				-lsetupapi -lcomctl32
WIN_LDFLAGS = $(LDFLAGS) -mwindows


DLL_LDFLAGS = -s -mdll -mno-cygwin \
	-Wl,--kill-at \
	-Wl,--out-implib,$(LIB_TARGET).a \
	-Wl,--enable-stdcall-fixup \
	-L. -lcfgmgr32 -lsetupapi 


DRIVER_LDFLAGS = -s -shared -Wl,--entry,_DriverEntry@8 \
	-nostartfiles -nostdlib -L. -lusbd -lntoskrnl -lhal


EXE_FILES = testlibusb.exe testlibusb-win.exe inf-wizard.exe install-filter.exe


.PHONY: all
all: $(DLL_TARGET).dll $(EXE_FILES) $(DRIVER_TARGET) README.txt

$(DLL_TARGET).dll: $(DLL_OBJECTS)
	$(CC) -o $@ $(DLL_OBJECTS) $(DLL_TARGET).def $(DLL_LDFLAGS)


$(DRIVER_TARGET): libusbd.a $(DRIVER_OBJECTS)
	$(CC) -o $@ $(DRIVER_OBJECTS) $(DLL_TARGET)_drv.def $(DRIVER_LDFLAGS)

libusbd.a:
	$(DLLTOOL) --dllname usbd.sys --add-underscore --def ./src/driver/usbd.def \
		--output-lib libusbd.a

inf-wizard.exe: inf_wizard_rc.o inf_wizard.o registry.o error.o
	$(CC) $(WIN_CFLAGS) -o $@ -I./src  $^ $(WIN_LDFLAGS)

testlibusb.exe: testlibusb.o
	$(CC) $(CFLAGS) -o $@ -I./src  $^ $(LDFLAGS)

install-filter.exe: install_filter.o
	$(CC) $(CFLAGS) -o $@ -I./src  $^ $(WIN_LDFLAGS)

testlibusb-win.exe: testlibusb_win.o testlibusb_win_rc.o
	$(CC) $(WIN_CFLAGS) -o $@ -I./src  $^ $(WIN_LDFLAGS)

%.o: %.c libusb_driver.h driver_api.h
	$(CC) -c $< -o $@ $(CFLAGS) $(CPPFLAGS) $(INCLUDES) 

%.o: %.rc
	$(WINDRES) $(WINDRES_FLAGS) $< -o $@

README.txt: README.in
	sed -e 's/@VERSION@/$(INST_VERSION)/' $< > $@


.PHONY: bcc_implib
bcc_lib:
	$(IMPLIB) -a $(LIB_TARGET).lib $(DLL_TARGET).dll

.PHONY: msvc_lib
msvc_lib:
	$(LIB) /machine:i386 /def:$(DLL_TARGET).def 
	$(MV) $(DLL_TARGET).lib $(LIB_TARGET).lib

.PHONY: bin_dist
bin_dist: all
	$(INSTALL) -d $(BIN_DIST_DIR)/lib/gcc
	$(INSTALL) -d $(BIN_DIST_DIR)/lib/bcc
	$(INSTALL) -d $(BIN_DIST_DIR)/lib/msvc
	$(INSTALL) -d $(BIN_DIST_DIR)/lib/msvc_x64
	$(INSTALL) -d $(BIN_DIST_DIR)/lib/dynamic
	$(INSTALL) -d $(BIN_DIST_DIR)/include
	$(INSTALL) -d $(BIN_DIST_DIR)/bin
	$(INSTALL) -d $(BIN_DIST_DIR)/examples

	$(INSTALL) $(EXE_FILES) $(BIN_DIST_DIR)/bin

	$(INSTALL) $(DRIVER_TARGET) $(BIN_DIST_DIR)/bin
	$(INSTALL) $(DLL_TARGET).dll $(BIN_DIST_DIR)/bin

	$(INSTALL) $(DDK_MAKE_DIR)/$(DRIVER_TARGET) $(BIN_DIST_DIR)/bin/$(DRIVER_TARGET_X64)
	$(INSTALL) $(DDK_MAKE_DIR)/$(DLL_TARGET).dll $(BIN_DIST_DIR)/bin/$(DLL_TARGET_X64).dll

	$(INSTALL) $(SRC_DIR)/usb.h $(BIN_DIST_DIR)/include
	$(INSTALL) $(LIB_TARGET).a $(BIN_DIST_DIR)/lib/gcc
	$(MAKE) bcc_lib 
	$(INSTALL) $(LIB_TARGET).lib $(BIN_DIST_DIR)/lib/bcc
	$(MAKE) msvc_lib
	$(INSTALL) $(LIB_TARGET).lib $(BIN_DIST_DIR)/lib/msvc
	$(INSTALL) $(DDK_MAKE_DIR)/$(LIB_TARGET).lib $(BIN_DIST_DIR)/lib/msvc_x64
	$(INSTALL) $(SRC_DIR)/libusb_dyn.c $(BIN_DIST_DIR)/lib/dynamic
	$(INSTALL) $(DIST_MISC_FILES) README.txt $(BIN_DIST_DIR)
	$(INSTALL) ./examples/*.iss $(BIN_DIST_DIR)/examples
	$(INSTALL) ./examples/*.c $(BIN_DIST_DIR)/examples
	$(UNIX2DOS) $(BIN_DIST_DIR)/examples/*.iss
	$(UNIX2DOS) $(BIN_DIST_DIR)/*.txt

.PHONY: src_dist
src_dist:
	$(INSTALL) -d $(SRC_DIST_DIR)/src
	$(INSTALL) -d $(SRC_DIST_DIR)/src/driver
	$(INSTALL) -d $(SRC_DIST_DIR)/tests
	$(INSTALL) -d $(SRC_DIST_DIR)/examples
	$(INSTALL) -d $(SRC_DIST_DIR)/ddk_make

	$(INSTALL) $(SRC_DIR)/*.c $(SRC_DIST_DIR)/src
	$(INSTALL) $(SRC_DIR)/*.h $(SRC_DIST_DIR)/src
	$(INSTALL) $(SRC_DIR)/*.rc $(SRC_DIST_DIR)/src

	$(INSTALL) ./examples/*.iss $(SRC_DIST_DIR)/examples
	$(INSTALL) ./ddk_make/sources* $(SRC_DIST_DIR)/ddk_make
	$(INSTALL) ./ddk_make/makefile $(SRC_DIST_DIR)/ddk_make
	$(INSTALL) ./ddk_make/*.txt $(SRC_DIST_DIR)/ddk_make
	$(INSTALL) ./ddk_make/*.bat $(SRC_DIST_DIR)/ddk_make
	$(UNIX2DOS)	$(SRC_DIST_DIR)/ddk_make/*

	$(INSTALL) $(SRC_DIR)/driver/*.h $(SRC_DIST_DIR)/src/driver
	$(INSTALL) $(SRC_DIR)/driver/*.c $(SRC_DIST_DIR)/src/driver
	$(INSTALL) $(SRC_DIR)/driver/*.def $(SRC_DIST_DIR)/src/driver
	$(INSTALL) $(SRC_DIR)/driver/*.rc $(SRC_DIST_DIR)/src/driver

	$(INSTALL) ./tests/*.c $(SRC_DIST_DIR)/tests
	$(INSTALL) ./tests/*.rc $(SRC_DIST_DIR)/tests
	$(INSTALL) $(DIST_MISC_FILES) *.in Makefile manifest.txt *.def \
		installer_license.txt $(SRC_DIST_DIR)
	$(UNIX2DOS) $(SRC_DIST_DIR)/*.txt


.PHONY: dist
dist: bin_dist src_dist
	sed -e 's/@VERSION@/$(INST_VERSION)/' \
		-e 's/@BIN_DIST_DIR@/$(BIN_DIST_DIR)/' \
		-e 's/@SRC_DIST_DIR@/$(SRC_DIST_DIR)/' \
		-e 's/@INSTALLER_TARGET@/$(INSTALLER_TARGET)/' \
		install.iss.in > install.iss
	$(UNIX2DOS) install.iss
	$(TAR) -czf $(SRC_DIST_DIR).tar.gz $(SRC_DIST_DIR) 
	$(TAR) -czf $(BIN_DIST_DIR).tar.gz $(BIN_DIST_DIR)
	$(ISCC) install.iss
	$(RM) $(SRC_DIST_DIR)
	$(RM) $(BIN_DIST_DIR)

.PHONY: snapshot
snapshot: INST_VERSION = $(DATE)
snapshot: dist

.PHONY: clean
clean:	
	$(RM) *.o *.dll *.a *.exp *.lib *.exe *.tar.gz *~ *.iss *.rc *.h
	$(RM) ./src/*~ *.sys *.log
	$(RM) $(DRIVER_SRC_DIR)/*~
	$(RM) README.txt

