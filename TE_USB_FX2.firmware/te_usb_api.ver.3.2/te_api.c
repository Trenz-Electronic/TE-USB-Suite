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

/*
EP1OUTBUF : EP1 (command) buffer from USB FX2 microcontroller to host computer => 
EP1OUTBUF[0] is written by host computer's software 
C++ TE_USB_FX2_SendCommand(...,command,...)  or 
C# TE_USB_FX2_SendCommand(...,command,...) or 
libusb(x) C libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, command, x, &actual_length, 1000) 
used with command[0] = USB FX2 API Command.
*/
/*
EP1INBUF: EP1 (reply) buffer from host computer to USB FX2 microcontroller => 
It is written in the host computer's SW byte array reply[] of 
C++ TE_USB_FX2_SendCommand(...,command,...) or 
C# TE_USB_FX2_SendCommand(...,command,...) or 
libusb(x) C libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, command, x, &actual_length, 1000) 
used with command[0] = USB FX2 API Command.
*/

#include "te_api.h"
#include "syncdelay.h"
#include "fx2regs.h"
#include "fpga.h"
#include "i2c.h"
#include "spi.h"
//==============================================================================
// Global definitions 
__xdata BYTE	sts_fifo_error = 0;     //fifo error flag status
__xdata BYTE	sts_current_mode = 1;   //current mode status
__xdata BYTE	sts_flash_busy = 1;     //SPI Flash memory busy status
__xdata BYTE	sts_fpga_prog = 1;      //FPGA DONE PIN status
__xdata BYTE	sts_booting = 0;
__xdata BYTE	sts_i2c_new_data = 0;
__xdata BYTE	sts_int_auto_configured = 0; 
__xdata BYTE	sts_high_speed_mode = 0; //HighSpeedMode USB FX2 enabled status

__xdata BYTE	iar_pin_status = 0;     //Interrupt AutoResponse Pin Status
__xdata BYTE	iar_adress = 0x3F;      //Interrupt AutoResponse I2C Address of I2C Slave device (normally the FPGA)
//The address must be set properly for an I2C Slave device to be recognized by FX2 microcontroller configured 
//as I2C master. Address  63 (0x3F) is used in all reference designs
__xdata BYTE	iar_count = 12;         //Interrupt AutoResponse Number of I2C of bytes to read from iar_adress
//The transactions of these I2C connections are usually 12 bytes long; both MB2FX2_REGs and FX22MB_REGs are 
//3 register of 4 bytes => 3*4 = 12 bytes (aka 0x0C)
__xdata BYTE	iar_int_idx = 0;        //Interrupt AutoResponse Index

__xdata BYTE	auto_response_data[32]; //AutoResponse bytes array. Normally, only auto_response_data[0-11] are used.

__xdata BYTE	lock = 0;               //Flag: device unlocked
BYTE prev_done = 0;                     //Flag: internal test not done before
BYTE cmd_cnt = 0;                       //Number of command arrived at EP1
//==============================================================================
/*******************************************************************************
* Pull INT pool aka Interrupt Pin polling aka
* Pull INT pool data from an I2C address when FPGA's chip rise INT0 pin and an autoresponse 
* interrupt is preconfigured by host computer's SW. The FX2 microcontroller pull 
* (using I2C in int_pin_pool()) x (x defined by EP1OUTBUF[2]) number of bytes 
* from y I2C address (y defined by EP1OUTBUF[1]). 
* The host computer's SW should use a polling procedure to retrieve the I2C bytes read 
* (and stored) by FX2 microcontroller.
* 
* NOTE
* The interrupt request USB_INT of XPS_I2C_SLAVE custom IP block is 
* served by a polling function (int_pin_pool()) that run in the superloop while(1) of fw.c, and not by an ISR.
* int_pin_pool() is wrapped with ep1_pool() inside activity() function.
* 
* It is used with MB Command connection type A. If XPS_I2C_SLAVE custom IP block is synthetized on the FPGA.
* Command (aka host computer software's MB command to FPGA's MB2FX2_REGs) and reply 
* (aka FPGA's MB2FX2_REGs to FX2 microcontroller firmware's autoresponse bytes array (3rd section of code) and 
* host computer software's reply bytes array (4th section of code)).
* 
* It is not used with MB Command: connection type B. If XPS_I2C_SLAVE custom IP block is synthetized on the FPGA.
* Command (aka host computer software's MB command to FPGA's MB2FX2_REGs) with no reply.
* 
* Pull INT pool. Every Logical Architecture Layer.
* If an autoresponse interrupt is preconfigured (by host computer'SW using SET_INTERRUPT => CMD_SET_AUTORESPONSE =>
* sts_int_auto_configured = 1) and an interrupt request is rised by FPGA chip (FPGA_INTERRUPT_REQUEST = 1 
* => pin INT0=1 => FPGA_INT0=1), FX2 microcontroller firmware reads (using I2C) a maximum of 32 byte (in the 
* firmware a maximum of 12 is preconfigured but it could be overwritten by a host software TE API Command 
* SET_INTERRUPT => CMD_SET_AUTORESPONSE => iar_adress = EP1OUTBUF[1];iar_count = EP1OUTBUF[2];) from an I2C address.
* The I2C bytes data are copied in the byte array auto_response_data by the int_pin_pool() firmware function. 
* This byte array coul be pulled out by host computer'SW using SET_INTERRUPT ( => CMD_GET_AUTORESPONSE => 
* for(i = 0; i < 32; i++) EP1INBUF[i+1] = auto_response_data[i];). 
* 
* Pull INT pool. Reference Design: Logical Archirecture Layer = Reference Architecture Layer
* It is usually used with XPS_I2C_SLAVE custom IP block for command, settings and status communication.
* When MicroBlaze write data to MB2FX2_REG0, an interrupt request (USB_INT) is rised by FPGA chip 
* (USB_INT =1=> pin INT0 =1 => FPGA_INT0 =1) is rised. 
* This pin is connected to PA0/INT0 pin of FX2 microcontroller. 
* If an autoresponse interrupt is preconfigured (sts_int_auto_configured = 1) and a FPGA_INT0=1, the FX2 
* microcontroller firmware reads all MB2FX2 registers. The registers value are copied in the byte array 
* auto_response_data by the int_pin_pool() firmware function. This byte array coul be pulled out by host 
* computer'SW using SET_INTERRUPT ( => CMD_GET_AUTORESPONSE => 
* for(i = 0; i < 32; i++) EP1INBUF[i+1] = auto_response_data[i];)
*******************************************************************************/

//"AUTORESPONSE YES: 3rd section of code to run if MB Command connection Type A is realized"
//for example https://wiki.trenz-electronic.de/display/TEUSB/FX22MB_REG0_GETVERSION+command
void int_pin_pool(void){
	if (sts_int_auto_configured){           //if (Interrupt AutoResponse Configured == 1) 
	        //pin INT0 (aka FPGA_INT0) is 1 if USB_INT=1 (Reference Design case)
		if(FPGA_INT0){                  //if (pin INT0 ==1)  
			iar_pin_status = 1;     //Interrupt AutoResponse Pin Status = 1;
			if (iar_count > 32)     //Interrupt AutoResponse Count > 32
				iar_count = 32; //Interrupt AutoResponse Count = 32
			I2CRead2(iar_adress, iar_count, &auto_response_data[0]); // adress, size, data
			iar_int_idx = iar_int_idx + 1; //iar_int_idx is Interrupt AutoResponse Index
			sts_i2c_new_data = 1;   //New I2C data bytes are available, the MB2FX2_REGs in particular
		}
		else 
			iar_pin_status = 0;     //Interrupt AutoResponse Pin Status = 0;
	}
}

/*******************************************************************************
* Pull EP1 data aka Polling for EP1 data aka
* Pull EP1 data aka pull possible TE API Commands (FW APIs) 
* from USB connection (with host computer's SW) 
* and execute the function requested by host computer's SW using TE API Commands.
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
		prev_done = 1;                          //Flag: internal test done before
	}

	if( !( EP1OUTCS & 0x02) ){ 			// Got something if availble at EP1OUTBUF
		cmd_cnt++;                              // Increment the number of commnad arrived
		for (i = 0; i < 0x40; i++)              
			EP1INBUF[i] = 0xFF;	        // Fill output "reply" buffer
			
		switch(EP1OUTBUF[0]){			// Decode USB FX2 API command
			// https://wiki.trenz-electronic.de/display/TEUSB/USB+FX2+API+Commands
			//-----------------------------------------------------------------
			default:
			// If the first byte received in EP1 is not a USB FX2 API Command, 
			//the case CMD_READ_VERSION is executed
			
			//If the first byte received in EP1 is CMD_READ_VERSION
			//the case CMD_READ_VERSION is executed
			//https://wiki.trenz-electronic.de/display/TEUSB/READ_VERSION+command
			//This command returns 4 bytes representing the USB FX2 firmware version.
			case	CMD_READ_VERSION:
				EP1INBUF[0] = fx2_ver_maj_;  //FX2 Firmware version major number
				EP1INBUF[1] = fx2_ver_min_;  //FX2 Firmware version minor number
				EP1INBUF[2] = fx2_tip_maj_;  //Device Major Number
				EP1INBUF[3] = fx2_tip_min_;  //Device Minor Number
				new_data = 1;                //A flag is raised:New data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			//This command sets address and number of bytes to read from I2C bus when 
			//USB_INT interrupt request is received on INT0 pin.
			//Another name for this USB FX2 API command could be SET_AUTORESPONSE.
			//"AUTORESPONSE YES: 1st section of code to run if MB Command connection Type A is realized"
			//https://wiki.trenz-electronic.de/display/TEUSB/SET_INTERRUPT+command
			case	CMD_SET_AUTORESPONSE:
				sts_int_auto_configured = 1; //Interrupt AutoResponse Configured =1, aka configured/enabled
				iar_adress = EP1OUTBUF[1];   //I2C address of the bytes to read, normally 0x3C, from EP1OUTBUF byte buffer
				iar_count = EP1OUTBUF[2];    //Number of bytes to read from I2C bus, normally 0x0C (aka 12), from EP1OUT byte buffer
				iar_int_idx = 0;             //Interrupt AutoResponse Index = 0
				new_data = 1;                //A flag is raised: new data are availble in EP1INBUF for the host computer's SW ???
				break;
			//-----------------------------------------------------------------
			//This command pulls the number of received interrupts request and the received data (number of bytes set by 
			//a previous SET_INTERRUPT command aka CMD_SET_AUTORESPONSE) from the USB FX2 microcontroller.
			//Another name for this USB FX2 API command could be GET_AUTORESPONSE.
			//"AUTORESPONSE YES: 4th section of code to run if MB Command connection Type A is realized"
			//https://wiki.trenz-electronic.de/display/TEUSB/GET_INTERRUPT+command
			case	CMD_GET_AUTORESPONSE:
				EP1INBUF[0] = iar_int_idx;   //Interrupt AutoResponse Index is assigned to the first byte of EP1INBUF byte buffer
				for(i = 0; i < 32; i++)      //Copy autoresponse data (normally MB2FX2_REGs) in the EP1INBUF byte buffer
					EP1INBUF[i+1] = auto_response_data[i];
				iar_int_idx = 0;             //Interrupt AutoResponse Index = 0
				new_data = 1;                //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			case	CMD_SWITCH_MODE:
				sts_current_mode = 1;        
				new_data = 1;                //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				EP1INBUF[0] = EP1OUTBUF[1];
				break;
			//-----------------------------------------------------------------
			//This command returns 8 bytes representing the USB FX2 microcontroler and FPGA DONE pin status
			//https://wiki.trenz-electronic.de/display/TEUSB/READ_STATUS+command
			case	CMD_READ_STATUS:
				sts_flash_busy = get_flash_busy(); //Is the SPI Flash busy?
				sts_booting = FPGA_DONE;           //Is the DONE PIN of FPGA high?
				sts_fpga_prog = 0xaa;
				sts_high_speed_mode = (USBCS & bmHSM) ? 1 : 255; //Is the FX2 microcontroller configured for High Speed USB connection?
				new_data = 1;		           //A flag is rised: new data are availble in EP1INBUF for the host computer's SW			
				EP1INBUF[0] = sts_fifo_error;      //Are there any EP FIFO errors rised?
				EP1INBUF[1] = sts_current_mode;    //Current mode
				EP1INBUF[2] = sts_flash_busy;      //Is the SPI Flash busy?
				EP1INBUF[3] = sts_fpga_prog;       //0xaa
				EP1INBUF[4] = sts_booting;         //Is the DONE PIN of FPGA high?
				EP1INBUF[5] = sts_i2c_new_data;    //Are new I2C data byte available?
				EP1INBUF[6] = sts_int_auto_configured; //Is the Interrupt AutoResponse configured?
				EP1INBUF[7] = sts_high_speed_mode; //Is the FX2 microcontroller configured for High Speed USB connection?
				sts_i2c_new_data = 0;              //A flag is lowered: new I2C data byte are no longer available ???
				break;
			//-----------------------------------------------------------------
			//This command resets the FIFO of the selected (by EP1OUTBUF[1]) endpoint 
			//(EP1OUTBUF[1] = 2/4/6, all endpoints with the exception of control endpoint 
			//for any other value)
			//https://wiki.trenz-electronic.de/display/TEUSB/RESET_FIFO+command
			case CMD_RESET_FIFO_STATUS:
				sts_fifo_error = 0;           // Reset FIFO error flag
				FIFORESET = 0x80;  SYNCDELAY; // NAK all requests from host.
				switch(EP1OUTBUF[1]){         // Select the EPx FIFO to reset using EP1OUTBUF[1] aka Command[1]
					case 2:               // EP2 FIFO reset
						EP2FIFOCFG = 0x4C;  SYNCDELAY;
						FIFORESET = 0x02;  SYNCDELAY;
						break;
					case 4:               // EP4 FIFO reset  
						EP4FIFOCFG = 0x4C;  SYNCDELAY;
						FIFORESET = 0x04;  SYNCDELAY;
						break;
					case 6:               // EP6 FIFO reset
						EP6FIFOCFG = 0x4C;  SYNCDELAY;
						FIFORESET = 0x06;  SYNCDELAY;
						break;
					default:	      // EP2,EP4,EP6 FIFOs reset
						EP2FIFOCFG = 0x4C;  SYNCDELAY;
						EP4FIFOCFG = 0x4C;  SYNCDELAY;
						EP6FIFOCFG = 0x4C;  SYNCDELAY;
						EP8FIFOCFG = 0x10;  SYNCDELAY;
						FIFORESET = 0x02;  SYNCDELAY;
						FIFORESET = 0x04;  SYNCDELAY;
						FIFORESET = 0x06;  SYNCDELAY;
				}
				FIFORESET = 0x00;  SYNCDELAY; // Resume normal operation.
				new_data = 1;                 //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			//This command writes data (from 1 to 59 bytes) to the requested SPI Flash address. 
			//Afterwards, CMD_FLASH_READ command is executed and the Firmware reads back data from SPI Flash memory. 
			//These reads backs data will be retrieved by host computer SW in a reply packet. 
			//https://wiki.trenz-electronic.de/display/TEUSB/FLASH_WRITE+command
			case CMD_FLASH_WRITE:
				if (EP1OUTBUF[4] > 59) EP1OUTBUF[4] = 59; //If more than 59 bytes are requested by host computer's SW, a limit of 59 is forced by the FW.
				page_write(EP1OUTBUF[1], EP1OUTBUF[2], EP1OUTBUF[3], &EP1OUTBUF[5], EP1OUTBUF[4]);
				              //highest,         high,      low adr,      read_ptr,         size
				// break; It is not an error. The final break of CMD_FLASH_WRITE is commented out; 
				//in this way the CMD_FLASH_READ is executed after CMD_FLASH_WRITE.
			//-----------------------------------------------------------------
			//This command reads data (from 1 to 64 bytes) from the requested SPI Flash address.
			//https://wiki.trenz-electronic.de/display/TEUSB/FLASH_READ+command
			case CMD_FLASH_READ:					
				if (EP1OUTBUF[4] > 64) EP1OUTBUF[4] = 64;
				page_read(EP1OUTBUF[1], EP1OUTBUF[2], EP1OUTBUF[3], &EP1INBUF[0], EP1OUTBUF[4]);
				             //highest,         high,      low adr,      read_ptr,         size
				new_data = 1;//A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;	//This is also the break of CMD_FLASH_WRITE.		
			//-----------------------------------------------------------------
			//This command starts an entire SPI Flash memory erase process. A full SPI Flash memory 
			//erase process may take up to 30 seconds for M25PS32 SPI Flash chip 
			//(check your SPI Flash data sheet for actual time values). 
			//To control Flash busy status, use READ_STATUS command. 
			//Reply[2]=EP1INBUF[2] = sts_flash_busy = get_flash_busy();
			case CMD_FLASH_ERASE:
				// busy_polling(); // On some modules busy_polling() cause API error - better to do it from software side
				bulk_erase(); //This function erase all the the SPI Flash sectors
				new_data = 1; //A flag is rised: new data are availble in EP1INBUF for the host computer's SW
				sts_flash_busy = 1; //A flag is raised: the SPI Flash is busy
				break;
			//-----------------------------------------------------------------
			//This command erase a single SPI Flash sector identified by EP1OUTBUF[1] aka Command[1].
			//TO DO: create FLASH_SECTOR_ERASE page in the Trenz Electonic wiki
			case CMD_SECTOR_ERASE:
				sector_erase(EP1OUTBUF[1]); //This function erase a single SPI Flash sector identified by EP1OUTBUF[1] aka Command[1]
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				sts_flash_busy = 1; //A flag is raised: the SPI Flash is busy
				break;
			//-----------------------------------------------------------------
			//This command sends instruction(s) (SPI command(s)) to the SPI Flash. 
			//See SPI Flash data sheet for detailed command description.
			//https://wiki.trenz-electronic.de/display/TEUSB/FLASH_WRITE_COMMAND+command
			//See also https://wiki.trenz-electronic.de/display/TEUSB/SPI+Flash+Commands
			case CMD_FLASH_WRITE_COMMAND:
				EP1INBUF[0] = 0x55;
				spi_command(EP1OUTBUF[1], &EP1OUTBUF[3], EP1OUTBUF[2], &EP1INBUF[1]);
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			//This command writes data (from 1 to 60 bytes) to the requested EEPROM address. 
			//Normally, it writes a 1 to 60 bytes of a new USB FX2 firmware. 
			//Afterwards, CMD_EEPROM_READ command is executed and the Firmware reads back data from EEPROM. 
			//These reads backs data will be retrieved by host computer SW in a reply packet.
			//https://wiki.trenz-electronic.de/display/TEUSB/EEPROM_WRITE+command
			case CMD_EEPROM_WRITE:					
				adr = EP1OUTBUF[1];
				adr = (adr << 8) + EP1OUTBUF[2];
				if (EP1OUTBUF[3] > 32) EP1OUTBUF[3] = 32;				
				EEPROMWrite(adr, EP1OUTBUF[3], &EP1OUTBUF[4]);	
				      // adress,         size,          data
				// break; It is not an error. The final break of CMD_EEPROM_WRITE is commented out; 
				//in this way the CMD_EEPROM_READ is executed after CMD_EEPROM_WRITE.
			//-----------------------------------------------------------------
			//This command reads data (from 1 to 64 bytes) from requested EEPROM address.
			//Normally, it read a 1 to 64 bytes of the current USB FX2 firmware.
			//These reads data will be retrieved by host computer SW in a reply packet.
			//https://wiki.trenz-electronic.de/display/TEUSB/EEPROM_READ+command
			case CMD_EEPROM_READ:
				adr = EP1OUTBUF[1];
				adr = (adr << 8) + EP1OUTBUF[2];
				EEPROMRead(adr, EP1OUTBUF[3], &EP1INBUF[0]);	
				     // adress,         size,         data
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;	//This is also the break of CMD_EEPROM_WRITE.		
			//-----------------------------------------------------------------
			//This command returns the FIFO status of all used endpoints. 
			//Status is the value of EP2CS, EP4CS, EP6CS, EP8CS, EP2FIFOBCH, EP4FIFOBCH, EP6FIFOBCH, 
			//EP8FIFOBCH, EP2FIFOBCL, EP4FIFOBCL, EP6FIFOBCL, EP8FIFOBCL, EP2FIFOFLGS, EP4FIFOFLGS, 
			//EP6FIFOFLGS and  EP8FIFOFLGS USB FX2 registers. 
			//See USB FX2 documentation for detailed information.
			//https://wiki.trenz-electronic.de/display/TEUSB/FIFO_STATUS+command
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
			//This command writes data (from 1 to 32 bytes) to the requested I2C address.
			//https://wiki.trenz-electronic.de/display/TEUSB/I2C_WRITE+command
			//Used with MB API Commands https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10620639
			//"AUTORESPONSE NO: "only" section of code to run if MB Command connection Type B is realized"
			//"AUTORESPONSE YES: 1st section of code to run if MB Command connection Type A is realized"
			case CMD_I2C_WRITE:
				I2CWrite(EP1OUTBUF[1], EP1OUTBUF[2], &EP1OUTBUF[3]);	
				            // adress,         size,          data
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			//This command reads data (from 1 to 32 bytes) from requested I2C address.
			//Reply packet contains requested data.
			case CMD_I2C_READ:
				I2CRead(EP1OUTBUF[1], EP1OUTBUF[2], &EP1INBUF[0]);	
				           // adress,         size,         data
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
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
			//This command controls some FPGA power supply sources.
			//https://wiki.trenz-electronic.de/display/TEUSB/POWER+command
			case CMD_FPGA_POWER:
				if (EP1OUTBUF[1] == 0){  //If Command[1] == 0  desired Power OFF state
					FPGA_POWER = 0;  
					//FPGA_POWER = 0 => FX2_PS_EN = 0; => (if switch correctly setted) PS_EN = 0
					//TE0300 DIP Slide Switch S3 (Configuration): https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10617832
					//TE0320 Slide Switch S2 (Configuration): https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10617411
					//TE0630 Dip Switch S1B (Configuration): https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10617860
					
					sts_int_auto_configured = 0; 
					//Interrupt AutoResponse Configured =0, aka NOT configured/enabled
				}
				else{ //If Command[1] == 1  desired Power ON state
					IOD = 0x03;	
					// Enable FX2_PS_EN and FX2_PROG_B as inputs  => 
					// 0b00000011; => PD1,PD0 pins input enabled;    // Enable Power and disable Reset???
					OED = 0x03;
					// Enable FX2_PS_EN and FX2_PROG_B as outputs => 
					// 0b00000011; => PD1,PD0 pins output enabled;   //  POWER and PROG_B
					FPGA_POWER = 1;
					//FPGA_POWER = 1 => FX2_PS_EN = 1; => (if switch correctly setted) PS_EN = 1
					//TE0300 DIP Slide Switch S3 (Configuration): https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10617832
					//TE0320 Slide Switch S2 (Configuration): https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10617411
					//TE0630 Dip Switch S1B (Configuration): https://wiki.trenz-electronic.de/pages/viewpage.action?pageId=10617860
					
				}
				EP1INBUF[0] = (FPGA_POWER) ? 1 : 0; 
				//The first byte of "Reply[]" (aka EP1INBUF[0]) is equal to 
				//the second byte of "Command[]" (aka EP1OUTBUF[1]).
				
				EP1INBUF[1] = 0xAA;
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			// TO DO: add FPGA_RESET to Trenz Electronic Wiki
			// In TE USB FX2 reference firmware (here), the "fpga reset" command drive pin PD1 to '1' 
                        // or '0' on the base of EP1OUTBUF[1] (Command[1]) value.
			// Currently the Reference FPGA image does NOT use INT1 pin for FPGA RESET;
			// #NET  USB_INT1_pin is commented out (#) in 
			// 1) TE0300 reference design constraints file: https://github.com/Trenz-Electronic/TE03XX-Reference-Designs/blob/master/reference-TE0300/data/system.ucf
			// 2) TE0320 reference design constraints file: https://github.com/Trenz-Electronic/TE03XX-Reference-Designs/blob/master/reference-TE0320/data/system.ucf
			// 3) TE0630 reference design constraints file: https://github.com/Trenz-Electronic/TE063X-Reference-Designs/blob/master/reference-TE0630/data/system.ucf
			// 
			// In vcom reference firmware, 
			// (https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/vcom-asyn/te_vcom.c
			// or https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/vcom-cli/te_vcom.c)
			// the "fpga reset" command execute FPGA reset sequence by driving FPGA 
			// FX2_PROG_B pin low and high after some delay. 
                        // FPGA_PROG = 0; //FX2_PROG_B = 0;
                        // SYNCDELAY; SYNCDELAY; SYNCDELAY;
                        // FPGA_PROG = 1; //FX2_PROG_B = 1;
                        //
			case CMD_FPGA_RESET:
				FPGA_INT1 = (EP1OUTBUF[1]) ? 1 : 0;
				EP1INBUF[0] = FPGA_INT1;
				EP1INBUF[1] = 0xAA;
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;
			//-----------------------------------------------------------------
			//TO DO: add a DEV_LOCK page in Trenz Electronic wiki
			case CMD_DEV_LOCK:
				if(EP1OUTBUF[1] == 0x01){	// Driver trying to lock device
					if(lock == 0){		// If the Device is free
						EP1INBUF[0] = 0x22; // A sucessfull lock indication will be transmitted to host computer's SW
						lock = 1;           //Raise a flag: the Device is locked
					}
					else		        // If the Device is already locked
						EP1INBUF[0] = 0x00;	// An already lock indication will transmitted to host computer's SW
				}
				else{				// Driver trying to unlock device
					if(lock == 1){		// If the Device is locked
						EP1INBUF[0] = 0x33;	// A sucessfull unlock indication will transmitted to host computer's SW
						lock = 0;       //Lower a flag: the Device is unlocked
					}
					else				// Device is unlocked
						EP1INBUF[0] = 0x00;	// Got problem
				}
				new_data = 1; //A flag is raised: new data are availble in EP1INBUF for the host computer's SW
				break;		
			//-----------------------------------------------------------------
		}
		EP1OUTBC = EP1DATA_COUNT;		// Free input buffer
	}

	if(new_data){				        // Have the FX2 microcontroller's FW some data EP1INBUF[] to send to host computer's SW?
		if ( !(EP1INCS & 0x02)){		// Could the FX2 microcontroller's FW send?
			EP1INBC = EP1DATA_COUNT;	// FX2 microcontroller's FW sends the new data EP1INBUF[] to host computer's SW
			new_data = 0;                   // Lower a flag: new data are NO LONGER availble in EP1INBUF for the host computer's SW
		}
	}
}

/*****************************************************************************
* One of the two main loop content at the end of initialization phase. 
* Activity is the Second and Third Polling Activity inside while(1) of fw.c 
* The first Polling activity is "if(usb_setup_packet_avail()) usb_handle_setup_packet();" inside while(1) of fw.c
******************************************************************************/
void activity(void)
{
	ep1_pool();     // Second Polling Activity inside while(1) of fw.c
        // Pull EP1 data aka Polling for EP1 data aka
        // Pull EP1 data aka pull possible TE API Commands (FW APIs) 
        // from USB connection (with host computer's SW) 
        // and execute the function requested by host computer's SW using TE API Commands.
        
        int_pin_pool(); // Third Polling Activity inside while(1) of fw.c
        // Pull INT pool aka Interrupt Pin polling aka
        // Pull INT pool data from an I2C address when FPGA's chip rise INT0 pin and an autoresponse 
        // interrupt is preconfigured by host computer's SW. The FX2 microcontroller pull 
        // (using I2C in int_pin_pool()) x (x defined by EP1OUTBUF[2]) number of bytes 
        // from y I2C address (y defined by EP1OUTBUF[1]). 
        // The host computer's SW should use a polling procedure to retrieve the I2C bytes read 
        // (and stored) by FX2 microcontroller.
}
//*****************************************************************************
