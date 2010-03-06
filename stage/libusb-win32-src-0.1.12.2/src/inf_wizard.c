/* LIBUSB-WIN32, Generic Windows USB Library
 * Copyright (c) 2002-2006 Stephan Meyer <ste_meyer@web.de>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#ifdef __GNUC__
#define _WIN32_IE 0x0400
#define WINVER 0x0500
#endif

#define INITGUID

#include <windows.h>
#include <commdlg.h>
#include <dbt.h>
#include <stdio.h>
#include <stdlib.h>
#include <initguid.h>
#include <commctrl.h>
#include <setupapi.h>

#include "registry.h"

#define __INF_WIZARD_C__
#include "inf_wizard_rc.rc"


#define _STRINGIFY(x) #x
#define STRINGIFY(x) _STRINGIFY(x)


DEFINE_GUID(GUID_DEVINTERFACE_USB_HUB, 0xf18a0e88, 0xc30c, 0x11d0, 0x88, \
            0x15, 0x00, 0xa0, 0xc9, 0x06, 0xbe, 0xd8);

DEFINE_GUID(GUID_DEVINTERFACE_USB_DEVICE, 0xA5DCBF10L, 0x6530, 0x11D2, \
            0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED);

const char cat_file_content[] =
"This file will contain the digital signature of the files to be installed\n"
"on the system.\n"
"This file will be provided by Microsoft upon certification of your "
"drivers.\n";

const char info_text_0[] = 
"This program will create an .inf file for your device.\n\n"
"Before clicking \"Next\" make sure that your device is connected to the "
"system.\n";

const char info_text_1[] = 
"An .inf and .cat file has been created successfully for the following "
"device:\n\n";

const char list_header_text[] = 
"Select your device from the list of detected devices below.\n"
"If your device isn't listed then either connect it or just click \"Next\"\n"
"and enter your device description manually\n";

const char inf_header[] = 
"[Version]\n"
"Signature = \"$Chicago$\"\n"
"provider  = %manufacturer%\n"
"DriverVer = " STRINGIFY(INF_DATE) "," STRINGIFY(INF_VERSION) "\n";

const char inf_body[] = 
"Class = LibUsbDevices\n"
"ClassGUID = {EB781AAF-9C70-4523-A5DF-642A87ECA567}\n"
"\n"
"[ClassInstall]\n"
"AddReg=libusb_class_install_add_reg\n"
"\n"
"[ClassInstall32]\n"
"AddReg=libusb_class_install_add_reg\n"
"\n"
"[libusb_class_install_add_reg]\n"
"HKR,,,,\"LibUSB-Win32 Devices\"\n"
"HKR,,Icon,,\"-20\"\n"
"\n"
"[Manufacturer]\n"
"%manufacturer%=Devices,NT,NTAMD64\n"
"\n"
";--------------------------------------------------------------------------\n"
"; Files\n"
";--------------------------------------------------------------------------\n"
"\n"
"[SourceDisksNames]\n"
"1 = \"Libusb-Win32 Driver Installation Disk\",,\n"
"\n"
"[SourceDisksFiles]\n"
"libusb0.sys = 1,,\n"
"libusb0.dll = 1,,\n"
"libusb0_x64.sys = 1,,\n"
"libusb0_x64.dll = 1,,\n"
"\n"
"[DestinationDirs]\n"
"libusb_files_sys = 10,system32\\drivers\n"
"libusb_files_sys_x64 = 10,system32\\drivers\n"
"libusb_files_dll = 10,system32\n"
"libusb_files_dll_wow64 = 10,syswow64\n"
"libusb_files_dll_x64 = 10,system32\n"
"\n"
"[libusb_files_sys]\n"
"libusb0.sys\n"
"\n"
"[libusb_files_sys_x64]\n"
"libusb0.sys,libusb0_x64.sys\n"
"\n"
"[libusb_files_dll]\n"
"libusb0.dll\n"
"\n"
"[libusb_files_dll_wow64]\n"
"libusb0.dll\n"
"\n"
"[libusb_files_dll_x64]\n"
"libusb0.dll,libusb0_x64.dll\n"
"\n"
";--------------------------------------------------------------------------\n"
"; Device driver\n"
";--------------------------------------------------------------------------\n"
"\n"
"[LIBUSB_DEV]\n"
"CopyFiles = libusb_files_sys, libusb_files_dll\n"
"AddReg    = libusb_add_reg\n"
"\n"
"[LIBUSB_DEV.NT]\n"
"CopyFiles = libusb_files_sys, libusb_files_dll\n"
"\n"
"[LIBUSB_DEV.NTAMD64]\n"
"CopyFiles = libusb_files_sys_x64, libusb_files_dll_wow64, libusb_files_dll_x64\n"
"\n"
"[LIBUSB_DEV.HW]\n"
"DelReg = libusb_del_reg_hw\n"
"AddReg = libusb_add_reg_hw\n"
"\n"
"[LIBUSB_DEV.NT.HW]\n"
"DelReg = libusb_del_reg_hw\n"
"AddReg = libusb_add_reg_hw\n"
"\n"
"[LIBUSB_DEV.NTAMD64.HW]\n"
"DelReg = libusb_del_reg_hw\n"
"AddReg = libusb_add_reg_hw\n"
"\n"
"[LIBUSB_DEV.NT.Services]\n"
"AddService = libusb0, 0x00000002, libusb_add_service\n"
"\n"
"[LIBUSB_DEV.NTAMD64.Services]\n"
"AddService = libusb0, 0x00000002, libusb_add_service\n"
"\n"
"[libusb_add_reg]\n"
"HKR,,DevLoader,,*ntkern\n"
"HKR,,NTMPDriver,,libusb0.sys\n"
"\n"
"; Older versions of this .inf file installed filter drivers. They are not\n"
"; needed any more and must be removed\n"
"[libusb_del_reg_hw]\n"
"HKR,,LowerFilters\n"
"HKR,,UpperFilters\n"
"\n"
"; Device properties\n"
"[libusb_add_reg_hw]\n"
"HKR,,SurpriseRemovalOK, 0x00010001, 1\n"
"\n"
";--------------------------------------------------------------------------\n"
"; Services\n"
";--------------------------------------------------------------------------\n"
"\n"
"[libusb_add_service]\n"
"DisplayName    = \"LibUsb-Win32 - Kernel Driver " 
STRINGIFY(INF_DATE) ", " STRINGIFY(INF_VERSION) "\"\n"
"ServiceType    = 1\n"
"StartType      = 3\n"
"ErrorControl   = 0\n"
"ServiceBinary  = %12%\\libusb0.sys\n"
"\n"
";--------------------------------------------------------------------------\n"
"; Devices\n"
";--------------------------------------------------------------------------\n"
"\n";

const char strings_header[] =
"\n"
";--------------------------------------------------------------------------\n"
"; Strings\n"
";--------------------------------------------------------------------------\n"
"\n"
"[Strings]\n";

typedef struct {
  int vid;
  int pid;
  char description[MAX_PATH];
  char manufacturer[MAX_PATH];
} device_context_t;


BOOL CALLBACK dialog_proc_0(HWND dialog, UINT message, 
                            WPARAM w_param, LPARAM l_param);
BOOL CALLBACK dialog_proc_1(HWND dialog, UINT message, 
                            WPARAM w_param, LPARAM l_param);
BOOL CALLBACK dialog_proc_2(HWND dialog, UINT message, 
                            WPARAM w_param, LPARAM l_param);
BOOL CALLBACK dialog_proc_3(HWND dialog, UINT message, 
                            WPARAM w_param, LPARAM l_param);

static void device_list_init(HWND list);
static void device_list_refresh(HWND list);
static void device_list_add(HWND list, device_context_t *device);
static void device_list_clean(HWND list);

static int save_file(HWND dialog, device_context_t *device);

int APIENTRY WinMain(HINSTANCE instance, HINSTANCE prev_instance,
                     LPSTR cmd_line, int cmd_show)
{
  int next_dialog;
  device_context_t device;

  LoadLibrary("comctl32.dll");
  InitCommonControls();

  memset(&device, 0, sizeof(device));

  next_dialog = ID_DIALOG_0;

  while(next_dialog)
    {
      switch(next_dialog)
        {
          case ID_DIALOG_0:
            next_dialog = (int)DialogBoxParam(instance, 
                                              MAKEINTRESOURCE(next_dialog), 
                                              NULL, (DLGPROC)dialog_proc_0,
                                              (LPARAM)&device);

            break;
          case ID_DIALOG_1:
            next_dialog = (int)DialogBoxParam(instance, 
                                              MAKEINTRESOURCE(next_dialog), 
                                              NULL, (DLGPROC)dialog_proc_1,
                                              (LPARAM)&device);
            break;
          case ID_DIALOG_2:
            next_dialog = (int)DialogBoxParam(instance, 
                                              MAKEINTRESOURCE(next_dialog), 
                                              NULL, (DLGPROC)dialog_proc_2,
                                              (LPARAM)&device);
            break;
          case ID_DIALOG_3:
            next_dialog = (int)DialogBoxParam(instance, 
                                              MAKEINTRESOURCE(next_dialog), 
                                              NULL, (DLGPROC)dialog_proc_3,
                                              (LPARAM)&device);
            break;
        default:
          ;
        }
    }

  return 0;
}


BOOL CALLBACK dialog_proc_0(HWND dialog, UINT message, 
                               WPARAM w_param, LPARAM l_param)
{
  switch(message)
    {
    case WM_INITDIALOG:
      SetWindowText(GetDlgItem(dialog, ID_INFO_TEXT), info_text_0);
      return TRUE;
      
    case WM_COMMAND:
      switch(LOWORD(w_param))
        {
        case ID_BUTTON_NEXT:
          EndDialog(dialog, ID_DIALOG_1);
          return TRUE ;
        case ID_BUTTON_CANCEL:
        case IDCANCEL:
          EndDialog(dialog, 0);
          return TRUE ;
        }
    }
  
  return FALSE;
}

BOOL CALLBACK dialog_proc_1(HWND dialog, UINT message, 
                            WPARAM w_param, LPARAM l_param)
{
  static HDEVNOTIFY notification_handle_hub = NULL;
  static HDEVNOTIFY notification_handle_dev = NULL;
  DEV_BROADCAST_HDR *hdr = (DEV_BROADCAST_HDR *) l_param;
  DEV_BROADCAST_DEVICEINTERFACE dev_if;
  static device_context_t *device = NULL;
  HWND list = GetDlgItem(dialog, ID_LIST);
  LVITEM item;

  switch(message)
    {
    case WM_INITDIALOG:
      device = (device_context_t *)l_param;
      memset(device, 0, sizeof(*device));

      SetWindowText(GetDlgItem(dialog, ID_LIST_HEADER_TEXT), list_header_text);
      device_list_init(list);
      device_list_refresh(list);

      dev_if.dbcc_size = sizeof(dev_if);
      dev_if.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
      
      dev_if.dbcc_classguid = GUID_DEVINTERFACE_USB_HUB;
      notification_handle_hub = RegisterDeviceNotification(dialog, &dev_if, 0);
      
      dev_if.dbcc_classguid = GUID_DEVINTERFACE_USB_DEVICE;
      notification_handle_dev = RegisterDeviceNotification(dialog, &dev_if, 0);

      return TRUE;
      
    case WM_DEVICECHANGE:
      switch(w_param)
        {
        case DBT_DEVICEREMOVECOMPLETE:
          if(hdr->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE) 
            device_list_refresh(list);
          break;
        case DBT_DEVICEARRIVAL:
          if(hdr->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
            device_list_refresh(list);
          break;
        default:
          ;
        }
      return TRUE;

    case WM_COMMAND:
      switch(LOWORD(w_param))
        {
        case ID_BUTTON_NEXT:
          if(notification_handle_hub)
            UnregisterDeviceNotification(notification_handle_hub);
          if(notification_handle_dev)
            UnregisterDeviceNotification(notification_handle_dev);

          memset(&item, 0, sizeof(item));
          item.mask = LVIF_TEXT | LVIF_PARAM; 
          item.iItem = ListView_GetNextItem(list, -1, LVNI_SELECTED);
          
          memset(device, 0, sizeof(*device));

          if(item.iItem >= 0)
            {
              if(ListView_GetItem(list, &item))
                {
                  if(item.lParam)
                    {
                      memcpy(device, (void *)item.lParam, sizeof(*device));
                    }
                }
            }

          if(!device->vid)
            {
              device->vid = 0x12AB;
              device->pid = 0x12AB;
            }

          if(!device->manufacturer[0])
            strcpy(device->manufacturer, "Insert manufacturer name");
          if(!device->description[0])
            strcpy(device->description,  "Insert device description");

          if(notification_handle_hub)
            UnregisterDeviceNotification(notification_handle_hub);
          if(notification_handle_dev)
            UnregisterDeviceNotification(notification_handle_dev);

          device_list_clean(list);

          EndDialog(dialog, ID_DIALOG_2);
          return TRUE;

        case ID_BUTTON_BACK:
          device_list_clean(list);
          if(notification_handle_hub)
            UnregisterDeviceNotification(notification_handle_hub);
          if(notification_handle_dev)
            UnregisterDeviceNotification(notification_handle_dev);
          EndDialog(dialog, ID_DIALOG_0);
          return TRUE ;

        case ID_BUTTON_CANCEL:
        case IDCANCEL:
          device_list_clean(list);
          if(notification_handle_hub)
            UnregisterDeviceNotification(notification_handle_hub);
          if(notification_handle_dev)
            UnregisterDeviceNotification(notification_handle_dev);
          EndDialog(dialog, 0);
          return TRUE ;
        }
    }
  
  return FALSE;
}

BOOL CALLBACK dialog_proc_2(HWND dialog, UINT message, 
                             WPARAM w_param, LPARAM l_param)
{
  static device_context_t *device = NULL;
  char tmp[MAX_PATH];

  switch(message)
    {
    case WM_INITDIALOG:
      device = (device_context_t *)l_param;

      if(device)
        {
          memset(tmp, 0, sizeof(tmp));
          sprintf(tmp, "0x%04X", device->vid);
          SetWindowText(GetDlgItem(dialog, ID_TEXT_VID), tmp);

          memset(tmp, 0, sizeof(tmp));
          sprintf(tmp, "0x%04X", device->pid);
          SetWindowText(GetDlgItem(dialog, ID_TEXT_PID), tmp);

          SetWindowText(GetDlgItem(dialog, ID_TEXT_MANUFACTURER), 
                        device->manufacturer);
          SetWindowText(GetDlgItem(dialog, ID_TEXT_DEV_NAME),
                        device->description);
        }
      return TRUE;
      
    case WM_COMMAND:
      switch(LOWORD(w_param))
        {
        case ID_BUTTON_NEXT:
          memset(device, 0, sizeof(*device));

          GetWindowText(GetDlgItem(dialog, ID_TEXT_MANUFACTURER), 
                        device->manufacturer, sizeof(tmp));
          GetWindowText(GetDlgItem(dialog, ID_TEXT_DEV_NAME),
                        device->description, sizeof(tmp));

          GetWindowText(GetDlgItem(dialog, ID_TEXT_VID), tmp, sizeof(tmp));
          sscanf(tmp, "0x%04x", &device->vid);

          GetWindowText(GetDlgItem(dialog, ID_TEXT_PID), tmp, sizeof(tmp));
          sscanf(tmp, "0x%04x", &device->pid);

          if(save_file(dialog, device))
            EndDialog(dialog, ID_DIALOG_3);
          return TRUE ;
        case ID_BUTTON_BACK:
          EndDialog(dialog, ID_DIALOG_1);
          return TRUE ;
        case ID_BUTTON_CANCEL:
        case IDCANCEL:
          EndDialog(dialog, 0);
          return TRUE ;
        }
    }
  
  return FALSE;
}

BOOL CALLBACK dialog_proc_3(HWND dialog, UINT message, 
                            WPARAM w_param, LPARAM l_param)
{
  static device_context_t *device = NULL;
  char tmp[MAX_PATH];

  switch(message)
    {
    case WM_INITDIALOG:
      device = (device_context_t *)l_param;

      sprintf(tmp,
              "%s\n"
              "Vendor ID:\t\t 0x%04X\n\n"
              "Product ID:\t\t 0x%04X\n\n"
              "Device description:\t %s\n\n"
              "Manufacturer:\t\t %s\n\n",
              info_text_1,
              device->vid,
              device->pid,
              device->description,
              device->manufacturer);

      SetWindowText(GetDlgItem(dialog, ID_INFO_TEXT), tmp);
      return TRUE;
      
    case WM_COMMAND:
      switch(LOWORD(w_param))
        {
        case ID_BUTTON_NEXT:
        case IDCANCEL:
          EndDialog(dialog, 0);
          return TRUE ;
        }
    }
  
  return FALSE;
}

static void device_list_init(HWND list)
{
  LVCOLUMN lvc; 

  ListView_SetExtendedListViewStyle(list, LVS_EX_FULLROWSELECT);

  memset(&lvc, 0, sizeof(lvc));

  lvc.mask = LVCF_FMT | LVCF_WIDTH | LVCF_TEXT; 
  lvc.fmt = LVCFMT_LEFT; 

  lvc.cx = 70; 
  lvc.iSubItem = 0;
  lvc.pszText = "Vendor ID";
  ListView_InsertColumn(list, 1, &lvc); 

  lvc.cx = 70; 
  lvc.iSubItem = 1;
  lvc.pszText = "Product ID";
  ListView_InsertColumn(list, 2, &lvc); 

  lvc.cx = 250; 
  lvc.iSubItem = 2;
  lvc.pszText = "Description";
  ListView_InsertColumn(list, 3, &lvc); 
}

static void device_list_refresh(HWND list)
{
  HDEVINFO dev_info;
  SP_DEVINFO_DATA dev_info_data;
  int dev_index = 0;
  device_context_t *device;
  char tmp[MAX_PATH];

  device_list_clean(list);

  dev_info_data.cbSize = sizeof(SP_DEVINFO_DATA);
  dev_index = 0;

  dev_info = SetupDiGetClassDevs(NULL, "USB", NULL,
                                 DIGCF_ALLCLASSES | DIGCF_PRESENT);
  
  if(dev_info == INVALID_HANDLE_VALUE)
    {
      return;
    }
  
  while(SetupDiEnumDeviceInfo(dev_info, dev_index, &dev_info_data))
    {
      if(usb_registry_match(dev_info, &dev_info_data))
        {
          device = (device_context_t *) malloc(sizeof(device_context_t));
          memset(device, 0, sizeof(*device));
        
          usb_registry_get_property(SPDRP_HARDWAREID, dev_info, 
                                    &dev_info_data,
                                    tmp, sizeof(tmp) - 1);

          sscanf(tmp + sizeof("USB\\VID_") - 1, "%04x", &device->vid);
          sscanf(tmp + sizeof("USB\\VID_XXXX&PID_") - 1, "%04x", &device->pid);
                    
          usb_registry_get_property(SPDRP_DEVICEDESC, dev_info, 
                                    &dev_info_data,
                                    tmp, sizeof(tmp) - 1);
          strcpy(device->description, tmp);

          usb_registry_get_property(SPDRP_MFG, dev_info, 
                                    &dev_info_data,
                                    tmp, sizeof(tmp) - 1);
          strcpy(device->manufacturer, tmp);

          device_list_add(list, device);
        }

      dev_index++;
    }
  
  SetupDiDestroyDeviceInfoList(dev_info);
}

static void device_list_add(HWND list, device_context_t *device)
{
  LVITEM item;
  char vid[32];
  char pid[32];

  memset(&item, 0, sizeof(item));
  memset(vid, 0, sizeof(vid));
  memset(pid, 0, sizeof(pid));

  sprintf(vid, "0x%04X", device->vid);
  sprintf(pid, "0x%04X", device->pid);

  item.mask = LVIF_TEXT | LVIF_PARAM; 
  item.lParam = (LPARAM)device;

  ListView_InsertItem(list, &item);                  

  ListView_SetItemText(list, 0, 0, vid);
  ListView_SetItemText(list, 0, 1, pid);
  ListView_SetItemText(list, 0, 2, device->description);
}

static void device_list_clean(HWND list)
{
  LVITEM item;

  memset(&item, 0, sizeof(LVITEM));

  while(ListView_GetItem(list, &item))
    {
      if(item.lParam)
        free((void *)item.lParam);
      
      ListView_DeleteItem(list, 0);
      memset(&item, 0, sizeof(LVITEM));
    }
}

static int save_file(HWND dialog, device_context_t *device)
{
  OPENFILENAME open_file;
  char inf_name[MAX_PATH];
  char inf_path[MAX_PATH];

  char cat_name[MAX_PATH];
  char cat_path[MAX_PATH];

  char cat_name_x64[MAX_PATH];
  char cat_path_x64[MAX_PATH];

  char error[MAX_PATH];
  FILE *file;

  memset(&open_file, 0, sizeof(open_file));
  strcpy(inf_path, "your_file.inf");

  open_file.lStructSize = sizeof(OPENFILENAME);
  open_file.hwndOwner = dialog;
  open_file.lpstrFile = inf_path;
  open_file.nMaxFile = sizeof(inf_path);
  open_file.lpstrFilter = "*.inf\0*.inf\0";
  open_file.nFilterIndex = 1;
  open_file.lpstrFileTitle = inf_name;
  open_file.nMaxFileTitle = sizeof(inf_name);
  open_file.lpstrInitialDir = NULL;
  open_file.Flags = OFN_PATHMUSTEXIST;
  open_file.lpstrDefExt = "inf";
  
  if(GetSaveFileName(&open_file))
    {
      strcpy(cat_path, inf_path);
      strcpy(cat_name, inf_name);
      strcpy(cat_path_x64, inf_path);
      strcpy(cat_name_x64, inf_name);

      strcpy(strstr(cat_path, ".inf"), ".cat");
      strcpy(strstr(cat_name, ".inf"), ".cat");
      strcpy(strstr(cat_path_x64, ".inf"), "_x64.cat");
      strcpy(strstr(cat_name_x64, ".inf"), "_x64.cat");

      file = fopen(inf_path, "w");

      if(file)
        {
          fprintf(file, "%s", inf_header);
          fprintf(file, "CatalogFile = %s\n", cat_name);
          fprintf(file, "CatalogFile.NT = %s\n", cat_name);
          fprintf(file, "CatalogFile.NTAMD64 = %s\n\n", cat_name_x64);
          fprintf(file, "%s", inf_body);

          fprintf(file, "[Devices]\n");
          fprintf(file, "\"%s\"=LIBUSB_DEV, USB\\VID_%04x&PID_%04x\n\n", 
                  device->description,
                  device->vid, device->pid);
          fprintf(file, "[Devices.NT]\n");
          fprintf(file, "\"%s\"=LIBUSB_DEV, USB\\VID_%04x&PID_%04x\n\n", 
                  device->description,
                  device->vid, device->pid);
          fprintf(file, "[Devices.NTAMD64]\n");
          fprintf(file, "\"%s\"=LIBUSB_DEV, USB\\VID_%04x&PID_%04x\n\n", 
                  device->description,
                  device->vid, device->pid);

          fprintf(file, strings_header);
          fprintf(file, "manufacturer = \"%s\"\n", device->manufacturer);

          fclose(file);
        }
      else
        {
          sprintf(error, "Error: unable to open file: %s", inf_name);
          MessageBox(dialog, error, "Error",
                     MB_OK | MB_APPLMODAL | MB_ICONWARNING);
        }


      file = fopen(cat_path, "w");

      if(file)
        {
          fprintf(file, "%s", cat_file_content);
          fclose(file);
        }
      else
        {
          sprintf(error, "Error: unable to open file: %s", cat_name);
          MessageBox(dialog, error, "Error",
                     MB_OK | MB_APPLMODAL | MB_ICONWARNING);
        }

      file = fopen(cat_path_x64, "w");

      if(file)
        {
          fprintf(file, "%s", cat_file_content);
          fclose(file);
        }
      else
        {
          sprintf(error, "Error: unable to open file: %s", cat_name_x64);
          MessageBox(dialog, error, "Error",
                     MB_OK | MB_APPLMODAL | MB_ICONWARNING);
        }

      return TRUE;
    }
  return FALSE;
}
