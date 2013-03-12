//The following ifedef block represents the standard mode to create macro that simplify 
// the export from a DLL. All file in the DLL is compiled with the TE_USB_FX2_CYAPI_EXPORTS 
// symbol defined in the line of command. This symbol must not be defined in projects 
// that uses this DLL. In this way any other project whose source files will see the functions 
// TE_USB_FX2_CYAPI as imported from a DLL, while the DLL will see the 
// symbols defined with this macro as exported

//#ifdef TE_USB_FX2_CYAPI_EXPORTS
#define TE_USB_FX2_CYAPI extern "C"  __declspec(dllexport)
//#else
//#define TE_USB_FX2_CYAPI_EXPORTS __declspec(dllimport)
//#endif

/*
This is the best option if you really intend to write in ANSI C (not C++).
 For this path, you write your functions as extern "C" returntype __stdcall __declspec(dllexport) func(params) { ... }
 You should also use a "caller-provides-the-buffer" memory model
 */


enum PI_PipeNumber
{
  PI_EP2	= 2,
  PI_EP4	= 4,
  PI_EP6	= 3,
  PI_EP8	= 5
};

//typedef int (WINAPI *_TE_USB_FX2_ScanCards)();
TE_USB_FX2_CYAPI int TE_USB_FX2_ScanCards ();

//typedef int (WINAPI *_TE_USB_FX2_Open)(unsigned int* PHandle, int CardNo);
TE_USB_FX2_CYAPI int TE_USB_FX2_Open (  int CardNumber, unsigned long TimeOut, int DriverBufferSize);

//typedef int (WINAPI *_TE_USB_FX2_Close)(unsigned int* PHandle);
TE_USB_FX2_CYAPI int TE_USB_FX2_Close ();

//typedef int (WINAPI *_TE_USB_FX2_SendCommand)(unsigned int PHandle, byte* cmd, int cmd_len, byte* reply, int* reply_len, int timeout);
TE_USB_FX2_CYAPI int TE_USB_FX2_SendCommand ( byte* Command, long CmdLength, byte* Reply, long ReplyLength, unsigned long Timeout);

int TE_USB_FX2_GetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkInEPx, PI_PipeNumber PipeNo, unsigned long Timeout, int BufferSize);

//typedef int (WINAPI *_TE_USB_FX2_GetData)(unsigned int PHandle, byte* data, int* len, PI_PipeNumber, int timeout);
//TE_USB_FX2_CYAPI int TE_USB_FX2_GetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo, int timeout);
TE_USB_FX2_CYAPI int TE_USB_FX2_GetData ( byte* DataRead, long DataReadLength);

int TE_USB_FX2_SetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkOutEPx, PI_PipeNumber PipeNo, unsigned long Timeout, int BufferSize);

//typedef int (WINAPI *_TE_USB_FX2_SetData)(unsigned int PHandle, byte* data, int len, PI_PipeNumber);
//TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (HANDLE PHandle, byte* data, long len, PI_PipeNumber PipeNo);
TE_USB_FX2_CYAPI int TE_USB_FX2_SetData ( byte* DataWrite, long DataWriteLength);

