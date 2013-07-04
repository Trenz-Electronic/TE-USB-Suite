//The following block ifdef represents the standard mode to create macro that simplify 
// the export from a DLL. All file in the DLL is compiled with the TE_USB_FX2_CYAPI_EXPORTS 
// symbol defined in the line of command. This symbol must not be definite in some it plans 
// that uses this DLL. In this way any other project whose file of add-in origin this file will see the functions 
// TE_USB_FX2_CYAPI as imported from a DLL, while the DLL will see the 
// symbols defined with this macro as exported

#pragma once

//#include <WinDef.h> NO, it fails at 32 bit
#include <windows.h>

#include "CyAPI.h"

typedef unsigned char byte;

//#include "stdafx.h"

#define TE_USB_FX2_CYAPI extern "C" __declspec(dllexport)
//#else
//#define TE_USB_FX2_CYAPI_EXPORTS __declspec(dllimport)
//#endif

enum PI_PipeNumber
{
  PI_EP2	= 2,
  PI_EP4	= 4,
  PI_EP6	= 3,
  PI_EP8	= 5
};

//typedef int (WINAPI *_TE_USB_FX2_ScanCards)();
TE_USB_FX2_CYAPI int TE_USB_FX2_ScanCards (CCyUSBDevice *USBdevList);

//typedef int (WINAPI *_TE_USB_FX2_Open)(unsigned int* PHandle, int CardNo);
TE_USB_FX2_CYAPI int TE_USB_FX2_Open (CCyUSBDevice *USBdevList, int CardNo);

//typedef int (WINAPI *_TE_USB_FX2_Close)(unsigned int* PHandle);
TE_USB_FX2_CYAPI int TE_USB_FX2_Close (CCyUSBDevice *USBdevList);

//typedef int (WINAPI *_TE_USB_FX2_SendCommand)(unsigned int PHandle, byte* cmd, int cmd_len, byte* reply, int* reply_len, int timeout);
TE_USB_FX2_CYAPI int TE_USB_FX2_SendCommand (CCyUSBDevice *USBdevList, byte* Command, long CmdLength, byte* Reply, long ReplyLength, unsigned long Timeout);

TE_USB_FX2_CYAPI int TE_USB_FX2_GetData_InstanceDriverBuffer (CCyUSBDevice *USBdevList, CCyBulkEndPoint **BulkInEP, PI_PipeNumber PipeNo, unsigned long Timeout, int BufferSize);

//typedef int (WINAPI *_TE_USB_FX2_GetData)(unsigned int PHandle, byte* data, int* len, PI_PipeNumber, int timeout);
//TE_USB_FX2_CYAPI int TE_USB_FX2_GetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo, int timeout);
TE_USB_FX2_CYAPI int TE_USB_FX2_GetData (CCyBulkEndPoint **BulkInEP, byte* DataRead, long DataReadLength);

TE_USB_FX2_CYAPI int TE_USB_FX2_SetData_InstanceDriverBuffer (CCyUSBDevice *USBdevList, CCyBulkEndPoint **BulkOutEP, PI_PipeNumber PipeNo, unsigned long Timeout, int BufferSize);

//typedef int (WINAPI *_TE_USB_FX2_SetData)(unsigned int PHandle, byte* data, int len, PI_PipeNumber);
//TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo);
TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (CCyBulkEndPoint **BulkOutEP, byte* DataWrite, long DataWriteLength);

