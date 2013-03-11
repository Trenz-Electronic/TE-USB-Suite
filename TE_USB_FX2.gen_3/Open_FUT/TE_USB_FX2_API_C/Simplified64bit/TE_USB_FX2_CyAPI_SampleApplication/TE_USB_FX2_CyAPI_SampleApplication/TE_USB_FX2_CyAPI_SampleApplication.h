#pragma once

#pragma comment (lib, "TE_USB_FX2_CyAPI.lib")

#include "TE_USB_FX2_CyAPI.h"

#include "StopWatch.h"

#pragma comment (lib, "CyAPI.lib")

#include "CyAPI.h"


CStopWatch ElapsedTime;
double TheElapsedTime;

#define	TIMEOUT_MS  	1000
#define	MB_I2C_ADRESS	0x3F
#define	I2C_BYTES		12

//#define	TX_PACKET_LEN	10240 //102400
//#define	RX_PACKET_LEN	10240 //102400


enum FX2_Commands
{
  READ_VERSION		= 0x00,
  INITALIZE			= 0xA0,
  READ_STATUS			= 0xA1,
  WRITE_REGISTER		= 0xA2,
  READ_REGISTER		= 0xA3,
  RESET_FIFO_STATUS	= 0xA4,
  FLASH_READ			= 0xA5,
  FLASH_WRITE			= 0xA6,
  FLASH_ERASE			= 0xA7,
  EEPROM_READ			= 0xA8,
  EEPROM_WRITE		= 0xA9,
  GET_FIFO_STATUS		= 0xAC,
  I2C_WRITE			= 0xAD,
  I2C_READ			= 0xAE,
  POWER_ON			= 0xAF,
  FLASH_WRITE_COMMAND	= 0xAA,
  SET_INTERRUPT		= 0xB0,
  GET_INTERRUPT		= 0xB1
};

enum MB_Commands
{
  FX22MB_REG0_NOP			= 0,
  FX22MB_REG0_GETVERSION	= 1,
  FX22MB_REG0_START_TX	= 2,
  FX22MB_REG0_START_RX	= 3,
  FX22MB_REG0_STOP		= 4,
  FX22MB_REG0_PING		= 5
};

/*enum FX2_Parameters
{
  I2C_BYTES = 0x0C,
  MB_I2C_ADRESS = 0x3F

};*/



enum ST_Status
{
  ST_OK = 0,
  ST_ERROR = 1
};

HINSTANCE hInstLibrary;//Dll handle

//functions
bool m_InitDLL;
//_TE_USB_FX2_ScanCards TE_USB_FX2_ScanCards;
//_TE_USB_FX2_Open TE_USB_FX2_Open;
//_TE_USB_FX2_Close TE_USB_FX2_Close;
//_TE_USB_FX2_SendCommand TE_USB_FX2_SendCommand;
//_TE_USB_FX2_GetData TE_USB_FX2_GetData;
//_TE_USB_FX2_SetData TE_USB_FX2_SetData;

bool LoadTE0300DLL()
{
  m_InitDLL = false;

#if defined(_WIN64)
  hInstLibrary = LoadLibrary(_T("TE_USB_FX2_CyAPI.dll"));
#else
  hInstLibrary = LoadLibrary(_T("TE_USB_FX2_CyAPI.dll"));
#endif
  if (!hInstLibrary)
    return false;

  //TE_USB_FX2_ScanCards = (_TE_USB_FX2_ScanCards)GetProcAddress(hInstLibrary, "TE_USB_FX2_ScanCards");
  //TE_USB_FX2_DLLCPPNOTCLR_API int
  FARPROC lpfnGetProcessID1;
  typedef long (__stdcall * pICFUNC1)();
  pICFUNC1 TE_USB_FX2_ScanCards;
  lpfnGetProcessID1 = GetProcAddress(hInstLibrary, "TE_USB_FX2_ScanCards");
  TE_USB_FX2_ScanCards = pICFUNC1(lpfnGetProcessID1);
  if (!TE_USB_FX2_ScanCards)
    return false;

  //TE_USB_FX2_Open = (_TE_USB_FX2_Open)GetProcAddress(hInstLibrary, "TE_USB_FX2_Open");
  FARPROC lpfnGetProcessID2;
  typedef long (__stdcall * pICFUNC2)();
  pICFUNC2 TE_USB_FX2_Open;
  lpfnGetProcessID2 = GetProcAddress(hInstLibrary, "TE_USB_FX2_Open");
  TE_USB_FX2_Open = pICFUNC2(lpfnGetProcessID2);
  if (!TE_USB_FX2_Open)
    return false;

  //TE_USB_FX2_Close = (_TE_USB_FX2_Close)GetProcAddress(hInstLibrary, "TE_USB_FX2_Close");
  //TE_USB_FX2_Close = (TE_USB_FX2_DLLCPPNOTCLR_API int)GetProcAddress(hInstLibrary, "TE_USB_FX2_Close");
  FARPROC lpfnGetProcessID3;
  typedef long (__stdcall * pICFUNC3)();
  pICFUNC3 TE_USB_FX2_Close;
  lpfnGetProcessID3 = GetProcAddress(hInstLibrary, "TE_USB_FX2_Close");
  TE_USB_FX2_Close = pICFUNC3(lpfnGetProcessID3);
  if (!TE_USB_FX2_Close)
    return false;

  //TE_USB_FX2_SendCommand = (_TE_USB_FX2_SendCommand)GetProcAddress(hInstLibrary, "TE_USB_FX2_SendCommand");
  //TE_USB_FX2_SendCommand = (TE_USB_FX2_DLLCPPNOTCLR_API int)GetProcAddress(hInstLibrary, "TE_USB_FX2_SendCommand");
  FARPROC lpfnGetProcessID4;
  typedef long (__stdcall * pICFUNC4)();
  pICFUNC4 TE_USB_FX2_SendCommand;
  lpfnGetProcessID4 = GetProcAddress(hInstLibrary, "TE_USB_FX2_SendCommand");
  TE_USB_FX2_SendCommand = pICFUNC4(lpfnGetProcessID4);
  if (!TE_USB_FX2_SendCommand)
    return false;

  //TE_USB_FX2_GetData = (_TE_USB_FX2_GetData)GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  //TE_USB_FX2_GetData = (TE_USB_FX2_DLLCPPNOTCLR_API int)GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  /*FARPROC lpfnGetProcessID5;
  typedef long (__stdcall * pICFUNC5)();
  pICFUNC5 TE_USB_FX2_GetData_InstanceDriverBuffer;
  lpfnGetProcessID5 = GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData_InstanceDriverBuffer");
  TE_USB_FX2_GetData_InstanceDriverBuffer = pICFUNC5(lpfnGetProcessID5);
  if (!TE_USB_FX2_GetData_InstanceDriverBuffer)
    return false;*/

  //TE_USB_FX2_GetData = (_TE_USB_FX2_GetData)GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  //TE_USB_FX2_GetData = (TE_USB_FX2_DLLCPPNOTCLR_API int)GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  FARPROC lpfnGetProcessID6;
  typedef long (__stdcall * pICFUNC6)();
  pICFUNC6 TE_USB_FX2_GetData;
  lpfnGetProcessID6 = GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  TE_USB_FX2_GetData = pICFUNC6(lpfnGetProcessID6);
  if (!TE_USB_FX2_GetData)
    return false;

  //TE_USB_FX2_SetData = (_TE_USB_FX2_GetData)GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  //TE_USB_FX2_SetData = (TE_USB_FX2_DLLCPPNOTCLR_API int)GetProcAddress(hInstLibrary, "TE_USB_FX2_GetData");
  /*FARPROC lpfnGetProcessID7;
  typedef long (__stdcall * pICFUNC7)();
  pICFUNC7 TE_USB_FX2_SetData_InstanceDriverBuffer;
  lpfnGetProcessID7 = GetProcAddress(hInstLibrary, "TE_USB_FX2_SetData_InstanceDriverBuffer");
  TE_USB_FX2_SetData_InstanceDriverBuffer = pICFUNC7(lpfnGetProcessID7);
  if (!TE_USB_FX2_GetData_InstanceDriverBuffer)
    return false;*/

  //TE_USB_FX2_SetData = (_TE_USB_FX2_SetData)GetProcAddress(hInstLibrary, "TE_USB_FX2_SetData");
  //TE_USB_FX2_SetData = (TE_USB_FX2_DLLCPPNOTCLR_API int)GetProcAddress(hInstLibrary, "TE_USB_FX2_SetData");
  FARPROC lpfnGetProcessID8;
  typedef long (__stdcall * pICFUNC8)();
  pICFUNC8 TE_USB_FX2_SetData;
  lpfnGetProcessID8 = GetProcAddress(hInstLibrary, "TE_USB_FX2_SetData");
  TE_USB_FX2_SetData = pICFUNC8(lpfnGetProcessID8);
  if (!TE_USB_FX2_SetData)
    return false;

  m_InitDLL = true;

  return true;
}

bool CloseTE0300DLL()
{
  m_InitDLL = false;
  return FreeLibrary(hInstLibrary) == 1;
}
