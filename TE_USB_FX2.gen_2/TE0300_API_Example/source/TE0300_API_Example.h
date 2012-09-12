/*
Copyright (C) 2012 Trenz Electronic

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included 
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
IN THE SOFTWARE.
*/
#include <iostream>
#include <windows.h>
#include "StopWatch.h"

CStopWatch ElapsedTime;
double TheElapsedTime;

#define	TIMEOUT_MS  	1000
#define	MB_I2C_ADRESS	0x3F
#define	I2C_BYTES		12

#define	TX_PACKET_LEN	102400
#define	RX_PACKET_LEN	102400


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

enum PI_PipeNumber
{
	PI_EP2	= 2,	
	PI_EP4	= 4,	
	PI_EP6	= 3,
	PI_EP8	= 5
};

enum ST_Status
{
	ST_OK = 0,
	ST_ERROR = 1
};


typedef int (WINAPI *_TE0300_ScanCards)();
typedef int (WINAPI *_TE0300_Open)(unsigned int* PHandle, int CardNo);
typedef int (WINAPI *_TE0300_Close)(unsigned int*);
typedef int (WINAPI *_TE0300_SendCommand)(unsigned int, byte*, int, byte*, int*, int);
typedef int (WINAPI *_TE0300_GetData)(unsigned int, byte*, int*, PI_PipeNumber, int);
typedef int (WINAPI *_TE0300_SetData)(unsigned int, byte*, int, PI_PipeNumber);


HINSTANCE hInstLibrary;//Dll handle

//functions
bool m_InitDLL;
_TE0300_ScanCards TE0300_ScanCards;
_TE0300_Open TE0300_Open;
_TE0300_Close TE0300_Close;
_TE0300_SendCommand TE0300_SendCommand;
_TE0300_GetData TE0300_GetData;
_TE0300_SetData TE0300_SetData;

bool LoadTE0300DLL()
{
	m_InitDLL = false;

	hInstLibrary = LoadLibrary(_T("TE0300DLL.dll"));
	if (!hInstLibrary)
		return false;
	TE0300_ScanCards = (_TE0300_ScanCards)GetProcAddress(hInstLibrary, "TE0300_ScanCards");
	if (!TE0300_ScanCards)
		return false;
	TE0300_Open = (_TE0300_Open)GetProcAddress(hInstLibrary, "TE0300_Open");
	if (!TE0300_Open)
		return false;
	TE0300_Close = (_TE0300_Close)GetProcAddress(hInstLibrary, "TE0300_Close");
	if (!TE0300_Close)
		return false;
	TE0300_SendCommand = (_TE0300_SendCommand)GetProcAddress(hInstLibrary, "TE0300_SendCommand");
	if (!TE0300_SendCommand)
		return false;
	TE0300_GetData = (_TE0300_GetData)GetProcAddress(hInstLibrary, "TE0300_GetData");
	if (!TE0300_GetData)
		return false;		
	TE0300_SetData = (_TE0300_SetData)GetProcAddress(hInstLibrary, "TE0300_SetData");
	if (!TE0300_SetData)
		return false;	
	m_InitDLL = true;

	return true;
}

bool CloseTE0300DLL()
{
	m_InitDLL = false;
	return FreeLibrary(hInstLibrary) == 1;
}
