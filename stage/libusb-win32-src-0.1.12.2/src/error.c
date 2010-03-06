/*
 * USB Error messages
 *
 * Copyright (c) 2000-2001 Johannes Erdfelt <johannes@erdfelt.com>
 *
 * This library is covered by the LGPL, read LICENSE for details.
 */

#include <errno.h>
#include <string.h>
#include <stdio.h>

#include "usb.h"
#include "error.h"
#include "driver_api.h"

char usb_error_str[1024] = "";
int usb_error_errno = 0;
int __usb_debug = LIBUSB_DEBUG_OFF;
usb_error_type_t usb_error_type = USB_ERROR_TYPE_NONE;

static void output_debug_string(const char *s, ...);

/* prints a message to the Windows debug system */
static void output_debug_string(const char *s, ...)
{
  char tmp[512];
  va_list args;
  va_start(args, s);
  _vsnprintf(tmp, sizeof(tmp) - 1, s, args);
  va_end(args);
  OutputDebugStringA(tmp);
}

char *usb_strerror(void)
{
  switch (usb_error_type) {
  case USB_ERROR_TYPE_NONE:
    return "No error";
  case USB_ERROR_TYPE_STRING:
    return usb_error_str;
  case USB_ERROR_TYPE_ERRNO:
    if (usb_error_errno > -USB_ERROR_BEGIN)
      return strerror(usb_error_errno);
    else
      /* Any error we don't know falls under here */
      return "Unknown error";
  }

  return "Unknown error";
}

void usb_error(char *format, ...)
{
  va_list args;
  
  usb_error_type = USB_ERROR_TYPE_STRING;

  va_start(args, format);
  _vsnprintf(usb_error_str, sizeof(usb_error_str) - 1, format, args);
  va_end(args);

  if(__usb_debug >= LIBUSB_DEBUG_ERR)
    {
      fprintf(stderr, "LIBUSB_DLL: error: %s\n", usb_error_str);
      fflush(stderr);
      output_debug_string("LIBUSB_DLL: error: %s\n", usb_error_str);
    }
}

void usb_message(char *format, ...)
{
  char tmp[512];
  va_list args;

  if(__usb_debug >= LIBUSB_DEBUG_MSG)
   {  
     va_start(args, format);
     _vsnprintf(tmp, sizeof(tmp) - 1, format, args);
     va_end(args);

     fprintf(stderr, "LIBUSB_DLL: info: %s\n", tmp);
     fflush(stderr);
     output_debug_string("LIBUSB_DLL: info: %s\n", tmp);
   }
}

/* returns Windows' last error in a human readable form */
const char *usb_win_error_to_string(void)
{
  static char tmp[512];

  FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), 
                LANG_USER_DEFAULT, tmp, sizeof(tmp) - 1, NULL);

  return tmp;
}


int usb_win_error_to_errno(void)
{
  switch(GetLastError())
    {
    case ERROR_SUCCESS:
      return 0;
    case ERROR_INVALID_PARAMETER:
      return EINVAL;
    case ERROR_SEM_TIMEOUT: 
    case ERROR_OPERATION_ABORTED:
      return ETIMEDOUT;
    case ERROR_NOT_ENOUGH_MEMORY:
      return ENOMEM;
    default:
      return EIO;
    }
}
