//The following block ifdef represents the standard mode to create macro that simplify 
// the export from a DLL. All file in the DLL is compiled with the TE_USB_FX2_DLLCPPNOTCLR_EXPORTS 
// symbol defined in the line of command. This symbol must not be definite in some it plans 
// that uses this DLL. In this way any other project whose file of add-in origin this file will see the functions 
// TE_USB_FX2_CYAPI as imported from a DLL, while the DLL will see the 
// symbols defined with this macro as exported

#pragma once

#include <WinDef.h>

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

// Questa classe è esportata da TE_USB_FX2_DLLcppNotCLR.dll
//class TE_USB_FX2_CYAPI CTE_USB_FX2_DLLcppNotCLR {
//public:
//	CTE_USB_FX2_DLLcppNotCLR(void);
// TODO: aggiungere qui i metodi.
//typedef int (WINAPI *_TE_USB_FX2_ScanCards)();
//static int TE_USB_FX2_ScanCards ();

//typedef int (WINAPI *_TE_USB_FX2_Open)(unsigned int* PHandle, int CardNo);
//static int TE_USB_FX2_Open (HANDLE PHandle, int CardNo);

//typedef int (WINAPI *_TE_USB_FX2_Close)(unsigned int* PHandle);
//static int TE_USB_FX2_Close (HANDLE PHandle);

//typedef int (WINAPI *_TE_USB_FX2_SendCommand)(unsigned int PHandle, byte* cmd, int cmd_len, byte* reply, int* reply_len, int timeout);
//static int TE_USB_FX2_SendCommand (HANDLE PHandle, byte* cmd, long cmd_len, byte* reply, long reply_len, int timeout);

//typedef int (WINAPI *_TE_USB_FX2_GetData)(unsigned int PHandle, byte* data, int* len, PI_PipeNumber, int timeout);
//static int TE_USB_FX2_GetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo, int timeout);

//typedef int (WINAPI *_TE_USB_FX2_SetData)(unsigned int PHandle, byte* data, int len, PI_PipeNumber);
//static int TE_USB_FX2_SetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo);
//};

//extern TE_USB_FX2_CYAPI int nTE_USB_FX2_DLLcppNotCLR;

//TE_USB_FX2_CYAPI int fnTE_USB_FX2_DLLcppNotCLR(void);

//typedef int (WINAPI *_TE_USB_FX2_ScanCards)();
TE_USB_FX2_CYAPI int TE_USB_FX2_ScanCards (CCyUSBDevice *USBDeviceList);

//typedef int (WINAPI *_TE_USB_FX2_Open)(unsigned int* PHandle, int CardNo);
TE_USB_FX2_CYAPI int TE_USB_FX2_Open (CCyUSBDevice *USBDeviceList, int CardNo);

//typedef int (WINAPI *_TE_USB_FX2_Close)(unsigned int* PHandle);
TE_USB_FX2_CYAPI int TE_USB_FX2_Close (CCyUSBDevice *USBDeviceList);

//typedef int (WINAPI *_TE_USB_FX2_SendCommand)(unsigned int PHandle, byte* cmd, int cmd_len, byte* reply, int* reply_len, int timeout);
TE_USB_FX2_CYAPI int TE_USB_FX2_SendCommand (CCyUSBDevice *USBDeviceList, byte* Command, long CmdLength, byte* Reply, long ReplyLength, unsigned long Timeout);

TE_USB_FX2_CYAPI int TE_USB_FX2_GetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkInEP, PI_PipeNumber PipeNo, unsigned long Timeout, int BufferSize);

//typedef int (WINAPI *_TE_USB_FX2_GetData)(unsigned int PHandle, byte* data, int* len, PI_PipeNumber, int timeout);
//TE_USB_FX2_CYAPI int TE_USB_FX2_GetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo, int timeout);
TE_USB_FX2_CYAPI int TE_USB_FX2_GetData (CCyBulkEndPoint **BulkInEP, byte* DataRead, long DataReadLength);

TE_USB_FX2_CYAPI int TE_USB_FX2_SetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkOutEP, PI_PipeNumber PipeNo, unsigned long Timeout, int BufferSize);

//typedef int (WINAPI *_TE_USB_FX2_SetData)(unsigned int PHandle, byte* data, int len, PI_PipeNumber);
//TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo);
TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (CCyBulkEndPoint **BulkOutEP, byte* DataWrite, long DataWriteLength);

