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

/*It contains the definition of usb_setup_packet_avail() and the associated usb_common.c contains 
the functions used inside the super-loop's usb_handle_setup_packet() */
#include "usb_common.h"

#include "usb_descriptors.h"
#include "usb_requests.h"
#include "syncdelay.h"

#include "te_api.h" //The associated te_api.c contains the functions used inside the super-loop's activity()
#include "fpga.h"

//-----------------------------------------------------------------------------
// Initialize the USB FX2 microcontroller register, memory, etc...
// Called once at startup
void system_init(void)              
{
	EP1OUTCFG	= 0xA0;
	EP1INCFG	= 0xA0;
	CPUCS = 0x12;  		// 48MHz, output to CLKOUT signal enabled.
	REVCTL = 0x03;	 	SYNCDELAY;  // See TRM...
	IFCONFIG = 0xE0;	SYNCDELAY; 
	IFCONFIG = 0xE3;	SYNCDELAY;		
	EP2CFG = 0xE2; 		SYNCDELAY;  // EP2 IN,  bulk, size 512x2
  	EP4CFG = 0xE0; 		SYNCDELAY;  // EP4 IN,  bulk, size 512x2
  	EP6CFG = 0xE2; 		SYNCDELAY;  // EP6 IN,  bulk, size 512x2
  	EP8CFG = 0xA0; 		SYNCDELAY;  // EP8 OUT, bulk, size 512x2
  	
	// Reset FIFO-te	 
	FIFORESET = 0x80;	SYNCDELAY;  // NAK all requests from host. Now is possible to reset individual FIFO EP (2,4,6,8)
	FIFORESET = 0x02;	SYNCDELAY;  // Reset FIFO EP2
	FIFORESET = 0x04;	SYNCDELAY;  // Reset FIFO EP4
	FIFORESET = 0x06;	SYNCDELAY;  // Reset FIFO EP6
	FIFORESET = 0x08;	SYNCDELAY;  // Reset FIFO EP8
	FIFORESET = 0x00;	SYNCDELAY;  // Resume normal operation.
	
	// PORTACFG: FLAGD SLCS(*) 0 0 0 0 INT1 INT0
	//               1       1 0 0 0 0    0    0
	PORTACFG = 0xC0;	SYNCDELAY;	// (delay maybe not needed) 
	//Bits PORTACFG.7 and PORTACFG.6 both affect pin PA7. If both bits are set, FLAGD takes precedence.
	// INT1 AND INT0 are  CONFIGURED AS INTERRUPT if 
	//PORTACFG = 0xC3
	// PORTACFG: FLAGD SLCS(*) 0 0 0 0 INT1 INT0
	//               1       1 0 0 0 0    1    1
	PINFLAGSAB = 0x00;	SYNCDELAY;	//FA: EPxP,	FB: EPxF
	PINFLAGSCD = 0xB0;	SYNCDELAY;	//FC: EPxE,	FD: EP8E
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
	EP2FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish (0x01FF in this case)
	EP4FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag
	EP4FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish (0x01FF in this case)
	EP6FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag
	EP6FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish (0x01FF in this case)
	OUTPKTEND = 0x88; SYNCDELAY;	// Arm both EP2 buffers to �prime the pump�		
	OUTPKTEND = 0x88; SYNCDELAY;
	
	EP2FIFOCFG = 0x4C;  SYNCDELAY; 	// Configure EP2 for AUTOIN, 8bit wide bus.
	EP4FIFOCFG = 0x4C;  SYNCDELAY;	// Configure EP4 for AUTOIN, 8bit wide bus.
	EP6FIFOCFG = 0x4C;  SYNCDELAY;	// Configure EP6 for AUTOIN, 8bit wide bus.
	EP8FIFOCFG = 0x10;  SYNCDELAY;	// Configure EP8 for AUTOOUT, 8bit wide bus.
	
	IOD = 0x03;			// Enable FX2_PS_EN and FX2_PROG_B as inputs  => 
					// 0b00000011; => PD1,PD0 pins input enabled;
	OED = 0x03;			// Enable FX2_PS_EN and FX2_PROG_B as outputs => 
					// 0b00000011; => PD1,PD0 pins output enabled;
	OEA = 0x82;			//Enable FlagD and INT1 as outputs 
					//=> 0b10000010 => PA7,PA1 pins output enabled;

	if( !( EP1OUTCS & 0x02) ) 	// Need to clear EP1 buffer
		EP1OUTBC = 0x40;
}

//-----------------------------------------------------------------------------
void main(void)
{
	//Code called once at startup: starts here
	EA = 0; 		        // Disable all interrupts
	setup_autovectors ();           // Setup interrupt table
	usb_install_handlers ();        // Setup/install handlers (aka functions) called to manage 
	                                // the interrupt set in interrupt table
	EA = 1; 		        // Enable all interrupts
	fx2_renumerate(); 		// Simulates disconnect / reconnect
	system_init();			// Initialize the USB FX2 microcontroller register, memory, etc...
	//Code called once at startup: ends here
	
	while(1) // Infinite loop aka super-loop
	{                       
		if(usb_setup_packet_avail())  //Check if new setup data is available
			usb_handle_setup_packet();
			//If new setup data is available, handles the setup package and the basic device requests 
			//like reading descriptors, get/set confifuration etc.
			//Manage incoming USB setup data packet.
			//It is used in connection and disconnection (of TE USB FX2 module) phases from
			//USB host computer: it is used to establish/remove a conection of the
			//TE USB FX2 module (FX2 microcontroller) with host computer's though USB
		
		activity(); //see fw.c
		//void activity(void)
		//{
        	//	ep1_pool();      
        		// Pull EP1 data aka Polling for EP1 data aka
        		// Pull EP1 data aka pull possible TE API Commands (FW APIs) 
        		// from USB connection (with host computer's SW) 
        		// and execute the function requested by host computer's SW using TE API Commands.
        		
        	//	int_pin_pool();
        		// Pull INT pool aka Interrupt Pin polling aka
        		// Pull INT pool data from an I2C address when FPGA's chip rise INT0 pin and an autoresponse 
        		// interrupt is preconfigured by host computer's SW. The FX2 microcontroller pull 
        		// (using I2C in int_pin_pool()) x (x defined by EP1OUTBUF[2]) number of bytes 
        		// from y I2C address (y defined by EP1OUTBUF[1]). 
        		// The host computer's SW should use a polling procedure to retrieve the I2C bytes read 
        		// (and stored) by FX2 microcontroller
        	//} 
	}
}
