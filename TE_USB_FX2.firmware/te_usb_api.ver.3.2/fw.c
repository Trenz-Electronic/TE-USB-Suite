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
#include "te_api.h"
#include "fpga.h"

//-----------------------------------------------------------------------------
void system_init(void)              // Called once at startup
{
	EP1OUTCFG	= 0xA0;
	EP1INCFG	= 0xA0;
	CPUCS = 0x12;  		// 48MHz, output to CLKOUT signal enabled.
	REVCTL = 0x03;	 	SYNCDELAY;  // See TRM...
	IFCONFIG = 0xE0;	SYNCDELAY; 
	IFCONFIG = 0xE3;	SYNCDELAY;		
	EP2CFG = 0xE2; 		SYNCDELAY;	// EP2 IN,  bulk, size 512x2
  	EP4CFG = 0xE0; 		SYNCDELAY;	// EP4 IN,  bulk, size 512x2
  	EP6CFG = 0xE2; 		SYNCDELAY;	// EP6 IN,  bulk, size 512x2
  	EP8CFG = 0xA0; 		SYNCDELAY;	// EP8 OUT, bulk, size 512x2
	// Resetiraj FIFO-te	 
	FIFORESET = 0x80;	SYNCDELAY;  // NAK all requests from host.
	FIFORESET = 0x02;	SYNCDELAY;  // Reset individual EP (2,4,6,8)
	FIFORESET = 0x04;	SYNCDELAY;
	FIFORESET = 0x06;	SYNCDELAY;
	FIFORESET = 0x08;	SYNCDELAY;
	FIFORESET = 0x00;	SYNCDELAY;  // Resume normal operation.
	// PORTACFG: FLAGD SLCS(*) 0 0 0 0 INT1 INT0
	PORTACFG = 0xC0;	SYNCDELAY;	// (delay maybe not needed) //INT0 interrupt
	PINFLAGSAB = 0x00;	SYNCDELAY;	//FA: EP6E,		FB: EP6F
	PINFLAGSCD = 0xB0;	SYNCDELAY;	//FC: EP6P,	FD: EP8 EF
	FIFOPINPOLAR=0x3F;  SYNCDELAY;	// All polarities active high
	// This determines how much data is accumulated in the FIFOs before a
	// USB packet is committed. Use 512 bytes to be sure.
	EP2AUTOINLENH = 0x02; SYNCDELAY;  // MSB
	EP2AUTOINLENL = 0x00; SYNCDELAY;  // LSB
	EP4AUTOINLENH = 0x02; SYNCDELAY;  // MSB
	EP4AUTOINLENL = 0x00; SYNCDELAY;  // LSB
	EP6AUTOINLENH = 0x02; SYNCDELAY;  // MSB
	EP6AUTOINLENL = 0x00; SYNCDELAY;  // LSB
	EP2FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag
	EP2FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish
	EP4FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag
	EP4FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish
	EP6FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag
	EP6FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish
	OUTPKTEND = 0x88; SYNCDELAY;	// Arm both EP2 buffers to “prime the pump”		
	OUTPKTEND = 0x88; SYNCDELAY;
	
	EP2FIFOCFG = 0x48;  SYNCDELAY; 	// Configure EP2 for AUTOIN, 8bit wide bus.
	EP4FIFOCFG = 0x48;  SYNCDELAY;	// Configure EP4 for AUTOIN, 8bit wide bus.
	EP6FIFOCFG = 0x48;  SYNCDELAY;	// Configure EP6 for AUTOIN, 8bit wide bus.
	EP8FIFOCFG = 0x10;  SYNCDELAY;	// Configure EP8 for AUTOOUT, 8bit wide bus.
	
	IOD = 0x03;			// Enable PS_ON and disable PROG_B 
	OED = 0x03;			// Configure PS_ON and PROG as outputs
	OEA = 0x82;			// FlagD and INT1 as outputs

	if( !( EP1OUTCS & 0x02) ) 	// Need to clear EP1 buffer
		EP1OUTBC = 0x40;
}

//-----------------------------------------------------------------------------
void main(void){
	EA = 0; 				// disable all interrupts
	setup_autovectors ();
	usb_install_handlers ();
	EA = 1; 				// enable interrupts
	fx2_renumerate(); 		// simulates disconnect / reconnect
	system_init();
	while(1){
		if(usb_setup_packet_avail()) usb_handle_setup_packet();
			activity();
	}
}
