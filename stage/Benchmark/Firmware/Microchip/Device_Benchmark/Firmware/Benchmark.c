

/** INCLUDES *******************************************************/
#include "Benchmark.h"
#include "PicFWCommands.h"

#include "Compiler.h"
#include "HardwareProfile.h"
#include "GenericTypeDefs.h"
#include "USB/usb_device.h"
#include "USB/usb.h"
#include "USB/usb_function_generic.h"
#include "usb_config.h"

#if (USB_PING_PONG_MODE==USB_PING_PONG__FULL_PING_PONG) || (USB_PING_PONG_MODE==USB_PING_PONG__ALL_BUT_EP0)

#if BENCH_MARK_BUFFER_COUNT < 2
	#error "The BENCH_MARK_BUFFER_COUNT_define cannot be less than 2"
#endif

/** VARIABLES ******************************************************/
//NOTE:  The below endpoint buffers need to be located in a section of
//system SRAM that is accessible by the USB module.  The USB module on all
//currently existing Microchip USB microcontrollers use a dedicated DMA
//interface for reading/writing USB data into/out of main system SRAM.

//On some USB PIC microcontrollers, all of the microcontroller SRAM is dual
//access, and therefore all of it can be accessed by either the USB 
//module or the microcontroller core.  On other devices, only a certain 
//portion of the SRAM is accessible by the USB module. Therefore, on some 
//devices, it is important to place USB data buffers in certain sections of
//SRAM, while on other devices, the buffers can be placed anywhere.


#if defined(__18CXX) //PIC18 devices
	//For all PIC18F87J50 family and PIC18F46J50 family devices: all SRAM is USB accessible.
	//For PIC18F1xK50 devices: 0x200-0x2FF is USB accessible.
	//For PIC18F4450/2450 devices: 0x400-0x4FF is USB accessible.
	//For PIC18F4550/4553/4455/4458/2550/2553/2455/2458: 0x400-0x7FF is USB accessible.
	#if defined(__18F14K50) || defined(__18F13K50) || defined(__18LF14K50) || defined(__18LF13K50) 
    
		#if (USBGEN_EP_SIZE != 8) && (USBGEN_EP_SIZE != 16) && (USBGEN_EP_SIZE != 32)
			#error "This pic only has 256 bytes of usb memory; change the USBGEN_EP_SIZE_define to 8, 16, or 32 in usb_config.h."
		#endif
		#pragma udata USB_VARIABLES=0x240

	#elif defined(__18F2455) || defined(__18F2550) || defined(__18F4455) || defined(__18F4550) || defined(__18F2458) || defined(__18F2453) || defined(__18F4558) || defined(__18F4553)

    	#pragma udata USB_VARIABLES=0x440

	#elif defined(__18F4450) || defined(__18F2450)

		#if ((USBGEN_EP_SIZE != 8) && (USBGEN_EP_SIZE != 16) && (USBGEN_EP_SIZE != 32))
			#error "This pic only has 256 bytes of usb memory; change the USBGEN_EP_SIZE_define to 8, 16, or 32 in usb_config.h."
		#endif
    	#pragma udata USB_VARIABLES=0x440

	#else
    	#pragma udata
	#endif

#endif

USB_VOLATILE BYTE BenchmarkBuffers[BENCH_MARK_BUFFER_COUNT][USBGEN_EP_SIZE];	//User buffer for receiving OUT packets sent from the host

//The below variables are only accessed by the CPU and can be placed anywhere in RAM.
#pragma udata

USB_HANDLE EP1RxHandles[2];	// OUT handles (EVEN,ODD)
USB_HANDLE EP1TxHandles[2]; // IN  handles (EVEN,ODD)
BOOL EP1RxBankIsOdd;
BOOL EP1TxBankIsOdd;

BYTE PicFW_TestType;
BYTE PicFW_PrevTestType;
BYTE counter;
BYTE fillCount;
BYTE mWriteTestPID;

BYTE* pUserBuffer;
/** EXTERNS ********************************************************/
extern void BlinkUSBStatus(void);
extern volatile BDT_ENTRY *pBDTEntryOut[USB_MAX_EP_NUMBER+1];
extern volatile BDT_ENTRY *pBDTEntryIn[USB_MAX_EP_NUMBER+1];

/** BMARK DEFINES **************************************************/
USB_HANDLE USBRollOnePacket(BYTE ep,BYTE dir,BYTE** data,BYTE len);
void doBenchmarkLoop(void);
void doBenchmarkWrite(void);
void doBenchmarkRead(void);
void doBenchmarkReadForLoop(void);
void fillBuffer(BYTE* pBuffer);

/** BMARK MACROS ****************************************************/
#define USBReadRoll(ep,data,len)	USBRollOnePacket(ep,OUT_FROM_HOST,data,len)
#define USBWriteRoll(ep,data,len)	USBRollOnePacket(ep,IN_TO_HOST,data,len)

#define	mBenchMarkInit()			\
{									\
	PicFW_TestType=TEST_LOOP;		\
	PicFW_PrevTestType=TEST_LOOP;	\
	fillCount=0;					\
	mWriteTestPID=0;				\
}


/** BMARK DECLARATIONS *********************************************/
#pragma code

// The Benchmark firmware "overrides" the USBCBInitEP to initialize
// the OUT (MCU Rx) endpoint with the first BenchmarkBuffer.
void USBCBInitEP(void)
{
	BYTE preLoadedBufferCount;

	if (EP1RxHandles[EVN]) return;

    USBEnableEndpoint(USBGEN_EP_NUM,USB_OUT_ENABLED|USB_IN_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);

	//Prepare the OUT endpoints to receive the first packets from the host.
	preLoadedBufferCount = 0;
	EP1RxHandles[EVN] = USBGenRead (USBGEN_EP_NUM, (BYTE*)BenchmarkBuffers[preLoadedBufferCount++],USBGEN_EP_SIZE);	
	EP1RxHandles[ODD] = USBGenRead (USBGEN_EP_NUM, (BYTE*)BenchmarkBuffers[preLoadedBufferCount++],USBGEN_EP_SIZE);
	EP1TxHandles[EVN] = USBGenWrite(USBGEN_EP_NUM, (BYTE*)BenchmarkBuffers[preLoadedBufferCount++],0);	
	EP1TxHandles[ODD] = USBGenWrite(USBGEN_EP_NUM, (BYTE*)BenchmarkBuffers[preLoadedBufferCount++],0);

	pUserBuffer=BenchmarkBuffers[preLoadedBufferCount++];
}

void USBCBCheckOtherReq(void)
{
	if (SetupPkt.RequestType != VENDOR) return;

	switch (SetupPkt.bRequest)
	{
	case PICFW_SET_TEST:
		PicFW_TestType=SetupPkt.wValue & 0xff;
		inPipes[0].pSrc.bRam = (BYTE*)&PicFW_TestType;  // Set Source
		inPipes[0].info.bits.ctrl_trf_mem = _RAM;		// Set memory type
		inPipes[0].wCount.v[0] = 1;						// Set data count
		inPipes[0].info.bits.busy = 1;
		break;
	case PICFW_GET_TEST:
		inPipes[0].pSrc.bRam = (BYTE*)&PicFW_TestType;  // Set Source
		inPipes[0].info.bits.ctrl_trf_mem = _RAM;		// Set memory type
		inPipes[0].wCount.v[0] = 1;						// Set data count
		inPipes[0].info.bits.busy = 1;
		break;
	case PICFW_GET_EEDATA:
		break;
	case PICFW_SET_EEDATA:
		break;
	default:
		break;
	}//end switch

}//end

void Benchmark_Init(void)
{
	EP1RxHandles[EVN]	= 0;	
	EP1RxHandles[ODD]	= 0;
	EP1RxBankIsOdd		= FALSE;

	EP1TxHandles[EVN]	= 0;	
	EP1TxHandles[ODD]	= 0;	
	EP1TxBankIsOdd		= FALSE;

	mBenchMarkInit();
}//end UserInit

void fillBuffer(BYTE* pBuffer)
{
	for (counter=0; counter < USBGEN_EP_SIZE; counter++)
		pBuffer[counter]=counter;
}

void Benchmark_ProcessIO(void)
{
	//Blink the LEDs according to the USB device status, but only do so if the PC application isn't connected and controlling the LEDs.
	BlinkUSBStatus();

	//Don't attempt to read/write over the USB until after the device has been fully enumerated.
	if((USBDeviceState < CONFIGURED_STATE)||(USBSuspendControl==1))
	{
		mBenchMarkInit();
		return;
	}

	if (PicFW_TestType!=PicFW_PrevTestType)
	{
		fillCount=0;
		mWriteTestPID=0;
		PicFW_PrevTestType=PicFW_TestType;
		return;
	}

	switch(PicFW_TestType)
	{
	case TEST_PCREAD:
		doBenchmarkWrite();
		break;
	case TEST_PCWRITE:
		doBenchmarkRead();
		break;
	case TEST_LOOP:
		doBenchmarkLoop();
		break;
	default:
		doBenchmarkRead();
		break;
	}
}//end Benchmark_ProcessIO

#define mSetWritePacketID(buffer)				\
{												\
	if (fillCount < 3)							\
	{											\
		fillCount++;							\
		fillBuffer((BYTE*)buffer);				\
	}											\
	buffer[1]=mWriteTestPID;					\
	mWriteTestPID++;							\
}

void doBenchmarkWrite(void)
{
	if (EP1TxBankIsOdd && USBHandleBusy(EP1TxHandles[ODD])==FALSE)
	{
		mSetWritePacketID(pUserBuffer);
		EP1TxHandles[ODD]	= USBWriteRoll(USBGEN_EP_NUM,&pUserBuffer,USBGEN_EP_SIZE);
		EP1TxBankIsOdd = FALSE;
	}
	else if(!EP1TxBankIsOdd && USBHandleBusy(EP1TxHandles[EVN])==FALSE)
	{
		mSetWritePacketID(pUserBuffer);
		EP1TxHandles[EVN]	= USBWriteRoll(USBGEN_EP_NUM,&pUserBuffer,USBGEN_EP_SIZE);
		EP1TxBankIsOdd = TRUE;
	}
}

void doBenchmarkLoop(void)
{
	BYTE packetRxLength;

	if (USBHandleBusy(EP1TxHandles[ODD]) && USBHandleBusy(EP1TxHandles[EVN])) return;

	if (EP1RxBankIsOdd && !USBHandleBusy(EP1RxHandles[ODD]))
	{
		packetRxLength = USBHandleGetLength(EP1RxHandles[ODD]);

		EP1RxHandles[ODD] = USBReadRoll(USBGEN_EP_NUM,&pUserBuffer,USBGEN_EP_SIZE);
		EP1RxBankIsOdd=FALSE;

		if (EP1TxBankIsOdd && !USBHandleBusy(EP1TxHandles[ODD]))
		{
			EP1TxHandles[ODD]	= USBWriteRoll(USBGEN_EP_NUM,&pUserBuffer,packetRxLength);
			EP1TxBankIsOdd = FALSE;
		}
		else if(!EP1TxBankIsOdd && !USBHandleBusy(EP1TxHandles[EVN]))
		{
			EP1TxHandles[EVN]	= USBWriteRoll(USBGEN_EP_NUM,&pUserBuffer,packetRxLength);
			EP1TxBankIsOdd = TRUE;
		}
	}
	else if(!EP1RxBankIsOdd && !USBHandleBusy(EP1RxHandles[EVN]))
	{
		packetRxLength = USBHandleGetLength(EP1RxHandles[EVN]);

		EP1RxHandles[EVN] = USBReadRoll(USBGEN_EP_NUM,&pUserBuffer,USBGEN_EP_SIZE);
		EP1RxBankIsOdd=TRUE;

		if (EP1TxBankIsOdd && !USBHandleBusy(EP1TxHandles[ODD]))
		{
			EP1TxHandles[ODD]	= USBWriteRoll(USBGEN_EP_NUM,&pUserBuffer,packetRxLength);
			EP1TxBankIsOdd = FALSE;
		}
		else if(!EP1TxBankIsOdd && !USBHandleBusy(EP1TxHandles[EVN]))
		{
			EP1TxHandles[EVN]	= USBWriteRoll(USBGEN_EP_NUM,&pUserBuffer,packetRxLength);
			EP1TxBankIsOdd = TRUE;
		}

	}
}

void doBenchmarkRead(void)
{
	if (EP1RxBankIsOdd && USBHandleBusy(EP1RxHandles[ODD])==FALSE)
	{
		EP1RxHandles[ODD] = USBReadRoll(USBGEN_EP_NUM,&pUserBuffer,USBGEN_EP_SIZE);
		EP1RxBankIsOdd=FALSE;
	}
	else if(!EP1RxBankIsOdd && USBHandleBusy(EP1RxHandles[EVN])==FALSE)
	{
		EP1RxHandles[EVN] = USBReadRoll(USBGEN_EP_NUM,&pUserBuffer,USBGEN_EP_SIZE);
		EP1RxBankIsOdd=TRUE;
	}
}

/********************************************************************
 * Function:        USB_HANDLE USBTransferOnePacket(
 *                      BYTE ep, 
 *                      BYTE dir, 
 *                      BYTE* data, 
 *                      BYTE len)
 *
 * PreCondition:    None
 *
 * Input:
 *   BYTE ep - the endpoint the data will be transmitted on
 *   BYTE dir - the direction of the transfer
                This value is either OUT_FROM_HOST or IN_TO_HOST
 *   BYTE* data - pointer to the data to be sent
 *   BYTE len - length of the data needing to be sent
 *
 * Output:          
 *   USB_HANDLE - handle to the transfer
 *
 * Side Effects:    None
 *
 * Overview:        Transfers one packet over the USB
 *
 * Note:            None
 *******************************************************************/
USB_HANDLE USBRollOnePacket(BYTE ep,BYTE dir,BYTE** data,BYTE len)
{
    volatile BDT_ENTRY* handle;
	BYTE* pRollBufferTemp;

    //If the direction is IN
    if(dir != 0)
    {
        //point to the IN BDT of the specified endpoint
        handle = pBDTEntryIn[ep];
    }
    else
    {
        //else point to the OUT BDT of the specified endpoint
        handle = pBDTEntryOut[ep];
    }

    //Toggle the DTS bit if required
    #if (USB_PING_PONG_MODE == USB_PING_PONG__NO_PING_PONG)
        handle->STAT.Val ^= _DTSMASK;
    #elif (USB_PING_PONG_MODE == USB_PING_PONG__EP0_OUT_ONLY)
        if(ep != 0)
        {
            handle->STAT.Val ^= _DTSMASK;
        }
    #endif

    //Set the data pointer, data length, and enable the endpoint
	pRollBufferTemp = handle->ADR;
    handle->ADR = (BYTE*)ConvertToPhysicalAddress(*data);
	*data = pRollBufferTemp;

    handle->CNT = len;
    handle->STAT.Val &= _DTSMASK;
    handle->STAT.Val |= _USIE | _DTSEN;

    //Point to the next buffer for ping pong purposes.
    if(dir != 0)
    {
        //toggle over the to the next buffer for an IN endpoint
        ((BYTE_VAL*)&pBDTEntryIn[ep])->Val ^= USB_NEXT_PING_PONG;
    }
    else
    {
        //toggle over the to the next buffer for an OUT endpoint
        ((BYTE_VAL*)&pBDTEntryOut[ep])->Val ^= USB_NEXT_PING_PONG;
    }
    return (USB_HANDLE)handle;
}

#endif

