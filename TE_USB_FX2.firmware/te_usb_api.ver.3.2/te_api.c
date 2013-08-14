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

#include "te_api.h"
#include "syncdelay.h"
#include "fx2regs.h"
#include "fpga.h"
#include "i2c.h"
#include "spi.h"
//==============================================================================
// Global definitions 
__xdata BYTE	sts_fifo_error = 0;
__xdata BYTE	sts_current_mode = 1;
__xdata BYTE	sts_flash_busy = 1;
__xdata BYTE	sts_fpga_prog = 1;
__xdata BYTE	sts_booting = 0;
__xdata BYTE	sts_i2c_new_data = 0;
__xdata BYTE	sts_int_auto_configured = 0;
__xdata BYTE	sts_high_speed_mode = 0;

__xdata BYTE	iar_pin_status = 0;
__xdata BYTE	iar_adress = 0x3F;
__xdata BYTE	iar_count = 12;
__xdata BYTE	iar_int_idx = 0;

__xdata BYTE	auto_response_data[32];

__xdata BYTE	lock = 0;
BYTE prev_done = 0;
BYTE cmd_cnt = 0;
//==============================================================================
/*******************************************************************************
* Pull INT pool 
*******************************************************************************/
void int_pin_pool(void){
	if (sts_int_auto_configured){
		if(FPGA_INT0){
			iar_pin_status = 1;
			if (iar_count > 32)
				iar_count = 32;
			I2CRead2(iar_adress, iar_count, &auto_response_data[0]);
			iar_int_idx = iar_int_idx + 1;
			sts_i2c_new_data = 1;
		}
		else 
			iar_pin_status = 0;
	}
}
/*******************************************************************************
* Pull EP1 data
*******************************************************************************/
void ep1_pool(void){
	BYTE i;
	WORD adr;
	BYTE new_data = 0;
	
	// Test data for internal test
	if(FPGA_INT0 && FPGA_DONE && !prev_done && !cmd_cnt){
		EP8FIFOCFG = 0x00;  SYNCDELAY;
		FIFORESET = 0x08; SYNCDELAY;
		FIFORESET = 0x00; SYNCDELAY;
		EP8FIFOBUF[0] = 0x12;
		EP8FIFOBUF[1] = 0x34;
		EP8FIFOBUF[2] = 0x56;
		EP8FIFOBUF[3] = 0x78;
		EP8FIFOBUF[4] = 0x90;
		EP8FIFOBUF[5] = 0xAB;
		EP8FIFOBUF[6] = 0xCD;
		EP8FIFOBUF[7] = 0xEF;
		EP8BCH = 0;
		EP8BCL = 8;
		EP8FIFOCFG = 0x10;  SYNCDELAY;
		prev_done = 1;
	}

	if( !( EP1OUTCS & 0x02) ){ 			// Got something
		cmd_cnt++;
		for (i = 0; i < 0x40; i++) 
			EP1INBUF[i] = 0xFF;			// fill output buffer
			
		switch(EP1OUTBUF[0]){			// Decode command
			//-----------------------------------------------------------------
			default:
			case	CMD_READ_VERSION:
				EP1INBUF[0] = fx2_ver_maj_;
				EP1INBUF[1] = fx2_ver_min_;
				EP1INBUF[2] = fx2_tip_maj_;
				EP1INBUF[3] = fx2_tip_min_;
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case	CMD_SET_AUTORESPONSE:
				sts_int_auto_configured = 1;
				iar_adress = EP1OUTBUF[1];
				iar_count = EP1OUTBUF[2];
				iar_int_idx = 0;
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case	CMD_GET_AUTORESPONSE:
				EP1INBUF[0] = iar_int_idx;
				for(i = 0; i < 32; i++)
					EP1INBUF[i+1] = auto_response_data[i];
				iar_int_idx = 0;
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case	CMD_SWITCH_MODE:
				sts_current_mode = 1;
				new_data = 1;
				EP1INBUF[0] = EP1OUTBUF[1];
				break;
			//-----------------------------------------------------------------
			case	CMD_READ_STATUS:
				sts_flash_busy = get_flash_busy();
				sts_booting = FPGA_DONE;
				sts_fpga_prog = 0xaa;
				sts_high_speed_mode = (USBCS & bmHSM) ? 1 : 255;
				new_data = 1;					
				EP1INBUF[0] = sts_fifo_error;
				EP1INBUF[1] = sts_current_mode;
				EP1INBUF[2] = sts_flash_busy;
				EP1INBUF[3] = sts_fpga_prog;
				EP1INBUF[4] = sts_booting;
				EP1INBUF[5] = sts_i2c_new_data;
				EP1INBUF[6] = sts_int_auto_configured;
				EP1INBUF[7] = sts_high_speed_mode;
				sts_i2c_new_data = 0;
				break;
			//-----------------------------------------------------------------
			case CMD_RESET_FIFO_STATUS:
				sts_fifo_error = 0;
				FIFORESET = 0x80;  SYNCDELAY;  // NAK all requests from host.
				switch(EP1OUTBUF[1]){
					case 2:
						EP2FIFOCFG = 0x4C;  SYNCDELAY;
						FIFORESET = 0x02;  SYNCDELAY;
						break;
					case 4:
						EP4FIFOCFG = 0x4C;  SYNCDELAY;
						FIFORESET = 0x04;  SYNCDELAY;
						break;
					case 6:
						EP6FIFOCFG = 0x4C;  SYNCDELAY;
						FIFORESET = 0x06;  SYNCDELAY;
						break;
					default:	// 0
						EP2FIFOCFG = 0x4C;  SYNCDELAY;
						EP4FIFOCFG = 0x4C;  SYNCDELAY;
						EP6FIFOCFG = 0x4C;  SYNCDELAY;
						EP8FIFOCFG = 0x10;  SYNCDELAY;
						FIFORESET = 0x02;  SYNCDELAY;
						FIFORESET = 0x04;  SYNCDELAY;
						FIFORESET = 0x06;  SYNCDELAY;
				}
				FIFORESET = 0x00;  SYNCDELAY;	// Resume normal operation.
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_FLASH_WRITE:
				if (EP1OUTBUF[4] > 59) EP1OUTBUF[4] = 59;
				page_write(EP1OUTBUF[1], EP1OUTBUF[2], EP1OUTBUF[3], &EP1OUTBUF[5], EP1OUTBUF[4]);	//highest, high, low adr, read_ptr, size
			//-----------------------------------------------------------------
			case CMD_FLASH_READ:					
				if (EP1OUTBUF[4] > 64) EP1OUTBUF[4] = 64;
				page_read(EP1OUTBUF[1], EP1OUTBUF[2], EP1OUTBUF[3], &EP1INBUF[0], EP1OUTBUF[4]);		//highest, high, low adr, read_ptr, size
				new_data = 1;
				break;			
			//-----------------------------------------------------------------
			case CMD_FLASH_ERASE:
				// busy_polling();	
				// On some modules it cause API error - better to do it from software side
				bulk_erase();
				new_data = 1;
				sts_flash_busy = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_SECTOR_ERASE:
				sector_erase(EP1OUTBUF[1]);
				new_data = 1;
				sts_flash_busy = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_FLASH_WRITE_COMMAND:
				EP1INBUF[0] = 0x55;
				spi_command(EP1OUTBUF[1], &EP1OUTBUF[3], EP1OUTBUF[2], &EP1INBUF[1]);
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_EEPROM_WRITE:					
				adr = EP1OUTBUF[1];
				adr = (adr << 8) + EP1OUTBUF[2];
				if (EP1OUTBUF[3] > 32) EP1OUTBUF[3] = 32;				
				EEPROMWrite(adr, EP1OUTBUF[3], &EP1OUTBUF[4]);	// adress, size, data
			//-----------------------------------------------------------------
			case CMD_EEPROM_READ:
				adr = EP1OUTBUF[1];
				adr = (adr << 8) + EP1OUTBUF[2];
				EEPROMRead(adr, EP1OUTBUF[3], &EP1INBUF[0]);	// adress, size, data
				new_data = 1;
				break;			
			//-----------------------------------------------------------------
			case CMD_GET_FIFO_STATUS:
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
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_I2C_WRITE:
				I2CWrite(EP1OUTBUF[1], EP1OUTBUF[2], &EP1OUTBUF[3]);	// adress, size, data
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_I2C_READ:
				I2CRead(EP1OUTBUF[1], EP1OUTBUF[2], &EP1INBUF[0]);	// adress, size, data
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			/*
			case CMD_I2C_WRITE_READ:
				i = EP1OUTBUF[1];
				I2CWrite(i, EP1OUTBUF[2], &EP1OUTBUF[4]);	// adress, size, data
				delaycnt = 0;
				while (INT0_PIN == 0){
					EZUSB_Delay1ms();
					delaycnt++;
					if (delaycnt > 800)
						break;
					continue;
				}
				I2CRead(i, EP1OUTBUF[3], &EP1INBUF[0]);	// adress, size, data					
				new_data = 1;
				break;
			*/
			//-----------------------------------------------------------------
			case CMD_FPGA_POWER:
				if (EP1OUTBUF[1] == 0){
					FPGA_POWER = 0;
					sts_int_auto_configured = 0;
				}
				else{
					IOD = 0x03;	// Enable Power and disable Reset
					OED = 0x03;	// PROG_B and POWER
					FPGA_POWER = 1;
				}
				EP1INBUF[0] = (FPGA_POWER) ? 1 : 0;
				EP1INBUF[1] = 0xAA;
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_FPGA_RESET:
				FPGA_INT1 = (EP1OUTBUF[1]) ? 1 : 0;
				EP1INBUF[0] = FPGA_INT1;
				EP1INBUF[1] = 0xAA;
				new_data = 1;
				break;
			//-----------------------------------------------------------------
			case CMD_DEV_LOCK:
				if(EP1OUTBUF[1] == 0x01){	// Driver trying to lock device
					if(lock == 0){		// Device is free
						EP1INBUF[0] = 0x22;	// Sucessfull lock
						lock = 1;
					}
					else				// Device is locked
						EP1INBUF[0] = 0x00;	// Already locked
				}
				else{						// Driver trying to unlock device
					if(lock == 1){		// Device is locked
						EP1INBUF[0] = 0x33;	// Sucessfull unlock
						lock = 0;
					}
					else				// Device is unlocked
						EP1INBUF[0] = 0x00;	// Got problem
				}
				new_data = 1;
				break;		
			//-----------------------------------------------------------------
		}
		EP1OUTBC = EP1DATA_COUNT;		// Free input buffer
	}

	if(new_data){						// Have something to send
		if ( !(EP1INCS & 0x02)){		// Can send ?
			EP1INBC = EP1DATA_COUNT;	// Send
			new_data = 0;
		}
	}
}
/*****************************************************************************
* Main loop 
******************************************************************************/
void activity(void){
	ep1_pool();
	int_pin_pool();
}
//*****************************************************************************
