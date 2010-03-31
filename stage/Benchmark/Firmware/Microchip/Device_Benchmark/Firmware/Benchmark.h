#ifndef _BENCHMARK_H
#define _BENCHMARK_H

/** INCLUDES *******************************************************/
#include "USB/usb.h"
#include "USB/usb_function_generic.h"
#include "HardwareProfile.h"

// These are vendor specific commands
// See the PICFW_COMMANDS for details
// on how this is implemented.
enum TestType
{
    TEST_NONE,
    TEST_PCREAD,
    TEST_PCWRITE,
    TEST_LOOP
};

typedef union _BENCHMARK_BUFFER_HANDLE
{
	struct
	{
		BYTE*  Buffer;
		BYTE   Count;
	};
}BENCHMARK_BUFFER_HANDLE;

/** BMARK DEFINITIONS **********************************************/
#if (USB_PING_PONG_MODE==USB_PING_PONG__FULL_PING_PONG) || (USB_PING_PONG_MODE==USB_PING_PONG__ALL_BUT_EP0)
	#define BENCH_MARK_BUFFER_COUNT	5
	#define	EVN	0
	#define	ODD	1
#else
	#define BENCH_MARK_BUFFER_COUNT	3
#endif

/** BMARK EXTERNS **************************************************/
extern BYTE PicFW_TestType;

/** BMARK CALLBACKS ************************************************/
void USBCBCheckOtherReq(void);
void USBCBInitEP(void);

/** USB FW EXTERNS DEFINES *****************************************/
extern volatile CTRL_TRF_SETUP SetupPkt;

/** BMARK DEFINES **************************************************/
void Benchmark_ProcessIO(void);
void Benchmark_Init(void);


#endif

