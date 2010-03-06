/* LIBUSB-WIN32, Generic Windows USB Library
 * Copyright (c) 2002-2005 Stephan Meyer <ste_meyer@web.de>
 * Copyright (c) 2000-2005 Johannes Erdfelt <johannes@erdfelt.com>
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


#include <stdlib.h>
#include <string.h>
#include <stdio.h> 
#include <errno.h>
#include <ctype.h>
#include <windows.h>
#include <winioctl.h>
#include <setupapi.h>

#include "usb.h"
#include "error.h"
#include "usbi.h"
#include "driver_api.h"
#include "registry.h"



#define LIBUSB_DEFAULT_TIMEOUT 5000
#define LIBUSB_DEVICE_NAME "\\\\.\\libusb0-"
#define LIBUSB_BUS_NAME "bus-0"
#define LIBUSB_MAX_DEVICES 256

extern int __usb_debug;

typedef struct {
  usb_dev_handle *dev;
  libusb_request req;
  char *bytes;
  int size;
  DWORD control_code;
  OVERLAPPED ol;
} usb_context_t;


static struct usb_version _usb_version = {
  { VERSION_MAJOR, 
    VERSION_MINOR, 
    VERSION_MICRO, 
    VERSION_NANO },
  { -1, -1, -1, -1 }
};


static int _usb_setup_async(usb_dev_handle *dev, void **context, 
                            DWORD control_code,
                            unsigned char ep, int pktsize);
static int _usb_transfer_sync(usb_dev_handle *dev, int control_code,
                              int ep, int pktsize, char *bytes, int size, 
                              int timeout);

/* static int usb_get_configuration(usb_dev_handle *dev); */
static int _usb_cancel_io(usb_context_t *context);
static int _usb_abort_ep(usb_dev_handle *dev, unsigned int ep);

static int _usb_io_sync(HANDLE dev, unsigned int code, void *in, int in_size,
                        void *out, int out_size, int *ret);
static int _usb_reap_async(void *context, int timeout, int cancel);
static int _usb_add_virtual_hub(struct usb_bus *bus);

static void _usb_free_bus_list(struct usb_bus *bus);
static void _usb_free_dev_list(struct usb_device *dev);
static void _usb_deinit(void);

/* DLL main entry point */
BOOL WINAPI DllMain(HANDLE module, DWORD reason, LPVOID reserved)
{
  switch(reason)
    {
    case DLL_PROCESS_ATTACH:
      break;
    case DLL_PROCESS_DETACH:
      _usb_deinit();
      break;
    case DLL_THREAD_ATTACH:
      break;
    case DLL_THREAD_DETACH:
      break;
    default:
      break;
    }
  return TRUE;
}

/* static int usb_get_configuration(usb_dev_handle *dev) */
/* { */
/*   int ret; */
/*   char config; */

/*   ret = usb_control_msg(dev, USB_RECIP_DEVICE | USB_ENDPOINT_IN,  */
/*                         USB_REQ_GET_CONFIGURATION, 0, 0, &config, 1,  */
/*                         LIBUSB_DEFAULT_TIMEOUT); */
  
/*   if(ret >= 0) */
/*     { */
/*       return config; */
/*     } */

/*   return ret; */
/* } */

int usb_os_open(usb_dev_handle *dev)
{
  char dev_name[LIBUSB_PATH_MAX];
  char *p;
  /*   int config; */

  if(!dev)
    {
      usb_error("usb_os_open: invalid device handle %p", dev);
      return -EINVAL;
    }

  dev->impl_info = INVALID_HANDLE_VALUE;
  dev->config = 0;
  dev->interface = -1;
  dev->altsetting = -1;

  if(!dev->device->filename)
    {
      usb_error("usb_os_open: invalid file name");
      return -ENOENT;
    }

  /* build the Windows file name from the unique device name */ 
  strcpy(dev_name, dev->device->filename);

  p = strstr(dev_name, "--");

  if(!p)
    {
      usb_error("usb_os_open: invalid file name %s", dev->device->filename);
      return -ENOENT;
    }
  
  *p = 0;

  dev->impl_info = CreateFile(dev_name, 0, 0, NULL, OPEN_EXISTING, 
                              FILE_FLAG_OVERLAPPED, NULL);
      
  if(dev->impl_info == INVALID_HANDLE_VALUE) 
    {
      usb_error("usb_os_open: failed to open %s: win error: %s",
                dev->device->filename, usb_win_error_to_string());
      return -ENOENT;
    }
  
  /* now, retrieve the device's current configuration, except from hubs */
  /*   if(dev->device->config && dev->device->config->interface */
  /*      && dev->device->config->interface[0].altsetting */
  /*      && dev->device->config->interface[0].altsetting[0].bInterfaceClass  */
  /*      != USB_CLASS_HUB) */
  /*     { */
  /*       config = usb_get_configuration(dev); */
      
  /*       if(config > 0) */
  /*         { */
  /*           dev->config = config; */
  /*         } */
  /*     } */

  return 0;
}

int usb_os_close(usb_dev_handle *dev)
{
  if(dev->impl_info != INVALID_HANDLE_VALUE)
    {
      if(dev->interface >= 0)
        {
          usb_release_interface(dev, dev->interface);
        }

      CloseHandle(dev->impl_info);
      dev->impl_info = INVALID_HANDLE_VALUE;
      dev->interface = -1;
      dev->altsetting = -1;
    }

  return 0;
}

int usb_set_configuration(usb_dev_handle *dev, int configuration)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_set_configuration: error: device not open");
      return -EINVAL;
    }

  if(dev->config == configuration)
    {
      return 0;
    }

  if(dev->interface >= 0)
    {
      usb_error("usb_set_configuration: can't change configuration, "
                "an interface is still in use (claimed)");
      return -EINVAL;
    }

  req.configuration.configuration = configuration;
  req.timeout = LIBUSB_DEFAULT_TIMEOUT;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_SET_CONFIGURATION, 
                   &req, sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_set_configuration: could not set config %d: "
                "win error: %s", configuration, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  
  dev->config = configuration;
  dev->interface = -1;
  dev->altsetting = -1;
  
  return 0;
}

int usb_claim_interface(usb_dev_handle *dev, int interface)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_claim_interface: device not open");
      return -EINVAL;
    }

  if(!dev->config)
    {
      usb_error("usb_claim_interface: could not claim interface %d, invalid "
                "configuration %d", interface, dev->config);
      return -EINVAL;
    }
  
  if(dev->interface == interface)
    {
      return 0;
    }

  req.interface.interface = interface;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_CLAIM_INTERFACE, 
                   &req, sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_claim_interface: could not claim interface %d, "
                "win error: %s", interface, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  else
    {
      dev->interface = interface;
      dev->altsetting = 0;
      return 0;
    }
}

int usb_release_interface(usb_dev_handle *dev, int interface)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_release_interface: device not open");
      return -EINVAL;
    }

  if(!dev->config)
    {
      usb_error("usb_release_interface: could not release interface %d, "
                "invalid configuration %d", interface, dev->config);
      return -EINVAL;
    }

  req.interface.interface = interface;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_RELEASE_INTERFACE, 
                   &req, sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_release_interface: could not release interface %d, "
                "win error: %s", interface, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  else
    {
      dev->interface = -1;
      dev->altsetting = -1;
      
      return 0;
    }
}

int usb_set_altinterface(usb_dev_handle *dev, int alternate)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_set_altinterface: device not open");
      return -EINVAL;
    }

  if(dev->config <= 0)
    {
      usb_error("usb_set_altinterface: could not set alt interface %d: "
                "invalid configuration %d", alternate, dev->config);
      return -EINVAL;
    }

  if(dev->interface < 0)
    {
      usb_error("usb_set_altinterface: could not set alt interface %d: "
                "no interface claimed", alternate);
      return -EINVAL;
    }

  req.interface.interface = dev->interface;
  req.interface.altsetting = alternate;
  req.timeout = LIBUSB_DEFAULT_TIMEOUT;
  
  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_SET_INTERFACE, 
                   &req, sizeof(libusb_request), 
                   NULL, 0, NULL))
    {
      usb_error("usb_set_altinterface: could not set alt interface "
                "%d/%d: win error: %s",
                dev->interface, alternate, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  
  dev->altsetting = alternate;

  return 0;
}

static int _usb_setup_async(usb_dev_handle *dev, void **context, 
                            DWORD control_code,
                            unsigned char ep, int pktsize)
{
  usb_context_t **c = (usb_context_t **)context;
  
  if(((control_code == LIBUSB_IOCTL_INTERRUPT_OR_BULK_WRITE)
      || (control_code == LIBUSB_IOCTL_ISOCHRONOUS_WRITE)) 
     && (ep & USB_ENDPOINT_IN))
    {
      usb_error("_usb_setup_async: invalid endpoint 0x%02x", ep);
      return -EINVAL;
    }

  if(((control_code == LIBUSB_IOCTL_INTERRUPT_OR_BULK_READ)
      || (control_code == LIBUSB_IOCTL_ISOCHRONOUS_READ))
     && !(ep & USB_ENDPOINT_IN))
    {
      usb_error("_usb_setup_async: invalid endpoint 0x%02x", ep);
      return -EINVAL;
    }

  *c = malloc(sizeof(usb_context_t));
  
  if(!*c)
    {
      usb_error("_usb_setup_async: memory allocation error");
      return -ENOMEM;
    }

  memset(*c, 0, sizeof(usb_context_t));

  (*c)->dev = dev;
  (*c)->req.endpoint.endpoint = ep;
  (*c)->req.endpoint.packet_size = pktsize;
  (*c)->control_code = control_code;

  (*c)->ol.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

  if(!(*c)->ol.hEvent)
    {
      free(*c);
      *c = NULL;
      usb_error("_usb_setup_async: creating event failed: win error: %s", 
                usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }

  return 0;
}

int usb_submit_async(void *context, char *bytes, int size)
{
  usb_context_t *c = (usb_context_t *)context;

  if(!c)
    {
      usb_error("usb_submit_async: invalid context");
      return -EINVAL;
    }
    
  if(c->dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_submit_async: device not open");
      return -EINVAL;
    }

  if(c->dev->config <= 0)
    {
      usb_error("usb_submit_async: invalid configuration %d", c->dev->config);
      return -EINVAL;
    }

  if(c->dev->interface < 0)
    {
      usb_error("usb_submit_async: invalid interface %d", c->dev->interface);
      return -EINVAL;
    }
  
  
  c->ol.Offset = 0;
  c->ol.OffsetHigh = 0;
  c->bytes = bytes;
  c->size = size;

  ResetEvent(c->ol.hEvent);
  
  if(!DeviceIoControl(c->dev->impl_info, 
                      c->control_code, 
                      &c->req, sizeof(libusb_request), 
                      c->bytes, 
                      c->size, NULL, &c->ol))
    {
      if(GetLastError() != ERROR_IO_PENDING)
        {
          usb_error("usb_submit_async: submitting request failed, "
                    "win error: %s", usb_win_error_to_string());
          return -usb_win_error_to_errno();
        }
    }

  return 0;
}

static int _usb_reap_async(void *context, int timeout, int cancel)
{
  usb_context_t *c = (usb_context_t *)context;
  ULONG ret = 0;
    
  if(!c)
    {
      usb_error("usb_reap: invalid context");
      return -EINVAL;
    }

  if(WaitForSingleObject(c->ol.hEvent, timeout) == WAIT_TIMEOUT)
    {
      /* request timed out */
      if(cancel)
        {
          _usb_cancel_io(c);
        }

      usb_error("usb_reap: timeout error");
      return -ETIMEDOUT;
    }
  
  if(!GetOverlappedResult(c->dev->impl_info, &c->ol, &ret, TRUE))
    {
      usb_error("usb_reap: reaping request failed, win error: %s", 
                usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }

  return ret;
}

int usb_reap_async(void *context, int timeout)
{
  return _usb_reap_async(context, timeout, TRUE);
}

int usb_reap_async_nocancel(void *context, int timeout)
{
  return _usb_reap_async(context, timeout, FALSE);
}


int usb_cancel_async(void *context)
{
  /* NOTE that this function will cancel all pending URBs */
  /* on the same endpoint as this particular context, or even */
  /* all pending URBs for this particular device. */
  
  usb_context_t *c = (usb_context_t *)context;
  
  if(!c)
    {
      usb_error("usb_cancel_async: invalid context");
      return -EINVAL;
    }

  if(c->dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_cancel_async: device not open");
      return -EINVAL;
    }
  
  _usb_cancel_io(c);

  return 0;
}

int usb_free_async(void **context)
{
  usb_context_t **c = (usb_context_t **)context;

  if(!*c)
    {
      usb_error("usb_free_async: invalid context");
      return -EINVAL;
    }

  CloseHandle((*c)->ol.hEvent);

  free(*c);
  *c = NULL;

  return 0;
}

static int _usb_transfer_sync(usb_dev_handle *dev, int control_code,
                              int ep, int pktsize, char *bytes, int size,
                              int timeout)
{
  void *context = NULL;
  int transmitted = 0;
  int ret;
  int requested;

  ret = _usb_setup_async(dev, &context, control_code, (unsigned char )ep, 
                         pktsize);

  if(ret < 0)
    {
      return ret;
    }

  do {
    requested = size > LIBUSB_MAX_READ_WRITE ? LIBUSB_MAX_READ_WRITE : size;

    ret = usb_submit_async(context, bytes, requested);
    
    if(ret < 0)
      {
        transmitted = ret;
        break;
      }

    ret = usb_reap_async(context, timeout);

    if(ret < 0)
      {
        transmitted = ret;
        break;
      }

    transmitted += ret;
    bytes += ret;
    size -= ret;
  } while(size > 0 && ret == requested);
  
  usb_free_async(&context);

  return transmitted;
}

int usb_bulk_write(usb_dev_handle *dev, int ep, char *bytes, int size,
                   int timeout)
{
  return _usb_transfer_sync(dev, LIBUSB_IOCTL_INTERRUPT_OR_BULK_WRITE,
                            ep, 0, bytes, size, timeout);
}

int usb_bulk_read(usb_dev_handle *dev, int ep, char *bytes, int size,
                  int timeout)
{
  return _usb_transfer_sync(dev, LIBUSB_IOCTL_INTERRUPT_OR_BULK_READ,
                            ep, 0, bytes, size, timeout);
}

int usb_interrupt_write(usb_dev_handle *dev, int ep, char *bytes, int size,
                        int timeout)
{
  return _usb_transfer_sync(dev, LIBUSB_IOCTL_INTERRUPT_OR_BULK_WRITE,
                            ep, 0, bytes, size, timeout);
}

int usb_interrupt_read(usb_dev_handle *dev, int ep, char *bytes, int size,
                       int timeout)
{
  return _usb_transfer_sync(dev, LIBUSB_IOCTL_INTERRUPT_OR_BULK_READ,
                            ep, 0, bytes, size, timeout);
}

int usb_isochronous_setup_async(usb_dev_handle *dev, void **context, 
                                unsigned char ep, int pktsize)
{
  if(ep & 0x80)
    return _usb_setup_async(dev, context, LIBUSB_IOCTL_ISOCHRONOUS_READ,
                            ep, pktsize);
  else
    return _usb_setup_async(dev, context, LIBUSB_IOCTL_ISOCHRONOUS_WRITE,
                            ep, pktsize);    
}

int usb_bulk_setup_async(usb_dev_handle *dev, void **context, unsigned char ep)
{
  if(ep & 0x80)
    return _usb_setup_async(dev, context, LIBUSB_IOCTL_INTERRUPT_OR_BULK_READ,
                            ep, 0);
  else
    return _usb_setup_async(dev, context, LIBUSB_IOCTL_INTERRUPT_OR_BULK_WRITE,
                            ep, 0);    
}

int usb_interrupt_setup_async(usb_dev_handle *dev, void **context, 
                              unsigned char ep)
{
  if(ep & 0x80)
    return _usb_setup_async(dev, context, LIBUSB_IOCTL_INTERRUPT_OR_BULK_READ,
                            ep, 0);
  else
    return _usb_setup_async(dev, context, LIBUSB_IOCTL_INTERRUPT_OR_BULK_WRITE,
                            ep, 0);    
}

int usb_control_msg(usb_dev_handle *dev, int requesttype, int request,
                    int value, int index, char *bytes, int size, int timeout)
{
  int read = 0;
  libusb_request req;
  void *out = &req;
  int out_size = sizeof(libusb_request);
  void *in = bytes;
  int in_size = size;
  int code;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_control_msg: device not open");
      return -EINVAL;
    }

  req.timeout = timeout;

  /* windows doesn't support generic control messages, so it needs to be */
  /* split up */ 
  switch(requesttype & (0x03 << 5))
    {
    case USB_TYPE_STANDARD:      
      switch(request)
        {
        case USB_REQ_GET_STATUS: 
          req.status.recipient = requesttype & 0x1F;
          req.status.index = index;
          code = LIBUSB_IOCTL_GET_STATUS;
          break;
      
        case USB_REQ_CLEAR_FEATURE:
          req.feature.recipient = requesttype & 0x1F;
          req.feature.feature = value;
          req.feature.index = index;
          code = LIBUSB_IOCTL_CLEAR_FEATURE;
          break;
	  
        case USB_REQ_SET_FEATURE:
          req.feature.recipient = requesttype & 0x1F;
          req.feature.feature = value;
          req.feature.index = index;
          code = LIBUSB_IOCTL_SET_FEATURE;
          break;

        case USB_REQ_GET_DESCRIPTOR:
          req.descriptor.recipient = requesttype & 0x1F;
          req.descriptor.type = (value >> 8) & 0xFF;
          req.descriptor.index = value & 0xFF;
          req.descriptor.language_id = index;
          code = LIBUSB_IOCTL_GET_DESCRIPTOR;
          break;
	  
        case USB_REQ_SET_DESCRIPTOR:
          req.descriptor.recipient = requesttype & 0x1F;
          req.descriptor.type = (value >> 8) & 0xFF;
          req.descriptor.index = value & 0xFF;
          req.descriptor.language_id = index;
          code = LIBUSB_IOCTL_SET_DESCRIPTOR;
          break;
	  
        case USB_REQ_GET_CONFIGURATION:
          code = LIBUSB_IOCTL_GET_CONFIGURATION;
          break;
      
        case USB_REQ_SET_CONFIGURATION:	  
          req.configuration.configuration = value;
          code = LIBUSB_IOCTL_SET_CONFIGURATION;
          break;
	  
        case USB_REQ_GET_INTERFACE:
          req.interface.interface = index;
          code = LIBUSB_IOCTL_GET_INTERFACE;	  
          break;
      
        case USB_REQ_SET_INTERFACE:
          req.interface.interface = index;
          req.interface.altsetting = value;
          code = LIBUSB_IOCTL_SET_INTERFACE;	  
          break;
	  
        default:
          usb_error("usb_control_msg: invalid request 0x%x", request);
          return -EINVAL;
        }
      break;

    case USB_TYPE_VENDOR:  
    case USB_TYPE_CLASS:

      req.vendor.type = (requesttype >> 5) & 0x03;
      req.vendor.recipient = requesttype & 0x1F;
      req.vendor.request = request;
      req.vendor.value = value;
      req.vendor.index = index;

      if(requesttype & 0x80)
        code = LIBUSB_IOCTL_VENDOR_READ;
      else
        code = LIBUSB_IOCTL_VENDOR_WRITE;
      break;

    case USB_TYPE_RESERVED:
    default:
      usb_error("usb_control_msg: invalid or unsupported request type: %x", 
                requesttype);
      return -EINVAL;
    }
  
  /* out request? */
  if(!(requesttype & USB_ENDPOINT_IN))
    {
      if(!(out = malloc(sizeof(libusb_request) + size)))
        {
          usb_error("usb_control_msg: memory allocation failed"); 
          return -ENOMEM;
        }
      
      memcpy(out, &req, sizeof(libusb_request));
      memcpy((char *)out + sizeof(libusb_request), bytes, size);
      out_size = sizeof(libusb_request) + size;
      in = NULL; in_size = 0;
    }

  if(!_usb_io_sync(dev->impl_info, code, out, out_size, in, in_size, &read))
    {
      usb_error("usb_control_msg: sending control message failed, "
                "win error: %s", usb_win_error_to_string());
      if(!(requesttype & USB_ENDPOINT_IN))
        {
          free(out);
        }
      return -usb_win_error_to_errno();
    }

  /* out request? */
  if(!(requesttype & USB_ENDPOINT_IN))
    {
      free(out);
      return size;
    }
  else
    return read;
}


int usb_os_find_busses(struct usb_bus **busses)
{
  struct usb_bus *bus = NULL;

  /* create one 'virtual' bus */
  
  bus = malloc(sizeof(struct usb_bus));

  if(!bus)
    {
      usb_error("usb_os_find_busses: memory allocation failed");
      return -ENOMEM;
    }
  
  memset(bus, 0, sizeof(*bus));
  strcpy(bus->dirname, LIBUSB_BUS_NAME);
  
  usb_message("usb_os_find_busses: found %s", bus->dirname);
  
  *busses = bus;

  return 0;
}

int usb_os_find_devices(struct usb_bus *bus, struct usb_device **devices)
{
  int i;
  struct usb_device *dev, *fdev = NULL;
  char dev_name[LIBUSB_PATH_MAX];
  int ret;
  HANDLE handle;
  libusb_request req;

  for(i = 1; i < LIBUSB_MAX_DEVICES; i++)
    {
      ret = 0;

      _snprintf(dev_name, sizeof(dev_name) - 1,"%s%04d", 
                LIBUSB_DEVICE_NAME, i);

      if(!(dev = malloc(sizeof(*dev)))) 
        {
          usb_error("usb_os_find_devices: memory allocation failed");
          return -ENOMEM;
        }
      
      memset(dev, 0, sizeof(*dev));
      dev->bus = bus;
      dev->devnum = (unsigned char)i;

      handle = CreateFile(dev_name, 0, 0, NULL, OPEN_EXISTING, 
                          FILE_FLAG_OVERLAPPED, NULL);

      if(handle == INVALID_HANDLE_VALUE) 
        {
          free(dev);
          continue;
        }

      /* retrieve device descriptor */
      req.descriptor.type = USB_DT_DEVICE;
      req.descriptor.recipient = USB_RECIP_DEVICE;
      req.descriptor.index = 0;
      req.descriptor.language_id = 0;
      req.timeout = LIBUSB_DEFAULT_TIMEOUT;
      
      _usb_io_sync(handle, LIBUSB_IOCTL_GET_DESCRIPTOR, 
                   &req, sizeof(libusb_request), 
                   &dev->descriptor, USB_DT_DEVICE_SIZE, &ret);
      
      if(ret < USB_DT_DEVICE_SIZE) 
        {
          usb_error("usb_os_find_devices: couldn't read device descriptor");
          free(dev);
          CloseHandle(handle);
          continue;
        }
      
      _snprintf(dev->filename, LIBUSB_PATH_MAX - 1, "%s--0x%04x-0x%04x", 
                dev_name, dev->descriptor.idVendor, dev->descriptor.idProduct);

      CloseHandle(handle);

      LIST_ADD(fdev, dev);

      usb_message("usb_os_find_devices: found %s on %s",
                  dev->filename, bus->dirname);
    }
  
  *devices = fdev;

  return 0;
}


void usb_os_init(void)
{
  HANDLE dev;
  libusb_request req;
  int i;
  int ret;
  char dev_name[LIBUSB_PATH_MAX];

  usb_message("usb_os_init: dll version: %d.%d.%d.%d",
              VERSION_MAJOR, VERSION_MINOR,
              VERSION_MICRO, VERSION_NANO);


  for(i = 1; i < LIBUSB_MAX_DEVICES; i++)
    {
      /* build the Windows file name */
      _snprintf(dev_name, sizeof(dev_name) - 1,"%s%04d", 
                LIBUSB_DEVICE_NAME, i);

      dev = CreateFile(dev_name, 0, 0, NULL, OPEN_EXISTING, 
                       FILE_FLAG_OVERLAPPED, NULL);
  
      if(dev == INVALID_HANDLE_VALUE) 
        {
          continue;
        }
      
      if(!_usb_io_sync(dev, LIBUSB_IOCTL_GET_VERSION,
                       &req, sizeof(libusb_request), 
                       &req, sizeof(libusb_request), &ret) 
         || (ret < sizeof(libusb_request)))
        {
          usb_error("usb_os_init: getting driver version failed");
          CloseHandle(dev);
          continue;
        }
      else 
        {
          _usb_version.driver.major = req.version.major;
          _usb_version.driver.minor = req.version.minor;
          _usb_version.driver.micro = req.version.micro;
          _usb_version.driver.nano = req.version.nano;
	  
          usb_message("usb_os_init: driver version: %d.%d.%d.%d",
                      req.version.major, req.version.minor, 
                      req.version.micro, req.version.nano);
      
          /* set debug level */
          req.timeout = 0;
          req.debug.level = __usb_debug;
          
          if(!_usb_io_sync(dev, LIBUSB_IOCTL_SET_DEBUG_LEVEL, 
                           &req, sizeof(libusb_request), 
                           NULL, 0, NULL))
            {
              usb_error("usb_os_init: setting debug level failed");
            }
          
          CloseHandle(dev);
          break;
        }
    }
}


int usb_resetep(usb_dev_handle *dev, unsigned int ep)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_resetep: device not open");
      return -EINVAL;
    }

  req.endpoint.endpoint = (int)ep;
  req.timeout = LIBUSB_DEFAULT_TIMEOUT;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_ABORT_ENDPOINT, &req, 
                   sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_resetep: could not abort ep 0x%02x, win error: %s", 
                ep, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_RESET_ENDPOINT, &req, 
                   sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_resetep: could not reset ep 0x%02x, win error: %s", 
                ep, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  
  return 0;
}

int usb_clear_halt(usb_dev_handle *dev, unsigned int ep)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_clear_halt: device not open");
      return -EINVAL;
    }

  req.endpoint.endpoint = (int)ep;
  req.timeout = LIBUSB_DEFAULT_TIMEOUT;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_RESET_ENDPOINT, &req, 
                   sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_clear_halt: could not clear halt, ep 0x%02x, "
                "win error: %s", ep, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  
  return 0;
}

int usb_reset(usb_dev_handle *dev)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("usb_reset: device not open");
      return -EINVAL;
    }

  req.timeout = LIBUSB_DEFAULT_TIMEOUT;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_RESET_DEVICE,
                   &req, sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("usb_reset: could not reset device, win error: %s", 
                usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }

  return 0;
}

const struct usb_version *usb_get_version(void)
{
  return &_usb_version;
}

void usb_set_debug(int level)
{
  HANDLE dev;
  libusb_request req;
  int i;
  char dev_name[LIBUSB_PATH_MAX];

  if(__usb_debug || level)
    {
      usb_message("usb_set_debug: setting debugging level to %d (%s)\n",
                  level, level ? "on" : "off");
    }

  __usb_debug = level;

  /* find a valid device */
  for(i = 1; i < LIBUSB_MAX_DEVICES; i++)
    {
      /* build the Windows file name */
      _snprintf(dev_name, sizeof(dev_name) - 1,"%s%04d", 
                LIBUSB_DEVICE_NAME, i);

      dev = CreateFile(dev_name, 0, 0, NULL, OPEN_EXISTING, 
                       FILE_FLAG_OVERLAPPED, NULL);
  
      if(dev == INVALID_HANDLE_VALUE) 
        {
          continue;
        }
      
      /* set debug level */
      req.timeout = 0;
      req.debug.level = __usb_debug;
      
      if(!_usb_io_sync(dev, LIBUSB_IOCTL_SET_DEBUG_LEVEL, 
                       &req, sizeof(libusb_request), 
                       NULL, 0, NULL))
        {
          usb_error("usb_os_init: setting debug level failed");
        }
      
      CloseHandle(dev);

      break;
    }
}

int usb_os_determine_children(struct usb_bus *bus)
{
  struct usb_device *dev;
  int i = 0;

  /* add a virtual hub to the bus to emulate this feature */
  if(_usb_add_virtual_hub(bus))
    {
      if(bus->root_dev->children)
        {
          free(bus->root_dev->children);
        }

      bus->root_dev->num_children = 0;
      for(dev = bus->devices; dev; dev = dev->next)
        bus->root_dev->num_children++;

      bus->root_dev->children 
        = malloc(sizeof(struct usb_device *) * bus->root_dev->num_children);

      for(dev = bus->devices; dev; dev = dev->next)
        bus->root_dev->children[i++] = dev; 
    }

  return 0;
}

static int _usb_cancel_io(usb_context_t *context)
{
  int ret;
  ret = _usb_abort_ep(context->dev, context->req.endpoint.endpoint);
  WaitForSingleObject(context->ol.hEvent, 0);
  return ret; 
}

static int _usb_abort_ep(usb_dev_handle *dev, unsigned int ep)
{
  libusb_request req;

  if(dev->impl_info == INVALID_HANDLE_VALUE)
    {
      usb_error("_usb_abort_ep: device not open");
      return -EINVAL;
    }

  req.endpoint.endpoint = (int)ep;
  req.timeout = LIBUSB_DEFAULT_TIMEOUT;

  if(!_usb_io_sync(dev->impl_info, LIBUSB_IOCTL_ABORT_ENDPOINT, &req, 
                   sizeof(libusb_request), NULL, 0, NULL))
    {
      usb_error("_usb_abort_ep: could not abort ep 0x%02x, win error: %s", 
                ep, usb_win_error_to_string());
      return -usb_win_error_to_errno();
    }
  
  return 0;
}

static int _usb_io_sync(HANDLE dev, unsigned int code, void *out, int out_size,
                        void *in, int in_size, int *ret)
{
  OVERLAPPED ol;
  DWORD _ret;

  memset(&ol, 0, sizeof(ol));  

  if(ret)
    *ret = 0;

  ol.hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

  if(!ol.hEvent)
    return FALSE;
  
  if(!DeviceIoControl(dev, code, out, out_size, in, in_size, NULL, &ol))
    {
      if(GetLastError() != ERROR_IO_PENDING)
        {
          CloseHandle(ol.hEvent);
          return FALSE;
        }
    }

  if(GetOverlappedResult(dev, &ol, &_ret, TRUE))
    {
      if(ret)
        *ret = (int)_ret;
      CloseHandle(ol.hEvent);
      return TRUE;
    }
  
  CloseHandle(ol.hEvent);
  return FALSE;
}

static int _usb_add_virtual_hub(struct usb_bus *bus)
{
  struct usb_device *dev;

  if(!bus->root_dev)
    {
      if(!(dev = malloc(sizeof(*dev))))
        return FALSE;
      
      memset(dev, 0, sizeof(*dev));
      strcpy(dev->filename, "virtual-hub");
      dev->bus = bus;
      
      dev->descriptor.bLength = USB_DT_DEVICE_SIZE;
      dev->descriptor.bDescriptorType = USB_DT_DEVICE;
      dev->descriptor.bcdUSB = 0x0200;
      dev->descriptor.bDeviceClass = USB_CLASS_HUB;
      dev->descriptor.bDeviceSubClass = 0;
      dev->descriptor.bDeviceProtocol = 0;
      dev->descriptor.bMaxPacketSize0 = 64;
      dev->descriptor.idVendor = 0;
      dev->descriptor.idProduct = 0;
      dev->descriptor.bcdDevice = 0x100;
      dev->descriptor.iManufacturer = 0;
      dev->descriptor.iProduct = 0;
      dev->descriptor.iSerialNumber = 0;
      dev->descriptor.bNumConfigurations = 0;
      
      bus->root_dev = dev;
    }

  return TRUE;
}

static void _usb_free_bus_list(struct usb_bus *bus)
{
  if(bus)
    {
      _usb_free_bus_list(bus->next);
      if(bus->root_dev)
        usb_free_dev(bus->root_dev);
      _usb_free_dev_list(bus->devices);
      usb_free_bus(bus);
    }
}

static void _usb_free_dev_list(struct usb_device *dev)
{
  if(dev)
    {
      _usb_free_dev_list(dev->next);
      usb_free_dev(dev);
    }
}

static void _usb_deinit(void)
{
  _usb_free_bus_list(usb_get_busses());
}
