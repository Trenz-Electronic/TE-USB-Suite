// TE_USB_FX2_CyAPI_SampleApplication.cpp : define the access point to console application.
//

//#include "stdafx.h"

#pragma once

#include "stdafx.h"
//#include <iostream>

#include "TE_USB_FX2_CyAPI_SampleApplication.h"

#include <iostream>

#include <tchar.h>

//#pragma comment (lib, "TE_USB_FX2_DLLcppNotCLR.lib")

//#include "TE_USB_FX2_DLLcppNotCLR.h"

using namespace std;

void DrawMenu()
{
  cout << endl;
  cout << "TE_USB DLL Example 1.0 " << endl;
  cout << "	1 - Get number of cards " << endl;
  cout << "	2 - Connect cardNo 0" << endl;
  cout << "	3 - Connect cardNo 1" << endl;
  cout << "	4 - Disconnect" << endl;
  cout << "	5 - Get FX2 firmware version"	<< endl;
  cout << "	6 - Get FPGA firmware version"	<< endl;
  cout << "	7 - Get FX2 FIFO Status" << endl;
  cout << "	8 - Reset FX2 FIFO Status" << endl;
  cout << "	r - Read high speed data throughput " << endl;
  cout << "	w - Write high speed data throughput" << endl;

  cout << "	0 - Exit " << endl;
}

int GetFPGAstatus(CCyUSBDevice *USBdevList)
{
  byte Command[64], Reply[64];
  int CmdLength = 64;
  long ReplyLength = 64;


  Command[0] = SET_INTERRUPT;
  Command[1] = MB_I2C_ADRESS;
  Command[2] = I2C_BYTES;

  if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000)) {
    cout << "Error" << endl;
    return -1;
  }

  Command[0] = GET_INTERRUPT;	//read from interrupt data register
  //if (TE_USB_FX2_SendCommand(handle, cmd, cmd_length, reply2, reply_length2,TIMEOUT_MS))
  //	{
  //		cout << "Error" << endl;
  //		return -1;
  //	}
  //This code has been grayed out because make the program stall if data transfered in trasmission is less than 409600.

  Reply[0]=0;
  //cout << "host->memory data Second step" << endl;
  while (Reply[0] == 0) {
    //cout << "Wait" << endl;
    if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
    {
      cout << "Error" << endl;
      return -1;
    }
  }

  //for ( int jj=0; jj<6; jj++)
  //TE_USB_FX2_SendCommand(USBDevice, CardNo, cmd, cmd_length, reply, reply_length, 1000);

  return (int)Reply[4]; //return data verification status




}

void SendFPGAcommand(CCyUSBDevice *USBdevList, enum MB_Commands Command2MB)
{
  byte Command[64], Reply[64];
  int CmdLength = 64;
  long ReplyLength = 64;

  Command[0] = I2C_WRITE;//comand I2C_WRITE
  Command[1] = MB_I2C_ADRESS;
  Command[2] = I2C_BYTES; //12 BYTES read/write
  Command[3] = 0;
  Command[4] = 0;
  Command[5] = 0;
  Command[6] = Command2MB;

  if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
    cout << "Error" << endl;
}


void GetNumberOfCards(CCyUSBDevice *USBdevList)
{
  cout << endl << TE_USB_FX2_ScanCards(USBdevList) << endl;
}

void GetFX2version(CCyUSBDevice *USBdevList)
{
  if (USBdevList == NULL)
  {
    cout << "Error,no device is selected" <<endl ;
    return;
  }

  byte Command[64], Reply[64];
  long CmdLength = 64;
  long ReplyLength = 64;

  Command[0] = 0x00;//comand read FX2 version

  if (!TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
  {
    if (ReplyLength >= 4)
    {
      printf("Major version: %d \n", Reply[0]);
      printf("Minor version: %d \n", Reply[1]);
      printf("Device hi: %d \n", Reply[2]);
      printf("Device lo: %d \n", Reply[3]);
    }
  }
  else
    cout << "Error" << endl;
}

void GetFPGAversion(CCyUSBDevice *USBdevList)
{
  if (USBdevList == NULL)
  {
    cout << "Error,no device is selected" <<endl ;
    return;
  }

  byte Command[64], Reply[64];
  long CmdLength = 64;
  long ReplyLength = 64;

  Command[0] = SET_INTERRUPT; //0xB0;//comand SET_INTERRUPT
  Command[1] = MB_I2C_ADRESS; //0x3F;//I2C slave address
  Command[2] = I2C_BYTES; //12;//12 bytes payload

  if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
    cout << "Error" << endl;

  Command[0] = 0xAD;//comand I2C_WRITE
  Command[3] = 0;
  Command[4] = 0;
  Command[5] = 0;
  Command[6] = 1; //get FPGA version

  if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
    cout << "Error" << endl;

  Command[0] = 0xB1;//comand GET_INTERRUPT

  if (!TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
  {

    if ((ReplyLength > 4) &&(Reply[0] != 0))
    {
      printf("Major version: %d \n", Reply[1]);
      printf("Minor version: %d \n", Reply[2]);
      printf("Release version: %d \n", Reply[3]);
      printf("Build version: %d \n", Reply[4]);
    }
  }
  else
    cout << "Error" << endl;
}

void GetFX2FifoStatus(CCyUSBDevice *USBdevList)
{

  if (USBdevList == NULL)
  {
    cout << "Error,no device is selected" <<endl ;
    return;
  }

  byte Command[64], Reply[64];
  long CmdLength = 64;
  long ReplyLength = 64;

  Command[0] = 0xAC;//comand GET_FIFO_STATUS

  if (!TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
  {
    if (ReplyLength >= 4)
    {
      printf("EP2 CS: 0x%02X\n", Reply[0]);
      printf("EP4 CS: 0x%02X\n", Reply[1]);
      printf("EP6 CS: 0x%02X\n", Reply[2]);
      printf("EP8 CS: 0x%02X\n", Reply[3]);
    }
  }
  else
    cout << "Error" << endl;
}

void ResetFX2FifoStatus(CCyUSBDevice *USBdevList)
{

  if (USBdevList == NULL)
  {
    cout << "Error,no device is selected" <<endl ;
    return;
  }

  byte Command[64], Reply[64];
  long CmdLength = 64;
  long ReplyLength = 64;

  Command[0] = 0xA4;//comand RESET_FIFO_STATUS
  Command[1] = 0;//RESET all FIFOs

  //cmd[0] = 0xA0;//comand SWITCH_MODE
  //cmd[1] = 0;//COMMAND mode

  if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
    cout << "Error" << endl;

  Command[0] = 0xA0;//comand SWITCH_MODE
  Command[1] = 1;//FIFO mode

  if (TE_USB_FX2_SendCommand(USBdevList, Command, CmdLength, Reply, ReplyLength, 1000))
    cout << "Error" << endl;
}


void ReadData_Throughput(CCyUSBDevice *USBdevList, unsigned int DeviceDriverBufferSize, unsigned int packets, int RX_PACKET_LEN, unsigned long TIMEOUT)
{
  if (USBdevList == NULL)
  {
    cout << "Error,no device is selected" <<endl ;
    return;
  }

  //int RX_PACKET_LEN =	256;//51200;//102400; //122880000; //512;//102400; //409600; //102400

  long packetlen = RX_PACKET_LEN;
  //unsigned int packets = 500;//1200;//1200;
  //unsigned int DeviceDriverBufferSize = 131072;//409600;//131072;
  //unsigned long TIMEOUT= 18;
  byte * data;
  byte * data_temp = NULL;
  unsigned int total_cnt = 0;
  unsigned int errors = 0;
  //unsigned int exactn = 0;
  bool printout= false;

  data = new byte [RX_PACKET_LEN*packets];
  //allocate memory

  //total_cnt=0;
  //ResetFX2FifoStatus(handle);

  bool bResultDataRead = false;

  unsigned int XferSizeRead = 0;

  //Shortest and more portable way to select the Address using the PipeNumber
  PI_PipeNumber PipeNo = PI_EP6;

  ResetFX2FifoStatus(USBdevList);

  SendFPGAcommand(USBdevList,FX22MB_REG0_START_TX);
  //starts test
  //Get_Data_Start(handle);

  CCyBulkEndPoint *BulkInEP = NULL;

  TE_USB_FX2_GetData_InstanceDriverBuffer ( USBdevList, &BulkInEP, PipeNo, TIMEOUT, DeviceDriverBufferSize);

  ElapsedTime.Start();
  //StopWatch start
  for (unsigned int i = 0; i < packets; i++)
  {
    packetlen = RX_PACKET_LEN;
    data_temp = &data[total_cnt];
    //cout << "Address &BulkInEP" << &BulkInEP << endl;
    //cout << "Address BulkInEP" << BulkInEP << endl;
    //cout << "Address *BulkInEP" << (*BulkInEP) << endl;
    if (TE_USB_FX2_GetData(&BulkInEP, data_temp, packetlen)) //+total_cnt
      //TE_USB_FX2_GetData(handle, data+total_cnt, &packetlen, PI_EP6,TIMEOUT_MS))
    {
      cout << "ERROR read" << endl;
      errors++;
      break;
    }
    total_cnt += (packetlen);
  }
  TheElapsedTime = ElapsedTime.Stop(false);
  //DEBUG StopWatch timer

  SendFPGAcommand(USBdevList,FX22MB_REG0_STOP);
  //end of test for FPGA

  //verify data
  unsigned int test_cnt=0;
  unsigned int verification=0;

  for (unsigned int j = 0; j < total_cnt; j+=4)
  {
    if (printout) {
      if ((0xF & j)==0) cout << endl; //puts CRLF every 16 bytes
      //printf("%02X %02X %02X %02X ", data[j+3], data[j+2], data[j+1], data[j]);
      printf("%02X %02X %02X %02X ", data[j], data[j+1], data[j+2], data[j+3]);
    }

    verification = (data[j]<<24) | (data[j+1]<<16) | (data[j+2]<<8) | data[j+3];
    //cout  << "verification" << verification <<endl;
    //cout  << "test_cnt" << test_cnt <<endl;
    if (verification != test_cnt)
    {
//				if (printout) cout << "VERIFICATION ERROR" << endl;
      errors++;
//				break;
    }
    test_cnt++;


    //for (unsigned int j = 0; j < (64); j++) {
    //		if (data[j] == data2[j]) exactn++;
    //		else errors++;
    //		cout<<"number" << j <<endl;
    //		cout <<"data received"<<data[j]<<endl;
    //		cout <<"data expected"<<data2[j]<<endl;
  }

  if (total_cnt == 0) errors++; //if no data is transferred then it an error
  if (errors) printf("\r\nmemory->host data verification FAILED: %d ERRORS\r\n", errors);
  else cout << endl << "memory->host data verification PASSED!!!" << endl;

  printf("Transferred %d kB in %2.3f s = %2.3f MB/s\r\n",
         total_cnt/1024, TheElapsedTime, (double)total_cnt/(TheElapsedTime*1024*1024));

  ResetFX2FifoStatus(USBdevList);
  delete data;
}

void WriteData_Throughput(CCyUSBDevice *USBdevList, unsigned int DeviceDriverBufferSize, unsigned int packets, int TX_PACKET_LEN, unsigned long TIMEOUT, unsigned int GetFpgaStatusFlag)
{

  if (USBdevList == NULL)
  {
    cout << "Error,no device is selected" <<endl ;
    return;
  }

  //int TX_PACKET_LEN =	256;//51200;//102400;//122880000;//512;//102400; //409600; //102400 //307200
  long packetlen = TX_PACKET_LEN;
  //unsigned int packets = 500; //1200;//1200;
  //unsigned long TIMEOUT = 1000;
  byte * data;
  byte * data_temp = NULL;
  unsigned int total_cnt = 0;
  unsigned int errors = 0;
  double TheElapsedTime = 0;

  PI_PipeNumber PipeNo = PI_EP8;
  //Shortest and more portable way to select the Address using the PipeNumber
  byte PipeNoHex = 0x08;

  unsigned int XferSizeRead=0;

  data = new byte [TX_PACKET_LEN*packets];
  //allocate memory
  //data_temp = new byte [ RX_PACKET_LEN];

  for (unsigned int j = 0; j < (TX_PACKET_LEN*packets); j+=4) {
    data[j]   = (0xFF000000 & total_cnt)>>24;
    data[j+1] = (0x00FF0000 & total_cnt)>>16;
    data[j+2] = (0x0000FF00 & total_cnt)>>8;
    data[j+3] = 0x000000FF & total_cnt;
    total_cnt++;
  }

  ResetFX2FifoStatus(USBdevList);

  SendFPGAcommand(USBdevList, FX22MB_REG0_START_RX);
  //starts test

  bool bResultDataWrite = false;

  //unsigned int DeviceDriverBufferSize = 131072;//409600;//131072;

  // Find a second bulk OUT endpoint in the EndPoints[] array
  CCyBulkEndPoint *BulkOutEP = NULL;

  TE_USB_FX2_SetData_InstanceDriverBuffer ( USBdevList, &BulkOutEP, PipeNo, TIMEOUT, DeviceDriverBufferSize);

  ElapsedTime.Start();
  //StopWatch start
  total_cnt = 0;
  for (unsigned int i = 0; i < packets; i++)
  {
    long packetlen = TX_PACKET_LEN;
    //long packetlen = TX_PACKET_LEN;
    data_temp = &data[total_cnt];
    //bResultDataWrite = BulkOutEP->XferData(data_temp, &packetlen);

    //cout << "Address &BulkOutEP" << &BulkOutEP << endl;
    //cout << "Address BulkOutEP" << BulkOutEP << endl;
    //cout << "Address *BulkInEP" << (*BulkInEP) << endl;

    if (TE_USB_FX2_SetData(&BulkOutEP, data_temp, packetlen)) //data+total_cnt
    {
      cout << "ERROR" << endl;
      break;
    }
    //cout << "host->memory first step" << endl;
    total_cnt += ( packetlen);
  }
  TheElapsedTime = ElapsedTime.Stop(false); //DEBUG StopWatch timer
  SendFPGAcommand(USBdevList, FX22MB_REG0_STOP); //stops test

  cout << "EndTime" << endl;

  int status = 0;

  if (GetFpgaStatusFlag==1)
  {
    status=GetFPGAstatus(USBdevList);
  }
  else
    status=2;

  if (status == 2) cout << "host->memory data verification SKIPPED!" << endl;
  if (status == 1) cout << "host->memory data verification PASSED!" << endl;
  else cout << "host->memory data verification FAILED!" << endl;

  printf("Transferred %d kB in %2.3f s = %2.3f MB/s\r\n",
         total_cnt/1024, TheElapsedTime, (double)total_cnt/(TheElapsedTime*1024*1024));

  //cout << "XMODE_DIRECT" << XMODE_DIRECT <<endl;
  //cout << "XMODE_BUFFERED" << XMODE_BUFFERED <<endl;

  ResetFX2FifoStatus(USBdevList);

  delete data;
}

int _tmain(int argc, _TCHAR* argv[])
{
  if (!LoadTE0300DLL())// Load DLL
  {

    char name[256];

    cout << "Could not load DWUSB .dll!" << endl;
    cin.getline(name,256);

    return 0;
  };
  //                            {80419720-177A-4ed0-97C0-0996A12A1F4F}
  //GUID TEUSBDRV_GUID = {0x80419720, 0x177A, 0x4ed0, 0x97C0, 0x0996A12A1F4F};
  //                            {80419720-177a-4ed0-97c0-0996a12a1f4f}
  //                             ae18aa60-7f6a-11d4-97dd-01229b959
  //static GUID CYUSBDRV_GUID {0xae18aa60, 0x7f6a, 0x11d4, 0x97, 0xdd, 0x0, 0x1, 0x2, 0x29, 0xb9, 0x59};

  //

  CCyUSBDevice *USBdevList = new CCyUSBDevice((HANDLE)0,CYUSBDRV_GUID,true);
  //CYUSBDRV_GUID,true);

  int CardNo = 1;

  //HANDLE m_handle = 0;//(HANDLE) 10;
  //m_handle = (HANDLE) USBDevice;
  //unsigned int handle_test = 0;

  unsigned int BufferSizeR = 131072;
  unsigned int PacketsNumberR = 5;
  int PacketLengthR = 51200;
  unsigned int TimeOutR = 1000;


  char m_sel = 'z';

  while (m_sel != '0')
  {
    DrawMenu();
    cin >> m_sel;
    switch (m_sel)
    {
    case '1' :
      GetNumberOfCards(USBdevList);
      //handle_test=(unsigned int) m_handle;
      //cout << "Handle Test" << handle_test <<endl;
      break;
    case '2' :
      if (TE_USB_FX2_Open(USBdevList, 0)==0)
        cout << "Module is connected!"  <<endl; //&m_handle
      //if (TE_USB_FX2_Open(&m_handle, 0)!=0)
      else
        cout << "Module is not connected!" <<endl;
      break;
    case '3' :
      if (TE_USB_FX2_Open(USBdevList, 1)==0)
        cout << "Module is connected!"  <<endl; //&m_handle
      //if (TE_USB_FX2_Open(&m_handle, 0)!=0)
      else
        cout << "Module is not connected!" <<endl;
      break;
      //if (TE_USB_FX2_Open(USBDevice, 1)!=0)
      //	cout << "Module is not connected!" << endl;
      //break;
    case '4' :
      TE_USB_FX2_Close(USBdevList);
      break;
    case '5' :
      GetFX2version(USBdevList);
      break;
    case '6' :
      GetFPGAversion(USBdevList);
      break;
    case '7' :
      GetFX2FifoStatus(USBdevList);
      break;
    case '8' :
      ResetFX2FifoStatus(USBdevList);
      break;
    case 'r' :
      //ReadDataFPGAIntegrity(TE_USB_FX2_USBdevice);

      cout << "Write the Buffer Size desired for EP6 (power of 2 is better)" <<endl;
      cout <<"Value suggested is 131072" <<endl;

      cin>>BufferSizeR;

      cout << "Write the Packets Number desired" <<endl;
      cout << "Value suggested is 1200 if you are testing TE0320 or TE0630" <<endl;
      cout <<"Value suggested is  600 if you are testing TE0300" <<endl;

      cin>>PacketsNumberR; // Try to parse the string as an integer

      cout << "Write the Packet Length desired" <<endl;
      cout << "Value suggested is 102400 if you are testing TE0320 or TE0630." <<endl;
	  cout << "Know issue: does not use 131072 or the test fail." <<endl;
      cout << "Value suggested is  51200 if you are testing TE0300" <<endl;
	  cout << "if the value used is >51200 the test fails." <<endl;
      cout << "In C# you are able to read even packet with length less than 512 byte" <<endl;
      cout << "512 byte AND it is what you implicitly do with point 5 (FX2 firmware version)" <<endl;
      cout << "6 (FPGA firmware version),7 (FX2 FIFO Status) BUT for this test " <<endl;
      cout << "you must use 1024 or more for Packet Size value" <<endl;

      cin>>PacketLengthR; // Try to parse the string as an integer

      cout << "Write the Timeout value desired, the integer is measured in milliseconds" <<endl;
      cout << "Value TimeOut (ms) > [PacketLength/DataThroughput ]+1 ms for high responsive" <<endl;
      cout << "computer. DataThroughput value expected is >30 Mbyte/s, " <<endl;
      cout << "so with PacketLength=102400 byte, the value is 5-6 ms. " <<endl;
      cout << "If the computer is not highly responsive you must set Timeout to large value:" <<endl;
      cout << "20,50,200,1000 ms (it depends on how much the computer lack real time behavior)." <<endl;

      cin>>TimeOutR; // Try to parse the string as an integer

      ReadData_Throughput(USBdevList,BufferSizeR,PacketsNumberR,PacketLengthR,TimeOutR);

      break;

    case 'w' :
      cout << "Write the Buffer Size desired for EP8 (power of 2 is better)" <<endl;
      cout <<"Value suggested is 131072" <<endl;
      unsigned int BufferSizeW = 131072;
      cin>>BufferSizeW;

      cout << "Write the Packets Number desired" <<endl;
      cout << "Value suggested is 1200 if you are testing TE0320 or TE0630" <<endl;
      cout <<"Value suggested is  600 if you are testing TE0300" <<endl;
      int PacketsNumberW = 5;
      cin>>PacketsNumberW; // Try to parse the string as an integer

      cout << "Write the Packet Length desired" <<endl;
      cout << "Value suggested is 102400 if you are testing TE0320 or TE0630." <<endl;
	  cout << "Know issue: does not use 102400 or the test fail." <<endl;
      cout << "Value suggested is 51200 if you are testing TE0300" <<endl;
	  cout << "if the value used is >51200 the test fails." <<endl;
      cout << "In C# you are able to write even packet with length less than 512 byte" <<endl;
      cout << "In C++ you are able to write even packet with length less than" <<endl;
      cout << "512 byte AND in this case you can also do this BUT" <<endl;
      cout << "in this case you are only able to test write data integrity" <<endl;
      int PacketLengthW = 51200;
      cin>>PacketLengthW; // Try to parse the string as an integer

      cout << "Write the Timeout value desired, the integer is measured in milliseconds" <<endl;
      cout << "Value TimeOut (ms) > [PacketLength/DataThroughput ]+1 ms for high responsive " <<endl;
      cout << "computer. DataThroughput value expected is >20 Mbyte/s," <<endl;
      cout << "so with PacketLength=102400 byte,the value is 5-6 ms." <<endl;
      cout << "If the computer is not highly responsive you must set Timeout to large value :" <<endl;
      cout << "20,50,200,1000 ms (it depends on how much the computer lack real time behavior)." <<endl;
      unsigned int TimeOutW = 1000;
      cin>>TimeOutW; // Try to parse the string as an integer

      cout << "You want make an integrity test on data writen on FPGA?" <<endl;
      cout << "1 for YES, 0 for NO" <<endl;

      unsigned int GetStatusFPGAyn = 1;
      cin>>GetStatusFPGAyn;   // Try to parse the string as an integer


      WriteData_Throughput(USBdevList,BufferSizeW,PacketsNumberW,PacketLengthW,TimeOutW,GetStatusFPGAyn);
      break;

    }
  }

  if (USBdevList != 0)
    TE_USB_FX2_Close(USBdevList);
  CloseTE0300DLL();

  system("Pause");

  return 0;
}



