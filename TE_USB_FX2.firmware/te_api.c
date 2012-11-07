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
//-----------------------------------------------------------------------------
#pragma NOIV					// Do not generate interrupt vectors
#include "fx2.h"
#include "fx2regs.h"
#include "fx2sdly.h"
#include "spi.h"
#include "fpga.h"
#include "i2c.h"
#include <string.h>

#define EXTFIFONOTFULL	GPIFREADYSTAT & bmBIT1
#define EXTFIFONOTEMPTY  GPIFREADYSTAT & bmBIT0
#define EZUSB_HIGHSPEED()		(USBCS & bmHSM)

extern BOOL GotSUD;				 // Received setup data flag
extern BOOL Sleep;
extern BOOL Rwuen;
extern BOOL Selfpwr;

BYTE Configuration;					// Current configuration
BYTE AlternateSetting;				// Alternate settings
BOOL in_enable = FALSE;				// flag to enable IN transfers
BOOL enum_high_speed = FALSE;		// flag to let firmware know FX2 enumerated at high speed

BYTE lock = 0;						// Lock device for driver instance

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
//-----------------------------------------------------------------------------
extern void page_write (BYTE addhighest, BYTE addhigh, BYTE addlow, unsigned char *wrptr, WORD p_write_size);
extern void page_read (BYTE addhighest, BYTE addhigh, BYTE addlow,	BYTE *rdptr, WORD count);
extern void sector_erase (unsigned char sector);
extern void bulk_erase(void);
extern char check_flash_busy(void);
extern void read_version (BYTE xdata *buf);
extern void spi_command(BYTE CmdLen, unsigned char *CmdData, BYTE RdLen, unsigned char *RdData);
//-----------------------------------------------------------------------------
void EP_Init(void);
void IntPinPool(void);
void EP1_Pool(void);
void COMMAND_Init(void);
void READ_REG_FPGA (BYTE reg, BYTE xdata *regdata);
void WRITE_REG_FPGA (BYTE reg, BYTE xdata *regdata);

#define	EP1DATA_COUNT				0x40

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
#define DEV_LOCK					0xBB

#define I2C_WRITE_READ				0xAB
#define GET_FIFO_STATUS				0xAC
#define I2C_WRITE					0xAD
#define I2C_READ					0xAE
#define FPGA_POWER					0xAF
#define SET_AUTORESPONSE			0xB0
#define GET_AUTORESPONSE			0xB1
#define FPGA_RESET					0xB2

//-----------------------------------------------------------------------------
// Interrupt Pin pooling
//-----------------------------------------------------------------------------
void IntPinPool(void){
	if (fx2_status.IntAutoConfigured == 0) return;

	if(INT0_PIN){
		IntAutoResponse.PinStatus = 1;
		if (IntAutoResponse.Count > 32)
			IntAutoResponse.Count = 32;
		I2CRead2(IntAutoResponse.Adress, IntAutoResponse.Count, &AutoResponseData[0]);	// adress, size, data
		IntAutoResponse.IntIDX = IntAutoResponse.IntIDX + 1;
		fx2_status.i2c_new_data = 1;
	}
	else 
		IntAutoResponse.PinStatus = 0;
}

//-----------------------------------------------------------------------------
// Polling for EP1 data
//-----------------------------------------------------------------------------
void EP1_Pool(void){
	BOOL	new_data = FALSE;
	WORD	adr, delaycnt;
	BYTE	i2cadr;
	char *p;
	
	if( !( EP1OUTCS & 0x02) ){ 		// Got something
		for (adr = 0; adr < 0x40; adr++) 
			EP1INBUF[adr] = 0xff;	// fill output buffer

		switch(EP1OUTBUF[0]){		// Decode command

			default:
			case	READ_VERSION:
				read_version(&EP1INBUF[0]);
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

			//case	SWITCH_MODE:
			//		fx2_status.current_mode = 1;
			//		new_data = TRUE;
			//		EP1INBUF[0] = EP1OUTBUF[1];
			//		break;

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
				new_data = TRUE;					
				p = memcpy (&EP1INBUF[0], &fx2_status.fifo_error, sizeof(fx2_status));

				//EP1INBUF[2] = check_flash_busy();
				fx2_status.i2c_new_data = 0;
				break;

			case RESET_FIFO_STATUS:
				fx2_status.fifo_error = 0;
				FIFORESET = 0x80;  SYNCDELAY;  // NAK all requests from host.
				FIFORESET = 0x02;  SYNCDELAY;
				FIFORESET = 0x04;  SYNCDELAY;
				FIFORESET = 0x06;  SYNCDELAY;
				FIFORESET = 0x00;  SYNCDELAY;	// Resume normal operation.
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
				//busy_polling();
				bulk_erase();
				new_data = TRUE;
				fx2_status.flash_busy = 1;
				break;

			case SECTOR_ERASE:
				sector_erase(EP1OUTBUF[1]);
				new_data = TRUE;
				fx2_status.flash_busy = check_flash_busy();
				break;

			case FLASH_WRITE_COMMAND:
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

			case DEV_LOCK:
				if(EP1OUTBUF[1] == 0x01){	// Driver trying to lock device
					if(lock == 0){		// Device is free
						EP1INBUF[0] = 0x22;	// Sucessfull lock
						lock = 1;
					}
					else {				// Device is locked
						EP1INBUF[0] = 0x00;	// Already locked
					}
				}
				else{						// Driver trying to unlock device
					if(lock == 1){		// Device is locked
						EP1INBUF[0] = 0x33;	// Sucessfull unlock
						lock = 0;
					}
					else {				// Device is unlocked
						EP1INBUF[0] = 0x00;	// Got problem
					}
				}
				new_data = TRUE;
				break;
		}
		EP1OUTBC = EP1DATA_COUNT;	// Free input buffer
	}
	
	if(new_data == TRUE){				// Have something to send
		if ( !(EP1INCS & 0x02)){		// Can send ?
			EP1INBC = EP1DATA_COUNT;	// Send
			new_data = FALSE;
		}
	}
}


//-----------------------------------------------------------------------------
// Task Dispatcher hooks
//	The following hooks are called by the task dispatcher.
//-----------------------------------------------------------------------------
void EP_Init(void)				 // Called once at startup
{
	EP1OUTCFG	= 0xA0;
	EP1INCFG = 0xA0;
	fx2_status.IntAutoConfigured = 0;

	//fx2_status.current_mode = 1;
	IntAutoResponse.PinStatus = 0;

	CPUCS = 0x12;  		// 48MHz, output to CLKOUT signal enabled.
	REVCTL = 0x03;	 	SYNCDELAY;  // See TRM...
	IFCONFIG = 0xE0;	SYNCDELAY; 
	IFCONFIG = 0xE3;	SYNCDELAY;		
	 
	 // Konfiguracija endpointov
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

	EP2FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag (FLAGA) (256 vzorcev)
	EP2FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish

	EP4FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag (FLAGA) (256 vzorcev)
	EP4FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish

	EP6FIFOPFH = 0x01;	SYNCDELAY;	// you can define the programmable flag (FLAGA) (256 vzorcev)
	EP6FIFOPFL = 0xFF;	SYNCDELAY;	// to be active at the level you wish

	OUTPKTEND = 0x88; SYNCDELAY;	// Arm both EP2 buffers to “prime the pump”		
	OUTPKTEND = 0x88; SYNCDELAY;

	EP2FIFOCFG = 0x48;  SYNCDELAY; 	// Configure EP2 for AUTOIN, 8bit wide bus.
	EP4FIFOCFG = 0x48;  SYNCDELAY;	// Configure EP4 for AUTOIN, 8bit wide bus.
	EP6FIFOCFG = 0x48;  SYNCDELAY;	// Configure EP6 for AUTOIN, 8bit wide bus.
	EP8FIFOCFG = 0x10;  SYNCDELAY;	// Configure EP8 for AUTOOUT, 8bit wide bus.
}

BOOL TD_Suspend(void)				// Called before the device goes into suspend mode
{
	return(TRUE);
}

BOOL TD_Resume(void)			 	// Called after the device resumes
{
	return(TRUE);
}

//-----------------------------------------------------------------------------
// Device Request hooks
//	The following hooks are called by the end point 0 device request parser.
//-----------------------------------------------------------------------------

BOOL DR_GetDescriptor(void)
{
	return(TRUE);
}

BOOL DR_SetConfiguration(void)	// Called when a Set Configuration command is received
{
  if( EZUSB_HIGHSPEED( ) )
  { // FX2 enumerated at high speed
	 SYNCDELAY;						// 
	 EP6AUTOINLENH = 0x02;		 // set AUTOIN commit length to 512 bytes
	 SYNCDELAY;						// 
	 EP6AUTOINLENL = 0x00;
	 SYNCDELAY;						
	 enum_high_speed = TRUE;
  }
  else
  { // FX2 enumerated at full speed
	 SYNCDELAY;						 
	 EP6AUTOINLENH = 0x00;		 // set AUTOIN commit length to 64 bytes
	 SYNCDELAY;						 
	 EP6AUTOINLENL = 0x40;
	 SYNCDELAY;						
	 enum_high_speed = FALSE;
  }

  Configuration = SETUPDAT[2];
  return(TRUE);				// Handled by user code
}

BOOL DR_GetConfiguration(void)	// Called when a Get Configuration command is received
{
	EP0BUF[0] = Configuration;
	EP0BCH = 0;
	EP0BCL = 1;
	return(TRUE);				// Handled by user code
}

BOOL DR_SetInterface(void)		 // Called when a Set Interface command is received
{
	AlternateSetting = SETUPDAT[2];
	return(TRUE);				// Handled by user code
}

BOOL DR_GetInterface(void)		 // Called when a Set Interface command is received
{
	EP0BUF[0] = AlternateSetting;
	EP0BCH = 0;
	EP0BCL = 1;
	return(TRUE);				// Handled by user code
}

BOOL DR_GetStatus(void)
{
	return(TRUE);
}

BOOL DR_ClearFeature(void)
{
	return(TRUE);
}

BOOL DR_SetFeature(void)
{
	return(TRUE);
}

BOOL DR_VendorCmnd(void)
{
  return(FALSE);
}

//-----------------------------------------------------------------------------
// USB Interrupt Handlers
//	The following functions are called by the USB interrupt jump table.
//-----------------------------------------------------------------------------

// Setup Data Available Interrupt Handler
void ISR_Sudav(void) interrupt 0
{
	GotSUD = TRUE;				// Set flag
	EZUSB_IRQ_CLEAR();
	USBIRQ = bmSUDAV;			// Clear SUDAV IRQ
}

// Setup Token Interrupt Handler
void ISR_Sutok(void) interrupt 0
{
	EZUSB_IRQ_CLEAR();
	USBIRQ = bmSUTOK;			// Clear SUTOK IRQ
}

void ISR_Sof(void) interrupt 0
{
	EZUSB_IRQ_CLEAR();
	USBIRQ = bmSOF;				// Clear SOF IRQ
}

void ISR_Ures(void) interrupt 0
{
	// whenever we get a USB reset, we should revert to full speed mode
	pConfigDscr = pFullSpeedConfigDscr;
	((CONFIGDSCR xdata *) pConfigDscr)->type = CONFIG_DSCR;
	pOtherConfigDscr = pHighSpeedConfigDscr;
	((CONFIGDSCR xdata *) pOtherConfigDscr)->type = OTHERSPEED_DSCR;

	EZUSB_IRQ_CLEAR();
	USBIRQ = bmURES;			// Clear URES IRQ
}

void ISR_Susp(void) interrupt 0
{
	Sleep = TRUE;
	EZUSB_IRQ_CLEAR();
	USBIRQ = bmSUSP;
}

void ISR_Highspeed(void) interrupt 0
{
	if (EZUSB_HIGHSPEED())
	{
		pConfigDscr = pHighSpeedConfigDscr;
		((CONFIGDSCR xdata *) pConfigDscr)->type = CONFIG_DSCR;
		pOtherConfigDscr = pFullSpeedConfigDscr;
		((CONFIGDSCR xdata *) pOtherConfigDscr)->type = OTHERSPEED_DSCR;
	}

	EZUSB_IRQ_CLEAR();
	USBIRQ = bmHSGRANT;
}
void ISR_Ep0ack(void) interrupt 0
{
}
void ISR_Stub(void) interrupt 0
{
}
void ISR_Ep0in(void) interrupt 0
{
}
void ISR_Ep0out(void) interrupt 0
{
}
void ISR_Ep1in(void) interrupt 0
{
}
void ISR_Ep1out(void) interrupt 0
{
}
void ISR_Ep2inout(void) interrupt 0
{
}
void ISR_Ep4inout(void) interrupt 0
{
}
void ISR_Ep6inout(void) interrupt 0
{
}
void ISR_Ep8inout(void) interrupt 0
{
}
void ISR_Ibn(void) interrupt 0
{
}
void ISR_Ep0pingnak(void) interrupt 0
{
}
void ISR_Ep1pingnak(void) interrupt 0
{
}
void ISR_Ep2pingnak(void) interrupt 0
{
}
void ISR_Ep4pingnak(void) interrupt 0
{
}
void ISR_Ep6pingnak(void) interrupt 0
{
}
void ISR_Ep8pingnak(void) interrupt 0
{
}
void ISR_Errorlimit(void) interrupt 0
{
}
void ISR_Ep2piderror(void) interrupt 0
{
}
void ISR_Ep4piderror(void) interrupt 0
{
}
void ISR_Ep6piderror(void) interrupt 0
{
}
void ISR_Ep8piderror(void) interrupt 0
{
}
void ISR_Ep2pflag(void) interrupt 0
{
}
void ISR_Ep4pflag(void) interrupt 0
{
}
void ISR_Ep6pflag(void) interrupt 0
{
}
void ISR_Ep8pflag(void) interrupt 0
{
}
void ISR_Ep2eflag(void) interrupt 0
{
}
void ISR_Ep4eflag(void) interrupt 0
{
}
void ISR_Ep6eflag(void) interrupt 0
{
}
void ISR_Ep8eflag(void) interrupt 0
{
}
void ISR_Ep2fflag(void) interrupt 0
{
}
void ISR_Ep4fflag(void) interrupt 0
{
}
void ISR_Ep6fflag(void) interrupt 0
{
}
void ISR_Ep8fflag(void) interrupt 0
{
}
void ISR_GpifComplete(void) interrupt 0
{
}
void ISR_GpifWaveform(void) interrupt 0
{
}
