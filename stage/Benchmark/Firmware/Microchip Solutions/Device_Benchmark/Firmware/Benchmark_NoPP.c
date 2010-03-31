

/** INCLUDES *******************************************************/
#include "USB/usb.h"
#include "USB/usb_function_generic.h"
#include "HardwareProfile.h"
#include "Benchmark.h"
#include "PicFWCommands.h"

//#include "Compiler.h"
//#include "HardwareProfile.h"
//#include "GenericTypeDefs.h"
//#include "USB/usb_device.h"
//#include "USB/usb.h"
//#include "USB/usb_function_generic.h"
//#include "usb_config.h"

#if (USB_PING_PONG_MODE==USB_PING_PONG__NO_PING_PONG) || (USB_PING_PONG_MODE==USB_PING_PONG__EP0_OUT_ONLY)

#if BENCH_MARK_BUFFER_COUNT != 3
	#error "The BENCH_MARK_BUFFER_COUNT_define must be equal to 3"
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
    
		#if (BENCH_MARK_BUFFER_COUNT > 3)
			#if (USBGEN_EP_SIZE != 8) && (USBGEN_EP_SIZE != 16) && (USBGEN_EP_SIZE != 32)
				#error "This pic only has 256 bytes of usb memory; change the USBGEN_EP_SIZE_define to 8, 16, or 32 in usb_config.h or disable ping pong buffering."
			#endif
		#endif
		#pragma udata USB_VARIABLES=0x240

	#elif defined(__18F2455) || defined(__18F2550) || defined(__18F4455) || defined(__18F4550) || defined(__18F2458) || defined(__18F2453) || defined(__18F4558) || defined(__18F4553)

    	#pragma udata USB_VARIABLES=0x440

	#elif defined(__18F4450) || defined(__18F2450)

		#if (BENCH_MARK_BUFFER_COUNT > 3)
		#if ((USBGEN_EP_SIZE != 8) && (USBGEN_EP_SIZE != 16) && (USBGEN_EP_SIZE != 32))
				#error "This pic only has 256 bytes of usb memory; change the USBGEN_EP_SIZE_define to 8, 16, or 32 in usb_config.h or disable ping pong buffering."
		#endif
		#endif
    	#pragma udata USB_VARIABLES=0x440

	#else
    	#pragma udata
	#endif

#endif

USB_VOLATILE BYTE BenchmarkBuffers[BENCH_MARK_BUFFER_COUNT][USBGEN_EP_SIZE];	//User buffer for receiving OUT packets sent from the host

//The below variables are only accessed by the CPU and can be placed anywhere in RAM.
#pragma udata

USB_HANDLE EP1RxHandle;	// OUT handle
USB_HANDLE EP1TxHandle; // IN  handle

BYTE PicFW_TestType;
BYTE PicFW_PrevTestType;
BYTE counter;
BYTE fillCount;
BYTE mWriteTestPID;

USB_VOLATILE BYTE* pUserBuffer;

/** EXTERNS ********************************************************/
extern void BlinkUSBStatus(void);

/** BMARK DEFINES **************************************************/
void doBenchmarkLoop(void);
void doBenchmarkWrite(void);
void doBenchmarkRead(void);
void doBenchmarkReadForLoop(void);
void fillBuffer(BYTE* pBuffer);

/** BMARK MACROS ****************************************************/
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

    USBEnableEndpoint(USBGEN_EP_NUM,USB_OUT_ENABLED|USB_IN_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);

	//Prepare the OUT endpoints to receive the first packets from the host.
	preLoadedBufferCount = 0;
	EP1RxHandle = USBGenRead (USBGEN_EP_NUM, (BYTE*)BenchmarkBuffers[preLoadedBufferCount++],USBGEN_EP_SIZE);	
	EP1TxHandle = USBGenWrite(USBGEN_EP_NUM, (BYTE*)BenchmarkBuffers[preLoadedBufferCount++],0);	

	pUserBuffer=BenchmarkBuffers[preLoadedBufferCount++];
}

void USBCBCheckOtherReq(void)
{
	if (SetupPkt.RequestType != USB_SETUP_TYPE_VENDOR_BITFIELD) return;

	switch (SetupPkt.bRequest)
	{
	case PICFW_SET_TEST:
		PicFW_TestType=SetupPkt.wValue & 0xff;
		inPipes[0].pSrc.bRam = (BYTE*)&PicFW_TestType;  // Set Source
		inPipes[0].info.bits.ctrl_trf_mem = USB_EP0_RAM;		// Set memory type
		inPipes[0].wCount.v[0] = 1;						// Set data count
		inPipes[0].info.bits.busy = 1;
		break;
	case PICFW_GET_TEST:
		inPipes[0].pSrc.bRam = (BYTE*)&PicFW_TestType;  // Set Source
		inPipes[0].info.bits.ctrl_trf_mem = USB_EP0_RAM;		// Set memory type
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
	EP1RxHandle = 0;	
	EP1TxHandle = 0;	

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
	if (fillCount < 2)							\
	{											\
		fillCount++;							\
		fillBuffer((BYTE*)buffer);				\
	}											\
	buffer[1]=mWriteTestPID;					\
	mWriteTestPID++;							\
}

void doBenchmarkWrite(void)
{
	BYTE* pBufferTemp;

	if (USBHandleBusy(EP1TxHandle)==FALSE)
	{
		mSetWritePacketID(pUserBuffer);
		pBufferTemp = USBHandleGetAddr(EP1TxHandle);
		EP1TxHandle = USBGenWrite(USBGEN_EP_NUM,(BYTE*)pUserBuffer,USBGEN_EP_SIZE);
		pUserBuffer = (USB_VOLATILE BYTE*)pBufferTemp;
	}
}

void doBenchmarkLoop(void)
{
	BYTE packetRxLength;
	BYTE* pBufferTemp;

	if (USBHandleBusy(EP1TxHandle)) return;

	if (!USBHandleBusy(EP1RxHandle))
	{
		packetRxLength = USBHandleGetLength(EP1RxHandle);

		pBufferTemp = USBHandleGetAddr(EP1RxHandle);
		EP1RxHandle = USBGenRead(USBGEN_EP_NUM,(BYTE*)pUserBuffer,USBGEN_EP_SIZE);
		pUserBuffer = (USB_VOLATILE BYTE*)pBufferTemp;

		pBufferTemp = USBHandleGetAddr(EP1TxHandle);
		EP1TxHandle	= USBGenWrite(USBGEN_EP_NUM,(BYTE*)pUserBuffer,packetRxLength);
		pUserBuffer = (USB_VOLATILE BYTE*)pBufferTemp;
	}
}

void doBenchmarkRead(void)
{
	BYTE* pBufferTemp;

	if (USBHandleBusy(EP1RxHandle)==FALSE)
	{
		pBufferTemp = USBHandleGetAddr(EP1RxHandle);
		EP1RxHandle = USBGenRead(USBGEN_EP_NUM,(BYTE*)pUserBuffer,USBGEN_EP_SIZE);
		pUserBuffer = (USB_VOLATILE BYTE*)pBufferTemp;
	}
}


#endif

