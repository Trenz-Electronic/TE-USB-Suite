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

#pragma NOIV               // Do not generate interrupt vectors
#include "fx2.h"
#include "fx2regs.h"
#include "fx2sdly.h"            // SYNCDELAY macro
#include <stdio.h>
#include <string.h>
#include "versions.h"
#include "i2c.h"
#include "eeprom.h"
#include "spi.h"

#define EXTFIFONOTFULL   GPIFREADYSTAT & bmBIT1
#define EXTFIFONOTEMPTY  GPIFREADYSTAT & bmBIT0

#define GPIFTRIGRD 4

#define GPIF_EP2 0
#define GPIF_EP4 1
#define GPIF_EP6 2
#define GPIF_EP8 3

#define EZUSB_HIGHSPEED()      (USBCS & bmHSM)

struct _fx2_status{
	BYTE	fifo_error;
	BYTE	current_mode;
	BYTE	flash_busy;
	BYTE	fpga_prog;
	BYTE	booting;
	BYTE	i2c_new_data;
	BYTE	IntAutoConfigured;
	BYTE	HighSpeedMode;
} fx2_status;

struct _IntAutoResponse{
	BYTE Adress, Count, IntIDX, PinStatus;	
} IntAutoResponse;
BYTE AutoResponseData[32];

xdata char sSerNum[18];

#define	EP1DATA_COUNT	            0x40  // data transfer to PC driver
// Commands
#define READ_VERSION				0x00		
#define SWITCH_MODE					0xA0
#define	READ_STATUS					0xA1
#define WRITE_REGISTER				0xA2
#define READ_REGISTER				0xA3
#define	RESET_FIFO_STATUS			0xA4
#define FLASH_READ					0xA5
#define FLASH_WRITE					0xA6
#define FLASH_ERASE					0xA7
#define SECTOR_ERASE				0xF7
#define EEPROM_READ					0xA8
#define EEPROM_WRITE				0xA9
#define FLASH_WRITE_COMMAND			0xAA
#define I2C_WRITE_READ				0xAB
#define GET_FIFO_STATUS				0xAC
#define I2C_WRITE					0xAD
#define I2C_READ					0xAE
#define FPGA_POWER					0xAF
#define SET_AUTORESPONSE			0xB0
#define GET_AUTORESPONSE			0xB1
#define FPGA_RESET					0xB2
#define CMD_ECHO                    0x45    // "E"

/*
 * Interrupt pooling
 */
void IntPinPool(void){
	if (fx2_status.IntAutoConfigured == 0) return;
	
	if(INT0_PIN){
			IntAutoResponse.PinStatus = 1;
			if (IntAutoResponse.Count > 32)
				IntAutoResponse.Count = 32;
			I2CRead2(IntAutoResponse.Adress, IntAutoResponse.Count, 
                &AutoResponseData[0]);	// adress, size, data
			IntAutoResponse.IntIDX = IntAutoResponse.IntIDX + 1;
			fx2_status.i2c_new_data = 1;
	}
	else 
		IntAutoResponse.PinStatus = 0;
}

/*
 * Endpoint 1 pooling
 */
void EP1_Pool(void){
	BYTE	DATA_RETURN_CNT = 0X40;
	BOOL	new_data = FALSE;
	WORD	adr, delaycnt;
	BYTE	i2cadr;
	char    *p;
    int     i;
	
	if( !( EP1OUTCS & 0x02) ){ // when new data received
		// process it
		for (adr = 0; adr < 0x40; adr++) 
				EP1INBUF[adr] = 0xff;	//fill buffer

		switch(EP1OUTBUF[0]){
			default:
			case	READ_VERSION:
					read_version (&EP1INBUF[0]);
					new_data = TRUE;
					break;
			case	SET_AUTORESPONSE:
					fx2_status.IntAutoConfigured = 1;
					IntAutoResponse.Adress = EP1OUTBUF[1];
					IntAutoResponse.Count = EP1OUTBUF[2];
					IntAutoResponse.IntIDX = 0;
					new_data = TRUE;
					break;
			case	GET_AUTORESPONSE:
					EP1INBUF[0] = IntAutoResponse.IntIDX;
					p = memcpy (&EP1INBUF[1], &AutoResponseData[0], 32);
					IntAutoResponse.IntIDX = 0;
					new_data = TRUE;
					break;
			case	SWITCH_MODE:
					//FIFO_Init();
					fx2_status.current_mode = 1;
					DATA_RETURN_CNT = 0x1; new_data = TRUE;
					EP1INBUF[0] = EP1OUTBUF[1];
					break;
			case	READ_STATUS:
					if (fx2_status.flash_busy == 1){
						fx2_status.flash_busy = check_flash_busy();
					}
					fx2_status.booting = FPGA_DONE;
					fx2_status.fpga_prog = 0xaa;
					if (EZUSB_HIGHSPEED()){
						fx2_status.HighSpeedMode = 1;
					}
					else {
						fx2_status.HighSpeedMode = 255;
					}

					DATA_RETURN_CNT = sizeof(fx2_status);
					new_data = TRUE;					
					p = memcpy (&EP1INBUF[0], &fx2_status.fifo_error, sizeof(fx2_status));
					fx2_status.i2c_new_data = 0;
					break;
			case WRITE_REGISTER:		//transfer data to FPGA
                    //	WRITE_REG_FPGA (EP1OUTBUF[1], &EP1OUTBUF[2]);
					new_data = TRUE;
					break;
			case READ_REGISTER:			//transfer data from FPGA (3 bytes)
                    //	READ_REG_FPGA(EP1OUTBUF[1], &EP1INBUF[1]);
                    //	EP1INBUF[0] = EP1OUTBUF[1];
					DATA_RETURN_CNT = 3; 
					new_data = TRUE;
					break;			
			case RESET_FIFO_STATUS:
					fx2_status.fifo_error = 0;
					// Resetiraj FIFO-te    
						FIFORESET = 0x80;  SYNCDELAY;  // NAK all requests from host.
						if (EP1OUTBUF[1] == 2) {
							EP2FIFOCFG = 0x48;  SYNCDELAY; // Configure EP2 for AUTOIN, 8bit wide bus.
							FIFORESET = 0x02;  SYNCDELAY;  // Reset individual EP (2,4,6,8)							
						}
						else 	if (EP1OUTBUF[1] == 4) {
										EP4FIFOCFG = 0x48;  SYNCDELAY; // Configure EP4 for AUTOIN, 8bit wide bus.										
										FIFORESET = 0x04;  SYNCDELAY;  // Reset individual EP (2,4,6,8)
									}
						else 	if (EP1OUTBUF[1] == 6) {
										EP6FIFOCFG = 0x48;  SYNCDELAY; // Configure EP6 for AUTOIN, 8bit wide bus.
										FIFORESET = 0x06;  SYNCDELAY;  // Reset individual EP (2,4,6,8)
									}
						else	if (EP1OUTBUF[1] == 0) {
										//EP2
										FIFORESET = 0x02;  SYNCDELAY;  // Reset individual EP (2,4,6,8)
										//EP4
										//EP4FIFOCFG = 0x48;  SYNCDELAY; // Configure EP4 for AUTOIN, 8bit wide bus.
										FIFORESET = 0x04;  SYNCDELAY;
										//EP6	
					    				//EP6FIFOCFG = 0x48;  SYNCDELAY; // Configure EP6 for AUTOIN, 8bit wide bus.
								    	FIFORESET = 0x06;  SYNCDELAY;
										//EP8
										/*OUTPKTEND = 0x88; SYNCDELAY;			// Arm both EP2 buffers to “prime the pump”		
										OUTPKTEND = 0x88;	SYNCDELAY;
										EP8BCL = 0;			SYNCDELAY;
										EP8BCH = 0;			SYNCDELAY;
								    	FIFORESET = 0x08;  SYNCDELAY;
										*/
									}

					FIFORESET = 0x00;  SYNCDELAY;  // Resume normal operation.
					new_data = TRUE;
					break;
			case FLASH_WRITE:
					if (EP1OUTBUF[4] > 59) EP1OUTBUF[4] = 59;
					page_write(EP1OUTBUF[1], EP1OUTBUF[2], EP1OUTBUF[3], &EP1OUTBUF[5], EP1OUTBUF[4]);	//highest, high, low adr, read_ptr, size
			case FLASH_READ:					
					if (EP1OUTBUF[4] > 64) EP1OUTBUF[4] = 64;
					page_read(EP1OUTBUF[1], EP1OUTBUF[2], EP1OUTBUF[3], &EP1INBUF[0], EP1OUTBUF[4]);		//highest, high, low adr, read_ptr, size
					new_data = TRUE;
					break;			
			case FLASH_ERASE:
					bulk_erase();
					new_data = TRUE;
					fx2_status.flash_busy = check_flash_busy();
					break;
			case SECTOR_ERASE:
					sector_erase(EP1OUTBUF[1]);
					new_data = TRUE;
					fx2_status.flash_busy = check_flash_busy();
					break;
			case FLASH_WRITE_COMMAND:
					//void spi_command(BYTE CmdLen, BYTE *CmdData, BYTE RdLen, BYTE *RdData);
					EP1INBUF[0] = 0x55;
					spi_command(EP1OUTBUF[1], &EP1OUTBUF[3], EP1OUTBUF[2], &EP1INBUF[1]);
					new_data = TRUE;
					break;
			case EEPROM_WRITE:					
					adr = EP1OUTBUF[1];
					adr = (adr << 8) + EP1OUTBUF[2];
					if (EP1OUTBUF[3] > 32) EP1OUTBUF[3] = 32;				
					EEPROMWrite(adr, EP1OUTBUF[3], &EP1OUTBUF[4]);	// adress, size, data
			case EEPROM_READ:
					adr = EP1OUTBUF[1];
					adr = (adr << 8) + EP1OUTBUF[2];
					EEPROMRead(adr, EP1OUTBUF[3], &EP1INBUF[0]);	// adress, size, data
					new_data = TRUE;
					break;			
			case GET_FIFO_STATUS:
					EP1INBUF[0] = EP2CS;
					EP1INBUF[1] = EP4CS;
					EP1INBUF[2] = EP6CS;
					EP1INBUF[3] = EP8CS;
					EP1INBUF[4] = EP2FIFOBCH;
					EP1INBUF[5] = EP4FIFOBCH;
					EP1INBUF[6] = EP6FIFOBCH;
					EP1INBUF[7] = EP8FIFOBCH;
					EP1INBUF[8] = EP2FIFOBCL;
					EP1INBUF[9] = EP4FIFOBCL;
					EP1INBUF[10] = EP6FIFOBCL;
					EP1INBUF[11] = EP8FIFOBCL;
					EP1INBUF[12] = EP2FIFOFLGS;
					EP1INBUF[13] = EP4FIFOFLGS;
					EP1INBUF[14] = EP6FIFOFLGS;
					EP1INBUF[15] = EP8FIFOFLGS;
					new_data = TRUE;
					break;
			case I2C_WRITE:
					i2cadr = EP1OUTBUF[1];
					I2CWrite(i2cadr, EP1OUTBUF[2], &EP1OUTBUF[3]);	// adress, size, data
					new_data = TRUE;
					break;
			case I2C_READ:
					i2cadr = EP1OUTBUF[1];
					I2CRead(i2cadr, EP1OUTBUF[2], &EP1INBUF[0]);	// adress, size, data
					new_data = TRUE;
					break;
			case I2C_WRITE_READ:
					i2cadr = EP1OUTBUF[1];
					I2CWrite(i2cadr, EP1OUTBUF[2], &EP1OUTBUF[4]);	// adress, size, data
					delaycnt = 0;
					while (INT0_PIN == 0){
						EZUSB_Delay1ms();
						delaycnt++;
						if (delaycnt > 800)
							break;
						continue;
					}
					I2CRead(i2cadr, EP1OUTBUF[3], &EP1INBUF[0]);	// adress, size, data					
					new_data = TRUE;
					break;
			case FPGA_POWER:
					if (EP1OUTBUF[1] == 0){
						FPGA_POWER_ON = 0;
						fx2_status.IntAutoConfigured = 0;
					}
					else
						FPGA_POWER_ON = 1;
					if (FPGA_POWER_ON)
						EP1INBUF[0] = 1;
					else
						EP1INBUF[0] = 0;
					EP1INBUF[1] = 0xAA;
					new_data = TRUE;
					break;
			case FPGA_RESET:
				if (EP1OUTBUF[1] == 0)
						INT1_PIN = 0;						
					else
						INT1_PIN = 1;

					EP1INBUF[0] = INT1_PIN;

					EP1INBUF[1] = 0xAA;
					new_data = TRUE;
					break;
            case CMD_ECHO:
                for(i=0;i<63;i++)
                    EP1INBUF[i] = EP1OUTBUF[i];
                new_data = TRUE;
                break;
		}
		EP1OUTBC = EP1DATA_COUNT;	//we ready for new data
	}
	
	if(new_data == TRUE){				// if we have new data to transmit
		if ( !(EP1INCS & 0x02)){		// wait for old data
			EP1INBC = EP1DATA_COUNT;	// send new
			new_data = FALSE;
		}
	}
}


//-----------------------------------------------------------------------------
// Task Dispatcher hooks
//   The following hooks are called by the task dispatcher.
//-----------------------------------------------------------------------------
BOOL TD_Suspend(void)          // Called before the device goes into suspend mode
{
   return(TRUE);
}

BOOL TD_Resume(void)          // Called after the device resumes
{
   return(TRUE);
}

void FIFO_Init(void)             // Called once at startup
{
	fx2_status.current_mode = 1;
	IntAutoResponse.PinStatus = 0;

	CPUCS = 0x12;  // 48MHz, output to CLKOUT signal enabled.
    REVCTL = 0x03;    SYNCDELAY;  // See TRM...
    IFCONFIG = 0xE0;	SYNCDELAY; 
	IFCONFIG = 0xE3;	SYNCDELAY;		
 
    // Endpoints configuration
	EP2CFG = 0xE2; SYNCDELAY;    // EP2OUT, bulk, size 512, 2x buffered //11100010                         
  	EP4CFG = 0xE0; SYNCDELAY;    // EP4 valid IN
  	EP6CFG = 0xE2; SYNCDELAY;    // EP6IN, bulk, size 512, 4x buffered       
  	EP8CFG = 0xA0; SYNCDELAY;    // EP8 valid OUT
  	
	// FIFO Reset
    FIFORESET = 0x80;  SYNCDELAY;  // NAK all requests from host.
    FIFORESET = 0x02;  SYNCDELAY;  // Reset individual EP (2,4,6,8)
    FIFORESET = 0x04;  SYNCDELAY;
    FIFORESET = 0x06;  SYNCDELAY;
    FIFORESET = 0x08;  SYNCDELAY;
    FIFORESET = 0x00;  SYNCDELAY;  // Resume normal operation.

	// Configure endpoint FIFO with auto in-out
	EP2FIFOCFG = 0;				//8 bit bus
	EP4FIFOCFG = 0;				
	EP6FIFOCFG = 0;				//8 bit bus//8 bit bus
	EP8FIFOCFG = 0;				//8 bit bus
	  
	// Setup port for FIFO
    // PORTACFG: FLAGD SLCS(*) 0 0 0 0 INT1 INT0
    PORTACFG = 0xC0;  SYNCDELAY; // (delay maybe not needed) //INT0 interrupt
		
	// Flags pin configuration
	PINFLAGSAB = 0;	SYNCDELAY;			//FA: EP6E,		FB: EP6F
	PINFLAGSCD = 0xB0;	SYNCDELAY;		//FC: EP6P,	FD: EP8 EF
		
    // All polarities active high
    FIFOPINPOLAR=0x3F;  SYNCDELAY;
    
    // This determines how much data is accumulated in the FIFOs before a
    // USB packet is committed. Use 512 bytes to be sure.
	EP2AUTOINLENH = 0x02; SYNCDELAY;  // MSB
    EP2AUTOINLENL = 0x00; SYNCDELAY;  // LSB

	EP4AUTOINLENH = 0x02; SYNCDELAY;  // MSB
    EP4AUTOINLENL = 0x00; SYNCDELAY;  // LSB

	EP6AUTOINLENH = 0x02; SYNCDELAY;  // MSB
    EP6AUTOINLENL = 0x00; SYNCDELAY;  // LSB

	//EP8AUTOINLENH = 0x02; SYNCDELAY;  // MSB
    //EP8AUTOINLENL = 0x00; SYNCDELAY;  // LSB

	EP2FIFOPFH = 0x01; SYNCDELAY;			// you can define the programmable flag (FLAGA) (256 vzorcev)
	EP2FIFOPFL = 0xFF; SYNCDELAY;			// to be active at the level you wish

	EP4FIFOPFH = 0x01; SYNCDELAY;			// you can define the programmable flag (FLAGA) (256 vzorcev)
	EP4FIFOPFL = 0xFF; SYNCDELAY;			// to be active at the level you wish

	EP6FIFOPFH = 0x01; SYNCDELAY;			// you can define the programmable flag (FLAGA) (256 vzorcev)
	EP6FIFOPFL = 0xFF; SYNCDELAY;			// to be active at the level you wish

	//EP8FIFOPFH = 0x01; SYNCDELAY;			// you can define the programmable flag (FLAGA) (256 vzorcev)
	//EP8FIFOPFL = 0xFF; SYNCDELAY;			// to be active at the level you wish

	// Izpraznim bufferje za ven
	OUTPKTEND = 0x88; SYNCDELAY;			// Arm both EP2 buffers to “prime the pump”		
	OUTPKTEND = 0x88; SYNCDELAY;
	// Configure endpoint FIFO with auto in-out
    EP2FIFOCFG = 0x48;  SYNCDELAY; // Configure EP2 for AUTOIN, 8bit wide bus.
	EP4FIFOCFG = 0x48;  SYNCDELAY; // Configure EP4 for AUTOIN, 8bit wide bus.
	EP6FIFOCFG = 0x48;  SYNCDELAY; // Configure EP6 for AUTOIN, 8bit wide bus.
    //EP8FIFOCFG = 0x08;  SYNCDELAY; // Configure EP8 for AUTOIN, 8bit wide bus.
	EP8FIFOCFG = 0x10;  SYNCDELAY; // Configure EP8 for AUTOOUT, 8bit wide bus.
	OED = 0x71;		// 0b01110001;
	OEA = 0x82;		// FlagD, INT1 output
	INT1_PIN = 0;	// FPGA_RESET pin is 0
}

void EP1_Init(void){
	EP1OUTCFG	= 0xA0;
	EP1INCFG = 0xA0;
	fx2_status.IntAutoConfigured = 0;
}
