/* LIBUSB-WIN32, Generic Windows USB Library
 * Copyright (c) 2002-2005 Stephan Meyer <ste_meyer@web.de>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */



#ifndef __USB_REGISTRY_H__
#define __USB_REGISTRY_H__

#include <windows.h>
#include <setupapi.h>


#define LIBUSB_DRIVER_NAME_NT "libusb0"
#define LIBUSB_DRIVER_NAME_9X "libusb0.sys"

typedef int bool_t;

#ifndef FALSE
#define FALSE 0
#endif
#ifndef TRUE
#define TRUE (!(FALSE)) 
#endif

#define REGISTRY_BUF_SIZE 512


bool_t usb_registry_is_nt(void);

bool_t usb_registry_restart_device(HDEVINFO dev_info, 
                                   SP_DEVINFO_DATA *dev_info_data);
bool_t usb_registry_stop_device(HDEVINFO dev_info, 
                                SP_DEVINFO_DATA *dev_info_data);
bool_t usb_registry_start_device(HDEVINFO dev_info, 
                                 SP_DEVINFO_DATA *dev_info_data);

bool_t usb_registry_get_property(DWORD which, HDEVINFO dev_info, 
                                 SP_DEVINFO_DATA *dev_info_data,
                                 char *buf, int size);
bool_t usb_registry_set_property(DWORD which, HDEVINFO dev_info, 
                                 SP_DEVINFO_DATA *dev_info_data, 
                                 char *buf, int size);

bool_t usb_registry_restart_all_devices(void);

bool_t usb_registry_insert_class_filter(void);
bool_t usb_registry_remove_class_filter(void);
bool_t usb_registry_remove_device_filter(void);

void usb_registry_stop_libusb_devices(void);
void usb_registry_start_libusb_devices(void);

bool_t usb_registry_match(HDEVINFO dev_info, SP_DEVINFO_DATA *dev_info_data);

bool_t usb_registry_get_mz_value(const char *key, const char *value, 
                                 char *buf, int size);
bool_t usb_registry_set_mz_value(const char *key, const char *value, 
                                 char *buf, int size);
int usb_registry_mz_string_size(const char *src);
char *usb_registry_mz_string_find(const char *src, const char *str);
char *usb_registry_mz_string_find_sub(const char *src, const char *str);
bool_t usb_registry_mz_string_insert(char *src, const char *str);
bool_t usb_registry_mz_string_remove(char *src, const char *str);
void usb_registry_mz_string_lower(char *src);


#endif
