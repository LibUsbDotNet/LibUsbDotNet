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


#include <windows.h>
#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef __GNUC__
#include <ddk/cfgmgr32.h>
#else
#include <cfgmgr32.h>
#define strlwr(p) _strlwr(p)
#endif

#include "registry.h"
#include "error.h"

#define CLASS_KEY_PATH_NT "SYSTEM\\CurrentControlSet\\Control\\Class\\"
#define CLASS_KEY_PATH_9X "SYSTEM\\CurrentControlSet\\Services\\Class\\"

#define USB_GET_DRIVER_NAME() \
  usb_registry_is_nt() ? driver_name_nt : driver_name_9x;

typedef struct _usb_class_key usb_class_key_t;

struct _usb_class_key {
  usb_class_key_t *next;
  char name[MAX_PATH];
};

static const char *driver_name_nt = "libusb0";
static const char *driver_name_9x = "libusb0.sys";

static const char *default_class_keys_nt[] =
  {
    /* USB devices */
    "{36fc9e60-c465-11cf-8056-444553540000}",
    /* HID devices */
    "{745a17a0-74d3-11d0-b6fe-00a0c90f57da}",
    /* Network devices */
    "{4d36e972-e325-11ce-bfc1-08002be10318}",
    /* Image devices */
    "{6bdd1fc6-810f-11d0-bec7-08002be2092f}",
    /* Media devices */
    "{4d36e96c-e325-11ce-bfc1-08002be10318}",
    /* Modem devices */
    "{4d36e96d-e325-11ce-bfc1-08002be10318}",
    /* SmartCardReader devices*/
    "{50dd5230-ba8a-11d1-bf5d-0000f805f530}",
    NULL
  };

static const char *default_class_keys_9x[] =
  {
    /* USB devices */
    "usb",
    /* HID devices */
    "hid",
    /* Network devices */
    "net",
    /* Image devices */
    "image",
    /* Media devices */
    "media",
    /* Modem devices */
    "modem",
    NULL
  };

static bool_t usb_registry_set_device_state(DWORD state, HDEVINFO dev_info, 
                                            SP_DEVINFO_DATA *dev_info_data);
static usb_class_key_t *usb_registry_get_usb_class_keys(void);
static usb_class_key_t *usb_registry_get_all_class_keys(void);
static bool_t usb_registry_add_class_key(usb_class_key_t **head,
                                         const char *key);
static bool_t usb_registry_free_class_keys(usb_class_key_t **head);


bool_t usb_registry_is_nt(void)
{
  return GetVersion() < 0x80000000 ? TRUE : FALSE;
}

bool_t usb_registry_get_property(DWORD which, HDEVINFO dev_info, 
                                 SP_DEVINFO_DATA *dev_info_data,
                                 char *buf, int size)
{
  DWORD reg_type;
  DWORD key_type;
  DWORD length = size;
  char *p = NULL;
  char *val_name = NULL;
  HKEY reg_key = NULL;
  
  memset(buf, 0, size);

  switch(which)
    {
    case SPDRP_LOWERFILTERS:
      val_name = "LowerFilters";
      key_type = DIREG_DEV; 
      break;
    case SPDRP_UPPERFILTERS:
      val_name = "UpperFilters";
      key_type = DIREG_DEV; 
      break;
    case SPDRP_SERVICE:
      val_name = "NTMPDriver";
      key_type = DIREG_DRV;
      break;
    case SPDRP_CLASSGUID:
      val_name = "ClassGUID";
      key_type = DIREG_DEV;    
    case SPDRP_CLASS:
      val_name = "Class";
      key_type = DIREG_DEV;    
      break;
    case SPDRP_HARDWAREID:
      val_name = "HardwareID";
      key_type = DIREG_DEV;
      break;
    case SPDRP_DEVICEDESC:
      val_name = "DeviceDesc";
      key_type = DIREG_DEV;
      break;
    case SPDRP_MFG:
      val_name = "Mfg";
      key_type = DIREG_DEV;
      break;

    default:
      return FALSE;
    }

  if(usb_registry_is_nt())
    {
      if(!SetupDiGetDeviceRegistryProperty(dev_info, dev_info_data, which,  
                                           &reg_type, (BYTE *)buf, 
                                           size, &length))
        {
          return FALSE;
        }

      if(length <= 2)
        {
          return FALSE;
        }
    }
  else /* Win9x */
    {
      reg_key = SetupDiOpenDevRegKey(dev_info, dev_info_data, 
                                     DICS_FLAG_GLOBAL, 
                                     0, key_type, KEY_ALL_ACCESS);
      
      if(reg_key == INVALID_HANDLE_VALUE)
        {
          usb_error("usb_registry_get_property(): reading "
                    "registry key failed");
          return FALSE;
        }
      
      if(RegQueryValueEx(reg_key, val_name, NULL, &reg_type, 
                         (BYTE *)buf, &length) != ERROR_SUCCESS
         || length <= 2)
        {
          RegCloseKey(reg_key);
          return FALSE;
        }

      RegCloseKey(reg_key);
      
      if(reg_type == REG_MULTI_SZ)
        {
          p = buf;
          while(*p)
            {
              if(*p == ',')
                {
                  *p = 0;
                }
              p++;
            }
        }
    }
 
  return TRUE;
}

bool_t usb_registry_set_property(DWORD which, HDEVINFO dev_info, 
                                 SP_DEVINFO_DATA *dev_info_data, 
                                 char *buf, int size)
{
  char *val_name = NULL;
  char *p = NULL;
  HKEY reg_key;
  DWORD reg_type;

  switch(which)
    {
    case SPDRP_LOWERFILTERS:
      reg_type = usb_registry_is_nt() ? REG_MULTI_SZ : REG_SZ;
      val_name = "LowerFilters";
      break;
    case SPDRP_UPPERFILTERS:
      reg_type = usb_registry_is_nt() ? REG_MULTI_SZ : REG_SZ;
      val_name = "UpperFilters";
      break;
    default:
      return 0;
    }

  if(usb_registry_is_nt())
    {
      if(size > 2)
        {
          if(!SetupDiSetDeviceRegistryProperty(dev_info, dev_info_data,
                                               which, (BYTE *)buf, size))
            {
              usb_error("usb_registry_set_property(): setting "
                        "property '%s' failed", val_name);
              return FALSE;
            }
        }
      else
        {
          if(!SetupDiSetDeviceRegistryProperty(dev_info, dev_info_data,
                                               which, NULL, 0))
            {
              usb_error("usb_registry_set_property(): deleting "
                        "property '%s' failed", val_name);
              return FALSE;
            }
        }
    }
  else
    {
      p = buf;
      
      while(*p)
        {
          if(*p == ',')
            {
              *p = 0;
            }
          p += (strlen(p) + 1);
        }

      reg_key = SetupDiOpenDevRegKey(dev_info, dev_info_data, 
                                     DICS_FLAG_GLOBAL, 
                                     0, DIREG_DEV, KEY_ALL_ACCESS);
      
      if(reg_key == INVALID_HANDLE_VALUE)
        {
          usb_error("usb_registry_set_property(): reading "
                    "registry key failed");
          return FALSE;
        }
      
      if(size > 3)
        {
          if(RegSetValueEx(reg_key, val_name, 0, reg_type, (BYTE *)buf, 
                           size) != ERROR_SUCCESS)
            {
              usb_error("usb_registry_set_property(): setting "
                        "property '%s' failed", val_name);
              RegCloseKey(reg_key);
              return FALSE;
            }
        }
      else
        {
          if(RegDeleteValue(reg_key, val_name) != ERROR_SUCCESS)
            {
              usb_error("usb_registry_set_property(): deleting "
                        "property '%s' failed", val_name);
              RegCloseKey(reg_key);
              return FALSE;
            }
        }
      RegCloseKey(reg_key);
    }

  return TRUE;
}

bool_t usb_registry_insert_class_filter(void)
{
  const char *driver_name;
  usb_class_key_t *keys;
  usb_class_key_t *key;
  char buf[MAX_PATH];
  
  driver_name = USB_GET_DRIVER_NAME();

  keys = usb_registry_get_usb_class_keys();

  if(!keys)
    {
      usb_error("usb_registry_insert_filter: unable to retrieve class keys\n");
      return FALSE;
    }

  key = keys;

  while(key)
    {
      if(usb_registry_get_mz_value(key->name, "UpperFilters", 
                                   buf, sizeof(buf)))
        {
          usb_registry_mz_string_lower(buf);
      
          if(usb_registry_mz_string_find(buf, driver_name))
            {
              key = key->next;
              continue;
            }
        }
      
      usb_registry_mz_string_insert(buf, driver_name);

      if(!usb_registry_set_mz_value(key->name, "UpperFilters", buf, 
                                    usb_registry_mz_string_size(buf)))
        {
          usb_error("usb_registry_insert_filter: unable to "
                    "set registry value\n");
        }

      key = key->next;
    }
 
  usb_registry_free_class_keys(&keys);

  return TRUE;
}


bool_t usb_registry_remove_class_filter(void)
{
  const char *driver_name;
  usb_class_key_t *keys;
  usb_class_key_t *key;
  char buf[MAX_PATH];
  
  driver_name = USB_GET_DRIVER_NAME();

  keys = usb_registry_get_all_class_keys();
      
  if(!keys)
    {
      usb_error("usb_registry_remove_filter: unable to retrieve class keys\n");
      return FALSE;
    }

  key = keys;

  while(key)
    {
      if(usb_registry_get_mz_value(key->name, "UpperFilters", 
                                   buf, sizeof(buf)))
        {
          usb_registry_mz_string_lower(buf);
      
          if(usb_registry_mz_string_find(buf, driver_name))
            {
              usb_registry_mz_string_remove(buf, driver_name);
              
              usb_registry_set_mz_value(key->name, "UpperFilters", buf, 
                                        usb_registry_mz_string_size(buf));
            }
        }

      key = key->next;
    }
 
  usb_registry_free_class_keys(&keys);

  return TRUE;
}

bool_t usb_registry_remove_device_filter(void)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index = 0;
  char filters[MAX_PATH];
  const char *driver_name;

  driver_name = USB_GET_DRIVER_NAME();

  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  dev_index = 0;

  dev_info = SetupDiGetClassDevs(NULL, "USB", NULL, DIGCF_ALLCLASSES);
  
  if(dev_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_registry_remove_device_filter(): getting "
                "device info set failed");
      return FALSE;
    }
  
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      /* remove libusb as a device upper filter */
      if(usb_registry_get_property(SPDRP_UPPERFILTERS, dev_info, 
                                   &dev_info_data, 
                                   filters, sizeof(filters)))
        {
          usb_registry_mz_string_lower(filters);

          if(usb_registry_mz_string_find(filters, driver_name))
            {
              int size;
              usb_registry_mz_string_remove(filters, driver_name);
              size = usb_registry_mz_string_size(filters);
 
              usb_registry_set_property(SPDRP_UPPERFILTERS, dev_info, 
                                        &dev_info_data, filters, size);
            }
        }

      /* remove libusb as a device lower filter */
      if(usb_registry_get_property(SPDRP_LOWERFILTERS, dev_info, 
                                   &dev_info_data, 
                                   filters, sizeof(filters)))
        {
          usb_registry_mz_string_lower(filters);

          if(usb_registry_mz_string_find(filters, driver_name))
            {
              int size;
              usb_registry_mz_string_remove(filters, driver_name);
              size = usb_registry_mz_string_size(filters);
 
              usb_registry_set_property(SPDRP_LOWERFILTERS, dev_info, 
                                        &dev_info_data, filters, size);
            }
        }

      dev_index++;
    }
  
  SetupDiDestroyDeviceInfoList(dev_info);
  
  return TRUE;
}

static bool_t usb_registry_set_device_state(DWORD state, HDEVINFO dev_info, 
                                            SP_DEVINFO_DATA *dev_info_data)
{
  SP_PROPCHANGE_PARAMS prop_params;

  memset(&prop_params, 0, sizeof(SP_PROPCHANGE_PARAMS));

  prop_params.ClassInstallHeader.cbSize = sizeof(SP_CLASSINSTALL_HEADER);
  prop_params.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE;
  prop_params.StateChange = state;
  prop_params.Scope = DICS_FLAG_CONFIGSPECIFIC;//DICS_FLAG_GLOBAL;
  prop_params.HwProfile = 0;
	  

  if(!SetupDiSetClassInstallParams(dev_info, dev_info_data,
                                   (SP_CLASSINSTALL_HEADER *)&prop_params,
                                   sizeof(SP_PROPCHANGE_PARAMS)))
    {
      usb_error("usb_registry_set_device_state(): setting class "
                "install parameters failed");
      return FALSE;
    }
	  

  if(!SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, dev_info, dev_info_data))
    {
      usb_error("usb_registry_set_device_state(): calling class "
                "installer failed");
      return FALSE;
    }

  return TRUE;
}

bool_t usb_registry_restart_device(HDEVINFO dev_info, 
                                   SP_DEVINFO_DATA *dev_info_data)
{
  return usb_registry_set_device_state(DICS_PROPCHANGE, dev_info, 
                                       dev_info_data);
}

bool_t usb_registry_stop_device(HDEVINFO dev_info, 
                                SP_DEVINFO_DATA *dev_info_data)
{
  return usb_registry_set_device_state(DICS_DISABLE, dev_info, 
                                       dev_info_data);
}

bool_t usb_registry_start_device(HDEVINFO dev_info, 
                                 SP_DEVINFO_DATA *dev_info_data)
{
  return usb_registry_set_device_state(DICS_ENABLE, dev_info, 
                                       dev_info_data);
}

bool_t usb_registry_is_service_libusb(HDEVINFO dev_info, 
                                      SP_DEVINFO_DATA *dev_info_data)
{
  char service_name[MAX_PATH];

  if(!usb_registry_get_property(SPDRP_SERVICE, dev_info, dev_info_data,
                                service_name, sizeof(service_name)))
    {
      return FALSE;
    }
  
  usb_registry_mz_string_lower(service_name);
   
  if(usb_registry_mz_string_find_sub(service_name, "libusb"))
    {
      return TRUE;
    }

  return FALSE;
}

void usb_registry_stop_libusb_devices(void)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index = 0;
  
  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  dev_index = 0;

  dev_info = SetupDiGetClassDevs(NULL, "USB", NULL,
                                 DIGCF_ALLCLASSES | DIGCF_PRESENT);
  
  if(dev_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_registry_stop_libusb_devices(): getting "
                "device info set failed");
      return;
    }
  
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      if(usb_registry_is_service_libusb(dev_info, &dev_info_data))
        {
          usb_registry_stop_device(dev_info, &dev_info_data);
        }
      dev_index++;
    }
  
  SetupDiDestroyDeviceInfoList(dev_info);
}

void usb_registry_start_libusb_devices(void)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index = 0;
  
  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  dev_index = 0;

  dev_info = SetupDiGetClassDevs(NULL, "USB", NULL,
                                 DIGCF_ALLCLASSES | DIGCF_PRESENT);
  
  if(dev_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_registry_stop_libusb_devices(): getting "
                "device info set failed");
      return;
    }
  
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      if(usb_registry_is_service_libusb(dev_info, &dev_info_data))
        {
          usb_registry_start_device(dev_info, &dev_info_data);
        }
      dev_index++;
    }
  
  SetupDiDestroyDeviceInfoList(dev_info);
}


bool_t usb_registry_match(HDEVINFO dev_info, 
                          SP_DEVINFO_DATA *dev_info_data)
{
  char tmp[MAX_PATH];
  
  if(!usb_registry_get_property(SPDRP_HARDWAREID, dev_info, dev_info_data,
                                tmp, sizeof(tmp)))
    {
      usb_error("usb_registry_match_no_hubs(): getting hardware id "
                "failed");
      return FALSE;
    }

  usb_registry_mz_string_lower(tmp);

  /* search for USB devices, skip root hubs and interfaces of composite */
  /* devices */
  if(usb_registry_mz_string_find_sub(tmp, "&mi_")
     || usb_registry_mz_string_find_sub(tmp, "root_hub"))
    {
      return FALSE;
    }

  return TRUE;
}

bool_t usb_registry_get_mz_value(const char *key, const char *value, 
                                 char *buf, int size)
{
  HKEY reg_key = NULL;
  DWORD reg_type;
  DWORD reg_length = size;
  bool_t ret = FALSE;
  char *p;

  memset(buf, 0, size);
      
  if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, KEY_ALL_ACCESS, &reg_key)
     == ERROR_SUCCESS)
    {
      if(RegQueryValueEx(reg_key, value, NULL, &reg_type, 
                         buf, &reg_length) == ERROR_SUCCESS)
        {
          if(reg_type == REG_SZ)
            {
              p = buf;
              while(*p)
                {
                  if(*p == ',')
                    {
                      *p = 0;
                    }
                  p++;
                }
            }

          ret = TRUE;
        }
    }

  if(reg_key)
    {
      RegCloseKey(reg_key);
    }

  return ret;
}


bool_t usb_registry_set_mz_value(const char *key, const char *value, 
                                 char *buf, int size)
{
  HKEY reg_key = NULL;
  bool_t ret = FALSE;
  char *p;

  /* convert REG_MULTI_SZ to REG_SZ */
  if(!usb_registry_is_nt())
    {
      p = buf;
      
      while(*p && *(p + 1))
        {
          if(*p == 0)
            {
              *p = ',';
            }
          p++;
        }
    }

  if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, KEY_ALL_ACCESS, &reg_key)
     == ERROR_SUCCESS)
    {
      if(size > 2)
        {
          if(usb_registry_is_nt())
            {
              if(RegSetValueEx(reg_key, value, 0, REG_MULTI_SZ, buf, size)
                 == ERROR_SUCCESS)
                {
                  ret = TRUE;
                }
            }
          else
            {
              if(RegSetValueEx(reg_key, value, 0, REG_SZ, buf, size)
                 == ERROR_SUCCESS)
                {
                  ret = TRUE;
                }
            }
        }
      else
        {
          if(RegDeleteValue(reg_key, value) == ERROR_SUCCESS)
            {
              ret = TRUE;
            }
        }
    }
  
  if(reg_key)
    {
      RegCloseKey(reg_key);
    }

  return ret;
}

int usb_registry_mz_string_size(const char *src)
{
  char *p = (char *)src;
  
  if(!src)
    {
      return 0;
    }

  while(*p)
    {
      p += (strlen(p) + 1);
    }
  
  return (int)(p - src) + 1;
}

char *usb_registry_mz_string_find_sub(const char *src, const char *str)
{
  while(*src)
    {
      if(strstr(src, str))
        {
          return (char *)src;
        }
      src += (strlen(src) + 1);
    }

  return NULL;
}

char *usb_registry_mz_string_find(const char *src, const char *str)
{
  while(*src)
    {
      if(!strcmp(src, str))
        {
          return (char *)src;
        }
      src += strlen(src) + 1;
    }

  return NULL;
}

bool_t usb_registry_mz_string_insert(char *src, const char *str)
{
  while(*src)
    {
      src += (strlen(src) + 1);
    }

  memcpy(src, str, strlen(str));

  src += strlen(str);

  *src = 0;
  *(src + 1) = 0;

  return TRUE;
}

bool_t usb_registry_mz_string_remove(char *src, const char *str)
{
  char *p;
  bool_t ret = FALSE;
  int size;

  do {
    src = usb_registry_mz_string_find(src, str);
    
    if(!src)
      {
        break;
      }
    else
      {
        ret = TRUE;
      }
    
    p = src;
    size = 0;

    while(*p)
      {
        p += strlen(p) + 1;
        size += strlen(p) + 1;
      }
    
    memmove(src, src + strlen(src) + 1, size);

  } while(1);

  return TRUE;
}

void usb_registry_mz_string_lower(char *src)
{
  while(*src)
    {
      strlwr(src);
      src += (strlen(src) + 1);
    }
}

bool_t usb_registry_restart_all_devices(void)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index;
  char id[MAX_PATH];

  dev_index = 0;
  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  
  dev_info = SetupDiGetClassDevs(NULL, "USB", NULL,
                                 DIGCF_ALLCLASSES | DIGCF_PRESENT);
  
  if(dev_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_registry_restart_all_devices(): getting "
                "device info set failed");
      return FALSE;
    }
    
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      if(!usb_registry_get_property(SPDRP_HARDWAREID, dev_info, 
                                    &dev_info_data, id, sizeof(id)))
        {
          usb_error("usb_registry_restart_all_devices(): getting hardware "
                    "id failed");
          dev_index++;
          continue;
        }
        
      usb_registry_mz_string_lower(id);
        
      /* restart root hubs */
      if(usb_registry_mz_string_find_sub(id, "root_hub"))
        {
          usb_registry_restart_device(dev_info, &dev_info_data);
        }

      dev_index++;
    }
    
  SetupDiDestroyDeviceInfoList(dev_info);

  return TRUE;
}


usb_class_key_t *usb_registry_get_usb_class_keys(void)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index = 0;
  int i;
  char class[MAX_PATH];
  char tmp[MAX_PATH];
  usb_class_key_t *keys = NULL;
  DWORD class_property;
  const char *class_path;
  const char **default_class_keys;

  if(usb_registry_is_nt())
    {
      class_property = SPDRP_CLASSGUID;
      class_path = CLASS_KEY_PATH_NT;
      default_class_keys = default_class_keys_nt;
    }
  else
    {
      class_property = SPDRP_CLASS;
      class_path = CLASS_KEY_PATH_9X;
      default_class_keys = default_class_keys_9x;
    }

  i = 0;

  while(default_class_keys[i])
    {
      if((strlen(class_path) + strlen(default_class_keys[i])) < sizeof(tmp))
        {
          sprintf(tmp, "%s%s", class_path, default_class_keys[i]);
          usb_registry_add_class_key(&keys, tmp);
        }
      i++;
    }


  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  dev_info = SetupDiGetClassDevs(NULL, "USB", NULL,
                                 DIGCF_ALLCLASSES);
  
  if(dev_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_registry_get_class_keys(): getting "
                "device info set failed");
      return NULL;
    }
  
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      if(!usb_registry_is_service_libusb(dev_info, &dev_info_data))
        {
          if(!usb_registry_get_property(SPDRP_CLASSGUID, dev_info, 
                                        &dev_info_data,
                                        class, sizeof(class)))
            {
              usb_error("usb_registry_get_class_keys(): getting "
                        "hardware id failed");
              dev_index++;
              continue;
            }
          
          strlwr(class);
          
          if((strlen(class_path) + strlen(class)) < sizeof(tmp))
            {
              sprintf(tmp, "%s%s", class_path, class);
              usb_registry_add_class_key(&keys, tmp);
            }
        }

      dev_index++;
    }
  
  SetupDiDestroyDeviceInfoList(dev_info);

  return keys;
}

static usb_class_key_t *usb_registry_get_all_class_keys(void)
{
  const char *class_path;
  usb_class_key_t *keys = NULL;
  HKEY reg_key;
  char class[MAX_PATH];
  char tmp[MAX_PATH];

  if(usb_registry_is_nt())
    {
      class_path = CLASS_KEY_PATH_NT;
    }
  else
    {
      class_path = CLASS_KEY_PATH_9X;
    }

  if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, class_path, 0, KEY_ALL_ACCESS, 
                  &reg_key) == ERROR_SUCCESS)
    {
      DWORD i = 0;
      DWORD size = sizeof(class);
      FILETIME junk;

      memset(class, 0, sizeof(class));

      while(RegEnumKeyEx(reg_key, i, class, &size, 0, NULL, NULL, &junk) 
            == ERROR_SUCCESS)
        {
          strlwr(class);

          if((strlen(class_path) + strlen(class)) < sizeof(tmp))
            {
              sprintf(tmp, "%s%s", class_path, class);
              usb_registry_add_class_key(&keys, tmp);
            }
          
          memset(class, 0, sizeof(class));
          size = sizeof(class);
          i++;
        }
 
      RegCloseKey(reg_key);
    }

  return keys;
}

static bool_t usb_registry_add_class_key(usb_class_key_t **head,
                                         const char *key)
{
  usb_class_key_t *p = *head;

  if(key)
    {

      if(strlen(key) >= MAX_PATH)
        return FALSE;

      while(p)
        {
          if(!strcmp(p->name, key))
            {
              return FALSE;
            }
          p = p->next;
        }

      p = malloc(sizeof(usb_class_key_t));
      
      if(!p)
        return FALSE;

      memset(p, 0, sizeof(usb_class_key_t));
      strcpy(p->name, key);

      p->next = *head;
      *head = p;

      return TRUE;
    }

  return FALSE;
}

static bool_t usb_registry_free_class_keys(usb_class_key_t **head)
{
  usb_class_key_t *p = *head;
  usb_class_key_t *q;

  while(p)
    {
      q = p->next;
      free(p);
      p = q;
    }

  *head = NULL;

  return TRUE;
}
