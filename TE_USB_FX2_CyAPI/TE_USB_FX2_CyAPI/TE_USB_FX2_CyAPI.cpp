// TE_USB_FX2_CyAPI.cpp: it defines expoerted function for DLL's application program.
// Main DLL File  

#include "stdafx.h"
#include "TE_USB_FX2_CyAPI.h"

#include <iostream>
using namespace std;

#include <stdio.h>
//#include <tchar.h>

#include <string>
#include <sstream>

/*
TE_USB_FX2_ScanCards()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_ScanCards(CCyUSBDevice *USBDeviceList)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_ScanCards(USBDeviceList);

  Description
This function takes an already initialized USB device list (USBDeviceList), searches for Trenz Electronic USB FX2 devices 
(Cypress driver derivative and VID = 0xbd0, PID=0x0300) and counts them.
This function returns the number of Trenz Electronic USB FX2 devices attached to the USB bus of the host computer.

  Parameters
1) CCyUSBDevice *USBDeviceList
CCyUSBDevice is a type defined in CyAPI.dll. Its name is misleading because it is not a class that represents a single USB device, 
but it rather represents a list of USB devices. CCyUSBDevice is the list of devices served by the CyUSB.sys driver 
(or a derivative like TE_USB_FX2.sys). 
This parameter is passed by pointer. See page 7 and pages 23-49 of CyAPI.pdf (Cypress CyAPI Programmer's Reference).

  Return Value
1) int : integer type.
This function returns the number of USB devices attached to the host computer USB bus.
*/


TE_USB_FX2_CYAPI int TE_USB_FX2_ScanCards ( CCyUSBDevice *USBDeviceList )
{
  int CardCount = 0;

  // Create an instance of CCyUSBDevice
  //CCyUSBDevice *USBDevice = new CCyUSBDevice(NULL);
  //This part has been moved outside the DLL.

  //This method give the number of card that use CyUSB.sys and its derivative 
  //(like TE_USB_FX2_64.sys and TE_USB_FX2_32.sys used by Trenz Electronic)
  int CypressDeviceNumberTOT = USBDeviceList->DeviceCount();
  //cout << "CypressDeviceNumberTOT" << CypressDeviceNumberTOT << endl;
  if (CypressDeviceNumberTOT==0) return 0;
  else
  // Look for a device having VID = 0bd0, PID = 0300 
  {
    int vID, pID;
    int CypressDeviceNumber = 0;
    do {
      // Open automatically calls Close() if necessary
      USBDeviceList->Open(CypressDeviceNumber);
      vID = USBDeviceList->VendorID;
      pID = USBDeviceList->ProductID;
      CypressDeviceNumber++;
      //cout << "VID" << vID << endl;
      //cout << "PID" << pID << endl;
      //cout << "CypressDeviceNumber" << CypressDeviceNumber << endl;
      USBDeviceList->Close();
	  // If i found a device having VID = 0bd0, PID = 0300 i augment by one CardCount variable
      if ( (vID == 0x0bd0) && (pID == 0x0300) )
      {
        CardCount++;
      }
    } while (CypressDeviceNumber < CypressDeviceNumberTOT );
    return CardCount;
  }
}

/*
TE_USB_FX2_Open()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_Open(CCyUSBDevice *USBDeviceList, int CardNumber)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_Open(USBDeviceList, CardNumber);

  Description
This function takes an already initialized USB device list, searches for Trenz Electronic USB FX2 devices 
(Cypress driver derivative and VID = 0xbd0, PID=0x0300) and counts them.
If no device is attached, USBDeviceList is not initialized to null (the device list is not erased). 
An internal operation that closes an handle to the CyUSB.sys driver (or a derivative like TE_USB_FX2.sys) is executed instead 
(see page 33 of CyAPI.pdf).
If one or more devices are attached and 
1) if 0 <= CardNumber <= (number of attached devices – 1), then the selected module is 
not directly given by USBDeviceList (CCyUSBDevice type). An internal operation that opens a handle to CyUSB.sys driver 
(or a derivative like TE_USB_FX2_xx.sys) is executed instead (see page 45 of CyAPI.pdf). This handle is internally managed by 
CyAPI.dll, therefore there is no need to expose them to the user.
2) if CardNumber >= number of attached devices, then USBDeviceList (CyUSBDevice type) is not initialized to null (the device list 
is not erased). An internal operation that closes an handle to CyUSB.sys driver (or a derivative like TE_USB_FX2.sys) is executed 
instead (see page 33 of CyAPI.pdf).

A more intuitive name for this function would have been TE_USB_FX2_SelectCard().
You can use this function to select the card desired without the need to call Close before.

  Parameters
1)  CCyUSBDevice *USBDeviceList
CCyUSBDevice is a type defined in CyAPI.dll. Its name is misleading because it is not a class that represents a single USB device, 
but it rather represents a list of USB devices. 
CCyUSBDevice is the list of devices served by the CyUSB.sys driver (or a derivative like TE_USB_FX2.sys). 
This parameter is passed by pointer. See page 7 and pages 23-49 of CyAPI.pdf (Cypress CyAPI Programmer's Reference).
2)  int CardNumber.
This is the number of the selected Trenz Electronic USB FX2 device.

  Return Value
1)  int : integer type
This function returns true (ST_OK=0) if it is able to find the module selected by CardNumber. If unable to do so, it returns false 

(ST_ERROR=1).
enum ST_Status
{
  ST_OK = 0,
  ST_ERROR = 1
};

*/


TE_USB_FX2_CYAPI int TE_USB_FX2_Open ( CCyUSBDevice *USBDeviceList, int CardNumber)
{
  int CardCount = 0;
  int CardCounted = 0;

  //Number of Cypress Device (Trenz Electronic or not)
  int CypressDeviceNumber = 0;
  //Number of Trenz Device
  int TrenzDeviceNumber = 0;
  //Position of Trenz Device desired in the USBDeviceList
  int DeviceNumber = 0;

  //This method give the number of card that use CyUSB.sys and its derivative 
  //(like TE_USB_FX2_64.sys and TE_USB_FX2_32.sys used by Trenz Electronic)
  int CypressDeviceNumberTOT = USBDeviceList->DeviceCount();
  int vID, pID;

  // Look for a device having VID = 0x0bd0, PID = 0x0300
  // Create an instance of CCyUSBDevice
  do {
    // Open automatically calls Close() if necessary
    USBDeviceList->Open(CypressDeviceNumber);
    vID = USBDeviceList->VendorID;
    pID = USBDeviceList->ProductID;
    CypressDeviceNumber++;
    USBDeviceList->Close();
    if ( (vID == 0x0bd0) && (pID == 0x0300) )
    {
	  //CardCount
      TrenzDeviceNumber++; 
      if((TrenzDeviceNumber-1)==CardNumber)
      {
		//I store the DeviceNumber that identify the Trenz Card (CardNumber) requested
		//Memorize this number for later use
		//This is the position of Trenz Device desired in the USBDeviceList
        DeviceNumber = CypressDeviceNumber-1;
      }
    }
  } while (CypressDeviceNumber < CypressDeviceNumberTOT );

  //At this point I memorize the Cards Counted and zeroed the variable that I have used in the counting.
  CardCounted=TrenzDeviceNumber;
  TrenzDeviceNumber=0;

  //cout << CardCounted << endl;
  //cout << CardNumber << endl;

  //Now I search the Trenz USB Device with the Card Number (CardNo) specified
  if ( ((CardNumber>=0) && (CardNumber<CardCounted)) )
  {
    //USBDevice USBdev = USBdevList[DeviceNumber] LIKE of C#;
    USBDeviceList->Open(DeviceNumber);
    vID = USBDeviceList->VendorID;
    pID = USBDeviceList->ProductID;
    if ((((pID == 0x0300) && (vID == 0x0bd0)) == true))
      return ST_OK;
    else
      return ST_ERROR;

  }
  else
    return ST_ERROR;
}

/*

TE_USB_FX2_Close()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_Close(CCyUSBDevice *USBDeviceList)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_Close(USBDeviceList);

  Description
This function takes an already initialized USB device list, searches for Trenz Electronic USB FX2 devices 
(any Cypress driver derivative and VID = 0xbd0, PID=0x0300) and opens (and immediately after closes) the first device found.
The selected module is not directly given by USBDeviceList (CCyUSBDevice type). An internal operation that opens and immediately 
after closes an handle to CyUSB.sys driver (or a derivative like TE_USB_FX2_xx.sys) is executed instead (see page 45 of CyAPI.pdf). 
The open method closes every other handle already opened, and close method closes the only handle open; in this way, all handles 
are closed. These handles are internally managed by CyAPI.dll and there is no need to expose them to the user.
Note. After the execution of this function, no internal handle is open.
This function does not differ much from from its homonym of the previous TE0300DLL.dll; the only difference is that this function 
closes a handle (like TE0300DLL.dll) to the  driver but the handle is not exposed to user (unlike TE0300DLL.dll).

  Parameters
1) CCyUSBDevice *USBDeviceList
CCyUSBDevice is a type defined in CyAPI.dll. Its name is misleading because it is not a class that represents a single USB device, but it rather represents a list of USB devices. CCyUSBDevice is the list of devices served by the CyUSB.sys driver (or a derivative like TE_USB_FX2.sys). This parameter is passed by pointer. See page 7 and pages 23-49 of CyAPI.pdf (Cypress CyAPI Programmer's Reference).
2) int CardNumber.
This is the number of the selected Trenz Electronic USB FX2 device.

  Return Value
1) int : integer type
This function returns true (ST_OK=0) if it is able to find the module selected by CardNumber. If unable to do so, it returns false (ST_ERROR=1).
enum ST_Status
{
  ST_OK = 0,
  ST_ERROR = 1
};

*/

TE_USB_FX2_CYAPI int TE_USB_FX2_Close (CCyUSBDevice *USBDeviceList)
{
  //int CardCount = 0;
  //int CardCounted = 0;

  int CypressDeviceNumber = 0;
  //int TrenzDeviceNumber = 0;
  //int DeviceNumber = 0;

  int CypressDeviceNumberTOT = USBDeviceList->DeviceCount();
  int vID, pID;
  do {
    // Open automatically calls Close() if necessary
    USBDeviceList->Open(CypressDeviceNumber);
    vID = USBDeviceList->VendorID;
    pID = USBDeviceList->ProductID;
    CypressDeviceNumber++;
    USBDeviceList->Close();
    /*if ( (vID == 0x0bd0) && (pID == 0x0300) )
    {
    	TrenzDeviceNumber++; //CardCount
    	if((TrenzDeviceNumber-1)==CardNumber)
            {
    		 DeviceNumber = CypressDeviceNumber-1;
            }
    }*/
  } while (CypressDeviceNumber < CypressDeviceNumberTOT );

  return ST_OK;

  /*
  CardCounted=TrenzDeviceNumber;
  CardCount=0;

  //cout << CardCounted << endl;
  //cout << CardNo << endl;


  if ( ((CardNumber>=0) && (CardNumber<CardCounted)) )
  {
  	//USBDevice USBdev = USBdevListToDispose[DeviceNumber];
  	USBDeviceList->Open(DeviceNumber);
  	vID = USBDeviceList->VendorID;
  	pID = USBDeviceList->ProductID;
  	USBDeviceList->Close();
  	if ((((pID == 0x0300) && (vID == 0x0bd0)) == true))
  		return ST_OK;
        else
  		return ST_ERROR;

  }
  else
  	return ST_ERROR;
  */
}


/*

TE_USB_FX2_SendCommand()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_SendCommand(CCyUSBDevice *USBDeviceList, byte* Command, long CmdLength, byte* Reply, long ReplyLength,
unsigned long Timeout)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_SendCommand(USBDeviceList, Command, CmdLength, Reply, ReplyLength, Timeout);

  Description
This function takes an already initialized USB device list (USBDeviceList previously selected by TE_USB_FX2_Open()) and sends a 
command (API command) to the USB FX2  microcontroller (USB FX2 API command) or to the MicroBlaze embedded processor (MicroBlaze API 
command) through the USB FX2 microcontroller endpoint EP1 buffer. 
This function is normally used to send 64 bytes packets to the USB endpoint EP1 (0x01).
This function is also able to obtain the response of the USB FX2 microcontroller or MicroBlaze embedded processor through the USB FX2
microcontroller endpoint EP1 (0x81).

  Parameters
1) CCyUSBDevice *USBDeviceList
CCyUSBDevice is a type defined in CyAPI.dll. Its name is misleading because it is not a class that represents a single USB device, 
but it rather represents a list of USB devices. CCyUSBDevice is the list of devices served by the CyUSB.sys driver (or a derivative 
like TE_USB_FX2_xx.sys). This parameter is passed by pointer. See page 7 and pages 23-49 of CyAPI.pdf (Cypress CyAPI Programmer's 
Reference).
2) byte* Command
This parameter is passed by pointer. It is the pointer to the byte array that contains the commands to send to USB FX2 microcontroller
(FX2_Commands) or to MicroBlaze (MB_Commands).
The byte array shall be properly initialized using instructions similar to the following ones:
Command[0] = I2C_WRITE;
Command[1] = MB_I2C_ADRESS;
Command[2] = I2C_BYTES;
Command[3] = 0;
Command[4] = 0;
Command[5] = 0;
Command[6] = Command2MB;
3) long CmdLength
This parameter is the length (in bytes) of the previous byte array; it is the length of the packet to transmit to USB FX2 controller 
endpoint EP1 (0x01). It is typically initialized to 64 bytes.
4) byte* Reply
This parameter (passed by pointer) is the pointer to the byte array that contains the response to the command sent to the USB FX2 
microcontroller (FX2_Commands) or to the MicroBlaze embedded processor (MB_Commands).
5) long ReplyLength.
This parameter is the length (in bytes) of the previous byte array; it is the length of the packet to transmit to the USB FX2 
microcontroller endpoint EP1 (0x81). It is typically initialized to 64 byes, but normally the meaningful bytes are less.
6) unsigned long Timeout.
The unsigned integer value is the time in milliseconds assigned to the synchronous method XferData() of data transfer used by CyAPI.dll. 
TimeOut is the time that is allowed to the function for sending/receiving the data packet passed to the function; this timeout 
shall be large enough to allow the data/command transmission/reception. Otherwise the transmission/reception will fail.  
See Timeout Setting.
  Return Value
1. int : integer type
This function returns true (ST_OK=0) if it is able to send a command to EP1 and receive a response within 2*Timeout milliseconds. 
This function returns false (ST_ERROR=1) otherwise.
enum ST_Status
{
	ST_OK = 0,
	ST_ERROR = 1
};


*/


TE_USB_FX2_CYAPI int TE_USB_FX2_SendCommand (CCyUSBDevice *USBDeviceList, byte* Command, long CmdLength, byte* Reply, long ReplyLength, unsigned long Timeout)
{
  
  bool bResultCommand = false;
  bool bResultReply = false;

  //Timeout 1000 ms = 1s
  //unsigned long TimeOut = 1000;

  CCyBulkEndPoint *BulkOutEP = NULL;
  CCyBulkEndPoint *BulkInEP = NULL;

  //Number of EndPoint of the Card/Device selected by a previous TE_USB_FX2_Open() function. 
  int eptCount = USBDeviceList->EndPointCount();
  //cout << eptCount << endl;

  //Search the EndPoint EP1 OUT for Command transmission from host computer to USB FX2 device
  for (int i=0; i<eptCount; i++)
  {
    bool bInC = ((USBDeviceList->EndPoints[i]->Address )==0x01); //& 0x01)==0x01); Cypress bad example
    bool bBulkC = (USBDeviceList->EndPoints[i]->Attributes == 2);
    //cout << "bInC" << bInC << endl;
    //cout << "bBulkC" << bBulkC << endl;

	//If the EndPoint EP1 OUT exists instantiate the abstract class in the concrete class CCyBulkEndPoint,
	//set the TimeOut to Timeout, use synchronous method XferData to send Command
    if (bBulkC && bInC)
    {
	  //If the EndPoint EP1 OUT exists instantiate the abstract class in the concrete class CCyBulkEndPoint
      BulkInEP = (CCyBulkEndPoint *) USBDeviceList->EndPoints[i];
	  //set the TimeOut to Timeout
      BulkInEP->TimeOut=Timeout;
	  //use synchronous method XferData to send Command
      bResultCommand=BulkInEP->XferData(Command, CmdLength);
      //cout << "sono bResultCommand" << bResultCommand << endl;
    }
  }

  //Search the EndPoint EP1 IN for Reply reception from USB FX2 device to host computer. 
  for (int i=1; i<eptCount; i++)
  {
    bool bInR = ((USBDeviceList->EndPoints[i]->Address )==0x81); //& 0x81)==0x81); Cypress bad example
    bool bBulkR = (USBDeviceList->EndPoints[i]->Attributes == 2);

	//If the EndPoint EP1 INPUT exists instantiate the abstract class in the concrete class CCyBulkEndPoint,
	//set the TimeOut to Timeout, use synchronous method XferData to receive Reply
    if (bBulkR && bInR && bResultCommand )
    {
	  //If the EndPoint EP1 OUT exists instantiate the abstract class in the concrete class CCyBulkEndPoint
      BulkOutEP = (CCyBulkEndPoint *) USBDeviceList->EndPoints[i];
	  //set the TimeOut to Timeout
      BulkOutEP->TimeOut=Timeout;
	  //use synchronous method XferData to receive Reply
      bResultReply=BulkOutEP->XferData(Reply, ReplyLength);
    }
  }

  if (bResultCommand && bResultReply)
    return ST_OK;
  else
    return ST_ERROR;

}

/*

TE_USB_FX2_GetData_InstanceDriverBuffer()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_GetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkInEP, 
PI_PipeNumber PipeNo,unsigned long TimeOut, int BufferSize)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_GetData_InstanceDriverBuffer (USBDeviceList, &BulkInEP, PipeNo, TimeOut, BufferSize);

  Description
This function takes an already initialized USB device list (USBDeviceList previously selected by TE_USB_FX2_Open()) and a not 
initialized CCyBulkEndPoint double pointer, BulkInEP. This function selects the endpoint to use: you shall choose EP6 (0x86) 
(endpoints EP4(0x84) or EP2(0x82) are also theoretically possible).
Currently (April 2012), only endpoint 0x86 is actually implemented in Trenz Electronic USB FPGA modules, so that endpoints EP2 and 
EP4 cannot be read or , more precisely, they are not even connected to the FPGA. That is why attempting to read them causes a 
function failure after Timeout expires.

TE_USB_FX2_GetData_InstanceDriverBuffer() function instantiates the class used by CyAPI to use bulk endpoint (CCyBulkEndPoint, 
see pages 9 to 11 of CyAPI.pdf (Cypress CyAPI Programmer's Reference)) and initializes the parameters of this class instantiation. 
The parameters are :
1. TimeOut
2. XMODE_DIRECT (this parameter set the driver to single buffering, instead the slower double buffering)
3. DeviceDriverBufferSize.
The last parameter force the instantiation of the driver buffer (SW side, on the host computer) for the endpoint 0x86; this buffer 
has a size in byte given by DeviceDriverBufferSize. This value is of great importance because the data throughput is strongly 
influenced by this parameter (see section 6 TE_USB_FX2_CyAPI.dll:  Data Transfer Throughput Optimization).
This function has not been included in TE_USB_FX2_GetData() for throughput reasons; if the driver buffer instantiation were 
repeated at every data reception, the data throughput would be halved. This function shall be used only one time to instantiate 
the driver buffer; after instantiation, TE_USB_FX2_GetData() can be used repeatedly without re-instantiating the driver buffer.

int RX_PACKET_LEN = 51200;//102400; 

int packetlen = RX_PACKET_LEN;
unsigned int packets = 500;//1200;//1200;
unsigned int DeviceDriverBufferSize = 131072;//409600;//131072;
unsigned long TIMEOUT= 18;
byte * data;
byte * data_temp = NULL;
unsigned int total_cnt = 0;
unsigned int errors = 0;

data = new byte [RX_PACKET_LEN*packets]; //allocate memory

PI_PipeNumber PipeNo = PI_EP6;

//starts test
SendFPGAcommand(USBDeviceList,FX22MB_REG0_START_TX); 

CCyBulkEndPoint *BulkInEP = NULL;

TE_USB_FX2_GetData_InstanceDriverBuffer (USBDeviceList,  &BulkInEP, PipeNo, TIMEOUT, DeviceDriverBufferSize);
 
ElapsedTime.Start(); //StopWatch start
for (unsigned int i = 0; i < packets; i++)
{
  packetlen = TX_PACKET_LEN;
  data_temp = &data[total_cnt];
  if (TE_USB_FX2_GetData(&BulkInEP, data_temp, packetlen)) 
  {
    cout << "ERROR read" << endl;
    errors++;
    break;
  }
  total_cnt += packetlen;
}
//DEBUG StopWatch 
TheElapsedTime = ElapsedTime.Stop(false); 

SendFPGAcommand(USBDevicelist,FX22MB_REG0_STOP); 

  Parameters
1)  CCyUSBDevice *USBDeviceList
CCyUSBDevice is a type defined in CyAPI.dll. Its name is misleading because it is not a class that represents a single USB device, 
but it rather represents a list of USB devices. CCyUSBDevice is the list of devices served by the CyUSB.sys driver 
(or a derivative like TE_USB_FX2_xx.sys). This parameter is passed by pointer. See page 7 and pages 23-49 of CyAPI.pdf 
(Cypress CyAPI Programmer's Reference).
2)  CCyBulkEndPoint **BulkInEP
This parameter is a double pointer to CCyBulkEndPoint. This parameter is used to pass the used BulkEndPoint parameter to 
TE_USB_FX2_GetData(). The double pointer is used because, if single pointer were used, the data modification of 
TE_USB_FX2_GetDataInstanceDriverBuffer() could not be passed over to TE_USB_FX2_GetData.()
3)  PI_PipeNumber PipeNo 
This parameter is the value that identifies the endpoint used for data transfer. It is called PipeNumber because it 
identifies the buffer (pipe) used by the USB FX2 microcontroller.
4)  unsigned long Timeout
It is the integer time value in milliseconds assigned to the synchronous method XferData() of data transfer used by CyAPI.dll. 
Timeout is the time that is allowed to to the function for sending/receiving the data packet passed to the function; this 
timeout shall be large enough to allow data/command transmission/reception.Otherwise the transmission/reception will fail. 
See Timeout Setting.
5)  int BufferSize
It is the dimension (in bytes) of the driver buffer (SW) used in data reception of a single endpoints (EP6 0x86 in this case); 
the total buffer size is the sum of BufferSize of every endpoint used. BufferSize has a strong influence on DataThroughput. 
If BufferSize is too small, DataThroughput can be 1/3-1/2 of the maximum value (from a maximum value of 36 Mbyte/s for read 
transactions to an actual value of 18 Mbyte/s). See 6 TE_USB_FX2_CyAPI.dll:  Data Transfer Throughput Optimization.
  Return Value
1) int : integer type
This function returns true (ST_OK=0) if the selected BulkEndPoint exists in the firmware. This function returns false (ST_ERROR=1) 
otherwise.
enum ST_Status
{
	ST_OK = 0,
	ST_ERROR = 1
};

*/

TE_USB_FX2_CYAPI int TE_USB_FX2_GetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkInEP, PI_PipeNumber PipeNo,unsigned long Timeout, int BufferSize)
{

  bool bResultDataRead = false;
  byte PipeNoHex = 0x00;

  //Shortest and more portable way to select the Address using the PipeNumber
  if (PipeNo == PI_EP2) PipeNoHex = 0x82;
  else PipeNoHex = 0x00;
  if (PipeNo == PI_EP4) PipeNoHex = 0x84;
  else PipeNoHex = 0x00;
  if (PipeNo == PI_EP6) PipeNoHex = 0x86;
  else PipeNoHex = 0x00;

  //Fundamental Note: currently (March 2012) only 0x86 EndPoint is actually implemented in TE-USB FPGA modules

  unsigned int XferSizeRead=0;

  unsigned int DeviceDriverBufferSize = BufferSize; //131072;//409600;//131072;

  //Number of EndPoint of the Card/Device selected by a previous TE_USB_FX2_Open() function. 
  int eptCount = USBDeviceList->EndPointCount();

  //Search the EndPoint EP6 INPUT for Read Fpga Ram transmission from USB FX2 device to host computer 
  for (int i=1; i<eptCount; i++)
  {
    bool bOutR = ((USBDeviceList->EndPoints[i]->Address )==PipeNoHex);
    bool bBulkR = (USBDeviceList->EndPoints[i]->Attributes == 2);

	//If the EndPoint EP6 INPUT exists instantiate the abstract class in the concrete class CCyBulkEndPoint,
	//set the TimeOut to Timeout, set the XferMode to XMODE_DIRECT,use synchronous method XferData to receive a packet
	//of DeviceDriverBufferSize
    if (bBulkR && bOutR)
    {
      (*BulkInEP) = (CCyBulkEndPoint *) USBDeviceList->EndPoints[i];
      (*BulkInEP)->TimeOut=Timeout;
      (*BulkInEP)->XferMode=XMODE_DIRECT;
      (*BulkInEP)->SetXferSize(DeviceDriverBufferSize);
      bResultDataRead=true;

	  /*This part must not be grayed out because has been moved to TE_USB_FX2_GetData() for Throughput reason*/
      //BulkInEP=&BulkInEP_Temp;
      //bResultDataWrite=BulkOutEP->XferData(data, len);
      //XferSizeRead=BulkOutEP->GetXferSize();
      //cout << "XferSizeRead" << XferSizeRead <<endl;
	  /*This part must not be grayed out because has been moved to TE_USB_FX2_GetData() for Throughput reason*/
    }
  }

  if (bResultDataRead)
    return ST_OK;
  else
    return ST_ERROR;
}

/*

TE_USB_FX2_GetData()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_GetData(CCyBulkEndPoint **BulkInEP, byte* DataRead, long DataReadLength)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_GetData(&BulkInEP, DataRead, DataReadLength);

  Description
This function takes an already initialized CCyBulkEndPoint double pointer. The device has been previously selected by 
TE_USB_FX2_Open(). TE_USB_FX2_GetData() reads data from the USB FX2 microcontroller endpoint EP6 (0x86) and transfers this 
data to the host computer. This data is generated by the FPGA.
  Expected Data Throughput
The maximum data throughput expected (with a DataReadLength= 120*10^6) is 37 Mbyte/s (PacketSize = BufferSize = 131,072), but in fact this value is variable between 31-36 Mbyte/s (the mean value seems 33.5 Mbyte/s); so if you measure this range of values, the data reception can be considered as normal.
The data throughput is variable in two ways:
1. depends on the used host computer;
2. varies with every function call.
  DataRead Size Shall Not Be Too Large
TE_USB_FX2_GetData() seems unable to use too large arrays or, more precisely, this fact seems variable by changing host computer. 
To be safe, do not try to transfer in a single packet very large data (e.g. 120 millions of byte); transfer the same data with many 
packets instead (1,200 packets * 100,000 byte) and copy the data in a single large data array if necessary.
  DataRead Size Shall Not Be Too Small
There are two reasons why DataRead size shall not be too small.
The first reason is described in section 1.1.4  PacketSize. PacketSize has also a strong influence on DataThroughput. 
If PacketSize is too small (e.g. 512 byte), you can have very low DataThroughput (2.2 Mbyte/s) even if you use a large 
driver buffer (driver buffer size = 131,072 bytes). See section 6 TE_USB_FX2_CyAPI.dll:  Data Transfer Throughput Optimization.
The second reason is that probably the FPGA imposes your minimum packet size. In a properly used read test mode 
(using FX22MB_REG0_START_TX and therefore attaching the FPGA), TE_USB_FX2_GetData() is unable to read less than 1024 byte. 
In a improperly used read test mode (not using FX22MB_REG0_START_TX and therefore detaching the FPGA), TE_USB_FX2_GetData() is 
able to read a packet size down to 64 byte. The same CyAPI method XferData() used (under the hood) in TE_USB_FX2_SendCommand() 
is able to read a packet size of 64 byte. These facts prove that the minimum packet size is imposed by FPGA. To be safe, 
we recommend to use this function with a size multiple of 1 kbyte. 

  Parameters
1) CCyBulkEndPoint **BulkInEP
This parameter is used to pass to TE_USB_FX2_GetData() the parameter of BulkEndPoint used. This parameter is a double pointer 
to CCyBulkEndPoint. The double pointer is used because if single pointer is used the data modification of 
TE_USB_FX2_GetDataInstanceDriverBuffer() cannot be passed to TE_USB_FX2_GetData().
2) byte* DataRead
C++ applications use directly TE_USB_FX2_CyAPI.dll based on CyAPI.dll. This parameter is passed by pointer to avoid copying back 
and forth large amount of data between these two DLLs. This parameter points the byte array that, after the function return, 
will contain the data read from the buffer EP6 of USB FX2 microcontroller. The data contained in EP6 is generated by the FPGA. 
If no data is contained in EP6, the byte array is left unchanged. 
3) long DataReadLength
This parameter is the length (in bytes) of the previous parameter.

  Return Value
1) int: integer type
This function returns true (ST_OK = 0) if it is able to receive the data from buffer EP6 within Timeout milliseconds. 
This function returns false (ST_ERROR = 1) otherwise.
enum ST_Status
{
	ST_OK = 0,
	ST_ERROR = 1
};

*/

TE_USB_FX2_CYAPI int TE_USB_FX2_GetData (CCyBulkEndPoint **BulkInEP, byte* DataRead, long DataReadLength)
{
 
  if (BulkInEP == NULL)
  {
    //cout << "Error,no device is selected. GetData" <<endl ;
    return ST_ERROR;
  }

  bool bResultDataRead = false;
  
  //use synchronous method XferData to receive DataRead from the HW buffer of EndPoint EP6 INPUT for Read Fpga Ram transmission 
  //from USB FX2 device to host computer
  bResultDataRead=(*BulkInEP)->XferData(DataRead, DataReadLength);

  if (bResultDataRead)
    return ST_OK;
  else
    return ST_ERROR;
}

/*

TE_USB_FX2_SetData_InstanceDriverBuffer()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_SetData_InstanceDriverBuffer(CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkOutEP, 
PI_PipeNumber PipeNo,unsigned long Timeout, int BufferSize)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_SetData_InstanceDriverBuffer (USBDeviceList, &BulkOutEP, PipeNo, Timeout, BufferSize);

  Description
This function takes an already initialized USB device list (USBDevice previously selected by TE_USB_FX2_Open()) and a not initialized
CCyBulkEndPoint double pointer, BulkOutEP. This function selects the endpoint to use: you shall choose EP8 (0x08) (endpoints EP4(0x04)
or EP2(0x02) are also theoretically possible).
Currently (April 2012), only endpoint 0x08 is actually implemented in Trenz Electronic USB FPGA modules, so that endpoints EP2 and EP4
cannot be written or , more precisely, they are not even connected to the FPGA. That is why attempting to write them causes a function
failure after Timeout expires.

TE_USB_FX2_SetData_InstanceDriverBuffer() function instantiates the class used by CyAPI to use Bulk EndPoint (CCyBulkEndPoint, 
see pages 9 to 11) and initializes the parameters of this class instantiation. The parameters are :
1. Timeout
2. XMODE_DIRECT (this parameter set the driver to single buffering, instead the slower double buffering)
3. DeviceDriverBufferSize.
The last parameter force the instantiation of the driver buffer (SW side, on the host computer) for the endpoint 0x86; this buffer 
has a size in byte given by DeviceDriverBufferSize. This value is of great importance because the data throughput is strongly 
influenced by this parameter (see section 6 TE_USB_FX2_CyAPI.dll:  Data Transfer Throughput Optimization).
This function has not been included in TE_USB_FX2_SetData() for throughput reasons; if the driver buffer instantiation were repeated 
at every data reception, the data throughput would be halved. This function shall be used only one time to instantiate the driver 
buffer; after instantiation, TE_USB_FX2_SetData() can be used repeatedly without re-instantiating the driver buffer.

int TX_PACKET_LEN = 51200;//102400; 

int packetlen = TX_PACKET_LEN;
unsigned int packets = 500;//1200;//1200;
unsigned int DeviceDriverBufferSize = 131072;//409600;//131072;
unsigned long TIMEOUT= 18;
byte * data;
byte * data_temp = NULL;
unsigned int total_cnt = 0;
unsigned int errors = 0;

data = new byte [TX_PACKET_LEN*packets]; //allocate memory

PI_PipeNumber PipeNo = PI_EP8;

//starts test
SendFPGAcommand(USBDeviceList,FX22MB_REG0_START_RX); 

CCyBulkEndPoint *BulkOutEP = NULL;

TE_USB_FX2_SetData_InstanceDriverBuffer (USBDeviceList,  &BulkOutEP, PipeNo, TIMEOUT, DeviceDriverBufferSize);
 
ElapsedTime.Start(); //StopWatch start
for (unsigned int i = 0; i < packets; i++)
{
  packetlen = TX_PACKET_LEN;
  data_temp = &data[total_cnt];
  //cout << "Address &BulkInEP" << &BulkInEP << endl;
  //cout << "Address BulkInEP" << BulkInEP << endl;
  //cout << "Address *BulkInEP" << (*BulkInEP) << endl;
  if (TE_USB_FX2_SetData(&BulkOutEP, data_temp, packetlen)) 
  {
    cout << "ERROR read" << endl;
    errors++;
    break;
  }
  total_cnt += packetlen;
}
//DEBUG StopWatch 
TheElapsedTime = ElapsedTime.Stop(false); 

SendFPGAcommand(USBDeviceList,FX22MB_REG0_STOP); 

  Parameters
1) CCyUSBDevice *USBDeviceList
CCyUSBDevice is a type defined in CyUSB.dll. Its name is misleading because it is not a class that represents a single USB device, 
but it rather represents a list of USB devices. CCyUSBDevice is the list of devices served by the CyUSB.sys driver (or a derivative 
like TE_USB_FX2_xx.sys). This parameter is passed by pointer. See page 7 and pages 23-49 of CyAPI.pdf (Cypress CyAPI Programmer's 
Reference).
2) CCyBulkEndPoint **BulkOutEP
This parameter is a double pointer to CCyBulkEndPoint. This parameter is used to pass the used BulkEndPoint parameter to 
TE_USB_FX2_SetData(). The double pointer is used because, if single pointer were used, the data modification of 
TE_USB_FX2_SetDataInstanceDriverBuffer() could not be passed over to TE_USB_FX2_SetData.()
3) PI_PipeNumber PipeNo 
This parameter is the value that identifies the endpoint used for data transfer. It is called PipeNumber because it identifies 
the buffer (pipe) used by the USB FX2 microcontroller.
4) unsigned long Timeout
It is the integer time value in milliseconds assigned to the synchronous method XferData() of data transfer used by CyAPI.dll. 
TimeOut is the time that is allowed to to the function for sending/receiving the data packet passed to the function; this Timeout 
shall be large enough to allow data/command transmission/reception.Otherwise the transmission/reception will fail. 
See Timeout Setting.
5) int BufferSize
Itis the dimension (in bytes) of the driver buffer (SW) used in data transmission of a single endpoints (EP8 0x08 in this case); 
the total buffer size is the sum of BufferSize of every endpoint used. BufferSize has a strong influence on DataThroughput. 
If BufferSize is too small, DataThroughput can be 1/3-1/2 of the maximum value (from a maximum value of 24 Mbyte/s for read 
transactions to an actual value of 18 Mbyte/s). 
See 6 TE_USB_FX2_CyAPI.dll:  Data Transfer Throughput Optimization.
3.7.4   Return Value
1. int : integer type
This function returns true (ST_OK=0) if the selected BulkEndPoint exists in the firmware. This function returns false (ST_ERROR=1) otherwise.
enum ST_Status
{
	ST_OK = 0,
	ST_ERROR = 1
};


*/


TE_USB_FX2_CYAPI int TE_USB_FX2_SetData_InstanceDriverBuffer (CCyUSBDevice *USBDeviceList, CCyBulkEndPoint **BulkOutEP, PI_PipeNumber PipeNo,unsigned long Timeout, int BufferSize)
{

  bool bResultDataWrite = false;
  byte PipeNoHex = 0x00;

  //Shortest and more portable way to select the Address using the PipeNumber
  if (PipeNo == PI_EP8) PipeNoHex = 0x08;
  else PipeNoHex = 0x00;

  unsigned int XferSizeRead=0;

  unsigned int DeviceDriverBufferSize = BufferSize; //131072;//409600;//131072;

  //Logic of the function GetData_InstanceDriverBuffer command START
  // Find a second bulk OUT endpoint in the EndPoints[] array

  //Number of EndPoint of the Card/Device selected by a previous TE_USB_FX2_Open() function. 
  int eptCount = USBDeviceList->EndPointCount();

  //Search the EndPoint EP8 OUTPUT for Write Fpga Ram transmission from host computer to USB FX2 device
  for (int i=1; i<eptCount; i++)
  {
    bool bOutR = ((USBDeviceList->EndPoints[i]->Address )==PipeNoHex);
    bool bBulkR = (USBDeviceList->EndPoints[i]->Attributes == 2);

	//If the EndPoint EP8 OUTPUT exists instantiate the abstract class in the concrete class CCyBulkEndPoint,
	//set the TimeOut to Timeout, set the XferMode to XMODE_DIRECT,use synchronous method XferData to transmit a packet
	//of DeviceDriverBufferSize
    if (bBulkR && bOutR)
    {
      (*BulkOutEP) = (CCyBulkEndPoint *) USBDeviceList->EndPoints[i];
      (*BulkOutEP)->TimeOut=Timeout;
      (*BulkOutEP)->XferMode=XMODE_DIRECT;
      (*BulkOutEP)->SetXferSize(DeviceDriverBufferSize);

	  /*This part must not be grayed out because has been moved to TE_USB_FX2_SetData() for Throughput reason*/
      //bResultDataWrite=BulkOutEP->XferData(data, len);
      //XferSizeRead=BulkOutEP->GetXferSize();
      //cout << "XferSizeRead" << XferSizeRead <<endl;
	  /*This part must not be grayed out because has been moved to TE_USB_FX2_SetData() for Throughput reason*/
    }
  }

  if (bResultDataWrite)
    return ST_OK;
  else
    return ST_ERROR;
}


/*

TE_USB_FX2_SetData()

  Declaration
TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (CCyBulkEndPoint **BulkOutEP, byte* DataWrite, long DataWriteLength)

  Function Call
Your application program shall call this function like this:
TE_USB_FX2_SetData (&BulkOutEP, DataWrite ,DataWriteLength);

  Description
This function takes an already initialized CCyBulkEndPoint double pointer. The device has been previously selected by 
TE_USB_FX2_Open(). TE_USB_FX2_SetData() reads data from the host computer and writes them to the USB FX2 microcontroller endpoint 
EP8 (0x08). This data is then passed over to the FPGA.
If there is not a proper connection (not using FX22MB_REG0_START_RX) between FPGA and USB FX2 microcontroller, the function can 
experience a strange behavior. For example, a very low throughput (9-10 Mbyte/s even if a 22-24 Mbyte/s are expected) is measured 
or the function fails returning false. These happen because buffer EP8 (the HW buffer, not the SW buffer of the driver whose size 
is given by BufferSize parameter) is already full (it is not properly read/emptied by the FPGA) and no longer able to receive 
further packets.
  Expected Data Throughput
The maximum data throughput expected (with a DataWriteLength= 120*10^6) is 24 Mbyte/s (PacketSize = BufferSize =131,072) but in 
fact this value is variable between 22-24 Mbyte/s (the mean value seems 24 Mbyte/s); so if you measure this range of values, 
the data reception can be considered as normal.
The data throughput is variable in two ways:
1. depends on the used host computer (on some host computers this value is even higher: 29 Mbyte/s)
2. varies with every function call.
  DataWrite Shall Not Be Too Large
TE_USB_FX2_SetData() seems unable to use too large arrays or, more precisely, this fact seems variable by changing host computer. To be safe, do not try to transfer in a single packet very large data (e.g. 120 millions of byte); transfer the same data with many packets instead (1,200 packets * 100,000 byte) and copy the data in a single large data array if necessary.
  DataWrite Shall Not Be Too Small
The reason is described in section 1.1.4 PacketSize. PacketSize has also a strong influence on DataThroughput. 
If PacketSize is too small (e.g. 512 byte), you can have very low DataThroughput (2.2 Mbyte/s) even if you use a large driver buffer 
(driver buffer size = 131,072 bytes). 
See section 6 TE_USB_FX2_CyAPI.dll:  Data Transfer Throughput Optimization.

  Parameters
1) CCyBulkEndPoint **BulkOutEP
This parameter is used to pass to TE_USB_FX2_SetData() the parameter of BulkEndPoint used. This parameter is a double pointer to 
CcyBulkEndPoint. The double pointer is used because if single pointer is used the data modification of 
TE_USB_FX2_SetDataInstanceDriverBuffer() cannot be passed to TE_USB_FX2_SetData().
2) byte* DataWrite
C++ applications use directly TE_USB_FX2_CyAPI.dll based on CyAPI.dll. This parameter is passed by pointer to avoid copying back 
and forth large amount of data between these two DLLs. This parameter points the byte array that, after the function return, 
will contain the data written into buffer EP8 of USB FX2 microcontroller. The data contained in EP8 is generated by the host computer. 
3) long DataWriteLength
This parameter is the length (in bytes) of the previous parameter.

  Return Value
1. int : integer type
This function returns true (ST_OK = 0) if it is able to write the data to buffer EP8 within Timeout milliseconds. This function returns false (ST_ERROR = 1) otherwise.
enum ST_Status
{
	ST_OK = 0,
	ST_ERROR = 1
};

*/

TE_USB_FX2_CYAPI int TE_USB_FX2_SetData (CCyBulkEndPoint **BulkOutEP, byte* DataWrite, long DataWriteLength)
{

  if (BulkOutEP == NULL)
  {
    //cout << "Error,no device is selected" <<endl ;
    return ST_ERROR;
  }

  bool bResultDataWrite=false;

  //use synchronous method XferData to trnsmit DataWrite from the HW buffer of EndPoint EP8 OUTPUT for Wtite Fpga Ram transmission 
  //from host computer to USB FX2 device
  bResultDataWrite=(*BulkOutEP)->XferData(DataWrite, DataWriteLength);

  if (bResultDataWrite)
    return ST_OK;
  else
    return ST_ERROR;
}
