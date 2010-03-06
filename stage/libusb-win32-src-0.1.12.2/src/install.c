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
#include <winsvc.h>
#include <setupapi.h>
#include <stdio.h>
#include <regstr.h>
#include <wchar.h>
#include <string.h>

#ifdef __GNUC__
#include <ddk/cfgmgr32.h>
#else
#include <cfgmgr32.h>
#define strlwr(p) _strlwr(p)
#endif

#include "usb.h"
#include "registry.h"
#include "error.h"
#include "driver_api.h"


#define LIBUSB_DRIVER_PATH  "system32\\drivers\\libusb0.sys"
#define LIBUSB_OLD_SERVICE_NAME_NT "libusbd"

#define INSTALLFLAG_FORCE 0x00000001

/* newdev.dll exports */
typedef BOOL (WINAPI * update_driver_for_plug_and_play_devices_t)(HWND, 
                                                                  LPCSTR, 
                                                                  LPCSTR, 
                                                                  DWORD,
                                                                  PBOOL);
/* setupapi.dll exports */
typedef BOOL (WINAPI * setup_copy_oem_inf_t)(PCSTR, PCSTR, DWORD, DWORD,
                                             PSTR, DWORD, PDWORD, PSTR*);

/* advapi32.dll exports */
typedef SC_HANDLE (WINAPI * open_sc_manager_t)(LPCTSTR, LPCTSTR, DWORD);
typedef SC_HANDLE (WINAPI * open_service_t)(SC_HANDLE, LPCTSTR, DWORD);
typedef BOOL (WINAPI * change_service_config_t)(SC_HANDLE, DWORD, DWORD, 
                                                DWORD, LPCTSTR, LPCTSTR, 
                                                LPDWORD, LPCTSTR, LPCTSTR, 
                                                LPCTSTR, LPCTSTR);
typedef BOOL (WINAPI * close_service_handle_t)(SC_HANDLE);
typedef SC_HANDLE (WINAPI * create_service_t)(SC_HANDLE, LPCTSTR, LPCTSTR,
                                              DWORD, DWORD,DWORD, DWORD,
                                              LPCTSTR, LPCTSTR, LPDWORD,
                                              LPCTSTR, LPCTSTR, LPCTSTR);
typedef BOOL (WINAPI * delete_service_t)(SC_HANDLE);
typedef BOOL (WINAPI * start_service_t)(SC_HANDLE, DWORD, LPCTSTR);
typedef BOOL (WINAPI * query_service_status_t)(SC_HANDLE, LPSERVICE_STATUS);
typedef BOOL (WINAPI * control_service_t)(SC_HANDLE, DWORD, LPSERVICE_STATUS);





static HINSTANCE advapi32_dll = NULL;

static open_sc_manager_t open_sc_manager = NULL;
static open_service_t open_service = NULL;
static change_service_config_t change_service_config = NULL;
static close_service_handle_t close_service_handle = NULL;
static create_service_t create_service = NULL;
static delete_service_t delete_service = NULL;
static start_service_t start_service = NULL;
static query_service_status_t query_service_status = NULL;
static control_service_t control_service = NULL;


static bool_t usb_service_load_dll(void);
static bool_t usb_service_free_dll(void);

static bool_t usb_service_create(const char *name, const char *display_name,
                                 const char *binary_path, unsigned long type,
                                 unsigned long start_type);
static bool_t usb_service_stop(const char *name);
static bool_t usb_service_delete(const char *name);


void CALLBACK usb_touch_inf_file_rundll(HWND wnd, HINSTANCE instance,
                                        LPSTR cmd_line, int cmd_show);

void CALLBACK usb_install_service_np_rundll(HWND wnd, HINSTANCE instance,
                                            LPSTR cmd_line, int cmd_show)
{
  usb_install_service_np();
}

void CALLBACK usb_uninstall_service_np_rundll(HWND wnd, HINSTANCE instance,
                                              LPSTR cmd_line, int cmd_show)
{
  usb_uninstall_service_np();
}

void CALLBACK usb_install_driver_np_rundll(HWND wnd, HINSTANCE instance,
                                           LPSTR cmd_line, int cmd_show)
{
  usb_install_driver_np(cmd_line);
}


int usb_install_service_np(void)
{
  char display_name[MAX_PATH];
  int ret = 0;

  /* uninstall old filter driver */
  usb_uninstall_service_np();

  /* stop devices that are handled by libusb's device driver */
  usb_registry_stop_libusb_devices();

  /* the old driver is unloaded now */ 

  if(usb_registry_is_nt())
    {
      memset(display_name, 0, sizeof(display_name));

      /* create the Display Name */
      _snprintf(display_name, sizeof(display_name) - 1,
                "LibUsb-Win32 - Kernel Driver, Version %d.%d.%d.%d", 
                VERSION_MAJOR, VERSION_MINOR, VERSION_MICRO, VERSION_NANO);
      
      /* create the kernel service */
      if(!usb_service_create(LIBUSB_DRIVER_NAME_NT, display_name, 
                             LIBUSB_DRIVER_PATH,
                             SERVICE_KERNEL_DRIVER, SERVICE_DEMAND_START))
        ret = -1;
    }
  
  /* restart devices that are handled by libusb's device driver */
  usb_registry_start_libusb_devices(); 

  /* insert filter drivers */
  usb_registry_insert_class_filter();

  /* restart the whole USB system so that the new drivers will be loaded */
  usb_registry_restart_all_devices(); 

  return ret;
}

int usb_uninstall_service_np(void)
{
  HANDLE win; 
  HKEY reg_key = NULL;

  /* older version of libusb used a system service, just remove it */
  if(usb_registry_is_nt())
    {
      usb_service_stop(LIBUSB_OLD_SERVICE_NAME_NT);
      usb_service_delete(LIBUSB_OLD_SERVICE_NAME_NT);
    }
  else
    {
      do {
        win = FindWindow("LIBUSB_SERVICE_WINDOW_CLASS", NULL);
        if(win != INVALID_HANDLE_VALUE)
          {
            PostMessage(win, WM_DESTROY, 0, 0);
            Sleep(500);
          }
      } while(win);

      if(RegOpenKeyEx(HKEY_LOCAL_MACHINE,
                      "Software\\Microsoft\\Windows\\CurrentVersion"
                      "\\RunServices",
                      0, KEY_ALL_ACCESS, &reg_key) == ERROR_SUCCESS)
        {
          RegDeleteValue(reg_key, "LibUsb-Win32 Daemon");
          RegCloseKey(reg_key);
        }
    } 

  /* old versions used device filters that have to be removed */
  usb_registry_remove_device_filter();

  /* remove class filter driver */
  usb_registry_remove_class_filter();

  /* unload filter drivers */
  usb_registry_restart_all_devices(); 

  return 0;
}

int usb_install_driver_np(const char *inf_file)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  INFCONTEXT inf_context;
  HINF inf_handle;
  DWORD config_flags, problem, status;
  BOOL reboot;
  char inf_path[MAX_PATH];
  char id[MAX_PATH];
  char tmp_id[MAX_PATH];
  char *p;
  int dev_index;
  HINSTANCE newdev_dll = NULL;
  HMODULE setupapi_dll = NULL;

  update_driver_for_plug_and_play_devices_t UpdateDriverForPlugAndPlayDevices;
  setup_copy_oem_inf_t SetupCopyOEMInf;
  newdev_dll = LoadLibrary("newdev.dll");

  if(!newdev_dll)
    {
      usb_error("usb_install_driver(): loading newdev.dll failed\n");
      return -1;
    }
  
  UpdateDriverForPlugAndPlayDevices =  
    (update_driver_for_plug_and_play_devices_t) 
    GetProcAddress(newdev_dll, "UpdateDriverForPlugAndPlayDevicesA");

  if(!UpdateDriverForPlugAndPlayDevices)
    {
      usb_error("usb_install_driver(): loading newdev.dll failed\n");
      return -1;
    }

  setupapi_dll = GetModuleHandle("setupapi.dll");
  
  if(!setupapi_dll)
    {
      usb_error("usb_install_driver(): loading setupapi.dll failed\n");
      return -1;
    }
  SetupCopyOEMInf = (setup_copy_oem_inf_t)
    GetProcAddress(setupapi_dll, "SetupCopyOEMInfA");
  
  if(!SetupCopyOEMInf)
    {
      usb_error("usb_install_driver(): loading setupapi.dll failed\n");
      return -1;
    }

  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);


  /* retrieve the full .inf file path */
  if(!GetFullPathName(inf_file, MAX_PATH, inf_path, NULL))
    {
      usb_error("usb_install_driver(): .inf file %s not found\n", 
                inf_file);
      return -1;
    }

  /* open the .inf file */
  inf_handle = SetupOpenInfFile(inf_path, NULL, INF_STYLE_WIN4, NULL);

  if(inf_handle == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_install_driver(): unable to open .inf file %s\n", 
                inf_file);
      return -1;
    }

  /* find the .inf file's device description section marked "Devices" */
  if(!SetupFindFirstLine(inf_handle, "Devices", NULL, &inf_context))
    {
      usb_error("usb_install_driver(): .inf file %s does not contain "
                "any device descriptions\n", inf_file);
      SetupCloseInfFile(inf_handle);
      return -1;
    }


  do {
    /* get the device ID from the .inf file */
    if(!SetupGetStringField(&inf_context, 2, id, sizeof(id), NULL))
      {
        continue;
      }

    /* convert the string to lowercase */
    strlwr(id);

    reboot = FALSE;

    /* copy the .inf file to the system directory so that is will be found */
    /* when new devices are plugged in */
    SetupCopyOEMInf(inf_path, NULL, SPOST_PATH, 0, NULL, 0, NULL, NULL);

    /* update all connected devices matching this ID, but only if this */
    /* driver is better or newer */
    UpdateDriverForPlugAndPlayDevices(NULL, id, inf_path, INSTALLFLAG_FORCE, 
                                      &reboot);
    

    /* now search the registry for device nodes representing currently  */
    /* unattached devices */


    /* get all USB device nodes from the registry, present and non-present */
    /* devices */
    dev_info = SetupDiGetClassDevs(NULL, "USB", NULL, DIGCF_ALLCLASSES);
    
    if(dev_info == INVALID_HANDLE_VALUE)
      {
        SetupCloseInfFile(inf_handle);
        break;
      }
 
    dev_index = 0;

    /* enumerate the device list to find all attached and unattached */
    /* devices */
    while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
      {
        /* get the harware ID from the registry, this is a multi-zero string */
        if(SetupDiGetDeviceRegistryProperty(dev_info, &dev_info_data,
                                            SPDRP_HARDWAREID, NULL,  
                                            (BYTE *)tmp_id, 
                                            sizeof(tmp_id), NULL))
          {
            /* check all possible IDs contained in that multi-zero string */
            for(p = tmp_id; *p; p += (strlen(p) + 1))
              {
                /* convert the string to lowercase */
                strlwr(p);
		
                /* found a match? */
                if(strstr(p, id))
                  {
                    /* is this device disconnected? */
                    if(CM_Get_DevNode_Status(&status,
                                             &problem,
                                             dev_info_data.DevInst,
                                             0) == CR_NO_SUCH_DEVINST)
                      {
                        /* found a device node that represents an unattached */
                        /* device */
                        if(SetupDiGetDeviceRegistryProperty(dev_info, 
                                                            &dev_info_data,
                                                            SPDRP_CONFIGFLAGS, 
                                                            NULL,  
                                                            (BYTE *)&config_flags, 
                                                            sizeof(config_flags),
                                                            NULL))
                          {
                            /* mark the device to be reinstalled the next time it is */
                            /* plugged in */
                            config_flags |= CONFIGFLAG_REINSTALL;
			    
                            /* write the property back to the registry */
                            SetupDiSetDeviceRegistryProperty(dev_info, 
                                                             &dev_info_data,
                                                             SPDRP_CONFIGFLAGS,
                                                             (BYTE *)&config_flags, 
                                                             sizeof(config_flags));
                          }
                      }
                    /* a match was found, skip the rest */
                    break;
                  }
              }
          }
        /* check the next device node */
        dev_index++;
      }
    
    SetupDiDestroyDeviceInfoList(dev_info);

    /* get the next device ID from the .inf file */ 
  } while(SetupFindNextLine(&inf_context, &inf_context));

  /* we are done, close the .inf file */
  SetupCloseInfFile(inf_handle);

  usb_registry_stop_libusb_devices(); /* stop all libusb devices */
  usb_registry_start_libusb_devices(); /* restart all libusb devices */

  return 0;
}

bool_t usb_service_load_dll()
{
  if(usb_registry_is_nt())
    {
      advapi32_dll = LoadLibrary("advapi32.dll");
      
      if(!advapi32_dll)
        {
          usb_error("usb_service_load_dll(): loading DLL advapi32.dll"
                          "failed");
          return FALSE;
        }
      
      open_sc_manager = (open_sc_manager_t)
        GetProcAddress(advapi32_dll, "OpenSCManagerA");
      
      open_service = (open_service_t)
        GetProcAddress(advapi32_dll, "OpenServiceA");
      
      change_service_config = (change_service_config_t)
        GetProcAddress(advapi32_dll, "ChangeServiceConfigA");
      
      close_service_handle = (close_service_handle_t)
        GetProcAddress(advapi32_dll, "CloseServiceHandle");
      
      create_service = (create_service_t)
        GetProcAddress(advapi32_dll, "CreateServiceA");
      
      delete_service = (delete_service_t)
        GetProcAddress(advapi32_dll, "DeleteService");
      
      start_service = (start_service_t)
        GetProcAddress(advapi32_dll, "StartServiceA");
      
      query_service_status = (query_service_status_t)
        GetProcAddress(advapi32_dll, "QueryServiceStatus");
      
      control_service = (control_service_t)
        GetProcAddress(advapi32_dll, "ControlService");
      
      if(!open_sc_manager || !open_service || !change_service_config
         || !close_service_handle || !create_service || !delete_service
         || !start_service || !query_service_status || !control_service)
        {
          FreeLibrary(advapi32_dll);
          usb_error("usb_service_load_dll(): loading exported "
                    "functions of advapi32.dll failed");

          return FALSE;
        }
    }
  return TRUE;
}

bool_t usb_service_free_dll()
{
  if(advapi32_dll)
    {
      FreeLibrary(advapi32_dll);
    }
  return TRUE;
}

bool_t usb_service_create(const char *name, const char *display_name,
                          const char *binary_path, unsigned long type,
                          unsigned long start_type)
{
  SC_HANDLE scm = NULL;
  SC_HANDLE service = NULL;
  bool_t ret = FALSE;

  if(!usb_service_load_dll())
    {
      return FALSE;
    }

  do 
    {
      scm = open_sc_manager(NULL, SERVICES_ACTIVE_DATABASE, 
                            SC_MANAGER_ALL_ACCESS);

      if(!scm)
        {
          usb_error("usb_service_create(): opening service control "
                    "manager failed: %s", usb_win_error_to_string());
          break;
        }
      
      service = open_service(scm, name, SERVICE_ALL_ACCESS);

      if(service)
        {
          if(!change_service_config(service,
                                    type,
                                    start_type,
                                    SERVICE_ERROR_NORMAL,
                                    binary_path,
                                    NULL,
                                    NULL,
                                    NULL,
                                    NULL,
                                    NULL,
                                    display_name))
            {
              usb_error("usb_service_create(): changing config of "
                        "service '%s' failed: %s", 
                        name, usb_win_error_to_string());
              break;
            }
          ret = TRUE;
          break;
        }
  
      if(GetLastError() == ERROR_SERVICE_DOES_NOT_EXIST)
        {
          service = create_service(scm,
                                   name,
                                   display_name,
                                   GENERIC_EXECUTE,
                                   type,
                                   start_type,
                                   SERVICE_ERROR_NORMAL, 
                                   binary_path,
                                   NULL, NULL, NULL, NULL, NULL);
	  
          if(!service)
            {
              usb_error("usb_service_create(): creating "
                        "service '%s' failed: %s",
                        name, usb_win_error_to_string());
            }
          ret = TRUE;	
        }
    } while(0);

  if(service)
    {
      close_service_handle(service);
    }
  
  if(scm)
    {
      close_service_handle(scm);
    }
  
  usb_service_free_dll();

  return ret;
}

bool_t usb_service_stop(const char *name)
{
  bool_t ret = FALSE;
  SC_HANDLE scm = NULL;
  SC_HANDLE service = NULL;
  SERVICE_STATUS status;

  if(!usb_service_load_dll())
    {
      return FALSE;
    }

  do
    {
      scm = open_sc_manager(NULL, SERVICES_ACTIVE_DATABASE, 
                            SC_MANAGER_ALL_ACCESS);
      
      if(!scm)
        {
          usb_error("usb_stop_service(): opening service control "
                    "manager failed: %s", usb_win_error_to_string());
          break;
        }
      
      service = open_service(scm, name, SERVICE_ALL_ACCESS);
      
      if(!service)
        {
          ret = TRUE;
          break;
        }

      if(!query_service_status(service, &status))
        {
          usb_error("usb_stop_service(): getting status of "
                    "service '%s' failed: %s", 
                    name, usb_win_error_to_string());
          break;
        }

      if(status.dwCurrentState == SERVICE_STOPPED)
        {
          ret = TRUE;
          break;
        }

      if(!control_service(service, SERVICE_CONTROL_STOP, &status))
        {
          usb_error("usb_stop_service(): stopping service '%s' failed: %s",
                    name, usb_win_error_to_string());
          break;
        }
      
      do 
        {
          int wait = 0;
	  
          if(!query_service_status(service, &status))
            {
              usb_error("usb_stop_service(): getting status of "
                        "service '%s' failed: %s", 
                        name, usb_win_error_to_string());
              break;
            }
          Sleep(500);
          wait += 500;
	  
          if(wait > 20000)
            {
              usb_error("usb_stop_service(): stopping "
                        "service '%s' failed, timeout", name);
              ret = FALSE;
              break;
            }
          ret = TRUE;
        } while(status.dwCurrentState != SERVICE_STOPPED);
    } while(0);

  if(service)
    {
      close_service_handle(service);
    }
  
  if(scm)
    {
      close_service_handle(scm);
    }
  
  usb_service_free_dll();

  return ret;
}

bool_t usb_service_delete(const char *name)
{
  bool_t ret = FALSE;
  SC_HANDLE scm = NULL;
  SC_HANDLE service = NULL;

  if(!usb_service_load_dll())
    {
      return FALSE;
    }

  do 
    {
      scm = open_sc_manager(NULL, SERVICES_ACTIVE_DATABASE, 
                            SC_MANAGER_ALL_ACCESS);
      
      if(!scm)
        {
          usb_error("usb_delete_service(): opening service control "
                    "manager failed: %s", usb_win_error_to_string());
          break;
        }
      
      service = open_service(scm, name, SERVICE_ALL_ACCESS);
  
      if(!service)
        {
          ret = TRUE;
          break;
        }
      

      if(!delete_service(service))
        {
          usb_error("usb_service_delete(): deleting "
                    "service '%s' failed: %s", 
                    name, usb_win_error_to_string());
          break;
        }
      ret = TRUE;
    } while(0);

  if(service)
    {
      close_service_handle(service);
    }
  
  if(scm)
    {
      close_service_handle(scm);
    }
  
  usb_service_free_dll();

  return ret;
}

void CALLBACK usb_touch_inf_file_np_rundll(HWND wnd, HINSTANCE instance,
                                           LPSTR cmd_line, int cmd_show)
{
  usb_touch_inf_file_np(cmd_line);
}

int usb_touch_inf_file_np(const char *inf_file)
{
  const char inf_comment[] = ";added by libusb to break this file's digital "
    "signature";
  const wchar_t inf_comment_uni[] = L";added by libusb to break this file's "
    L"digital signature";

  char buf[1024];
  wchar_t wbuf[1024];
  int found = 0;
  OSVERSIONINFO version;
  FILE *f;

  version.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);

  if(!GetVersionEx(&version))
    return -1;


  /* XP system */
  if((version.dwMajorVersion == 5) && (version.dwMinorVersion >= 1))
    {
      f = fopen(inf_file, "rb");
      
      if(!f)
        return -1;

      while(fgetws(wbuf, sizeof(wbuf)/2, f))
        {
          if(wcsstr(wbuf, inf_comment_uni))
            {
              found = 1;
              break;
            }
        }

      fclose(f);

      if(!found)
        {
          f = fopen(inf_file, "ab");
          /*           fputwc(0x000d, f); */
          /*           fputwc(0x000d, f); */
          fputws(inf_comment_uni, f);
          fclose(f);
        }
    }
  else
    {
      f = fopen(inf_file, "r");
      
      if(!f)
        return -1;

      while(fgets(buf, sizeof(buf), f))
        {
          if(strstr(buf, inf_comment))
            {
              found = 1;
              break;
            }
        }

      fclose(f);

      if(!found)
        {
          f = fopen(inf_file, "a");
          fputs("\n", f);
          fputs(inf_comment, f);
          fputs("\n", f);
          fclose(f);
        }
    }

  return 0;
}

int usb_install_needs_restart_np(void)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index = 0;
  SP_DEVINSTALL_PARAMS install_params;
  int ret = FALSE;

  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  dev_info = SetupDiGetClassDevs(NULL, NULL, NULL,
                                 DIGCF_ALLCLASSES | DIGCF_PRESENT);
  
  SetEnvironmentVariable("LIBUSB_NEEDS_REBOOT", "1");

  if(dev_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_install_needs_restart_np(): getting "
                "device info set failed");
      return ret;
    }
  
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      memset(&install_params, 0, sizeof(SP_PROPCHANGE_PARAMS));
      install_params.cbSize = sizeof(SP_DEVINSTALL_PARAMS);

      if(SetupDiGetDeviceInstallParams(dev_info, &dev_info_data, 
                                       &install_params))
        {
          if(install_params.Flags & (DI_NEEDRESTART | DI_NEEDREBOOT))
            {
              usb_message("usb_install_needs_restart_np(): restart needed");
              ret = TRUE;
            }
        }
      
      dev_index++;
    }
  
  SetupDiDestroyDeviceInfoList(dev_info);

  return ret;
}
