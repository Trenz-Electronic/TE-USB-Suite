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
/*
-- Company: Trenz Electronics GmbH
-- Engineer: Oleksandr Kiyenko (a.kienko@gmail.com)
*/

#ifndef __TE_API_H__
#define __TE_API_H__

#include "fx2regs.h"

//-----------------------------------------------------------------------------
// Version Definition
#define fx2_ver_maj_ 	3
#define fx2_ver_min_	2
#define fx2_tip_maj_ 	1
#define fx2_tip_min_	1
//-----------------------------------------------------------------------------
// Commands definition
#define	EP1DATA_COUNT				0x40
#define CMD_READ_VERSION			0x00
#define CMD_SWITCH_MODE				0xA0
#define	CMD_READ_STATUS				0xA1
#define CMD_WRITE_REGISTER			0xA2
#define CMD_READ_REGISTER			0xA3
#define	CMD_RESET_FIFO_STATUS		0xA4
#define CMD_FLASH_READ				0xA5
#define CMD_FLASH_WRITE				0xA6
#define CMD_FLASH_ERASE				0xA7
#define CMD_SECTOR_ERASE			0xF7
#define CMD_EEPROM_READ				0xA8
#define CMD_EEPROM_WRITE			0xA9
#define CMD_FLASH_WRITE_COMMAND		0xAA
#define CMD_DEV_LOCK				0xBB
#define CMD_START_TEST				0xBD
#define CMD_I2C_WRITE_READ			0xAB
#define CMD_GET_FIFO_STATUS			0xAC
#define CMD_I2C_WRITE				0xAD
#define CMD_I2C_READ				0xAE
#define CMD_FPGA_POWER				0xAF
#define CMD_SET_AUTORESPONSE		0xB0
#define CMD_GET_AUTORESPONSE		0xB1
#define CMD_FPGA_RESET				0xB2
//-----------------------------------------------------------------------------
// Global mode
#define	MODE_COM				0
#define	MODE_CLI        		1
#define	MODE_EEPROM_WRITE  		2
#define MODE_FLASH_WRITE		3
//-----------------------------------------------------------------------------
#define	min(a,b) (((a)<(b))?(a):(b))
#define	max(a,b) (((a)>(b))?(a):(b))
//-----------------------------------------------------------------------------
// Variables declarations
extern BYTE mode;
extern WORD eeprom_addr;
extern BYTE flash_addr_h, flash_addr_m, flash_addr_l;
//-----------------------------------------------------------------------------
// Functions declarations
void int_pin_pool(void);
void ep1_pool(void);
void activity(void);
//-----------------------------------------------------------------------------
void process_cli(void);
void process_eeprom_write(void);
void process_flash_write(void);
//-----------------------------------------------------------------------------
#endif
