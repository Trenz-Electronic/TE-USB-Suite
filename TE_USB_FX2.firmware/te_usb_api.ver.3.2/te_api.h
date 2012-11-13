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
void activity(void);
void process_line_coding(void);
void process_cli(void);
void process_eeprom_write(void);
void process_flash_write(void);
//-----------------------------------------------------------------------------
#endif
