/* LIBUSB-WIN32, Generic Windows USB Library
 * Copyright (c) 2002-2004 Stephan Meyer <ste_meyer@web.de>
 * Copyright (c) 2000-2004 Johannes Erdfelt <johannes@erdfelt.com>
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


#ifdef __GNUC__
#define WINVER 0x0500
#endif

#define INITGUID

#include <windows.h>
#include <dbt.h>
#include <stdio.h>
#include <initguid.h>

#include "usb.h"


#define LIBUSB_WINDOW_CLASS "LIBUSB_WINDOW_CLASS"
#define LIBUSB_BUTTON_HEIGHT 30
#define LIBUSB_BUTTON_WIDTH 70
#define LIBUSB_BORDER 10

DEFINE_GUID(GUID_DEVINTERFACE_USB_HUB, 0xf18a0e88, 0xc30c, 0x11d0, 0x88, \
            0x15, 0x00, 0xa0, 0xc9, 0x06, 0xbe, 0xd8);

DEFINE_GUID(GUID_DEVINTERFACE_USB_DEVICE, 0xA5DCBF10L, 0x6530, 0x11D2, \
            0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED);

#define EDIT_BUF_INCREMENT 1024

enum {
  ID_EXIT = 100,
  ID_REFRESH,
  ID_EDIT
};

static char *edit_buffer = NULL;
static int edit_buffer_size = 0;
static int edit_buffer_pos = 0;

static HWND main_win;
static HWND exit_button;
static HWND refresh_button;
static HWND edit_box;

static HDEVNOTIFY notification_handle_hub, notification_handle_dev;

LRESULT CALLBACK win_proc(HWND handle, UINT message, WPARAM w_param, 
                          LPARAM l_param);

static void on_size(int width, int height);
static void on_refresh(void);

static void edit_printf_init(void);
static void edit_printf_free(void);
static void edit_printf(const char *s, ...);

static void print_configuration(struct usb_config_descriptor *config);
static void print_interface(struct usb_interface *interface);
static void print_altsetting(struct usb_interface_descriptor *interface);
static void print_endpoint(struct usb_endpoint_descriptor *endpoint);

int APIENTRY WinMain(HINSTANCE instance, HINSTANCE prev_instance,
                     LPSTR cmd_line, int cmd_show)
{
  MSG msg;
  WNDCLASSEX win_class;
  DEV_BROADCAST_DEVICEINTERFACE dev_if;

  LoadLibrary("comctl32.dll");

  win_class.cbSize = sizeof(WNDCLASSEX); 
  
  win_class.style = CS_HREDRAW | CS_VREDRAW | CS_GLOBALCLASS ;
  win_class.lpfnWndProc = win_proc;
  win_class.cbClsExtra = 0;
  win_class.cbWndExtra = 0;
  win_class.hInstance = instance;
  win_class.hIcon = NULL;
  win_class.hCursor = LoadCursor(NULL, IDC_ARROW);
  win_class.hbrBackground = (HBRUSH)(COLOR_3DFACE + 1);
  win_class.lpszMenuName = NULL;
  win_class.lpszClassName = LIBUSB_WINDOW_CLASS;
  win_class.hIconSm = NULL;

  RegisterClassEx(&win_class);

  main_win = CreateWindowEx(WS_EX_APPWINDOW| WS_EX_CONTROLPARENT,
                            LIBUSB_WINDOW_CLASS, 
                            "TestLibUsb - Windows Version", 
                            WS_OVERLAPPEDWINDOW | WS_CLIPCHILDREN 
                            | WS_DLGFRAME,
                            CW_USEDEFAULT, 0, 500, 500, NULL, NULL, 
                            instance, NULL);
  if(!main_win) 
    {
      return FALSE;
    }


  exit_button = CreateWindow("BUTTON", "Exit",
                             WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
                             10, 10, 
                             LIBUSB_BUTTON_WIDTH, LIBUSB_BUTTON_HEIGHT, 
                             main_win, (HMENU) ID_EXIT, instance, NULL);

  refresh_button = CreateWindow("BUTTON", "Refresh",
                                WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
                                10, 100, 
                                LIBUSB_BUTTON_WIDTH, LIBUSB_BUTTON_HEIGHT, 
                                main_win, (HMENU) ID_REFRESH, instance, 
                                NULL);

  edit_box = CreateWindowEx(WS_EX_CLIENTEDGE, "EDIT", NULL,
                            WS_CHILD | WS_VISIBLE | WS_VSCROLL | 
                            ES_LEFT | ES_MULTILINE | ES_AUTOVSCROLL 
                            | ES_AUTOHSCROLL | ES_READONLY, 
                            CW_USEDEFAULT, CW_USEDEFAULT, 
                            CW_USEDEFAULT, CW_USEDEFAULT,
                            main_win, (HMENU) ID_EDIT, instance, NULL); 

  SendMessage(edit_box, WM_SETFONT, (WPARAM) CreateFont(13, 8, 0, 0,
                                                        400, 0, 0, 0,
                                                        0, 1, 2, 1,
                                                        49, "Courier"), 0);

  ShowWindow(main_win, cmd_show);
  UpdateWindow(main_win);
  BringWindowToTop(main_win);

  usb_set_debug(4);
  usb_init();
  usb_find_busses();

  on_refresh();

  dev_if.dbcc_size = sizeof(dev_if);
  dev_if.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
  dev_if.dbcc_classguid = GUID_DEVINTERFACE_USB_HUB;

  notification_handle_hub = RegisterDeviceNotification(main_win, &dev_if, 0);

  dev_if.dbcc_classguid = GUID_DEVINTERFACE_USB_DEVICE;

  notification_handle_dev = RegisterDeviceNotification(main_win, &dev_if, 0);

  while(GetMessage(&msg, NULL, 0, 0) ) 
    {
      TranslateMessage(&msg);
      DispatchMessage(&msg);
    }

  DestroyWindow(main_win);
  UnregisterClass(LIBUSB_WINDOW_CLASS, instance);

  return 0;
}


LRESULT CALLBACK win_proc(HWND win, UINT message, WPARAM w_param, 
                          LPARAM l_param)
{
  DEV_BROADCAST_HDR *hdr = (DEV_BROADCAST_HDR *) l_param;

  switch(message) 
    {
    case WM_DESTROY:
      if(notification_handle_hub)
        UnregisterDeviceNotification(notification_handle_hub);
      if(notification_handle_dev)
        UnregisterDeviceNotification(notification_handle_dev);

      PostQuitMessage(0);
      break;

    case WM_SIZE:
      on_size(LOWORD(l_param), HIWORD(l_param));
      break;

    case WM_COMMAND:
      switch(LOWORD(w_param))
        {
        case ID_EXIT:
          PostQuitMessage(0);
          break;
        case ID_REFRESH:
          on_refresh();
          break;
        default:
          return DefWindowProc(win, message, w_param, l_param );
        }
      break;

    case WM_DEVICECHANGE:
      switch(w_param)
        {
        case DBT_DEVICEREMOVECOMPLETE:
          if(hdr->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE) 
            on_refresh();
          break;
        case DBT_DEVICEARRIVAL:
          if(hdr->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
            on_refresh();
          break;
        default:
          ;
        }
      break;

    default:
      return DefWindowProc(win, message, w_param, l_param);
    }

  return 0;
}

static void on_size(int width, int height)
{
  int _width, _height, _x, _y;

  _width = LIBUSB_BUTTON_WIDTH;
  _height = LIBUSB_BUTTON_HEIGHT;
  _x = width - LIBUSB_BORDER - LIBUSB_BUTTON_WIDTH;
  _y = height - LIBUSB_BORDER - LIBUSB_BUTTON_HEIGHT;

  SetWindowPos(exit_button, HWND_TOP, _x, _y, _width, _height, 
               SWP_SHOWWINDOW);

  _x -= LIBUSB_BUTTON_WIDTH + LIBUSB_BORDER;
  SetWindowPos(refresh_button, HWND_TOP, _x, _y, _width, _height, 
               SWP_SHOWWINDOW);
 
  _width = width - 2 * LIBUSB_BORDER;
  _height = height - 4 * LIBUSB_BORDER - LIBUSB_BUTTON_HEIGHT;
  _y = LIBUSB_BORDER;
  _x = LIBUSB_BORDER;

  SetWindowPos(edit_box, HWND_TOP, _x, _y, _width, _height, 
               SWP_SHOWWINDOW);
}

static void on_refresh(void)
{
  struct usb_bus *bus;
  struct usb_device *dev;
  const struct usb_version *version;

  edit_printf_init();

  usb_find_devices();

  version = usb_get_version();

  if(version)
    {
      edit_printf("DLL version:\t%d.%d.%d.%d\r\n",
                  version->dll.major, version->dll.minor, 
                  version->dll.micro, version->dll.nano);

      edit_printf("Driver version:\t%d.%d.%d.%d\r\n\r\n",
                  version->driver.major, version->driver.minor, 
                  version->driver.micro, version->driver.nano);
    }

  edit_printf("bus/device  idVendor/idProduct\r\n");

  for (bus = usb_get_busses(); bus; bus = bus->next) {
    for (dev = bus->devices; dev; dev = dev->next) {
      int ret, i;
      char string[256];
      usb_dev_handle *udev;

      edit_printf("%s/%s     %04X/%04X\r\n", bus->dirname, dev->filename,
                  dev->descriptor.idVendor, dev->descriptor.idProduct);
      udev = usb_open(dev);
      
      if (udev) {
        if (dev->descriptor.iManufacturer) {
          ret = usb_get_string_simple(udev, dev->descriptor.iManufacturer,
                                      string, sizeof(string));
          if (ret > 0)
            edit_printf("- Manufacturer : %s\r\n", string);
          else
            edit_printf("- Unable to fetch manufacturer string\r\n");
        }

        if (dev->descriptor.iProduct) {
          ret = usb_get_string_simple(udev, dev->descriptor.iProduct, string,
                                      sizeof(string));
          if (ret > 0)
            edit_printf("- Product      : %s\r\n", string);
          else
            edit_printf("- Unable to fetch product string\r\n");
        }

        if (dev->descriptor.iSerialNumber) {
          ret = usb_get_string_simple(udev, dev->descriptor.iSerialNumber, 
                                      string, sizeof(string));
          if (ret > 0)
            edit_printf("- Serial Number: %s\r\n", string);
          else
            edit_printf("- Unable to fetch serial number string\r\n");
        }

        usb_close (udev);
      }

      if (!dev->config) {
        edit_printf("  Couldn't retrieve descriptors\r\n");
        continue;
      }

      for (i = 0; i < dev->descriptor.bNumConfigurations; i++)
        print_configuration(&dev->config[i]);
    }
  }

  SendMessage(edit_box, WM_SETTEXT, 0, (LPARAM) edit_buffer); 
  edit_printf_free();
}

static void edit_printf_init(void)
{
  if(edit_buffer)
    {
      free(edit_buffer);
    }

  edit_buffer = malloc(EDIT_BUF_INCREMENT);
  
  if(edit_buffer)
    {
      edit_buffer_size = EDIT_BUF_INCREMENT;
    }
  edit_buffer_size = 0;
  edit_buffer_pos = 0;
}

static void edit_printf_free(void)
{
  if(edit_buffer)
    {
      free(edit_buffer);
    }
  edit_buffer = NULL;
  edit_buffer_size = 0;
  edit_buffer_pos = 0;
}

static void edit_printf(const char *s, ...)
{
  va_list args;
  va_start(args, s);

  if(edit_buffer_size - edit_buffer_pos < EDIT_BUF_INCREMENT)
    {
      char *tmp = realloc(edit_buffer, 
                          edit_buffer_size + EDIT_BUF_INCREMENT);

      if(tmp)
        {
          edit_buffer = tmp;
          edit_buffer_size += EDIT_BUF_INCREMENT;
        }
      else
        {
          return;
        }
    }

  edit_buffer_pos += vsnprintf(edit_buffer + edit_buffer_pos, 
                               EDIT_BUF_INCREMENT - 1, s, args);
  va_end(args);
}


static void print_endpoint(struct usb_endpoint_descriptor *endpoint)
{
  edit_printf("      bEndpointAddress: %02xh\r\n", endpoint->bEndpointAddress);
  edit_printf("      bmAttributes:     %02xh\r\n", endpoint->bmAttributes);
  edit_printf("      wMaxPacketSize:   %d\r\n", endpoint->wMaxPacketSize);
  edit_printf("      bInterval:        %d\r\n", endpoint->bInterval);
  edit_printf("      bRefresh:         %d\r\n", endpoint->bRefresh);
  edit_printf("      bSynchAddress:    %d\r\n", endpoint->bSynchAddress);
}


static void print_altsetting(struct usb_interface_descriptor *interface)
{
  int i;

  edit_printf("    bInterfaceNumber:   %d\r\n", interface->bInterfaceNumber);
  edit_printf("    bAlternateSetting:  %d\r\n", interface->bAlternateSetting);
  edit_printf("    bNumEndpoints:      %d\r\n", interface->bNumEndpoints);
  edit_printf("    bInterfaceClass:    %d\r\n", interface->bInterfaceClass);
  edit_printf("    bInterfaceSubClass: %d\r\n", interface->bInterfaceSubClass);
  edit_printf("    bInterfaceProtocol: %d\r\n", interface->bInterfaceProtocol);
  edit_printf("    iInterface:         %d\r\n", interface->iInterface);

  for (i = 0; i < interface->bNumEndpoints; i++)
    print_endpoint(&interface->endpoint[i]);
}


static void print_interface(struct usb_interface *interface)
{
  int i;

  for (i = 0; i < interface->num_altsetting; i++)
    print_altsetting(&interface->altsetting[i]);
}


static void print_configuration(struct usb_config_descriptor *config)
{
  int i;

  edit_printf("  wTotalLength:         %d\r\n", config->wTotalLength);
  edit_printf("  bNumInterfaces:       %d\r\n", config->bNumInterfaces);
  edit_printf("  bConfigurationValue:  %d\r\n", config->bConfigurationValue);
  edit_printf("  iConfiguration:       %d\r\n", config->iConfiguration);
  edit_printf("  bmAttributes:         %02xh\r\n", config->bmAttributes);
  edit_printf("  MaxPower:             %d\r\n", config->MaxPower);

  for (i = 0; i < config->bNumInterfaces; i++)
    print_interface(&config->interface[i]);
}
