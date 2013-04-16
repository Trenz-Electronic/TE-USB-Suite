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

#ifndef __SPI_H__
#define __SPI_H__

#include "fx2regs.h"

//-----------------------------------------------------------------------------
// SPI Flash commands
#define SPI_READ		0x03
#define	SPI_WREN		0x06
#define	SPI_WRDI		0x04
#define	SPI_RDSR1		0x05
#define	SPI_RDSR2		0x35
#define	SPI_WRSR		0x01
#define	SPI_READ		0x03
#define	SPI_FAST_READ	0x0b
#define	SPI_WRITE		0x02
#define	SPI_SE			0xD8
#define	SPI_BE			0xC7
#define	SPI_DP			0xb9
#define	SPI_RES			0xab
#define SPI_RDID		0x9F

//-----------------------------------------------------------------------------
// SPI pins
#define SPI_Q	IOD7
#define	SPI_D	IOD6
#define	SPI_CS	IOD4
#define	SPI_CLK	IOD5
//-----------------------------------------------------------------------------
//#define FLASH_DISABLE SPI_CS = 1; SYNCDELAY; SPI_D = 0; SYNCDELAY; SPI_CLK = 0
//#define FLASH_ENABLE  SPI_CLK = 0; SYNCDELAY; SPI_CS = 0
#define FLASH_ENABLE  SPI_D = 0; SYNCDELAY; SPI_CLK = 0; SYNCDELAY; SPI_CS = 0
#define FLASH_DISABLE SPI_CS = 1; SYNCDELAY; SPI_D = 0; SYNCDELAY; SPI_CLK = 0

//sbit 	SPI_Q   = IOD ^ 7;			//PD7
//sbit 	SPI_D   = IOD ^ 6;			//PD6
//sbit 	SPI_CS  = IOD ^ 4;			//PD4
//sbit 	SPI_CLK = IOD ^ 5;			//PD5
//-----------------------------------------------------------------------------
void putcSPI(unsigned char data_wr);
unsigned char getcSPI(void);
void page_write (BYTE addhighest, BYTE addhigh, BYTE addlow, 
	unsigned char *wrptr, WORD p_write_size);
void write_enable (void);
void busy_polling (void);
void sector_erase (unsigned char sector);
//-----------------------------------------------------------------------------
#endif
