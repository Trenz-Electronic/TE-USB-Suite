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
#include "eeprom.h"
#include "spi.h"

#define MAX_CMD_LENGTH	20

#define VERSION 	"1.03 beta"

BYTE mode = MODE_COM;
WORD eeprom_addr;
BYTE flash_addr_h, flash_addr_m, flash_addr_l;
BYTE command_buf_cnt = 0;					// buffer data count
BYTE command_buf[MAX_CMD_LENGTH+1];			// buffer to store command

/*******************************************************************************
* Main loop 
*******************************************************************************/
void activity(void){

}

/* ProcessLineCoding() Control FPGA/IIC/SPI interfaces according settings
 * received via COM port configuration
 */
void process_line_coding(void){
/*
 * LineCode[0:3] = dwDTERate - Data terminal rate (in bits per second)
 * LineCode[4] = bCharFormat
 *	Stop bits: 0 - 1 stop bit, 1 - 1.5 Stop bits, 2 - 2 Stop bits 
 * LineCode[5] = bParityType 
 *	Parity: 0 - None, 1 - Odd, 2 - Even, 3 - Mark, 4 - Space
 * LineCode[6] = bDataBits 
 *	Data bits 5, 6, 7, 8, or 16
 */
    if( //BAUD=123456
        (EP0BUF[0] == 0x40) && 
        (EP0BUF[1] == 0xE2) && 
        (EP0BUF[2] == 0x01)){
		if(mode != MODE_CLI){	// Start of CLI Mode
			eeprom_addr = 0;
			flash_addr_h = 0;
			flash_addr_m = 0;
			flash_addr_l = 0;
			mode = MODE_CLI;
			EP2FIFOCFG = 0x00; SYNCDELAY;   // EP2 FIFO to Manual mode
			EP6FIFOCFG = 0x00; SYNCDELAY;   // EP6 FIFO to Manual mode
			FIFORESET = 0x82;  SYNCDELAY;   // Reset EP2
			FIFORESET = 0x86;  SYNCDELAY;   // Reset EP6
			FIFORESET = 0x00;  SYNCDELAY;   // Resume normal operation.
			// Arm EP2 buffers
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm First buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Second buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Third buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Fourth buffer
        }
    }
	
    if( // PARITY=O BAUD=222222
        (EP0BUF[0] == 0x0E) &&
        (EP0BUF[1] == 0x64) && 
        (EP0BUF[2] == 0x03) &&
        (EP0BUF[5] == 0x01)){
		if(mode != MODE_EEPROM_WRITE){	// Start of EEPROM Write Mode
			eeprom_addr = 0;
			mode = MODE_EEPROM_WRITE;
			EP2FIFOCFG = 0x00; SYNCDELAY;   // EP2 FIFO to Manual mode
			EP6FIFOCFG = 0x00; SYNCDELAY;   // EP6 FIFO to Manual mode
			FIFORESET = 0x82;  SYNCDELAY;   // Reset EP2
			FIFORESET = 0x86;  SYNCDELAY;   // Reset EP6
			FIFORESET = 0x00;  SYNCDELAY;   // Resume normal operation.
			// Arm EP2 buffers
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm First buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Second buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Third buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Fourth buffer
			FPGA_POWER = 0;
		}
	}

    if( // PARITY=E BAUD=333333
        (EP0BUF[0] == 0x15) && 
        (EP0BUF[1] == 0x16) && 
        (EP0BUF[2] == 0x05) &&
        (EP0BUF[5] == 0x02)){
		if(mode != MODE_FLASH_WRITE){		// Start of EEPROM Write Mode
			eeprom_addr = 0;
			mode = MODE_FLASH_WRITE;
			EP2FIFOCFG = 0x00; SYNCDELAY;   // EP2 FIFO to Manual mode
			EP6FIFOCFG = 0x00; SYNCDELAY;   // EP6 FIFO to Manual mode
			FIFORESET = 0x82;  SYNCDELAY;   // Reset EP2
			FIFORESET = 0x86;  SYNCDELAY;   // Reset EP6
			FIFORESET = 0x00;  SYNCDELAY;   // Resume normal operation.
			// Arm EP2 buffers
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm First buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Second buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Third buffer
			OUTPKTEND = 0x82; SYNCDELAY;    // Arm Fourth buffer
			FPGA_POWER = 0;
		}
	}
}

void print_ep6_char(char c){
	EP6FIFOBUF[0] = c;
	EP6BCH = 0;
	EP6BCL = 1;
}

void print_ep6_byte(char c){
	BYTE t;
	t = c >> 4;
	EP6FIFOBUF[0] =  (t < 10) ? (t + '0') : (t - 10 + 'A');
	t = c & 0x0F;
	EP6FIFOBUF[1] =  (t < 10) ? (t + '0') : (t - 10 + 'A');
	EP6BCH = 0;
	EP6BCL = 2;
}

unsigned char hex_char_h(unsigned char c){
	unsigned char t;
	t = c >> 4;
	return (t < 10) ? (t + '0') : (t - 10 + 'A');
}

char hex_char_l(char c){
	char t;
	t = c & 0x0F;
	return (t < 10) ? (t + '0') : (t - 10 + 'A');
}

void print_ep6_prompt(void){
	EP6FIFOBUF[0] = '>';
	EP6FIFOBUF[1] = ' ';
	EP6BCH = 0;
	EP6BCL = 2;
}

void print_ep6_newline(void){
	EP6FIFOBUF[0] = '\r';
	EP6FIFOBUF[1] = '\n';
	EP6BCH = 0;
	EP6BCL = 2;
}

void print_ep6_string(char *str){
	BYTE i = 0;
	while (*str)
		EP6FIFOBUF[i++] = *str++;
	EP6FIFOBUF[i++] = '\r';
	EP6FIFOBUF[i++] = '\n';
	EP6BCH = 0;
	EP6BCL = i;
}

BYTE string_match(char *stra, char *strb){
	while(*stra){
		if(*stra++ != *strb++)
			return 0;
	}
	return 1;
}

BYTE printable(BYTE c){
	if(((c >= 'a') && (c <='z')) || ((c >= '0') && (c <='9')) || (c == ' '))
		return 1;
	else
		return 0;
}

void process_flash_status(void){
	BYTE sr1, sr2;
	char msg[] = "SR1 ?? SR2 ??";
	
	FLASH_ENABLE;			// assert chip select
    putcSPI(SPI_RDSR1);		// Read Status Register 1
	sr1 = getcSPI();
   	FLASH_DISABLE;			// negate chip select
	FLASH_ENABLE;			// assert chip select
    putcSPI(SPI_RDSR2);		// Read Status Register 1
	sr2 = getcSPI();
   	FLASH_DISABLE;			// negate chip select
	msg[4] = hex_char_h(sr1);
	msg[5] = hex_char_l(sr1);
	msg[11] = hex_char_h(sr2);
	msg[12] = hex_char_l(sr2);
	print_ep6_string(msg);
}

void process_flash_id(void){
	BYTE mid, did, uid;
	char msg[] = "IDCODE ??????";
	
	busy_polling();
	FLASH_ENABLE;        	// assert chip select
    putcSPI(SPI_RDID);  	// get ID command
	mid = getcSPI();
	did = getcSPI();
	uid = getcSPI();
 	FLASH_DISABLE;       	// negate chip select
	msg[7] = hex_char_h(mid);
	msg[8] = hex_char_l(mid);
	msg[9] = hex_char_h(did);
	msg[10] = hex_char_l(did);
	msg[11] = hex_char_h(uid);
	msg[12] = hex_char_l(uid);
	print_ep6_string(msg);
}

void process_flash_erase(void){
	BYTE status_reg = 1;
	busy_polling();
    FLASH_ENABLE;		// assert chip select
    putcSPI(SPI_WREN);  // write status command
    FLASH_DISABLE;		// negate chip select
    FLASH_ENABLE;		// assert chip select
    putcSPI(SPI_BE);  	// Bulk Erase command
    FLASH_DISABLE;      // negate chip select
	while((status_reg & 0x01) != 0){// Pulling Flash Busy
        FLASH_ENABLE;			//assert chip select
        putcSPI(SPI_RDSR1);		//send read status command
        status_reg = getcSPI();	//read data byte
        FLASH_DISABLE;			//negate chip select
	}
	print_ep6_string("Done");
}

void process_flash_unlock(void){
	busy_polling();
    FLASH_ENABLE;		// assert chip select
    putcSPI(SPI_WREN);  // write status command
    FLASH_DISABLE;		// negate chip select
    FLASH_ENABLE;		// assert chip select
    putcSPI(SPI_WRSR);  // Write SR
    putcSPI(0x00);  	// Clear all bits
    FLASH_DISABLE;      // negate chip select
	print_ep6_string("Done");
}

void process_flash_read(void){
	__xdata char msg[] = "?? ?? ?? ?? ?? ?? ?? ?? ";
	BYTE i,d;
	
	busy_polling();
    FLASH_ENABLE;				// assert chip select
    putcSPI(SPI_READ);			// send read command
    putcSPI(flash_addr_h);		// send high byte of address
    putcSPI(flash_addr_m);		// send high byte of address
    putcSPI(flash_addr_l);		// send low byte of address
	for(i = 0; i < 8; i++){
		d = getcSPI();
		msg[i*3] = hex_char_h(d);
		msg[i*3+1] = hex_char_l(d);
		msg[i*3+2] = ' ';
	}
	FLASH_DISABLE;				// negate chip select
	if(flash_addr_l == (256 - 8)){
		flash_addr_l = 0x00;
		if(flash_addr_m == 0xFF)
			flash_addr_h++;
		flash_addr_m++;
	}
	else
		flash_addr_l += 8;
	print_ep6_string(msg);
}

void process_fpga_status(void){
	__xdata char msg[7] = {'d','i','s','p','-','-',0};
	if(FPGA_DONE)
		msg[0] = 'D';
		
	if(FPGA_INIT)
		msg[1] = 'I';
	
	if(FPGA_POWER)
		msg[2] = 'S';
		
	if(FPGA_PROG)
		msg[3] = 'P';
		
	if(FPGA_INT0)
		msg[4] = '0';
		
	if(FPGA_INT1)
		msg[5] = '1';

	print_ep6_string(msg);
}

void process_fpga_reset(void){
	FPGA_PROG = 0;
	SYNCDELAY; SYNCDELAY; SYNCDELAY;
	FPGA_PROG = 1;
	print_ep6_string("Done");
}

void process_command(void){
	BYTE processed = 0;
	
	if(string_match("ver",command_buf)){
		print_ep6_string(VERSION);
		print_ep6_string("Build " __DATE__ " " __TIME__);
		processed = 1;
	}
	
	if(string_match("power off",command_buf)){
		FPGA_POWER = 0;
		print_ep6_string("OK");
		processed = 1;
	}

	if(string_match("power on",command_buf)){
		FPGA_POWER = 1;
		print_ep6_string("OK");
		processed = 1;
	}
	
	if(string_match("flash status",command_buf)){
		//FPGA_POWER = 0;
		FPGA_PROG = 0;
		OED = 0x73;		// Configure MOSI, CCLK, CSO_B, PS_ON, PROG as outputs
		process_flash_status();
		processed = 1;
	}
	
	if(string_match("flash id",command_buf)){
		//FPGA_POWER = 0;
		FPGA_PROG = 0;
		OED = 0x73;		// Configure MOSI, CCLK, CSO_B, PS_ON, PROG as outputs
		process_flash_id();
		processed = 1;
	}
	
	if(string_match("flash erase",command_buf)){
		//FPGA_POWER = 0;
		FPGA_PROG = 0;
		OED = 0x73;		// Configure MOSI, CCLK, CSO_B, PS_ON, PROG as outputs
		process_flash_erase();
		processed = 1;
	}
	
	if(string_match("flash unlock",command_buf)){
		//FPGA_POWER = 0;
		FPGA_PROG = 0;
		OED = 0x73;		// Configure MOSI, CCLK, CSO_B, PS_ON, PROG as outputs
		process_flash_unlock();
		processed = 1;
	}
	
	if(string_match("flash read",command_buf)){
		//FPGA_POWER = 0;
		FPGA_PROG = 0;
		OED = 0x73;		// Configure MOSI, CCLK, CSO_B, PS_ON, PROG as outputs
		process_flash_read();
		processed = 1;
	}
	
	if(string_match("fpga status",command_buf)){
		process_fpga_status();
		processed = 1;
	}

	if(string_match("fpga reset",command_buf)){
		OED = 0x03;		// Confifure only PS_ON and PROG as outputs
		process_fpga_reset();
		processed = 1;
	}

	if(string_match("quit",command_buf)){
		OED = 0x03;		// Confifure only PS_ON and PROG as outputs
		mode = MODE_COM;		
		EP2FIFOCFG = 0x10; SYNCDELAY;   // Configure EP2 FIFO in 8-bit AutoOut mode
		EP6FIFOCFG = 0x0C; SYNCDELAY;   // Configure EP6 FIFO in 8-bit AutoIn mode
		FIFORESET = 0x82;  SYNCDELAY;   // Reset EP2
		FIFORESET = 0x86;  SYNCDELAY;   // Reset EP6
		FIFORESET = 0x00;  SYNCDELAY;   // Resume normal operation.
		// Arm EP2 buffers
		OUTPKTEND = 0x82; SYNCDELAY;    // Arm First buffer
		OUTPKTEND = 0x82; SYNCDELAY;    // Arm Second buffer
		OUTPKTEND = 0x82; SYNCDELAY;    // Arm Third buffer
		OUTPKTEND = 0x82; SYNCDELAY;    // Arm Fourth buffer
		FPGA_POWER = 1;
		processed = 1;
	}

	if(!processed)
		print_ep6_string("Invalid command");
}

void process_cli(void){
	BYTE i,j;
	if( !( EP2468STAT & 0x01 )){// EP2 FIFO NOT empty, host sent packet
		FIFORESET = 0x80; SYNCDELAY;
		
		for(i = 0; i < EP2BCL; i++){	// Ignore long (>256 bytes commands)
			if(command_buf_cnt < MAX_CMD_LENGTH){ 
				if(printable(EP2FIFOBUF[i])){
					command_buf[command_buf_cnt++] = EP2FIFOBUF[i];
					print_ep6_char(EP2FIFOBUF[i]);	// Echo
				}
				if(EP2FIFOBUF[i] == 0x0D){		// Enter
					EP2FIFOBUF[i] = 0;
					command_buf_cnt = 0;
					print_ep6_newline();
					process_command();
					print_ep6_prompt();
					break;
				}
				if(EP2FIFOBUF[i] == 0x7F){		// Backspace
					print_ep6_char(EP2FIFOBUF[i]);	// Echo
					command_buf_cnt--;
				}
			}
			else{
				print_ep6_string("Error");
				print_ep6_prompt();
				command_buf_cnt = 0;
				for(j = 0; j < MAX_CMD_LENGTH; j++)
					command_buf[j] = 0;
				break;
			}
		}
		
		FIFORESET = 0x00; SYNCDELAY;
		OUTPKTEND = 0x82; // SKIP=1, do NOT pass buffer on to master
    }
}

void process_eeprom_write(void){
	WORD packet_length,i,c;
	if( !( EP2468STAT & 0x01 )){// EP2 FIFO NOT empty, host sent packet
		FIFORESET = 0x80; SYNCDELAY;
		packet_length = (EP2BCH << 8) | EP2BCL;
		i = 0;
		while(i < packet_length){
			if((packet_length - i) >= 32){
				EEPROMWrite(eeprom_addr, 32, &EP2FIFOBUF[i]);
				i += 32;
				eeprom_addr += 32;
			}
			else {
				c = packet_length - i;
				EEPROMWrite(eeprom_addr, (BYTE)c, &EP2FIFOBUF[i]);
				i += c;
				eeprom_addr += c;
			}
		}
		FIFORESET = 0x00; SYNCDELAY;
		OUTPKTEND = 0x82; // SKIP=1, do NOT pass buffer on to master
	}
}

void process_flash_write(void){
	WORD packet_length,i,c;
	WORD block_remainder, packet_remainder;
	if( !( EP2468STAT & 0x01 )){// EP2 FIFO NOT empty, host sent packet
		FIFORESET = 0x80; SYNCDELAY;
		packet_length = (EP2BCH << 8) | EP2BCL;
		i = 0;
		while(i < packet_length){
			block_remainder = 256 - flash_addr_l;
			packet_remainder = (packet_length > 256) ? 256 : packet_length;
			c = min(block_remainder, packet_remainder);
			if((flash_addr_m == 0) && (flash_addr_l == 0))
				sector_erase(flash_addr_h);
			page_write(flash_addr_h, flash_addr_m, flash_addr_l, &EP2FIFOBUF[i], c);
			i += c;
			// recalculate address
			if((c + flash_addr_l) == 256){
				flash_addr_l = 0x00;
				if(flash_addr_m == 0xFF){
					flash_addr_m = 0x00;
					flash_addr_h += 1;
				}
				else
					flash_addr_m += 1;
			}
			else
				flash_addr_l += c;
		}
		FIFORESET = 0x00; SYNCDELAY;
		OUTPKTEND = 0x82; // SKIP=1, do NOT pass buffer on to master
	}
}

