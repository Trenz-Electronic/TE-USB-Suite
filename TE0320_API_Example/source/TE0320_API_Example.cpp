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
#include "stdafx.h"
#include <iostream>
#include "TE0320_API_Example.h"

using namespace std;

void DrawMenu()
{
	cout << endl;
	cout << "TE0320 DLL Example 1.0 " << endl;
	cout << "	1 - Get number of modules" << endl;
	cout << "	2 - Connect module No 0" << endl;
	cout << "	3 - Connect module No 1" << endl;
	cout << "	4 - Disconnect" << endl;
	cout << "	5 - Get FX2 status" << endl;
	cout << "	6 - Get FX2 version" << endl;
	cout << "	7 - Get FPGA firmware version"	<< endl;
	cout << "	8 - Get FX2 FIFO Status" << endl;
	cout << "	9 - Reset FX2 FIFO Status" << endl;
	cout << "	w - Write high speed data (FPGA RX)" << endl;
	cout << "	r - Read high speed data (FPGA TX)" << endl;
	cout << "	0 - Exit " << endl;
}

void GetNumberOfCards(unsigned int handle)
{
	cout << endl << TE0300_ScanCards() << endl;
}

void GetFX2status(unsigned int handle)
{
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;
	
	cmd[0] = 0xA1;//comand read FX2 version

	if (!TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
	{
		if (reply_length >= 4)
		{
			printf("fifo_error: %d \n", reply[0]);
			printf("current_mode: %d \n", reply[1]);
			printf("flash_busy: %d \n", reply[2]);
			printf("fpga_prog.: %d \n", reply[3]);
			printf("booting: %d \n", reply[4]);
//			printf("i2c_int_data: %d \n", reply[5]);
		}
	}
	else
		cout << "Error" << endl;
}

void GetFX2version(unsigned int handle)
{
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;
	
	cmd[0] = READ_VERSION;//comand read FX2 version

	if (!TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
	{
		if (reply_length >= 4)
		{
			printf("Major version: %d \n", reply[0]);
			printf("Minor version: %d \n", reply[1]);
			printf("Device hi: %d \n", reply[2]);
			printf("Device lo: %d \n", reply[3]);
		}
	}
	else
		cout << "Error" << endl;
}

void SendFPGAcommand(unsigned int handle, enum MB_Commands Command)
{
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;
	
	cmd[0] = I2C_WRITE;//comand I2C_WRITE
	cmd[1] = MB_I2C_ADRESS;
	cmd[2] = I2C_BYTES; //12 BYTES read/write
	cmd[3] = 0;
	cmd[4] = 0;
	cmd[5] = 0;
	cmd[6] = Command;
	
	if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
		cout << "Error" << endl;
}

void GetFPGAversion(unsigned int handle)
{
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;

	cmd[0] = SET_INTERRUPT;
	cmd[1] = MB_I2C_ADRESS;
	cmd[2] = I2C_BYTES;

	if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS)){
		cout << "Error" << endl;
		return;
	}

	cmd[0] = GET_INTERRUPT;	//clear interrupt data register

	if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS)){
		cout << "Error" << endl;
		return;
	}
	
	SendFPGAcommand(handle,FX22MB_REG0_GETVERSION); //get FPGA version

	cmd[0] = GET_INTERRUPT;	//read from interrupt data register
	reply[0]=0;

	while (reply[0] == 0) {
		if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
		{
			cout << "Error" << endl;
			break;
		}
	}

	printf("INT# : %d \n", reply[0]);
	printf("Major version: %d \n", reply[1]);
	printf("Minor version: %d \n", reply[2]);
	printf("Release version: %d \n", reply[3]);
	printf("Build version: %d \n", reply[4]);

}

int GetFPGAstatus(unsigned int handle)
{
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;

	cmd[0] = SET_INTERRUPT;
	cmd[1] = MB_I2C_ADRESS;
	cmd[2] = I2C_BYTES;

	if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS)){
		cout << "Error" << endl;
		return -1;
	}

	cmd[0] = GET_INTERRUPT;	//read from interrupt data register
	if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
		{
			cout << "Error" << endl;
			return -1;
		}

	reply[0]=0;
	while (reply[0] == 0) {
		if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
		{
			cout << "Error" << endl;
			return -1;
		}
	}
	return (int)reply[4]; //return data verification status
}

void GetFX2FifoStatus(unsigned int handle)
{
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;
	
	cmd[0] = GET_FIFO_STATUS;

	if (!TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
	{
		if (reply_length >= 4)
		{
			printf("EP2 FIFO CS: 0x%02X\n", reply[0]);
			printf("EP4 FIFO CS: 0x%02X\n", reply[1]);
			printf("EP6 FIFO CS: 0x%02X\n", reply[2]);
			printf("EP8 FIFO CS: 0x%02X\n", reply[3]);
			printf("EP2 FIFO BCH: 0x%02X\n", reply[4]);
			printf("EP4 FIFO BCH: 0x%02X\n", reply[5]);
			printf("EP6 FIFO BCH: 0x%02X\n", reply[6]);
			printf("EP8 FIFO BCH: 0x%02X\n", reply[7]);
		}
	}
	else
		cout << "Error" << endl;
}

void ResetFX2FifoStatus(unsigned int handle)
{
	cout << endl << "Resetting all FIFOs" << endl;
	byte cmd[64], reply[64];
	int cmd_length = 64;
	int reply_length = 64;

	cmd[0] = RESET_FIFO_STATUS; //not working properly
	cmd[1] = 0; //reset all fifos
	
	if (TE0300_SendCommand(handle, cmd, cmd_length, reply, &reply_length,TIMEOUT_MS))
		cout << "Error" << endl;
}

void ReadData(unsigned int handle)
{
	int packetlen = TX_PACKET_LEN;
	unsigned int packets = 1200;
	byte * data;
	unsigned int total_cnt = 0;
	unsigned int errors = 0;
	bool printout=false;

	data = new byte [TX_PACKET_LEN*packets]; //allocate memory

	ResetFX2FifoStatus(handle);

	SendFPGAcommand(handle,FX22MB_REG0_START_TX); //starts test

	ElapsedTime.Start(); //StopWatch start
	for (unsigned int i = 0; i < packets; i++)
	{
		packetlen = TX_PACKET_LEN;
		if (TE0300_GetData(handle, data+total_cnt, &packetlen, PI_EP6,TIMEOUT_MS))
		{
			cout << "ERROR" << endl;
			errors++;
			break;
		}
		total_cnt += packetlen;
	}
	TheElapsedTime = ElapsedTime.Stop(false); //DEBUG StopWatch timer
	
	//verify data
	unsigned int test_cnt=0;
	for (unsigned int j = 0; j < total_cnt; j+=4)
	{
		if (printout) {
			if ((0xF & j)==0) cout << endl; //puts CRLF every 16 bytes
			//printf("%02X %02X %02X %02X ", data[j+3], data[j+2], data[j+1], data[j]);
			printf("%02X %02X %02X %02X ", data[j], data[j+1], data[j+2], data[j+3]);
		}
		unsigned int verification;
		verification = (data[j]<<24) | (data[j+1]<<16) | (data[j+2]<<8) | data[j+3];
		//verification = (data[j+3]<<24) | (data[j+2]<<16) | (data[j+1]<<8) | data[j];
		if (verification != test_cnt) 
		{
//				if (printout) cout << "VERIFICATION ERROR" << endl;
			errors++;
//				break;
		}
		test_cnt++;
	}

	if (total_cnt == 0) errors++; //if no data is transferred then it an error
	if (errors) printf("\r\nmemory->host data verification FAILED: %d ERRORS\r\n", errors);
	else cout << endl << "memory->host data verification PASSED!!!" << endl;

	printf("Transferred %d kB in %2.3f s = %2.3f MB/s\r\n", 
		total_cnt/1024, TheElapsedTime, (double)total_cnt/(TheElapsedTime*1024*1024));

	SendFPGAcommand(handle,FX22MB_REG0_STOP); //stops test
	delete data;
}

void WriteData(unsigned int handle)
{
	int packetlen = RX_PACKET_LEN;
	unsigned int packets = 1200;
	byte * data;
	unsigned int total_cnt = 0;
	unsigned int errors = 0;
	double TheElapsedTime;

	data = new byte [RX_PACKET_LEN*packets]; //allocate memory

	for (unsigned int j = 0; j < (RX_PACKET_LEN*packets); j+=4) {
			data[j]   = (0xFF000000 & total_cnt)>>24;
			data[j+1] = (0x00FF0000 & total_cnt)>>16;
			data[j+2] = (0x0000FF00 & total_cnt)>>8;
			data[j+3] = 0x000000FF & total_cnt;
			total_cnt++;
	}

	ResetFX2FifoStatus(handle);

	SendFPGAcommand(handle,FX22MB_REG0_START_RX); //starts test

	ElapsedTime.Start(); //StopWatch start
	total_cnt = 0;
	for (unsigned int i = 0; i < packets; i++)
	{
		packetlen = RX_PACKET_LEN;
		if (TE0300_SetData(handle, data+total_cnt, packetlen, PI_EP8))
		{
			cout << "ERROR" << endl;
			break;
		}
		total_cnt += packetlen;
	}
	TheElapsedTime = ElapsedTime.Stop(false); //DEBUG StopWatch timer
	SendFPGAcommand(handle,FX22MB_REG0_STOP); //stops test
	int status = GetFPGAstatus(handle);
	if (status == 1)
		cout << "host->memory data verification PASSED!" << endl;
	else
		cout << "host->memory data verification FAILED!" << endl;
	printf("Transferred %d kB in %2.3f s = %2.3f MB/s\r\n", 
		total_cnt/1024, TheElapsedTime, (double)total_cnt/(TheElapsedTime*1024*1024));

	delete data;
}

int _tmain(int argc, _TCHAR* argv[])
{
	if (!LoadTE0300DLL())// Load DLL
	{
		cout << "Could not load DWUSB .dll!" << endl;
		return 0;
	}; 

	unsigned int m_handle = 0;

	char m_sel = 'd';

	while (m_sel != '0')
	{
		DrawMenu();
		cin >> m_sel;		
		switch (m_sel)
		{
			case '1' :
				GetNumberOfCards(m_handle);
				break;
			case '2' :
				if (TE0300_Open(&m_handle, 0)!=0)
					cout << "Module is not connected!" << endl;
				break;
			case '3' :
				if (TE0300_Open(&m_handle, 1)!=0)
					cout << "Module is not connected!" << endl;
				break;
			case '4' :
				TE0300_Close(&m_handle);
				break;
			case '5' :
				GetFX2status(m_handle);
				break;
			case '6' :
				GetFX2version(m_handle);
				break;
			case '7' :
				GetFPGAversion(m_handle);
				break;
			case '8' :
				GetFX2FifoStatus(m_handle);
				break;
			case '9' :
				ResetFX2FifoStatus(m_handle);
				break;
			case 'w' :
				WriteData(m_handle);
				break;
			case 'r' :
				ReadData(m_handle);
		}
	}

	if (m_handle != 0)
		TE0300_Close(&m_handle);
	CloseTE0300DLL();

	system("Pause");

	return 0;
}

