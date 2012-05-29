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
#include "eeprom.h"
#include "fx2regs.h"

void EEPROMWrite(WORD addr, BYTE length, BYTE xdata *buf){
    BYTE	i;
    while (I2CS & bmSTOP);  // wait for transfer end
    I2CS |= bmSTART;
    EEsendData(0xA2);		// send I2C address b'10100010
    EEsendData(MSB(addr));	// send message to EEPROM
    EEsendData(LSB(addr));

    if (length > 32) 
        length = 32;	        // send only 32 bytes
    for(i=0; i<length; i++) {	// send data
        EEsendData(buf[i]);
    }
    I2CS |= bmSTOP;				// send stop

    // data processing, wait for EEPROM write
    while (I2CS & bmSTOP);      // wait for transfer end
    //ack pooling:
    do {
        I2CS |= bmSTART;
        EEsendData(0xA2);
    } while(!(I2CS & bmACK));
}

void EEsendData(BYTE DATA) {
    I2DAT = DATA;
    while (!(I2CS & bmDONE));	// wait fot done =1
}

BYTE EEreadData(void) {
    BYTE DATA;
    DATA = I2DAT;
    while (!(I2CS & bmDONE));	// wait for done =1
    return DATA;
}

void EEPROMRead(WORD addr, BYTE length, BYTE xdata *buf)
{
    BYTE		i = 0;
    BYTE		j = 0;

    if (length < 1) 
        return;
    // set read address
    while (I2CS & bmSTOP);      // wait for transfer ends
    I2CS |= bmSTART;
    EEsendData(0xA2);		    // send I2C address
    EEsendData(MSB(addr));		// send message to EEPROM
    EEsendData(LSB(addr));

    I2CS |= bmSTART;		    // set start bit
    EEsendData(0xA3);		    // send I2C address

    j = EEreadData();           // start read transfer

    if (length > 31) 
        length = 31;	        // send only 32 bytes
    for (i=0; i<(length-1); i++) {
        buf[i] = EEreadData();
    }
    // process last byte
    I2CS |= bmLASTRD;
    buf[i] = EEreadData();
    I2CS |= bmSTOP;				// send stop
}

