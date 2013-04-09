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
#ifndef __I2C_H__
#define __I2C_H__

#include "fx2regs.h"

#define MSB(x)	(((WORD) x) >> 8)
#define LSB(x)	(((WORD) x) & 0xff)

void EEPROMWriteByte(WORD addr, BYTE value);
void EEPROMWrite(WORD addr, BYTE length, BYTE __xdata *buf);
void EEPROMRead(WORD addr, BYTE length, BYTE __xdata *buf);
void EEsendData(BYTE DATA);
BYTE EEreadData(void);
void I2CWrite(BYTE addr, BYTE length, BYTE __xdata *buf);
void I2CRead(BYTE addr, BYTE length, BYTE __xdata *buf);
void I2CRead2(BYTE addr, BYTE length, BYTE *buf);

#endif