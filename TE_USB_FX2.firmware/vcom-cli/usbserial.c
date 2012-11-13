/*-----------------------------------------------------------------------------
 * Main firmware file. FX2 act as USB Serial interface with several modes.
 *-----------------------------------------------------------------------------
 * Copyright (C) 2012 Trenz Electronic
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE.
*/
/*
-- Company: Trenz Electronics GmbH
-- Engineer: Oleksandr Kiyenko (a.kienko@gmail.com)
*/
 
#include "isr.h"
#include "timer.h"
#include "delay.h"
#include "fx2regs.h"
#include "fx2utils.h"
#include "usb_common.h"
#include "usb_descriptors.h"
#include "usb_requests.h"
#include "syncdelay.h"
#include "eeprom.h"
#include "te_vcom.h"
#include "fpga.h"

//-----------------------------------------------------------------------------
void usb_serial_init(void)              // Called once at startup
{
    CPUCS = ((CPUCS & ~bmCLKSPD) | bmCLKSPD1); // set the CPU clock to 48MHz
    IFCONFIG = 0xE3; SYNCDELAY;     // Enable slave FIFO Interface @48MHz
    REVCTL = 0x03; SYNCDELAY;
    // Endpoints configuration
    EP1INCFG = 0xB0; SYNCDELAY;     // Configure EP1IN as INT IN EP
	EP4CFG = 0x20; SYNCDELAY;		// Disabled
	EP8CFG = 0x20; SYNCDELAY;		// Disabled
	EP2CFG = 0xA0; SYNCDELAY;       // EP2 is DIR=OUT, TYPE=BULK, SIZE=512x4
	EP6CFG = 0xE0; SYNCDELAY;       // EP6 is DIR=IN, TYPE=BULK, SIZE=512x4
	// FIFO Reset
    FIFORESET = 0x80;  SYNCDELAY;   // "NAK-All" requests from host.
    FIFORESET = 0x82;  SYNCDELAY;   // Reset EP2
    FIFORESET = 0x86;  SYNCDELAY;   // Reset EP6
    FIFORESET = 0x00;  SYNCDELAY;   // Resume normal operation.
	// Arm buffers
	EP1OUTBC = 0x04; SYNCDELAY;		// Arm EP1 buffer
    // Arm EP2 buffers
    OUTPKTEND = 0x82; SYNCDELAY;    // Arm First buffer
    OUTPKTEND = 0x82; SYNCDELAY;    // Arm Second buffer
    OUTPKTEND = 0x82; SYNCDELAY;    // Arm Third buffer
    OUTPKTEND = 0x82; SYNCDELAY;    // Arm Fourth buffer
    // Configure flags
    EP6FIFOPFH = 0x01; SYNCDELAY;   // programmable flag
    EP6FIFOPFL = 0x80; SYNCDELAY;   // less than 128 bytes to write
    EP2FIFOPFH = 0x00; SYNCDELAY;   // programmable flag
    EP2FIFOPFL = 0x80; SYNCDELAY;   // more than 128 bytes to read
    PINFLAGSAB = 0xE8; SYNCDELAY;	// FlagB = EP6FF,   FlagA = EP2EF
    PINFLAGSCD = 0x46; SYNCDELAY;	// FlagD = EP2PF,	FlagC = EP6PF
    // Fifo Config
    EP4FIFOCFG = 0x00; SYNCDELAY;	// Disable Wordwide bit
    EP8FIFOCFG = 0x00; SYNCDELAY;   // Disable Wordwide bit
	EP2FIFOCFG = 0x10; SYNCDELAY;   // Configure EP2 FIFO in 8-bit AutoOut mode
    EP6FIFOCFG = 0x0C; SYNCDELAY;   // Configure EP6 FIFO in 8-bit AutoIn mode
    // Other settings
    FIFOPINPOLAR = 0x3F; SYNCDELAY;  // slave FIFO interface pins as active high
    EP6AUTOINLENH = 0x02; SYNCDELAY; // auto commit data in 512-byte chunks
    EP6AUTOINLENL = 0x00; SYNCDELAY;
	IOD = 0xFF;			// Enable PS_ON and disable PROG_B 
	OED = 0x73;			// Configure MOSI, CCLK, CSO_B, PS_ON, PROG as outputs
}

//-----------------------------------------------------------------------------
void send_ssn(void){
	if (!(EP1INCS & 0x02)){	// check if EP1IN is available
		EP1INBUF[0] = 0x0A; // if it is available, then fill the first 
		EP1INBUF[1] = 0x20; // 10 bytes of the buffer with 
		EP1INBUF[2] = 0x00;	// appropriate data. 
		EP1INBUF[3] = 0x00;
		EP1INBUF[4] = 0x00;
		EP1INBUF[5] = 0x00;
		EP1INBUF[6] = 0x00;
		EP1INBUF[7] = 0x02;
		EP1INBUF[8] = 0x00;
		EP1INBUF[9] = 0x00;
		EP1INBC = 10;       // manually commit once the buffer is filled
	}
}

//-----------------------------------------------------------------------------
void activity(void){ // Called repeatedly while the device is idle
	send_ssn();
//	process_line_coding();
	switch(mode){
		case MODE_COM:
			//nothing to do here
		break;
		case MODE_CLI:
			process_cli();
		break;
		case MODE_EEPROM_WRITE:
			process_eeprom_write();
		break;
		case MODE_FLASH_WRITE:
			process_flash_write();
		break;
	}
}

//-----------------------------------------------------------------------------
void main(void){
	EA = 0; 				// disable all interrupts
	setup_autovectors ();
	usb_install_handlers ();
	EA = 1; 				// enable interrupts
	fx2_renumerate(); 		// simulates disconnect / reconnect
	usb_serial_init();
	while(1){
		if(usb_setup_packet_avail()) usb_handle_setup_packet();
			activity();
	}
}
