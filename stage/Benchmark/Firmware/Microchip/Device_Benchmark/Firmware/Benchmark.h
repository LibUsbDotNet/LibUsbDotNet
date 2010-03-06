#ifndef _BENCHMARK_H
#define _BENCHMARK_H

/** INCLUDES *******************************************************/
#include "Compiler.h"
#include "HardwareProfile.h"
#include "GenericTypeDefs.h"
#include "USB/usb_device.h"

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
#define BENCH_MARK_BUFFER_COUNT	5
#define	EVN	0
#define	ODD	1
/** BMARK EXTERNS **************************************************/
extern BYTE PicFW_TestType;

/** BMARK CALLBACKS ************************************************/
void USBCBCheckOtherReq(void);
void USBCBInitEP(void);

/** BMARK DEFINES **************************************************/
void Benchmark_ProcessIO(void);
void Benchmark_Init(void);

#endif

