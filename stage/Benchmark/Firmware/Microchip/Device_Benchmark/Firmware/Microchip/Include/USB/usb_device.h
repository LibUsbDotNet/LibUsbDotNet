/*******************************************************************************

    USB Device header file

Summary:
    This file, with its associated C source file, provides the main substance of
    the USB device side stack.  These files will receive, transmit, and process
    various USB commands as well as take action when required for various events
    that occur on the bus.

Description:
    This file, with its associated C source file, provides the main substance of
    the USB device side stack.  These files will receive, transmit, and process
    various USB commands as well as take action when required for various events
    that occur on the bus.

    This file is located in the "\<Install Directory\>\\Microchip\\Include\\USB"
    directory.
    
    When including this file in a new project, this file can either be
    referenced from the directory in which it was installed or copied
    directly into the user application folder. If the first method is
    chosen to keep the file located in the folder in which it is installed
    then include paths need to be added so that the library and the
    application both know where to reference each others files. If the
    application folder is located in the same folder as the Microchip
    folder (like the current demo folders), then the following include
    paths need to be added to the application's project:
    
    ..\\Include
    
    ..\\..\\Include
    
    ..\\..\\MicrochipInclude
    
    ..\\..\\\<Application Folder\>
    
    ..\\..\\..\\\<Application Folder\>
    
    If a different directory structure is used, modify the paths as
    required. An example using absolute paths instead of relative paths
    would be the following:
    
    C:\\Microchip Solutions\\Microchip\\Include
    
    C:\\Microchip Solutions\\My Demo Application 

******************************************************************************/
//DOM-IGNORE-BEGIN
/******************************************************************************
 FileName:     	usb_device.h
 Dependencies:	See INCLUDES section
 Processor:		PIC18 or PIC24 USB Microcontrollers
 Hardware:		The code is natively intended to be used on the following
 				hardware platforms: PICDEM™ FS USB Demo Board, 
 				PIC18F87J50 FS USB Plug-In Module, or
 				Explorer 16 + PIC24 USB PIM.  The firmware may be
 				modified for use on other USB platforms by editing the
 				HardwareProfile.h file.
 Complier:  	Microchip C18 (for PIC18) or C30 (for PIC24)
 Company:		Microchip Technology, Inc.

 Software License Agreement:

 The software supplied herewith by Microchip Technology Incorporated
 (the “Company”) for its PIC® Microcontroller is intended and
 supplied to you, the Company’s customer, for use solely and
 exclusively on Microchip PIC Microcontroller products. The
 software is owned by the Company and/or its supplier, and is
 protected under applicable copyright laws. All rights are reserved.
 Any use in violation of the foregoing restrictions may subject the
 user to criminal sanctions under applicable laws, as well as to
 civil liability for the breach of the terms and conditions of this
 license.

 THIS SOFTWARE IS PROVIDED IN AN “AS IS” CONDITION. NO WARRANTIES,
 WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED
 TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. THE COMPANY SHALL NOT,
 IN ANY CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL OR
 CONSEQUENTIAL DAMAGES, FOR ANY REASON WHATSOEVER.

********************************************************************
 File Description:

 Change History:
  Rev   Date         Description
  1.0   11/19/2004   Initial release
  2.1   02/26/2007   Added "(" & ")" to EP definitions
                     Updated for simplicity and to use common
                     coding style
*******************************************************************/

#ifndef USBDEVICE_H
#define USBDEVICE_H
//DOM-IGNORE-END

#include "GenericTypeDefs.h"
#include "Compiler.h"
#include "usb_config.h"
#include "USB\usb_ch9.h"
//#include "USB\usb_hal.h"

/** DEFINITIONS ****************************************************/

/********************************************************************
USB Endpoint Definitions
USB Standard EP Address Format: DIR:X:X:X:EP3:EP2:EP1:EP0
This is used in the descriptors. See usbcfg.c

NOTE: Do not use these values for checking against USTAT.
To check against USTAT, use values defined in usbd.h.
********************************************************************/
#define _EP_IN      0x80
#define _EP_OUT     0x00
#define _EP01_OUT   0x01
#define _EP01_IN    0x81
#define _EP02_OUT   0x02
#define _EP02_IN    0x82
#define _EP03_OUT   0x03
#define _EP03_IN    0x83
#define _EP04_OUT   0x04
#define _EP04_IN    0x84
#define _EP05_OUT   0x05
#define _EP05_IN    0x85
#define _EP06_OUT   0x06
#define _EP06_IN    0x86
#define _EP07_OUT   0x07
#define _EP07_IN    0x87
#define _EP08_OUT   0x08
#define _EP08_IN    0x88
#define _EP09_OUT   0x09
#define _EP09_IN    0x89
#define _EP10_OUT   0x0A
#define _EP10_IN    0x8A
#define _EP11_OUT   0x0B
#define _EP11_IN    0x8B
#define _EP12_OUT   0x0C
#define _EP12_IN    0x8C
#define _EP13_OUT   0x0D
#define _EP13_IN    0x8D
#define _EP14_OUT   0x0E
#define _EP14_IN    0x8E
#define _EP15_OUT   0x0F
#define _EP15_IN    0x8F

/* Configuration Attributes */
#define _DEFAULT    (0x01<<7)       //Default Value (Bit 7 is set)
#define _SELF       (0x01<<6)       //Self-powered (Supports if set)
#define _RWU        (0x01<<5)       //Remote Wakeup (Supports if set)
#define _HNP	        (0x01 << 1)    //HNP (Supports if set)
#define _SRP	  	 (0x01)		 //SRP (Supports if set)

/* Endpoint Transfer Type */
#define _CTRL       0x00            //Control Transfer
#define _ISO        0x01            //Isochronous Transfer
#define _BULK       0x02            //Bulk Transfer

#define _INTERRUPT        0x03            //Interrupt Transfer
#if defined(__18CXX) || defined(__C30__)
    #define _INT        0x03            //Interrupt Transfer
#endif

/* Isochronous Endpoint Synchronization Type */
#define _NS         (0x00<<2)       //No Synchronization
#define _AS         (0x01<<2)       //Asynchronous
#define _AD         (0x02<<2)       //Adaptive
#define _SY         (0x03<<2)       //Synchronous

/* Isochronous Endpoint Usage Type */
#define _DE         (0x00<<4)       //Data endpoint
#define _FE         (0x01<<4)       //Feedback endpoint
#define _IE         (0x02<<4)       //Implicit feedback Data endpoint

#define _ROM        USB_INPIPES_ROM
#define _RAM        USB_INPIPES_RAM

//These are the directional indicators used for the USBTransferOnePacket()
//  function.
#define OUT_FROM_HOST 0
#define IN_TO_HOST 1

/********************************************************************
 * CTRL_TRF_SETUP: Every setup packet has 8 bytes.  This structure
 * allows direct access to the various members of the control
 * transfer.
 *******************************************************************/
typedef union __attribute__ ((packed)) _CTRL_TRF_SETUP
{
    /** Standard Device Requests ***********************************/
    struct __attribute__ ((packed))
    {
        BYTE bmRequestType; //from table 9-2 of USB2.0 spec
        BYTE bRequest; //from table 9-2 of USB2.0 spec
        WORD wValue; //from table 9-2 of USB2.0 spec
        WORD wIndex; //from table 9-2 of USB2.0 spec
        WORD wLength; //from table 9-2 of USB2.0 spec
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        WORD_VAL W_Value; //from table 9-2 of USB2.0 spec, allows byte/bitwise access
        WORD_VAL W_Index; //from table 9-2 of USB2.0 spec, allows byte/bitwise access
        WORD_VAL W_Length; //from table 9-2 of USB2.0 spec, allows byte/bitwise access
    };
    struct __attribute__ ((packed))
    {
        unsigned Recipient:5;   //Device,Interface,Endpoint,Other
        unsigned RequestType:2; //Standard,Class,Vendor,Reserved
        unsigned DataDir:1;     //Host-to-device,Device-to-host
        unsigned :8;
        BYTE bFeature;          //DEVICE_REMOTE_WAKEUP,ENDPOINT_HALT
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned :8;
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        BYTE bDscIndex;         //For Configuration and String DSC Only
        BYTE bDescriptorType;          //Device,Configuration,String
        WORD wLangID;           //Language ID
        unsigned :8;
        unsigned :8;
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        BYTE_VAL bDevADR;       //Device Address 0-127
        BYTE bDevADRH;          //Must equal zero
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned :8;
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        BYTE bConfigurationValue;         //Configuration Value 0-255
        BYTE bCfgRSD;           //Must equal zero (Reserved)
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned :8;
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        BYTE bAltID;            //Alternate Setting Value 0-255
        BYTE bAltID_H;          //Must equal zero
        BYTE bIntfID;           //Interface Number Value 0-255
        BYTE bIntfID_H;         //Must equal zero
        unsigned :8;
        unsigned :8;
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned :8;
        BYTE bEPID;             //Endpoint ID (Number & Direction)
        BYTE bEPID_H;           //Must equal zero
        unsigned :8;
        unsigned :8;
    };
    struct __attribute__ ((packed))
    {
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned :8;
        unsigned EPNum:4;       //Endpoint Number 0-15
        unsigned :3;
        unsigned EPDir:1;       //Endpoint Direction: 0-OUT, 1-IN
        unsigned :8;
        unsigned :8;
        unsigned :8;
    };

    /** End: Standard Device Requests ******************************/

} CTRL_TRF_SETUP;

//USB_HANDLE is a pointer to an entry in the BDT.  This pointer can be used
//  to read the length of the last transfer, the status of the last transfer,
//  and various other information.  Insure to initialize USB_HANDLE objects
//  to NULL so that they are in a known state during their first usage.
#define USB_HANDLE void*

// Defintion of the PIPE structure
//  This structure is used to keep track of data that is sent out
//  of the stack automatically.
typedef struct __attribute__ ((packed))
{
    union __attribute__ ((packed))
    {
        //Various options of pointers that are available to
        // get the data from
        BYTE *bRam;
        ROM BYTE *bRom;
        WORD *wRam;
        ROM WORD *wRom;
    }pSrc;
    union __attribute__ ((packed))
    {
        struct __attribute__ ((packed))
        {
            //is this transfer from RAM or ROM?
            BYTE ctrl_trf_mem          :1;
            BYTE reserved              :5;
            //include a zero length packet after
            //data is done if data_size%ep_size = 0?
            BYTE includeZero           :1;
            //is this PIPE currently in use
            BYTE busy                  :1;
        }bits;
        BYTE Val;
    }info;
    WORD_VAL wCount;
}IN_PIPE;

#define CTRL_TRF_RETURN void
#define CTRL_TRF_PARAMS void
typedef struct __attribute__ ((packed))
{
    union __attribute__ ((packed))
    {
        //Various options of pointers that are available to
        // get the data from
        BYTE *bRam;
        WORD *wRam;
    }pDst;
    union __attribute__ ((packed))
    {
        struct __attribute__ ((packed))
        {
            BYTE reserved              :7;
            //is this PIPE currently in use
            BYTE busy                  :1;
        }bits;
        BYTE Val;
    }info;
    WORD_VAL wCount;
    CTRL_TRF_RETURN (*pFunc)(CTRL_TRF_PARAMS);
}OUT_PIPE;

//Various options for setting the PIPES
#define USB_INPIPES_ROM            0x00     //Data comes from RAM
#define USB_INPIPES_RAM            0x01     //Data comes from ROM
#define USB_INPIPES_BUSY           0x80     //The PIPE is busy
#define USB_INPIPES_INCLUDE_ZERO   0x40     //include a trailing zero packet
#define USB_INPIPES_NO_DATA        0x00     //no data to send
#define USB_INPIPES_NO_OPTIONS     0x00     //no options set

#define USB_EP0_ROM            USB_INPIPES_ROM
#define USB_EP0_RAM            USB_INPIPES_RAM
#define USB_EP0_BUSY           USB_INPIPES_BUSY
#define USB_EP0_INCLUDE_ZERO   USB_INPIPES_INCLUDE_ZERO
#define USB_EP0_NO_DATA        USB_INPIPES_NO_DATA
#define USB_EP0_NO_OPTIONS     USB_INPIPES_NO_OPTIONS

/********************************************************************
 * Standard Request Codes
 * USB 2.0 Spec Ref Table 9-4
 *******************************************************************/
#define GET_STATUS  0
#define CLR_FEATURE 1
#define SET_FEATURE 3
#define SET_ADR     5
#define GET_DSC     6
#define SET_DSC     7
#define GET_CFG     8
#define SET_CFG     9
#define GET_INTF    10
#define SET_INTF    11
#define SYNCH_FRAME 12

/* Section: Standard Feature Selectors */
#define DEVICE_REMOTE_WAKEUP    0x01
#define ENDPOINT_HALT           0x00

/* Section: USB Device States - To be used with [BYTE usb_device_state] */

/* Detached is the state in which the device is not attached to the bus.  When
in the detached state a device should not have any pull-ups attached to either
the D+ or D- line.  This defintions is a return value of the 
function USBGetDeviceState() */
#define DETACHED_STATE          0x00
/* Attached is the state in which the device is attached ot the bus but the
hub/port that it is attached to is not yet configured. This defintions is a 
return value of the function USBGetDeviceState() */
#define ATTACHED_STATE          0x01
/* Powered is the state in which the device is attached to the bus and the 
hub/port that it is attached to is configured. This defintions is a return 
value of the function USBGetDeviceState() */
#define POWERED_STATE           0x02
/* Default state is the state after the device receives a RESET command from
the host. This defintions is a return value of the function USBGetDeviceState()
 */
#define DEFAULT_STATE           0x04
/* Address pending state is not an official state of the USB defined states.
This state is internally used to indicate that the device has received a 
SET_ADDRESS command but has not received the STATUS stage of the transfer yet.
The device is should not switch addresses until after the STATUS stage is
complete.  This defintions is a return value of the function 
USBGetDeviceState() */
#define ADR_PENDING_STATE       0x08
/* Address is the state in which the device has its own specific address on the
bus. This defintions is a return value of the function USBGetDeviceState().*/
#define ADDRESS_STATE           0x10
/* Configured is the state where the device has been fully enumerated and is
operating on the bus.  The device is now allowed to excute its application
specific tasks.  It is also allowed to increase its current consumption to the
value specified in the configuration descriptor of the current configuration.
This defintions is a return value of the function USBGetDeviceState(). */
#define CONFIGURED_STATE        0x20

/* UCFG Initialization Parameters */
#if defined(__18CXX)
    #if defined(UCFG_VAL)
        //This has been depricated in v2.2 - it will be removed in future releases
        #define SetConfigurationOptions() {U1CNFG1 = UCFG_VAL;U1EIE = 0x9F;UIE = 0x39 | USB_SOF_INTERRUPT | USB_ERROR_INTERRUPT; }	//UCFG_VAL defined in usb_config.h
    #else
        #define SetConfigurationOptions()   {\
                                                U1CNFG1 = USB_PULLUP_OPTION | USB_TRANSCEIVER_OPTION | USB_SPEED_OPTION | USB_PING_PONG_MODE;\
                                                U1EIE = 0x9F;\
                                                UIE = 0x39 | USB_SOF_INTERRUPT | USB_ERROR_INTERRUPT;\
                                            }  
    #endif      

#elif defined(__C30__)
   #if defined(UCFG_VAL)
        //This has been depricated in v2.2 - it will be removed in future releases
        #define SetConfigurationOptions() {U1CNFG1 = UCFG_VAL;U1EIE = 0x9F;U1IE = 0x99 | USB_SOF_INTERRUPT | USB_ERROR_INTERRUPT;}	//UCFG_VAL defined in usb_config.h
    #else
        #define SetConfigurationOptions()   {\
                                                U1CNFG1 = USB_PING_PONG_MODE;\
                                                U1CNFG2 = USB_TRANSCEIVER_OPTION | USB_SPEED_OPTION | USB_PULLUP_OPTION;\
                                                U1EIE = 0x9F;\
                                                U1IE = 0x99 | USB_SOF_INTERRUPT | USB_ERROR_INTERRUPT;\
                                            } 
    #endif

    #if defined(USB_SPEED_OPTION) && (USB_SPEED_OPTION != USB_FULL_SPEED)
        #error "Low speed operation in device mode is not currently supported in the PIC24F family devices."
    #endif
#elif defined(__C32__)
    #define SetConfigurationOptions()   {U1CNFG1 = 0;U1EIE = 0x9F;U1IE = 0x99 | USB_SOF_INTERRUPT | USB_ERROR_INTERRUPT;}
#else
    #error
#endif
//#define _UTEYE      0x80            // Use Eye-Pattern test

/* UEPn Initialization Parameters */
#if defined (__18CXX)
    #define EP_CTRL     0x06            // Cfg Control pipe for this ep
    #define EP_OUT      0x0C            // Cfg OUT only pipe for this ep
    #define EP_IN       0x0A            // Cfg IN only pipe for this ep
    #define EP_OUT_IN   0x0E            // Cfg both OUT & IN pipes for this ep
                                    // Handshake should be disable for isoch

    #define USB_HANDSHAKE_ENABLED   0x10
    #define USB_HANDSHAKE_DISABLED  0x00

    #define USB_OUT_ENABLED         0x04
    #define USB_OUT_DISABLED        0x00

    #define USB_IN_ENABLED          0x02
    #define USB_IN_DISABLED         0x00

    #define USB_ALLOW_SETUP         0x00
    #define USB_DISALLOW_SETUP      0x08

    #define USB_STALL_ENDPOINT      0x01
#elif defined(__C30__) || defined(__C32__)
    #define EP_CTRL     0x0C            // Cfg Control pipe for this ep
    #define EP_OUT      0x18            // Cfg OUT only pipe for this ep
    #define EP_IN       0x14            // Cfg IN only pipe for this ep
    #define EP_OUT_IN   0x1C            // Cfg both OUT & IN pipes for this ep
    #define HSHK_EN     0x01            // Enable handshake packet
                                    // Handshake should be disable for isoch

    #define USB_HANDSHAKE_ENABLED   0x01
    #define USB_HANDSHAKE_DISABLED  0x00

    #define USB_OUT_ENABLED         0x08
    #define USB_OUT_DISABLED        0x00

    #define USB_IN_ENABLED          0x04
    #define USB_IN_DISABLED         0x00

    #define USB_ALLOW_SETUP         0x00
    #define USB_DISALLOW_SETUP      0x10

    #define USB_STALL_ENDPOINT      0x02
#endif

#if !defined(USBDEVICE_C)
/** EXTERNS ********************************************************/
    //Depricated in v2.2 - will be removed in a future revision
    #if !defined(USB_USER_DEVICE_DESCRIPTOR)
        //Device descriptor
        extern ROM USB_DEVICE_DESCRIPTOR device_dsc;
    #else
        USB_USER_DEVICE_DESCRIPTOR_INCLUDE;
    #endif

    //Configuration descriptor
    extern ROM BYTE configDescriptor1[];

    #if !defined(USB_USER_CONFIG_DESCRIPTOR)
        //Array of configuration descriptors
        extern ROM BYTE *ROM USB_CD_Ptr[];
    #else
        USB_USER_CONFIG_DESCRIPTOR_INCLUDE;
    #endif

    //Array of string descriptors
    extern ROM BYTE *ROM USB_SD_Ptr[];

    #if defined(USB_USE_HID)
    //Class specific - HID report descriptor
    #if !defined(__USB_DESCRIPTORS_C)
        extern ROM struct{BYTE report[HID_RPT01_SIZE];} hid_rpt01;
    #endif
    #endif

    //Buffer for control transfers
    extern volatile CTRL_TRF_SETUP SetupPkt;           // 8-byte only
    //Buffer for control transfer data
    extern volatile BYTE CtrlTrfData[USB_EP0_BUFF_SIZE];

    #if defined(USB_USE_HID)
    //class specific data buffers
    extern volatile unsigned char hid_report_out[HID_INT_OUT_EP_SIZE];
    extern volatile unsigned char hid_report_in[HID_INT_IN_EP_SIZE];
    #endif


#endif

/* Control Transfer States */
#define WAIT_SETUP          0
#define CTRL_TRF_TX         1
#define CTRL_TRF_RX         2

/* v2.1 fix - Short Packet States - Used by Control Transfer Read  - CTRL_TRF_TX */
#define SHORT_PKT_NOT_USED  0
#define SHORT_PKT_PENDING   1
#define SHORT_PKT_SENT      2

/* USB PID: Token Types - See chapter 8 in the USB specification */
#define SETUP_TOKEN         0x0D    // 0b00001101
#define OUT_TOKEN           0x01    // 0b00000001
#define IN_TOKEN            0x09    // 0b00001001

/* bmRequestType Definitions */
#define HOST_TO_DEV         0
#define DEV_TO_HOST         1

#define STANDARD            0x00
#define CLASS               0x01
#define VENDOR              0x02

#define RCPT_DEV            0
#define RCPT_INTF           1
#define RCPT_EP             2
#define RCPT_OTH            3

/** EXTERNS ********************************************************/


/** PUBLIC PROTOTYPES **********************************************/

/**************************************************************************
  Function:
        void USBDeviceTasks(void)
    
  Summary:
    This function is the main state machine of the USB device side stack.
    This function should be called periodically to receive and transmit
    packets through the stack. This function should be called preferably
    once every 100us during the enumeration process. After the enumeration
    process this function still needs to be called periodically to respond
    to various situations on the bus but is more relaxed in its time
    requirements. This function should also be called at least as fast as
    the OUT data expected from the PC.

  Description:
    This function is the main state machine of the USB device side stack.
    This function should be called periodically to receive and transmit
    packets through the stack. This function should be called preferably
    once every 100us during the enumeration process. After the enumeration
    process this function still needs to be called periodically to respond
    to various situations on the bus but is more relaxed in its time
    requirements. This function should also be called at least as fast as
    the OUT data expected from the PC.

    Typical usage:
    <code>
    void main(void)
    {
        USBDeviceInit()
        while(1)
        {
            USBDeviceTasks();
            if((USBGetDeviceState() \< CONFIGURED_STATE) ||
               (USBIsDeviceSuspended() == TRUE))
            {
                //Either the device is not configured or we are suspended
                //  so we don't want to do execute any application code
                continue;   //go back to the top of the while loop
            }
            else
            {
                //Otherwise we are free to run user application code.
                UserApplication();
            }
        }
    }
    </code>

  Conditions:
    None
  Remarks:
    This function should be called preferably once every 100us during the
    enumeration process. After the enumeration process this function still
    needs to be called periodically to respond to various situations on the
    bus but is more relaxed in its time requirements.                      
  **************************************************************************/
void USBDeviceTasks(void);

/**************************************************************************
    Function:
        void USBDeviceInit(void)
    
    Description:
        This function initializes the device stack it in the default state. The
        USB module will be completely reset including all of the internal
        variables, registers, and interrupt flags.
                
    Precondition:
        This function must be called before any of the other USB Device
        functions can be called, including USBDeviceTasks().
        
    Parameters:
        None
     
    Return Values:
        None
        
    Remarks:
        None
                                                          
  **************************************************************************/
void USBDeviceInit(void);

/********************************************************************
  Function:
        BOOL USBGetRemoteWakeupStatus(void)
    
  Summary:
    This function indicates if remote wakeup has been enabled by the host.
    Devices that support remote wakeup should use this function to
    determine if it should send a remote wakeup.

  Description:
    This function indicates if remote wakeup has been enabled by the host.
    Devices that support remote wakeup should use this function to
    determine if it should send a remote wakeup.
    
    If a device does not support remote wakeup (the Remote wakeup bit, bit
    5, of the bmAttributes field of the Configuration descriptor is set to
    1), then it should not send a remote wakeup command to the PC and this
    function is not of any use to the device. If a device does support
    remote wakeup then it should use this function as described below.
    
    If this function returns FALSE and the device is suspended, it should
    not issue a remote wakeup (resume).
    
    If this function returns TRUE and the device is suspended, it should
    issue a remote wakeup (resume).
    
    A device can add remote wakeup support by having the _RWU symbol added
    in the configuration descriptor (located in the usb_descriptors.c file
    in the project). This done in the 8th byte of the configuration
    descriptor. For example:

    <code lang="c">
    ROM BYTE configDescriptor1[]={
        0x09,                           // Size 
        USB_DESCRIPTOR_CONFIGURATION,   // descriptor type 
        DESC_CONFIG_WORD(0x0022),       // Total length 
        1,                              // Number of interfaces 
        1,                              // Index value of this cfg 
        0,                              // Configuration string index 
        _DEFAULT | _SELF | _RWU,        // Attributes, see usb_device.h 
        50,                             // Max power consumption in 2X mA(100mA)
        
        //The rest of the configuration descriptor should follow
    </code>

    For more information about remote wakeup, see the following section of
    the USB v2.0 specification available at www.usb.org: 
        * Section 9.2.5.2
        * Table 9-10 
        * Section 7.1.7.7 
        * Section 9.4.5

  Conditions:
    None

  Return Values:
    TRUE -   Remote Wakeup has been enabled by the host
    FALSE -  Remote Wakeup is not currently enabled

  Remarks:
    None
                                                                                                                                                                                                                                                                                                                       
  *******************************************************************/
#define USBGetRemoteWakeupStatus() RemoteWakeup

/***************************************************************************
  Function:
        BYTE USBGetDeviceState(void)
    
  Summary:
    This function will return the current state of the device on the USB.
    This function should return CONFIGURED_STATE before an application
    tries to send information on the bus.
  Description:
    This function returns the current state of the device on the USB. This
    \function is used to determine when the device is ready to communicate
    on the bus. Applications should not try to send or receive data until
    this function returns CONFIGURED_STATE.
    
    It is also important that applications yield as much time as possible
    to the USBDeviceTasks() function as possible while the this function
    \returns any value between ATTACHED_STATE through CONFIGURED_STATE.
    
    For more information about the various device states, please refer to
    the USB specification section 9.1 available from www.usb.org.
    
    Typical usage:
    <code>
    void main(void)
    {
        USBDeviceInit()
        while(1)
        {
            USBDeviceTasks();
            if((USBGetDeviceState() \< CONFIGURED_STATE) ||
               (USBIsDeviceSuspended() == TRUE))
            {
                //Either the device is not configured or we are suspended
                //  so we don't want to do execute any application code
                continue;   //go back to the top of the while loop
            }
            else
            {
                //Otherwise we are free to run user application code.
                UserApplication();
            }
        }
    }
    </code>
  Conditions:
    None
  Return Values:
    DETACHED_STATE -     The device is not attached to the bus
    ATTACHED_STATE -     The device is attached to the bus but
    POWERED_STATE -      The device is not officially in the powered state
    DEFAULT_STATE -      The device has received a RESET from the host
    ADR_PENDING_STATE -  The device has received the SET_ADDRESS command but
                         hasn't received the STATUS stage of the command so
                         it is still operating on address 0.
    ADDRESS_STATE -      The device has an address assigned but has not
                         received a SET_CONFIGURATION command yet or has
                         received a SET_CONFIGURATION with a configuration
                         number of 0 (deconfigured)
    CONFIGURED_STATE -   the device has received a non\-zero
                         SET_CONFIGURATION command is now ready for
                         communication on the bus.
  Remarks:
    None                                                                    
  ***************************************************************************/
#define USBGetDeviceState() USBDeviceState

/***************************************************************************
  Function:
        BOOL USBGetSuspendState(void)
    
  Summary:
    This function indicates if this device is currently suspended. When a
    device is suspended it will not be able to transfer data over the bus.
  Description:
    This function indicates if this device is currently suspended. When a
    device is suspended it will not be able to transfer data over the bus.
    This function can be used by the application to skip over section of
    code that do not need to exectute if the device is unable to send data
    over the bus.
    
    Typical usage:
    <code>
       void main(void)
       {
           USBDeviceInit()
           while(1)
           {
               USBDeviceTasks();
               if((USBGetDeviceState() \< CONFIGURED_STATE) ||
                  (USBIsDeviceSuspended() == TRUE))
               {
                   //Either the device is not configured or we are suspended
                   //  so we don't want to do execute any application code
                   continue;   //go back to the top of the while loop
               }
               else
               {
                   //Otherwise we are free to run user application code.
                   UserApplication();
               }
           }
       }
    </code>
  Conditions:
    None
  Return Values:
    TRUE -   this device is suspended.
    FALSE -  this device is not suspended.
  Remarks:
    None                                                                    
  ***************************************************************************/
#define USBIsDeviceSuspended() USBSuspendControl 

#define USBSoftDetach()  U1CON = 0; U1IE = 0; USBDeviceState = DETACHED_STATE;
void USBCtrlEPService(void);
void USBCtrlTrfSetupHandler(void);
void USBCtrlTrfInHandler(void);
void USBCheckStdRequest(void);
void USBStdGetDscHandler(void);
void USBCtrlEPServiceComplete(void);
void USBCtrlTrfTxService(void);
void USBPrepareForNextSetupTrf(void);
void USBCtrlTrfRxService(void);
void USBStdSetCfgHandler(void);
void USBStdGetStatusHandler(void);
void USBStdFeatureReqHandler(void);
void USBCtrlTrfOutHandler(void);
BOOL USBIsTxBusy(BYTE EPNumber);
void USBPut(BYTE EPNum, BYTE Data);
void USBEPService(void);
void USBConfigureEndpoint(BYTE EPNum, BYTE direction);

void USBProtocolResetHandler(void);
void USBWakeFromSuspend(void);
void USBSuspend(void);
void USBStallHandler(void);
USB_HANDLE USBTransferOnePacket(BYTE ep, BYTE dir, BYTE* data, BYTE len);
void USBEnableEndpoint(BYTE ep,BYTE options);

#if defined(USB_DYNAMIC_EP_CONFIG)
    void USBInitEP(BYTE ROM* pConfig);
#else
    #define USBInitEP(a)
#endif

#if defined(ENABLE_EP0_DATA_RECEIVED_CALLBACK)
    void USBCBEP0DataReceived(void);
    #define USBCB_EP0_DATA_RECEIVED() USBCBEP0DataReceived()
#else
    #define USBCB_EP0_DATA_RECEIVED()
#endif

/** Section: CALLBACKS ******************************************************/

/*************************************************************************
  Function:
      void USBCBSuspend(void)
    
  Summary:
    Call back that is invoked when a USB suspend is detected.
  Description:
    Call back that is invoked when a USB suspend is detected.
    
    \Example power saving code. Insert appropriate code here for the
    desired application behavior. If the microcontroller will be put to
    sleep, a process similar to that shown below may be used:
    
    \Example Psuedo Code:
    <code>
    ConfigureIOPinsForLowPower();
    SaveStateOfAllInterruptEnableBits();
    DisableAllInterruptEnableBits();
    
    //should enable at least USBActivityIF as a wake source
    EnableOnlyTheInterruptsWhichWillBeUsedToWakeTheMicro();
    
    Sleep();
    
    //Preferrably, this should be done in the
    //  USBCBWakeFromSuspend() function instead.
    RestoreStateOfAllPreviouslySavedInterruptEnableBits();
    
    //Preferrably, this should be done in the
    //  USBCBWakeFromSuspend() function instead.
    RestoreIOPinsToNormal();
    </code>
    
    IMPORTANT NOTE: Do not clear the USBActivityIF (ACTVIF) bit here. This
    bit is cleared inside the usb_device.c file. Clearing USBActivityIF
    here will cause things to not work as intended.
  Conditions:
    None
    
    Paramters: None
  Side Effects:
    None
    
    Remark: None                                                          
  *************************************************************************/
void USBCBSuspend(void);

/******************************************************************************
 Function:      
   void USBCBWakeFromSuspend(void)
 
 Summary:
   This call back is invoked when a wakeup from USB suspend 
   is detected.

 Description:   
   The host may put USB peripheral devices in low power
   suspend mode (by "sending" 3+ms of idle).  Once in suspend
   mode, the host may wake the device back up by sending non-
   idle state signalling.
 					
   This call back is invoked when a wakeup from USB suspend 
   is detected.

   If clock switching or other power savings measures were taken when
   executing the USBCBSuspend() function, now would be a good time to
   switch back to normal full power run mode conditions.  The host allows
   a few milliseconds of wakeup time, after which the device must be 
   fully back to normal, and capable of receiving and processing USB
   packets.  In order to do this, the USB module must receive proper
   clocking (IE: 48MHz clock must be available to SIE for full speed USB
   operation).

 PreCondition:  None
 
 Parameters:    None
 
 Return Values: None
 
 Remarks:       None
 *****************************************************************************/
void USBCBWakeFromSuspend(void);

/********************************************************************
  Function:        
    void USBCB_SOF_Handler(void)

  Summary:
    This callback is called when a SOF packet is received by the host. 
    (optional)

  Description:   
    This callback is called when a SOF packet is received by the host. 
    (optional)
  
    The USB host sends out a SOF packet to full-speed
    devices every 1 ms. This interrupt may be useful
    for isochronous pipes. End designers should
    implement callback routine as necessary.

  PreCondition:    
    None
 
  Parameters:      
    None
 
  Return Values:   
    None
 
  Remarks:         
    None
 *******************************************************************/
void USBCB_SOF_Handler(void);

/*******************************************************************
  Function:
    void USBCBErrorHandler(void)
 
  Summary:
    This callback is called whenever a USB error occurs. (optional)

  Description:        
    This callback is called whenever a USB error occurs. (optional)

    The purpose of this callback is mainly for
    debugging during development. Check UEIR to see
    which error causes the interrupt.
 
  PreCondition:
    None
 
  Parameters:
    None
 
  Return Values:
    None

  Remarks:
    No need to clear UEIR to 0 here.
    Callback caller is already doing that.
    
    Typically, user firmware does not need to do anything special
    if a USB error occurs.  For example, if the host sends an OUT
    packet to your device, but the packet gets corrupted (ex:
    because of a bad connection, or the user unplugs the
    USB cable during the transmission) this will typically set
    one or more USB error interrupt flags.  Nothing specific
    needs to be done however, since the SIE will automatically
    send a "NAK" packet to the host.  In response to this, the
    host will normally retry to send the packet again, and no
    data loss occurs.  The system will typically recover
    automatically, without the need for application firmware
    intervention.
    
    Nevertheless, this callback function is provided, such as
    for debugging purposes.
 *******************************************************************/
void USBCBErrorHandler(void);

/**************************************************************************
  Function:
      void USBCBCheckOtherReq(void)
    
  Summary:
    This function is called whenever a request comes over endpoint 0 (the
    control endpoint) that the stack does not know how to handle.
  Description:
    When SETUP packets arrive from the host, some firmware must process the
    request and respond appropriately to fulfill the request. Some of the
    SETUP packets will be for standard USB "chapter 9" (as in, fulfilling
    chapter 9 of the official USB specifications) requests, while others
    may be specific to the USB device class that is being implemented. For
    \example, a HID class device needs to be able to respond to "GET
    REPORT" type of requests. This is not a standard USB chapter 9 request,
    and therefore not handled by usb_device.c. Instead this request should
    be handled by class specific firmware, such as that contained in
    usb_function_hid.c.
    
    Typical Usage:
    <code>
    void USBCBCheckOtherReq(void)
    {
        //Since the stack didn't handle the request I need to check
        //  my class drivers to see if it is for them
        USBCheckMSDRequest();
    }
    </code>
  Conditions:
    None
  Remarks:
    None                                                                   
  **************************************************************************/
void USBCBCheckOtherReq(void);

/*******************************************************************
  Function:
    void USBCBStdSetDscHandler(void)
 
  Summary:
    This callback is called when a SET_DESCRIPTOR request is received (optional)

  Description:
    The USBCBStdSetDscHandler() callback function is
    called when a SETUP, bRequest: SET_DESCRIPTOR request
    arrives.  Typically SET_DESCRIPTOR requests are
    not used in most applications, and it is
    optional to support this type of request.

  PreCondition:
    None
 
  Parameters:
    None
 
  Return Values:
    None
 
  Remark:            None
 *******************************************************************/
void USBCBStdSetDscHandler(void);

/*****************************************************************************************************************
  Function:
      void USBCBInitEP(void)
    
  Summary:
    This function is called whenever the device receives a
    SET_CONFIGURATION request.
  Description:
    This function is called when the device becomes initialized, which
    occurs after the host sends a SET_CONFIGURATION (wValue not = 0)
    request. This callback function should initialize the endpoints for the
    device's usage according to the current configuration.
    
    Typical Usage:
    <code>
    void USBCBInitEP(void)
    {
        USBEnableEndpoint(MSD_DATA_IN_EP,USB_IN_ENABLED|USB_OUT_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);
        USBMSDInit();
    }
    </code>
  Conditions:
    None
  Remarks:
    None                                                                                                          
  *****************************************************************************************************************/
void USBCBInitEP(void);

/******************************************************************************
  Function:
        void USBCBSendResume(void)
    
  Summary:
    This function should be called to initiate a remote wakeup. (optional)
  Description:
    The USB specifications allow some types of USB peripheral devices to
    wake up a host PC (such as if it is in a low power suspend to RAM
    state). This can be a very useful feature in some USB applications,
    such as an Infrared remote control receiver. If a user presses the
    "power" button on a remote control, it is nice that the IR receiver can
    detect this signalling, and then send a USB "command" to the PC to wake
    up.
    
    The USBCBSendResume() "callback" function is used to send this special
    USB signalling which wakes up the PC. This function may be called by
    application firmware to wake up the PC. This function should only be
    called when:
    
      1. The USB driver used on the host PC supports the remote wakeup
         capability.
      2. The USB configuration descriptor indicates the device is remote
         wakeup capable in the bmAttributes field. (see usb_descriptors.c and
         _RWU)
      3. The USB host PC is currently sleeping, and has previously sent
         your device a SET FEATURE setup packet which "armed" the remote wakeup
         capability. (see USBGetRemoteWakeupStatus())
    
    This callback should send a RESUME signal that has the period of
    1-15ms.
    
    Typical Usage:
    <code>
    if((USBDeviceState == CONFIGURED_STATE)
        &amp;&amp; (USBIsDeviceSuspended() == TRUE)
        &amp;&amp; (USBGetRemoteWakeupStatus() == TRUE))
    {
        if(ButtonPressed)
        {
            //Wake up the USB module from suspend
            USBWakeFromSuspend();
    
            //Issue a remote wakeup command on the bus
            USBCBSendResume();
        }
    }
    </code>
  Conditions:
    None
  Remarks:
    A user can switch to primary first by calling USBCBWakeFromSuspend() if
    required/desired.
    
    The modifiable section in this routine should be changed to meet the
    application needs. Current implementation temporary blocks other
    functions from executing for a period of 1-13 ms depending on the core
    frequency.
    
    According to USB 2.0 specification section 7.1.7.7, "The remote wakeup
    device must hold the resume signaling for at lest 1 ms but for no more
    than 15 ms." The idea here is to use a delay counter loop, using a
    common value that would work over a wide range of core frequencies.
    That value selected is 1800. See table below:
    <table>
    Core Freq(MHz)   MIP (for PIC18)   RESUME Signal Period (ms)
    ---------------  ----------------  --------------------------
    48               12                1.05
    4                1                 12.6
    </table>
      * These timing could be incorrect when using code optimization or
        extended instruction mode, or when having other interrupts enabled.
        Make sure to verify using the MPLAB SIM's Stopwatch and verify the
        actual signal on an oscilloscope.
      * These timing numbers should be recalculated when using PIC24 or
        PIC32 as they have different clocking structures.
      * A timer can be used in place of the blocking loop if desired.
                                                                               
******************************************************************************/
void USBCBSendResume(void);

/*******************************************************************
  Function:
    void USBCBEP0DataReceived(void)
  
  Summary:
    This function is called whenever a EP0 data
    packet is received. (optional)

  Description:
    This function is called whenever a EP0 data
    packet is received.  This gives the user (and
    thus the various class examples a way to get
    data that is received via the control endpoint.
    This function needs to be used in conjunction
    with the USBCBCheckOtherReq() function since 
    the USBCBCheckOtherReq() function is the apps
    method for getting the initial control transfer
    before the data arrives.

  PreCondition:
    ENABLE_EP0_DATA_RECEIVED_CALLBACK must be
    defined already (in usb_config.h)
 
  Parameters:
    None
 
  Return Values:
    None

  Remarks:
    None
*******************************************************************/
void USBCBEP0DataReceived(void);




/** Section: MACROS ******************************************************/

#define DESC_CONFIG_BYTE(a) (a)
#define DESC_CONFIG_WORD(a) (a&0xFF),((a>>8)&0xFF)
#define DESC_CONFIG_DWORD(a) (a&0xFF),((a>>8)&0xFF),((a>>16)&0xFF),((a>>24)&0xFF)

/*************************************************************************
  Function:
    BOOL USBHandleBusy(USB_HANDLE handle)
    
  Summary:
    Checks to see if the input handle is busy

  Description:
    Checks to see if the input handle is busy

    Typical Usage
    <code>
    //make sure that the last transfer isn't busy by checking the handle
    if(!USBHandleBusy(USBGenericInHandle))
    {
        //Send the data contained in the INPacket[] array out on
        //  endpoint USBGEN_EP_NUM
        USBGenericInHandle = USBGenWrite(USBGEN_EP_NUM,(BYTE*)&INPacket[0],sizeof(INPacket));
    }
    </code>

  Conditions:
    None
  Input:
    USB_HANDLE handle -  handle of the transfer that you want to check the
                         status of
  Return Values:
    TRUE -   The specified handle is busy
    FALSE -  The specified handle is free and available for a transfer
  Remarks:
    None                                                                  
  *************************************************************************/
#define USBHandleBusy(handle) (handle==0?0:((volatile BDT_ENTRY*)handle)->STAT.UOWN)

/********************************************************************
    Function:
        WORD USBHandleGetLength(USB_HANDLE handle)
        
    Summary:
        Retrieves the length of the destination buffer of the input
        handle
        
    Description:
        Retrieves the length of the destination buffer of the input
        handle

    PreCondition:
        None
        
    Parameters:
        USB_HANDLE handle - the handle to the transfer you want the
        address for.
        
    Return Values:
        WORD - length of the current buffer that the input handle
        points to.  If the transfer is complete then this is the 
        length of the data transmitted or the length of data
        actually received.
        
    Remarks:
        None
 
 *******************************************************************/
#define USBHandleGetLength(handle) (((volatile BDT_ENTRY*)handle)->CNT)

/********************************************************************
    Function:
        WORD USBHandleGetAddr(USB_HANDLE)
        
    Summary:
        Retrieves the address of the destination buffer of the input
        handle
        
    Description:
        Retrieves the address of the destination buffer of the input
        handle

    PreCondition:
        None
        
    Parameters:
        USB_HANDLE handle - the handle to the transfer you want the
        address for.
        
    Return Values:
        WORD - address of the current buffer that the input handle
        points to.
       
    Remarks:
        None
 
 *******************************************************************/
#define USBHandleGetAddr(handle) (((volatile BDT_ENTRY*)handle)->ADR)

/********************************************************************
    Function:
        void USBEP0SetSourceRAM(BYTE* src)
        
    Summary:
        Sets the address of the data to send over the
        control endpoint
   
    PreCondition:
        None
        
    Paramters:
        src - address of the data to send
        
    Return Values:
        None
        
    Remarks:
        None
  
 *******************************************************************/
#define USBEP0SetSourceRAM(src) inPipes[0].pSrc.bRam = src

/********************************************************************
    Function:
        void USBEP0SetSourceROM(BYTE* src)
        
    Summary:
        Sets the address of the data to send over the
        control endpoint
        
    PreCondition:
        None
        
    Parameters:
        src - address of the data to send
        
    Return Values:
        None
        
    Remarks:
        None
 
 *******************************************************************/
#define USBEP0SetSourceROM(src) inPipes[0].pSrc.bRom = src

/********************************************************************
    Function:
        void USBEP0Transmit(BYTE options)
        
    Summary:
        Sets the address of the data to send over the
        control endpoint
        
    PreCondition:
        None
        
    Paramters:
        options - the various options that you want
                  when sending the control data. Options are:
                       USB_INPIPES_ROM
                       USB_INPIPES_RAM
                       USB_INPIPES_BUSY
                       USB_INPIPES_INCLUDE_ZERO
                       USB_INPIPES_NO_DATA
                       USB_INPIPES_NO_OPTIONS
                       
    Return Values:
        None
    
    Remarks:
        None
 
 *******************************************************************/
#define USBEP0Transmit(options) inPipes[0].info.Val = options | USB_INPIPES_BUSY

/********************************************************************
    Function:
        void USBEP0SetSize(WORD size)
       
    Summary:
        Sets the size of the data to send over the
        control endpoint
        
    PreCondition:
        None
        
    Parameters:
        size - the size of the data needing to be transmitted
        
    Return Values:
        None
        
    Remarks:
        None
  
 *******************************************************************/
#define USBEP0SetSize(size) inPipes[0].wCount.Val = size

/********************************************************************
    Function:
        void USBEP0SendRAMPtr(BYTE* src, WORD size, BYTE Options)
        
    Summary:
        Sets the source, size, and options of the data
        you wish to send from a RAM source
       
    PreCondition:
        None
        
    Parameters:
        src - address of the data to send
        size - the size of the data needing to be transmitted
        options - the various options that you want
        when sending the control data. Options are:
        USB_INPIPES_ROM
        USB_INPIPES_RAM
        USB_INPIPES_BUSY
        USB_INPIPES_INCLUDE_ZERO
        USB_INPIPES_NO_DATA
        USB_INPIPES_NO_OPTIONS
        
    Return Values:
        None
        
    Remarks:
        None
 
 *******************************************************************/
#define USBEP0SendRAMPtr(src,size,options)  {USBEP0SetSourceRAM(src);USBEP0SetSize(size);USBEP0Transmit(options | USB_EP0_RAM);}

/********************************************************************
    Function:
        void USBEP0SendROMPtr(BYTE* src, WORD size, BYTE Options)
        
    Summary:
        Sets the source, size, and options of the data
        you wish to send from a ROM source
       
    PreCondition:
        None
        
    Parameters:
        src - address of the data to send
        size - the size of the data needing to be transmitted
        options - the various options that you want
        when sending the control data. Options are:
        USB_INPIPES_ROM
        USB_INPIPES_RAM
        USB_INPIPES_BUSY
        USB_INPIPES_INCLUDE_ZERO
        USB_INPIPES_NO_DATA
        USB_INPIPES_NO_OPTIONS
 
    Return Values:
        None
        
    Remarks:
        None
 
 *******************************************************************/
#define USBEP0SendROMPtr(src,size,options)  {USBEP0SetSourceROM(src);USBEP0SetSize(size);USBEP0Transmit(options | USB_EP0_ROM);}

/********************************************************************
    Function:
        void USBEP0Receive(BYTE* dest, WORD size, void (*function))
        
    Summary:
        Sets the destination, size, and a function to call on the 
        completion of the next control write.
       
    PreCondition:
        None
        
    Parameters:
        dest - address of where the incoming data will go (make sure that
               this address is directly accessable by the USB module - for
               parts with dedicated USB RAM this address must be in that space)
        size - the size of the data being received (is almost always going to
               be presented by the preceeding setup packet - SetupPkt.wLength)
        function - a function that you want called once the data is received.  
               If this is specificed as NULL then no function is called.
        
    Return Values:
        None
        
    Remarks:
        None
 
 *******************************************************************/
#define USBEP0Receive(dest,size,function)  {outPipes[0].pDst.bRam = dest;outPipes[0].wCount.Val = size;outPipes[0].pFunc = function;outPipes[0].info.bits.busy = 1; }


/********************************************************************
    Function:
        USB_HANDLE USBTxOnePacket(BYTE ep, BYTE* data, WORD len)
        
    Summary:
        Sends the specified data out the specified endpoint
        
    PreCondition:
        None
        
    Parameters:
        ep - the endpoint you want to send the data out of
        data - the data that you wish to send
        len - the length of the data that you wish to send
        
    Return Values:
        USB_HANDLE - a handle for the transfer.  This information
        should be kept to track the status of the transfer
        
    Remarks:
        None
  
 *******************************************************************/
#define USBTxOnePacket(ep,data,len)     USBTransferOnePacket(ep,IN_TO_HOST,data,len)

/********************************************************************
    Function:
        USB_HANDLE USBRxOnePacket(BYTE ep, BYTE* data, WORD len)
        
    Summary:
        Receives the specified data out the specified endpoint
        
    PreCondition:
        None
        
    Parameters:
        ep - the endpoint you want to receive the data into
        data - where the data will go when it arrives
        len - the length of the data that you wish to receive
        
    Return Values:
        None
        
    Remarks:
        None
  
 *******************************************************************/
#define USBRxOnePacket(ep,data,len)      USBTransferOnePacket(ep,OUT_FROM_HOST,data,len)

/********************************************************************
    Function:
        void USBClearInterruptFlag(WORD reg, BYTE flag)
        
    Summary:
        Clears the specified interrupt flag
        
    PreCondition:
        None
        
    Parameters:
        WORD reg - the register name holding the interrupt flag
        BYTE flag - the bit number needing to be cleared
        
    Return Values:
        None
        
    Remarks:
        None
 
 *******************************************************************/
void USBClearInterruptFlag(BYTE* reg, BYTE flag);

/********************************************************************
    Function:
        void USBClearInterruptRegister(WORD reg)
        
    Summary:
        Clears the specified interrupt register
        
    PreCondition:
        None
        
    Parameters:
        WORD reg - the register name that needs to be cleared
        
    Return Values:
        None
        
    Remarks:
        None
 
 *******************************************************************/
#if defined(__18CXX)
    #define USBClearInterruptRegister(reg) reg = 0;
#elif  defined(__C30__) || defined(__C32__)
    #define USBClearInterruptRegister(reg) reg = 0xFF;
#endif

/********************************************************************
    Function:
        void USBStallEndpoint(BYTE ep, BYTE dir)
        
    Summary:
         STALLs the specified endpoint
    
    PreCondition:
        None
        
    Parameters:
        BYTE ep - the endpoint the data will be transmitted on
        BYTE dir - the direction of the transfer
        
    Return Values:
        None
        
    Remarks:
        None

 *******************************************************************/
void USBStallEndpoint(BYTE ep, BYTE dir);

#if defined USB_ENABLE_ALL_HANDLERS
    #define USB_ENABLE_SUSPEND_HANDLER
    #define USB_ENABLE_WAKEUP_FROM_SUSPEND_HANDLER
    #define USB_ENABLE_SOF_HANDLER
    #define USB_ENABLE_ERROR_HANDLER
    #define USB_ENABLE_OTHER_REQUEST_HANDLER
    #define USB_ENABLE_SET_DESCRIPTOR_HANDLER
    #define USB_ENABLE_INIT_EP_HANDLER
    #define USB_ENABLE_EP0_DATA_HANDLER
    #define USB_ENABLE_TRANSFER_COMPLETE_HANDLER
#else
    #error
#endif

#if defined USB_ENABLE_SUSPEND_HANDLER
    #define USB_SUSPEND_HANDLER                 USB_SUSPEND_HANDLER_FUNC
#else
    #define USB_SUSPEND_HANDLER              
#endif

#if defined USB_ENABLE_WAKEUP_FROM_SUSPEND_HANDLER
    #define USB_WAKEUP_FROM_SUSPEND_HANDLER                 USB_WAKEUP_FROM_SUSPEND_HANDLER_FUNC
#else
    #define USB_WAKEUP_FROM_SUSPEND_HANDLER              
#endif

#if defined USB_ENABLE_SOF_HANDLER
    #define USB_SOF_HANDLER                 USB_SOF_HANDLER_FUNC
#else
    #define USB_SOF_HANDLER              
#endif

#if defined USB_ENABLE_ERROR_HANDLER 
    #define USB_ERROR_HANDLER                 USB_ERROR_HANDLER_FUNC
#else
    #define USB_ERROR_HANDLER               
#endif

#if defined USB_ENABLE_OTHER_REQUEST_HANDLER 
    #define USB_OTHER_REQUEST_HANDLER                 USB_OTHER_REQUEST_HANDLER_FUNC
#else
    #define USB_OTHER_REQUEST_HANDLER               
#endif

#if defined USB_ENABLE_SET_DESCRIPTOR_HANDLER 
    #define USB_SET_DESCRIPTOR_HANDLER                USB_SET_DESCRIPTOR_HANDLER_FUNC
#else
    #define USB_SET_DESCRIPTOR_HANDLER               
#endif

#if defined USB_ENABLE_INIT_EP_HANDLER
    #define USB_INIT_EP_HANDLER                USB_INIT_EP_HANDLER_FUNC
#else
    #define USB_INIT_EP_HANDLER               
#endif

#if defined USB_ENABLE_EP0_DATA_HANDLER 
    #define USB_EP0_DATA_HANDLER               USB_EP0_DATA_HANDLER_FUNC
#else
    #define USB_EP0_DATA_HANDLER               
#endif

#if defined USB_ENABLE_TRANSFER_COMPLETE_HANDLER 
    #define USB_TRASFER_COMPLETE_HANDLER               USB_TRASFER_COMPLETE_HANDLER_FUNC
#else
    #define USB_TRASFER_COMPLETE_HANDLER             
#endif


#if defined(USB_INTERRUPT_LEGACY_CALLBACKS)
    #define USB_SUSPEND_HANDLER_FUNC(event,pointer,size)             USBCBSuspend()
    #define USB_WAKEUP_FROM_SUSPEND_HANDLER_FUNC(event,pointer,size) USBCBWakeFromSuspend()
    #define USB_SOF_HANDLER_FUNC(event,pointer,size)                 USBCB_SOF_Handler()
    #define USB_ERROR_HANDLER_FUNC(event,pointer,size)               USBCBErrorHandler()
    #define USB_OTHER_REQUEST_HANDLER_FUNC(event,pointer,size)       USBCBCheckOtherReq()
    #define USB_SET_DESCRIPTOR_HANDLER_FUNC(event,pointer,size)      USBCBStdSetDscHandler()
    #define USB_INIT_EP_HANDLER_FUNC(event,pointer,size)             USBCBInitEP()
    #define USB_EP0_DATA_HANDLER_FUNC(event,pointer,size)            USBCBEP0DataReceived()
    #define USB_TRASFER_COMPLETE_HANDLER_FUNC(event,pointer,size)
#else
    #define USB_SUSPEND_HANDLER_FUNC(event,pointer,size)             USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_WAKEUP_FROM_SUSPEND_HANDLER_FUNC(event,pointer,size) USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_SOF_HANDLER_FUNC(event,pointer,size)                 USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_ERROR_HANDLER_FUNC(event,pointer,size)               USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_OTHER_REQUEST_HANDLER_FUNC(event,pointer,size)       USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_SET_DESCRIPTOR_HANDLER_FUNC(event,pointer,size)      USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_INIT_EP_HANDLER_FUNC(event,pointer,size)             USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_EP0_DATA_HANDLER_FUNC(event,pointer,size)            USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
    #define USB_TRASFER_COMPLETE_HANDLER_FUNC(event,pointer,size)    USER_USB_CALLBACK_EVENT_HANDLER(event,pointer,size)
#endif

#if defined(USB_INTERRUPT)
void USBDeviceDetach(void);
void USBDeviceAttach(void);
#endif

#if defined(USB_POLLING)
    #define USB_VOLATILE
#else
    #define USB_VOLATILE volatile
#endif

#define USB_PING_PONG__NO_PING_PONG         0x00    //0b00
#define USB_PING_PONG__EP0_OUT_ONLY         0x01    //0b01
#define USB_PING_PONG__FULL_PING_PONG       0x02    //0b10
#define USB_PING_PONG__ALL_BUT_EP0          0x03    //0b11


// When using the PIC32, ping pong mode must be set to FULL.
#if defined (__PIC32MX__)
    #if (USB_PING_PONG_MODE != USB_PING_PONG__FULL_PING_PONG)
        #undef USB_PING_PONG_MODE
        #define USB_PING_PONG_MODE USB_PING_PONG__FULL_PING_PONG
    #endif
#endif

#if (USB_PING_PONG_MODE == USB_PING_PONG__NO_PING_PONG)
    #define USB_NEXT_EP0_OUT_PING_PONG 0x0000   // Used in USB Device Mode only
    #define USB_NEXT_EP0_IN_PING_PONG 0x0000    // Used in USB Device Mode only
    #define USB_NEXT_PING_PONG 0x0000           // Used in USB Device Mode only
    #define EP0_OUT_EVEN    0                   // Used in USB Device Mode only
    #define EP0_OUT_ODD     0                   // Used in USB Device Mode only
    #define EP0_IN_EVEN     1                   // Used in USB Device Mode only
    #define EP0_IN_ODD      1                   // Used in USB Device Mode only
    #define EP1_OUT_EVEN    2                   // Used in USB Device Mode only
    #define EP1_OUT_ODD     2                   // Used in USB Device Mode only
    #define EP1_IN_EVEN     3                   // Used in USB Device Mode only
    #define EP1_IN_ODD      3                   // Used in USB Device Mode only
    #define EP2_OUT_EVEN    4                   // Used in USB Device Mode only
    #define EP2_OUT_ODD     4                   // Used in USB Device Mode only
    #define EP2_IN_EVEN     5                   // Used in USB Device Mode only
    #define EP2_IN_ODD      5                   // Used in USB Device Mode only
    #define EP3_OUT_EVEN    6                   // Used in USB Device Mode only
    #define EP3_OUT_ODD     6                   // Used in USB Device Mode only
    #define EP3_IN_EVEN     7                   // Used in USB Device Mode only
    #define EP3_IN_ODD      7                   // Used in USB Device Mode only
    #define EP4_OUT_EVEN    8                   // Used in USB Device Mode only
    #define EP4_OUT_ODD     8                   // Used in USB Device Mode only
    #define EP4_IN_EVEN     9                   // Used in USB Device Mode only
    #define EP4_IN_ODD      9                   // Used in USB Device Mode only
    #define EP5_OUT_EVEN    10                  // Used in USB Device Mode only
    #define EP5_OUT_ODD     10                  // Used in USB Device Mode only
    #define EP5_IN_EVEN     11                  // Used in USB Device Mode only
    #define EP5_IN_ODD      11                  // Used in USB Device Mode only
    #define EP6_OUT_EVEN    12                  // Used in USB Device Mode only
    #define EP6_OUT_ODD     12                  // Used in USB Device Mode only
    #define EP6_IN_EVEN     13                  // Used in USB Device Mode only
    #define EP6_IN_ODD      13                  // Used in USB Device Mode only
    #define EP7_OUT_EVEN    14                  // Used in USB Device Mode only
    #define EP7_OUT_ODD     14                  // Used in USB Device Mode only
    #define EP7_IN_EVEN     15                  // Used in USB Device Mode only
    #define EP7_IN_ODD      15                  // Used in USB Device Mode only
    #define EP8_OUT_EVEN    16                  // Used in USB Device Mode only
    #define EP8_OUT_ODD     16                  // Used in USB Device Mode only
    #define EP8_IN_EVEN     17                  // Used in USB Device Mode only
    #define EP8_IN_ODD      17                  // Used in USB Device Mode only
    #define EP9_OUT_EVEN    18                  // Used in USB Device Mode only
    #define EP9_OUT_ODD     18                  // Used in USB Device Mode only
    #define EP9_IN_EVEN     19                  // Used in USB Device Mode only
    #define EP9_IN_ODD      19                  // Used in USB Device Mode only
    #define EP10_OUT_EVEN   20                  // Used in USB Device Mode only
    #define EP10_OUT_ODD    20                  // Used in USB Device Mode only
    #define EP10_IN_EVEN    21                  // Used in USB Device Mode only
    #define EP10_IN_ODD     21                  // Used in USB Device Mode only
    #define EP11_OUT_EVEN   22                  // Used in USB Device Mode only
    #define EP11_OUT_ODD    22                  // Used in USB Device Mode only
    #define EP11_IN_EVEN    23                  // Used in USB Device Mode only
    #define EP11_IN_ODD     23                  // Used in USB Device Mode only
    #define EP12_OUT_EVEN   24                  // Used in USB Device Mode only
    #define EP12_OUT_ODD    24                  // Used in USB Device Mode only
    #define EP12_IN_EVEN    25                  // Used in USB Device Mode only
    #define EP12_IN_ODD     25                  // Used in USB Device Mode only
    #define EP13_OUT_EVEN   26                  // Used in USB Device Mode only
    #define EP13_OUT_ODD    26                  // Used in USB Device Mode only
    #define EP13_IN_EVEN    27                  // Used in USB Device Mode only
    #define EP13_IN_ODD     27                  // Used in USB Device Mode only
    #define EP14_OUT_EVEN   28                  // Used in USB Device Mode only
    #define EP14_OUT_ODD    28                  // Used in USB Device Mode only
    #define EP14_IN_EVEN    29                  // Used in USB Device Mode only
    #define EP14_IN_ODD     29                  // Used in USB Device Mode only
    #define EP15_OUT_EVEN   30                  // Used in USB Device Mode only
    #define EP15_OUT_ODD    30                  // Used in USB Device Mode only
    #define EP15_IN_EVEN    31                  // Used in USB Device Mode only
    #define EP15_IN_ODD     31                  // Used in USB Device Mode only

    #define EP(ep,dir,pp) (2*ep+dir)            // Used in USB Device Mode only

    #define BD(ep,dir,pp)   ((8 * ep) + (4 * dir))      // Used in USB Device Mode only

#elif (USB_PING_PONG_MODE == USB_PING_PONG__EP0_OUT_ONLY)
    #define USB_NEXT_EP0_OUT_PING_PONG 0x0004
    #define USB_NEXT_EP0_IN_PING_PONG 0x0000
    #define USB_NEXT_PING_PONG 0x0000
    #define EP0_OUT_EVEN    0
    #define EP0_OUT_ODD     1
    #define EP0_IN_EVEN     2
    #define EP0_IN_ODD      2
    #define EP1_OUT_EVEN    3
    #define EP1_OUT_ODD     3
    #define EP1_IN_EVEN     4
    #define EP1_IN_ODD      4
    #define EP2_OUT_EVEN    5
    #define EP2_OUT_ODD     5
    #define EP2_IN_EVEN     6
    #define EP2_IN_ODD      6
    #define EP3_OUT_EVEN    7
    #define EP3_OUT_ODD     7
    #define EP3_IN_EVEN     8
    #define EP3_IN_ODD      8
    #define EP4_OUT_EVEN    9
    #define EP4_OUT_ODD     9
    #define EP4_IN_EVEN     10
    #define EP4_IN_ODD      10
    #define EP5_OUT_EVEN    11
    #define EP5_OUT_ODD     11
    #define EP5_IN_EVEN     12
    #define EP5_IN_ODD      12
    #define EP6_OUT_EVEN    13
    #define EP6_OUT_ODD     13
    #define EP6_IN_EVEN     14
    #define EP6_IN_ODD      14
    #define EP7_OUT_EVEN    15
    #define EP7_OUT_ODD     15
    #define EP7_IN_EVEN     16
    #define EP7_IN_ODD      16
    #define EP8_OUT_EVEN    17
    #define EP8_OUT_ODD     17
    #define EP8_IN_EVEN     18
    #define EP8_IN_ODD      18
    #define EP9_OUT_EVEN    19
    #define EP9_OUT_ODD     19
    #define EP9_IN_EVEN     20
    #define EP9_IN_ODD      20
    #define EP10_OUT_EVEN   21
    #define EP10_OUT_ODD    21
    #define EP10_IN_EVEN    22
    #define EP10_IN_ODD     22
    #define EP11_OUT_EVEN   23
    #define EP11_OUT_ODD    23
    #define EP11_IN_EVEN    24
    #define EP11_IN_ODD     24
    #define EP12_OUT_EVEN   25
    #define EP12_OUT_ODD    25
    #define EP12_IN_EVEN    26
    #define EP12_IN_ODD     26
    #define EP13_OUT_EVEN   27
    #define EP13_OUT_ODD    27
    #define EP13_IN_EVEN    28
    #define EP13_IN_ODD     28
    #define EP14_OUT_EVEN   29
    #define EP14_OUT_ODD    29
    #define EP14_IN_EVEN    30
    #define EP14_IN_ODD     30
    #define EP15_OUT_EVEN   31
    #define EP15_OUT_ODD    31
    #define EP15_IN_EVEN    32
    #define EP15_IN_ODD     32

    #define EP(ep,dir,pp) (2*ep+dir+(((ep==0)&&(dir==0))?pp:2))
    #define BD(ep,dir,pp) (4*(ep+dir+(((ep==0)&&(dir==0))?pp:2)))

#elif (USB_PING_PONG_MODE == USB_PING_PONG__FULL_PING_PONG)
    #if defined (__18CXX) || defined(__C30__)
        #define USB_NEXT_EP0_OUT_PING_PONG 0x0004
        #define USB_NEXT_EP0_IN_PING_PONG 0x0004
        #define USB_NEXT_PING_PONG 0x0004
    #elif defined(__C32__)
        #define USB_NEXT_EP0_OUT_PING_PONG 0x0008
        #define USB_NEXT_EP0_IN_PING_PONG 0x0008
        #define USB_NEXT_PING_PONG 0x0008
    #else
        #error "Not defined for this compiler"
    #endif
    #define EP0_OUT_EVEN    0
    #define EP0_OUT_ODD     1
    #define EP0_IN_EVEN     2
    #define EP0_IN_ODD      3
    #define EP1_OUT_EVEN    4
    #define EP1_OUT_ODD     5
    #define EP1_IN_EVEN     6
    #define EP1_IN_ODD      7
    #define EP2_OUT_EVEN    8
    #define EP2_OUT_ODD     9
    #define EP2_IN_EVEN     10
    #define EP2_IN_ODD      11
    #define EP3_OUT_EVEN    12
    #define EP3_OUT_ODD     13
    #define EP3_IN_EVEN     14
    #define EP3_IN_ODD      15
    #define EP4_OUT_EVEN    16
    #define EP4_OUT_ODD     17
    #define EP4_IN_EVEN     18
    #define EP4_IN_ODD      19
    #define EP5_OUT_EVEN    20
    #define EP5_OUT_ODD     21
    #define EP5_IN_EVEN     22
    #define EP5_IN_ODD      23
    #define EP6_OUT_EVEN    24
    #define EP6_OUT_ODD     25
    #define EP6_IN_EVEN     26
    #define EP6_IN_ODD      27
    #define EP7_OUT_EVEN    28
    #define EP7_OUT_ODD     29
    #define EP7_IN_EVEN     30
    #define EP7_IN_ODD      31
    #define EP8_OUT_EVEN    32
    #define EP8_OUT_ODD     33
    #define EP8_IN_EVEN     34
    #define EP8_IN_ODD      35
    #define EP9_OUT_EVEN    36
    #define EP9_OUT_ODD     37
    #define EP9_IN_EVEN     38
    #define EP9_IN_ODD      39
    #define EP10_OUT_EVEN   40
    #define EP10_OUT_ODD    41
    #define EP10_IN_EVEN    42
    #define EP10_IN_ODD     43
    #define EP11_OUT_EVEN   44
    #define EP11_OUT_ODD    45
    #define EP11_IN_EVEN    46
    #define EP11_IN_ODD     47
    #define EP12_OUT_EVEN   48
    #define EP12_OUT_ODD    49
    #define EP12_IN_EVEN    50
    #define EP12_IN_ODD     51
    #define EP13_OUT_EVEN   52
    #define EP13_OUT_ODD    53
    #define EP13_IN_EVEN    54
    #define EP13_IN_ODD     55
    #define EP14_OUT_EVEN   56
    #define EP14_OUT_ODD    57
    #define EP14_IN_EVEN    58
    #define EP14_IN_ODD     59
    #define EP15_OUT_EVEN   60
    #define EP15_OUT_ODD    61
    #define EP15_IN_EVEN    62
    #define EP15_IN_ODD     63

    #define EP(ep,dir,pp) (4*ep+2*dir+pp)

    #if defined (__18CXX) || defined(__C30__)
        #define BD(ep,dir,pp) (4*(4*ep+2*dir+pp))
    #elif defined(__C32__)
        #define BD(ep,dir,pp) (8*(4*ep+2*dir+pp))
    #else
        #error "Not defined for this compiler"
    #endif

#elif (USB_PING_PONG_MODE == USB_PING_PONG__ALL_BUT_EP0)
    #define USB_NEXT_EP0_OUT_PING_PONG 0x0000
    #define USB_NEXT_EP0_IN_PING_PONG 0x0000
    #define USB_NEXT_PING_PONG 0x0004
    #define EP0_OUT_EVEN    0
    #define EP0_OUT_ODD     0
    #define EP0_IN_EVEN     1
    #define EP0_IN_ODD      1
    #define EP1_OUT_EVEN    2
    #define EP1_OUT_ODD     3
    #define EP1_IN_EVEN     4
    #define EP1_IN_ODD      5
    #define EP2_OUT_EVEN    6
    #define EP2_OUT_ODD     7
    #define EP2_IN_EVEN     8
    #define EP2_IN_ODD      9
    #define EP3_OUT_EVEN    10
    #define EP3_OUT_ODD     11
    #define EP3_IN_EVEN     12
    #define EP3_IN_ODD      13
    #define EP4_OUT_EVEN    14
    #define EP4_OUT_ODD     15
    #define EP4_IN_EVEN     16
    #define EP4_IN_ODD      17
    #define EP5_OUT_EVEN    18
    #define EP5_OUT_ODD     19
    #define EP5_IN_EVEN     20
    #define EP5_IN_ODD      21
    #define EP6_OUT_EVEN    22
    #define EP6_OUT_ODD     23
    #define EP6_IN_EVEN     24
    #define EP6_IN_ODD      25
    #define EP7_OUT_EVEN    26
    #define EP7_OUT_ODD     27
    #define EP7_IN_EVEN     28
    #define EP7_IN_ODD      29
    #define EP8_OUT_EVEN    30
    #define EP8_OUT_ODD     31
    #define EP8_IN_EVEN     32
    #define EP8_IN_ODD      33
    #define EP9_OUT_EVEN    34
    #define EP9_OUT_ODD     35
    #define EP9_IN_EVEN     36
    #define EP9_IN_ODD      37
    #define EP10_OUT_EVEN   38
    #define EP10_OUT_ODD    39
    #define EP10_IN_EVEN    40
    #define EP10_IN_ODD     41
    #define EP11_OUT_EVEN   42
    #define EP11_OUT_ODD    43
    #define EP11_IN_EVEN    44
    #define EP11_IN_ODD     45
    #define EP12_OUT_EVEN   46
    #define EP12_OUT_ODD    47
    #define EP12_IN_EVEN    48
    #define EP12_IN_ODD     49
    #define EP13_OUT_EVEN   50
    #define EP13_OUT_ODD    51
    #define EP13_IN_EVEN    52
    #define EP13_IN_ODD     53
    #define EP14_OUT_EVEN   54
    #define EP14_OUT_ODD    55
    #define EP14_IN_EVEN    56
    #define EP14_IN_ODD     57
    #define EP15_OUT_EVEN   58
    #define EP15_OUT_ODD    59
    #define EP15_IN_EVEN    60
    #define EP15_IN_ODD     61

    #define EP(ep,dir,pp) (4*ep+2*dir+((ep==0)?0:(pp-2)))
    #define BD(ep,dir,pp) (4*(4*ep+2*dir+((ep==0)?0:(pp-2))))

#else
    #error "No ping pong mode defined."
#endif

extern USB_VOLATILE BOOL RemoteWakeup;
extern USB_VOLATILE BYTE USBDeviceState;
extern USB_VOLATILE BYTE USBActiveConfiguration;
extern USB_VOLATILE IN_PIPE inPipes[];
extern USB_VOLATILE OUT_PIPE outPipes[];

#endif //USBD_H
