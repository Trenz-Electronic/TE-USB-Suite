using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyUSB;
using System.Runtime.InteropServices;
using System.Runtime;
using Microsoft.Win32;
//using System.ComponentModel;
using System.IO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SINGLE_TRANSFER
{
  public byte bmRequest;
  public byte bRequest;
  public ushort wValue;
  public ushort wIndex;
  public ushort wLength;
  public uint ulTimeout;

  public byte WaitForever;
  public byte EptAddress;
  public byte NtStatus;
  public uint UsbdtStatus;
  public uint IsoPacketOffset;
  public uint IsoPacketLength;
  public uint BufferOffset;
  public uint BufferLength;
}

namespace TE_USB_FX2
{

  public class TE_USB_FX2
  {
    /// <summary>
    /// //This class contain all the function used as C# DLL
    /// //In the application you can call these functions in the following way
    /// //TE_USB_FX2.TE_USB_FX2.FunctionName()
    /// </summary>
    /// <returns></returns>

    /// <summary>
    /// 3.1   TE_USB_FX2_ScanCards()
    /// 
    ///3.1.1   Declaration
    ///public static int TE_USB_FX2_ScanCards(ref USBDeviceList USBdevList)
    ///
    ///3.1.2   Function Call
    ///Your application program shall call this function like this:
    ///TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_ScanCards( ref USBdevList);
    ///
    ///3.1.3   Description
    ///This function takes (a null initialized or an already initialized) USB device list, (re-)creates a USB device list, 
    ///searches for Trenz Electronic USB FX2 devices (Cypress driver derivative and VID = 0xbd0, PID=0x0300) devices and counts them.
    ///This function returns the number of Trenz Electronic USB FX2 devices attached to the USB bus of the host computer.
    ///
    ///3.1.4   Parameters
    ///1. ref USBDeviceList USBdevList
    ///USBDeviceList is a type defined in CyUSB.dll.
    ///USBdevList is the list of devices served by the CyUSB.sys driver (or a derivative like TE_USB_FX2.sys). This parameter is 
    ///passed by reference (ref). See page 139-140 of CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's Reference).
    ///
    ///3.1.5   Return Value
    ///1. int : integer type.
    ///This function returns the number of USB devices attached to the host computer USB bus.
    /// </summary>
    /// <param name="USBdevList"></param>
    /// <returns = int></returns>

    public static int TE_USB_FX2_ScanCards(ref USBDeviceList USBdevList)
    {
      int CardCount = 0;
      UInt16 PID = 0x0000;
      UInt16 VID = 0x0000;
      //Creation of a list of USB device that use the CYUSB.SYS driver
      USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);

      //USBdevList.Count : this parameter give the number of card that use CyUSB.sys and its derivative 
      //(like TE_USB_FX2_64.sys and TE_USB_FX2_32.sys used by Trenz Electronic)

      //If exist at least an USB device that use the CYUSB.SYS driver,
      //I search and count the number of these devices that are of Trenz Electronic
      if (USBdevList.Count != 0)
      {
        // Look for a device having VID = 0bd0, PID = 0300
        foreach (USBDevice dev in USBdevList)
        {
          PID = dev.ProductID;
          VID = dev.VendorID;
          if ((((PID == 0x0300) && (VID == 0x0bd0)) == true)) CardCount++;
        }
        USBdevList.Dispose();
        return CardCount;
      }
      else
      {
        USBdevList.Dispose();
        return 0;
      }
    }

    /*
     * 3.2   TE_USB_FX2_Open()   
     * 
     * 3.2.1   Declaration
     * public static bool TE_USB_FX2_Open(ref CyUSBDevice TE_USB_FX2_USBDevice, ref USBDeviceList USBdevList, int CardNumber)
     * 
     * 3.2.2   Function Call
     * Your application program shall call this function like this:
     * TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Open( ref TE_USB_FX2_USBDevice, ref USBdevList, CardNumber);
     * 
     * 3.2.3   Description
     * This function takes (a null initialized or an already initialized) USB device list, (re-)creates a USB device list , 
     * searches for Trenz Electronic USB FX2 devices (Cypress driver derivative and VID = 0xbd0, PID=0x0300) and counts them. 
     * If no device is attached, TE_USB_FX2_USB_device (CyUSBDevice type) is initialized to null.
     * If one or more devices are attached and
     * 1) if 0 <= CardNumber <= (number of attached devices – 1), then 
     * TE_USB_FX2_USBDevice (CyUSBDevice type) will point to and will be initialized according to the selected device.
     * 2) if CardNumber >= number of attached devices, then 
     * TE_USB_FX2_USBDevice (CyUSBDevice type) is initialized to null.
     * 
     * A more intuitive name for this function would have been TE_USB_FX2_SelectCard().
     * 
     * 3.2.4   Parameters
     * 1. ref CyUSBDevice TE_USB_FX2_USBDevice
     * TE_USB_FX2_USBDevice is the module selected by this function. This is the most useful value returned by this function. This parameter is passed by reference (ref). See pages 70-93 of CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's Reference).
     * 2. ref USBDeviceList USBdevList
     * USBDeviceList is a type defined in CyUSB.dll. USBdevList is the list of devices served by the CyUSB.sys driver (or a derivative like TE_USB_FX2.sys). This parameter is passed by reference (ref). See page 139-140 of CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's Reference)
     * 3. int CardNumber
     * This is the number of the selected Trenz Electronic USB FX2 device.
     * 
     * 3.2.5   Return Value
     * 1. bool : logical type
     * This function returns true if it is able to find the module selected by CardNumber. If unable to do so, it returns false.
     * 
     */ 
    
    /// <summary>
    /// //////
    /// </summary>
    /// <param name="TE_USB_FX2_USBDevice"></param>
    /// <param name="USBdevList"></param>
    /// <param name="CardNumber"></param>
    /// <returns=bool></returns>

    public static bool TE_USB_FX2_Open(ref CyUSBDevice TE_USB_FX2_USBDevice, ref USBDeviceList USBdevList, int CardNumber)
    {

      int CardCounted = 0;   // Trenz Device
      //int DeviceNumber = 0;  // Cypress Device ( number >= TrenzDevice)

      //Number of Cypress Device (Trenz Electronic or not)
      int CypressDeviceNumber = 0;
      //Number of Trenz Device
      int TrenzDeviceNumber = 0;
      //Position of Trenz Device desired in the USBDeviceList
      int DeviceNumber = 0;

      UInt16 PID = 0x0000;
      UInt16 VID = 0x0000;

      //Creation of a list of USB device that use the CYUSB.SYS driver
      USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
      //If exist at least an USB device that use the CYUSB.SYS driver,
      //I search and count the number of these devices that are of Trenz Electronic
      if (USBdevList.Count != 0)
      {
        foreach (USBDevice USBdev in USBdevList)
        {
          PID = USBdev.ProductID;
          VID = USBdev.VendorID;
          //Number of Cypress Card augmented by one
          CypressDeviceNumber++;
          if ((((PID == 0x0300) && (VID == 0x0bd0)) == true))
          {
            //Number of Trenz Card (a subcategory of Cypress Card) augmented by one
            //CardCount++;
            TrenzDeviceNumber++;
            //Console.WriteLine("PID e VID: {0}, {1}", PID, VID);
            // CardNumber=TrenzDeviceNumber-1 by definition.
            if ((TrenzDeviceNumber - 1) == CardNumber)
            {
              //I store the DeviceNumber that identify the Trenz Card (CardNumber) requested
              //Memorize this number for later use
              //This is the position of Trenz Device desired in the USBDeviceList
              DeviceNumber = CypressDeviceNumber - 1;
              //Console.WriteLine("DeviceNumber: {0}", DeviceNumber);
            }
          }
        }
      }

      //At this point I memorize the Cards Counted and zeroed the variable that I have used in the counting.
      CardCounted = TrenzDeviceNumber;
      //Console.WriteLine("CardCounted: {0}", CardCount);
      TrenzDeviceNumber = 0;

      //Now I search the Trenz USB Device with the Card Number (CardNo) specified
      if (((CardNumber >= 0) && (CardNumber < CardCounted)) == true)  //CardCounted
      {
        USBDevice USBdev = USBdevList[DeviceNumber];
        PID = USBdev.ProductID;
        VID = USBdev.VendorID;
        if ((((PID == 0x0300) && (VID == 0x0bd0)) == true))
        {
          TE_USB_FX2_USBDevice = USBdev as CyUSBDevice;
          //Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
          return true;
        }
        else
        {
          TE_USB_FX2_USBDevice = null;
          return false;
        }
      }
      else
      {
        TE_USB_FX2_USBDevice = null;
        return false;
      }
    }

    /*
     * 3.3   TE_USB_FX2_Close()
     * 
     * 3.3.1   Declaration
     * public static bool TE_USB_FX2_Close(ref USBDeviceList USBdevList)
     * 
     * 3.3.2   Function Call
     * Your application program shall call this function like this:
     * TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Close(ref USBdevList);
     * 
     * 3.3.3   Description
     * This function takes an already initialized USB device list and disposes it.
     * Due to the fact that we are coding C# here, the device list can or cannot be erased; this is in the scope of the garbage 
     * collector and it cannot be forced by the user. Sometimes it is erased instantly, some other times it is never erased, 
     * until the user closes the application program that uses this data.
     * Use of TE_USB_FX2_Close() function is NOT recommended for new software projects. Users may use this function only just before 
     * exiting their applications. If users use this function anywhere else, they shall
     * manage System.ObjectDisposedException exceptions (try – catch) or
     * avoid using disposed resources.
     * Note: USBdevList is disposed, not set to null.
     * try
     * {
     *   Application Code
     * }
     * catch (System.ObjectDisposedException)
     * {
     *   Console.WriteLine("TE_USB_FX2_USBDevice disposed: you have used the wrong procedure!");
     * }
     * 
     * If you want to close the current USB device (card) without opening another one, you shall use TE_USB_FX2_Open() with a device 
     * number (CardNumber) that certainly does not exist (e.g. CardNumber = 200, because there can be a maximum of 127 USB devices 
     * connected to a single host controller). The reason of this behavior is due to the CyUSB.dll as explained by Cypress document 
     * CyUSB.NET.pdf, pages 132-133 and pages 139-140: “You should never invoke the Dispose method of a USBDevice directly. Rather, 
     * the appropriate technique is to call the Dispose method of the USBDeviceList object that contains the USBDevice objects”
     * This function differs from its homonym of the previous TE0300DLL.dll in that it does not close a Handle but disposes (erases) 
     * all USB devices in the list.
     * A more intuitive name for this function would have been TE_USB_FX2_CloseAll or TE_USB_FX2_Dispose.
     * 
     * 3.3.4   Parameters
     * 1. ref USBDeviceList USBdevList
     * USBDeviceList is a type defined in CyUSB.dll. USBdevList is the list of Trenz Electronic USB FX2 devices attached to the USB bus 
     * host computer. This parameter is passed by reference (ref). See page 139-140 of CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's
     * Reference).
     * 3.3.5   Return Value
     * 1. bool : logical type
     * This function returns true if it is able to dispose the list. If unable to do so, it returns false.
     */

    /// <summary>
    /// //////
    /// </summary>
    /// <param name="USBdevList"></param>
    /// <returns></returns>

    public static bool TE_USB_FX2_Close(ref USBDeviceList USBdevList)
    {
      if (USBdevList != null)
      {
        USBdevList.Dispose();
        return true;
      }
      else
        return false;
    }

    /*
    // This one must be corrected, cause trouble
    public static bool TE_USB_FX2_DisplayDriverInformation(ref CyUSBDevice TE_USB_FX2_USBDevice, ref USBDeviceList USBdevList, int CardNumber)
    {

      int CardCounted = 0;   // Trenz Board
      int DeviceNumber = 0;  // Cypress Board ( number >= TrenzBoard)

      int CypressDeviceNumber = 0;
      int TrenzDeviceNumber = 0;

      UInt16 PID = 0x0000;
      UInt16 VID = 0x0000;

      uint DriverVersion1 = 0;

      string DriverName1 = null;

      //Creation of a list of USB device that use the CYUSB.SYS driver
      USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
      //If exist at least an USB device that use the CYUSB.SYS driver,
      //I search and count the number of these devices that are of Trenz Electronic
      if (USBdevList.Count != 0)
      {
        foreach (USBDevice USBdev in USBdevList)
        {
          PID = USBdev.ProductID;
          VID = USBdev.VendorID;
          //Number of Cypress Card augmented by one
          CypressDeviceNumber++;
          if ((((PID == 0x0300) && (VID == 0x0bd0)) == true))  //0x0bd0 . 0x0bd0
          {
            //Number of Trenz Card (a subcategory of Cypress Card) augmented by one
            //CardCount++;
            TrenzDeviceNumber++;
            Console.WriteLine("PID e VID: {0}, {1}", PID, VID);
            // CardNumber=TrenzDeviceNumber-1 by definition.
            if ((TrenzDeviceNumber - 1) == CardNumber)
            {
              //I store the DeviceNumber that identify the Trenz Card (CardNumber) requested
              DeviceNumber = CypressDeviceNumber - 1;
              Console.WriteLine("DeviceNumber: {0}", DeviceNumber);
            }
          }
        }
      }

      //At this point I memorize the Cards Counted and zeroed the variable that I have used in the counting.
      CardCounted = TrenzDeviceNumber;
      //Console.WriteLine("CardCounted: {0}", CardCount);
      TrenzDeviceNumber = 0;
      //Now I search the Trenz USB Device with the Card Number (CardNo) specified
      if (((CardNumber >= 0) && (CardNumber < CardCounted)) == true)  //CardCounted
      {
        USBDevice USBdev = USBdevList[DeviceNumber];
        PID = USBdev.ProductID;
        VID = USBdev.VendorID;
        if ((((PID == 0x0300) && (VID == 0x0bd0)) == true))
        {
          TE_USB_FX2_USBDevice = USBdev as CyUSBDevice;
          Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);

          //I cast the abstract USBdev in a concrete CyUSBDevice
          TE_USB_FX2_USBDevice = USBdev as CyUSBDevice;
          DriverVersion1 = TE_USB_FX2_USBDevice.DriverVersion;
          Console.WriteLine("DriverVersion {0} ", DriverVersion1);
          DriverName1 = TE_USB_FX2_USBDevice.DriverName;
          Console.WriteLine("Original Name of the Driver {0} ", DriverName1);

          return true;
        }
        else
        {
          TE_USB_FX2_USBDevice = null;
          return false;
        }
      }
      else
      {
        TE_USB_FX2_USBDevice = null;
        return false;
      }
    }
    */


    /*
     * 
     *3.4   TE_USB_FX2_SendCommand()
     *
     *3.4.1   Declaration
     *public static bool TE_USB_FX2_SendCommand(ref CyUSBDevice TE_USB_FX2_USBDevice, ref byte[] Command, ref int CmdLength,
     *ref byte[] Reply, ref int ReplyLength, uint Timeout)
     *
     *3.4.2   Function Call
     *Your application program shall call this function like this:
     *TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand (ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, 
     *ref ReplyLength, Timeout);
     *
     *3.4.3   Description
     *This function takes an already initialized USB device (previously selected by TE_USB_FX2_Open()) and sends a command 
     *(API command) to the USB FX2 microcontroller (USB FX2 API command) or to the MicroBlaze embedded processor 
     *(MicroBlaze API command) through the USB FX2 microcontroller endpoint EP1 buffer. 
     *This function is normally used to send 64 bytes packets to the USB endpoint EP1 (0x01).
     *This function is also able to obtain the response of the USB FX2 microcontroller or MicroBlaze embedded processor through 
     *the USB FX2 microcontroller endpoint EP1 (0x81).
     *3.4.4   Parameters
     *1. ref CyUSBDevice TE_USB-FX2_USBDevice
     *CyUSBDevice is a type defined in CyUSB.dll. This parameter points to the module selected by TE_USB_FX2_Open(). 
     *This parameter is passed by reference (ref). See pages 70-93 of CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's Reference)
     *2. ref byte[] Command
     *This parameter is passed by reference (ref). It is the byte array that contains the commands to send to USB FX2 microcontroller
     *(FX2_Commands) or to the MicroBlaze embedded processor (MB_Commands).
     *The byte array shall be properly initialized using instructions similar to the following ones:
     *Command[0] = (byte)FX2_Commands.I2C_WRITE;
     *Command[1] = (byte)FX2_Commands.MB_I2C_ADDRESS;
     *Command[2] = (byte)FX2_Commands.I2C_BYTES;
     *Command[3] = (byte)0;
     *Command[4] = (byte)0;
     *Command[5] = (byte)0;
     *Command[6] = (byte)Command2MB;
     *3. ref int CmdLength
     *This parameter (passed by reference (ref)) is the length (in bytes) of the previous byte array; it is the length of the 
     *packet to transmit to USB FX2 controller endpoint EP1 (0x01). It is typically initialized to 64 bytes.
     *4. ref byte[] Reply
     *This parameter (passed by reference (ref)) is the byte array that contains the response to the command sent to the 
     *USB FX2 microcontroller (FX2_Commands) or to the MicroBlaze embedded processor (MB_Commands).
     *5. ref int ReplyLength
     *This parameter (passed by reference (ref)) is the length (in bytes) of the previous byte array; it is the length of 
     *the packet to transmit to the USB FX2 microcontroller endpoint EP1 (0x81). It is typically initialized to 64 byes, 
     *but normally the meaningful bytes are less. The parameter is a reference, meaning that the method can modify its value. 
     *The number of bytes actually received is passed back in ReplyLength.
     *6. uint Timeout
     *The unsigned integer value is the time in milliseconds assigned to the synchronous method XferData() of data transfer used 
     *by CyUSB.dll. 
     *Timeout is the time that is allowed to the function for sending/receiving the data packet passed to the function; 
     *this timeout shall be large enough to allow the data/command transmission/reception. Otherwise the transmission/reception will fail. See 1.1.2 Timeout Setting.
     *3.4.5   Return Value
     *1. bool : logical type
     *This function returns true if it is able to send a command to EP1 and  receive a response within 2*Timeout milliseconds. 
     *This function returns false otherwise.
     * 
     */

    /// <summary>
    /// /////
    /// </summary>
    /// <param name="TE03xxUSBdevice"></param>
    /// <param name="Command"></param>
    /// <param name="CmdLength"></param>
    /// <param name="Reply"></param>
    /// <param name="ReplyLength"></param>
    /// <param name="Timeout"></param>
    /// <returns></returns>

    public static bool TE_USB_FX2_SendCommand(ref CyUSBDevice TE_USB_FX2_USBDevice, ref byte[] Command, ref int CmdLength,
        ref byte[] Reply, ref int ReplyLength, uint Timeout)
    {
      bool bResultCommand = false;
      bool bResultReply = false;


      //Concrete class
      CyBulkEndPoint inEndpoint1 = null;
      CyBulkEndPoint outEndpoint1 = null;

      if (TE_USB_FX2_USBDevice != null)
      {
        //CyBulkEndPoint TE03xxUSBDeviceConcrete = TE03xxUSBdevice as CyBulkEndPoint;
        //Select the endpoint of IN number 1 (EP1 INPUT)
        inEndpoint1 = TE_USB_FX2_USBDevice.EndPointOf(0x81) as CyBulkEndPoint;
        //Select the endpoint of OUT number 1 (EP1 OUTPUT) 
        outEndpoint1 = TE_USB_FX2_USBDevice.EndPointOf(0x01) as CyBulkEndPoint;

        // Set the timeout
        outEndpoint1.TimeOut = Timeout;
        inEndpoint1.TimeOut = Timeout;

        //calls the XferData function for bulk transfer(OUT) in the cyusb.dll
        bResultCommand = outEndpoint1.XferData(ref Command, ref CmdLength);
        //Console.WriteLine("bResultCommand  {0} ", bResultCommand);
        //Console.WriteLine("Command[0] {0:X2} ", Command[0]);
        //Console.WriteLine("CmdLength {0} ", CmdLength);

        //uint inUSBstatus1 = inEndpoint1.UsbdStatus;
        //Console.WriteLine("UsbdStatus {0:X8} e ", inUSBstatus1);

        //uint inUSBstatus2 = inEndpoint1.NtStatus;
        //Console.WriteLine("NtStatus {0:X8} e ", inUSBstatus2);

        if (bResultCommand == true)
        {
          //calls the XferData function for bulk transfer(IN) in the cyusb.dll
          bResultReply = inEndpoint1.XferData(ref Reply, ref ReplyLength);
          //Console.WriteLine("bResultReply  {0}  ", bResultReply);
        }
        else
          return false;
        if ((bResultCommand && bResultReply) == true)
          return true;
        else
          return false;
      }
      else
        return false;
    }

    /*
     * 3.5   TE_USB_FX2_GetData()
     * 
     * 3.5.1   Declaration
     * public static bool TE_USB_FX2_GetData(ref CyUSBDevice TE_USB_FX2_USBDevice, ref byte[] DataRead, ref int DataReadLength, 
     * int PipeNo, uint Timeout, int BufferSize)
     * 
     * 3.5.2   Function Call
     * Your application program shall call this function like this:
     * TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetData(ref TE_USB_FX2_USBDevice, ref DataRead, ref DataReadLength, PI_EP6, Timeout, 
     * BufferSize);
     * 
     * 3.5.3   Description
     * This function takes an already initialized USB Device (previously selected by TE_USB_FX2_Open()) and reads data from 
     * USB FX2 microcontroller endpoint EP6 (0x86) (endpoints EP4(0x84) or EP2(0x82) are also theoretically possible). 
     * Data comes from the FPGA.
     * Currently (April 2012), only endpoint 0x86 is actually implemented in Trenz Electronic USB FPGA modules, so that endpoints 
     * EP2 and EP4 cannot be read or , more precisely, they are not even connected to the FPGA. That is why attempting to read them 
     * causes a function failure after Timeout expires.
     * 3.5.4   Expected Data Throughput
     * The maximum data throughput expected (with a DataReadLength= 120*10^6) is 37 Mbyte/s (PacketSize = BufferSize = 131,072), 
     * but in fact this value is variable between 31-36 Mbyte/s (the mean value seems 33.5 Mbyte/s); so if you measure this range 
     * of values, the data reception can be considered as normal.
     * The data throughput is variable in two ways:
     * 1. depends on the used host computer;
     * 2. varies with every function call.
     * 3.5.5   DataRead Size Shall Not Be Too Large
     * TE_USB_FX2_GetData() seems unable to use too large arrays or, more precisely, this fact seems variable by changing host 
     * computer. To be safe, do not try to transfer in a single packet very large data (e.g. 120 millions of byte); transfer the 
     * same data with many packets instead (1,200 packets * 100,000 byte) and copy the data in a single large data array if necessary
     * (with Buffer.BlockCopy()). Buffer.BlockCopy seems not to hinder throughput too much (max 2 Mbyte/s)
     * 3.5.5.1   Reduced version (pseudo code)
     * PACKETLENGTH=100000;
     * packets=1200;
     * byte[] data = new byte[packetlen*packets];
     * byte[] buffer = new byte[packetlen];
     * for (int i = 0; i < packets; i++)
     * {
     *   TE_USB_FX2_GetData(ref TE_USB_FX2_USBDevice, ref buffer, ref packetlen, PI_EP6, TIMEOUT_MS,BUFFER_SIZE)
     *   Buffer.BlockCopy(buffer, 0, data, total_cnt, packetlen);
     *   total_cnt += packetlen; 
     * }
     * 3.5.5.2   Expanded version (code)
     * PACKETLENGTH=100000;
     * packets=1200;
     * byte[] data = new byte[packetlen*packets];
     * byte[] buffer = new byte[packetlen];
     * //starts test: the FPGA start to write data in the buffer EP6 of FX2 chip
     * SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_START_TX, TIMEOUT_MS);
     * test_cnt = 0;
     * total_cnt = 0;
     * for (int i = 0; i < packets; i++)
     * {
     *   //buffer = &data[total_cnt];
     *   packetlen = PACKETLENGTH;
     *   //fixed (byte* buffer = &data[total_cnt]
     *   bResultXfer = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetData(ref   TE_USB_FX2_USBDevice, ref buffer, ref packetlen, PI_EP6, 
     *   TIMEOUT_MS,BUFFER_SIZE);
     *   Buffer.BlockCopy(buffer, 0, data, total_cnt, packetlen); 
     *   if (bResultXfer == false)
     *   {
     *     //cout << "ERROR" << endl;
     *     Console.WriteLine("Error Get Data");
     *     SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_STOP, TIMEOUT_MS);
     *     return;
     *   }
     *   total_cnt += packetlen
     *}
     *  //stop test: the FPGA start to write data in the buffer EP6 of //FX2 chip
     *SendFPGAcommand(ref TE_USB_FX2_USBDevice,
     *MB_Commands.FX22MB_REG0_STOP, TIMEOUT_MS);
     *3.5.6   DataRead Size Shall Not Be Too Small
     *There are two reasons why DataRead size shall not be too small.
     *The first reason is described in section 1.1.4 PacketSize. PacketSize has also a strong influence on DataThroughput. 
     *If PacketSize is too small (e.g. 512 byte), you can have very low DataThroughput (2.2 Mbyte/s) even if you use a large driver 
     *buffer (driver buffer size = 131,072 bytes). See section 6 TE_USB_FX2_CyUSB.dll:  Data Transfer Throughput Optimization.
     *The second reason is that probably the FPGA imposes your minimum packet size. In a properly used read test mode 
     *(using FX22MB_REG0_START_TX and therefore attaching the FPGA), TE_USB_FX2_GetData() is unable to read less than 1024 byte. 
     *In a improperly used read test mode (not using FX22MB_REG0_START_TX and therefore detaching the FPGA), TE_USB_FX2_GetData() 
     *is able to read a packet size down to 64 byte. The same CyUSB method XferData() used (under the hood) in 
     *TE_USB_FX2_SendCommand() is able to read a packet size of 64 byte. These facts prove that the minimum packet size is imposed 
     *by FPGA. To be safe, we recommend to use this function with a size multiple of 1 kbyte. 
     *
     *3.5.7   Parameters
     *1. ref CyUSBDevice TE_USB-FX2_USBDevice
     *This parameter points to the module selected by TE_USB_FX2_Open(). This parameter is passed by reference (ref). See pages 70-93
     *of CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's Reference)
     *2. ref byte[] DataRead
     *This parameter is passed by reference (ref). C# applications use directly TE_USB_FX2_CyUSB.dll based on CyUSB.dll. To avoid 
     *copying back and forth large amount of data between these two DLLs, data is passed by reference rather than by value. This 
     *parameter points to the byte array that, after the function returns, will contain the data read from the buffer EP6 of the 
     *USB FX2 microcontroller. The data contained in EP6 generated by the FPGA. If no data is contained in EP6, the byte array is 
     *left unchanged. 
     *3. ref int DataReadLength
     *This parameter is the length (in bytes) of the previous byte array; it is the length of the packet read from the USB FX2 
     *microcontroller endpoint EP6 (0x86). It is typically PacketLength. This parameter is passed by reference (ref).
     *4. int PipeNumber
     *This parameter is the value that identifies the endpoint used for data transfer. It is called PipeNumber because it identifies 
     *the buffer (pipe) used by the USB FX2 microcontroller.
     *5. uint Timeout
     *It is the integer time value in milliseconds assigned to the synchronous method XferData() of data transfer used by CyUSB.dll. 
     *Timeout is the time that is allowed to the function for sending/receiving the data packet passed to the function; this timeout 
     *shall be large enough to allow data/command transmission/reception.. Otherwise the transmission/reception will fail. 
     *See 1.1.2 Timeout Setting.
     *6. int BufferSize
     *It is the dimension (in bytes) of the driver buffer (SW) used in data reception of a single endpoint (EP6 0x86 in this case)
     *single endpoint (EP6 0x86 in this case); the total buffer size is the sum of BufferSize of every endpoint used. BufferSize has 
     *a strong influence on DataThroughput. If the BufferSize is too small,  DataThroughput can be 1/3-1/2 of the maximum value 
     *(from a maximum value of 36 Mbyte/s for read transactions to an actual value of 18 Mbyte/s). If BufferSize has a large value 
     *(a roomy buffer), the program shall be able to cope with the non-deterministic behavior of C# without losing packets.
     *
     *3.5.8   Return Value
     *1. bool : logical type
     *This function returns true if it is able to receive the data from buffer EP6 within Timeout milliseconds. 
     *This function returns false otherwise.
     * 
     */

    /// <summary>
    /// ////////
    /// </summary>
    /// <param name="TE03xxUSBdevice"></param>
    /// <param name="DataRead"></param>
    /// <param name="DataReadLength"></param>
    /// <param name="PipeNo"></param>
    /// <param name="Timeout"></param>
    ///  /// <returns></returns>

    //public static bool TE_USB_FX2_GetData(ref CyUSBDevice TE_USB_FX2_USBDevice,ref CyBulkEndPoint inEndpointPipeNo, ref byte[] DataRead, ref int DataReadLength, int PipeNo, uint Timeout)
    public static bool TE_USB_FX2_GetData(ref CyUSBDevice TE_USB_FX2_USBDevice, ref byte[] DataRead, ref int DataReadLength, int PipeNo, uint Timeout, int BufferSize)
    {
      bool bResultDataRead = false;
      byte PipeNoHex = 0x00;

      //Shortest and more portable way to select the Address using the PipeNumber

      CyBulkEndPoint inEndpointPipeNo = null;
      //Shortest and more portable way
      if (PipeNo == 2) PipeNoHex = 0x82;
      else PipeNoHex = 0x00;
      if (PipeNo == 4) PipeNoHex = 0x84;
      else PipeNoHex = 0x00;
      if (PipeNo == 6) PipeNoHex = 0x86;
      else PipeNoHex = 0x00;

      //Fundamental Note: currently (March 2012) only 0x86 EndPoint is actually implemented in TE-USB FPGA modules

      if ((TE_USB_FX2_USBDevice != null) && (PipeNoHex == 0x86))  //(TE_USB_FX2_USBDevice != null) &&
      {
        inEndpointPipeNo = TE_USB_FX2_USBDevice.EndPointOf(PipeNoHex) as CyBulkEndPoint;
        inEndpointPipeNo.TimeOut = Timeout;

        //int MaxPacketSize= outEndpointPipeNo.MaxPktSize;
        //Console.WriteLine("MaxPacketSize {0} ", MaxPacketSize);

        //int XferSize1 = outEndpointPipeNo.XferSize;
        //Console.WriteLine("XferSize {0} ", XferSize1);

        //outEndpointPipeNo.XferMode = XMODE.DIRECT;

        inEndpointPipeNo.XferSize = BufferSize; // 131072;

        //calls the XferData function for bulk transfer(IN) in the cyusb.dll
        bResultDataRead = inEndpointPipeNo.XferData(ref DataRead, ref DataReadLength);
        //uint inUSBstatus1 = inEndpointPipeNo.UsbdStatus;
        //Console.WriteLine("UsbdStatus {0:X8} ", inUSBstatus1);

        //uint inUSBstatus2 = inEndpointPipeNo.NtStatus;
        //Console.WriteLine("NtStatus {0:X8} ", inUSBstatus2);

        if (bResultDataRead == true) return true;
        else return false;
      }
      else return false;

    }


    /*
     * 3.6   TE_USB_FX2_SetData()
     * 
     * 3.6.1   Declaration
     * public static bool TE_USB_FX2_SetData(ref CyUSBDevice TE_USB_FX2_USBDevice, ref byte[] DataWrite, ref int DataWriteLength, 
     * int PipeNo, uint Timeout, int BufferSize)
     * 
     * 3.6.2   Function Call
     * Your application program shall call this function like this:
     * TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SetData(ref TE_USB_FX2_USBDevice, ref DataWrite, ref DataWriteLength, PI_EP8, Timeout, 
     * BufferSize);
     * 
     * 3.6.3   Description
     * This function takes an already initialized USB device (CyUSBDevice is a type defined in CyUSB.dll), selected by 
     * TE_USB_FX2_Open(), and writes data to the USB FX2 microcontroller endpoint EP8 (0x08). This data is then passed to the FPGA.
     * If there is not a proper connection (not using FX22MB_REG0_START_RX) between FPGA and USB FX2 microcontroller, 
     * the function can experience a strange behavior. For example, a very low throughput (9-10 Mbyte/s even if a 22-24 Mbyte/s are 
     * expected) is measured or the function fails returning false. These happen because buffer EP8 (the HW buffer, not the 
     * SW buffer of the driver whose size is given by BufferSize parameter) is already full (it is not properly read/emptied by the 
     * FPGA) and no longer able to receive further packets.
     * 3.6.4   Data throughput expected
     * The maximum data throughput expected (with a DataWriteLength= 120*10^6) is 24 Mbyte/s (PacketSize = BufferSize =131,072) 
     * but in fact this value is variable between 22-24 Mbyte/s (the mean value seems 24 Mbyte/s); so if you measure this range 
     * of values, the data reception can be considered normal.
     * The data throughput is variable in two way:
     * 1. depends on which host computer is used (on some host computers this value is even higher: 29 Mbyte/s)
     * 2. vary with every function call
     * 3.6.5   DataWrite size shall not be too large
     * TE_USB_FX2_SetData() seems unable to use too large arrays or, more precisely, this fact seems variable by changing host 
     * computer. To be safe, do not try to transfer in a single packet very large data (120 millions of byte); transfer the same 
     * data with many packets (1,200 packets * 100,000 byte) and copy the data in a single large data array if necessary (with 
     * Buffer.BlockCopy()). Buffer.BlockCopy seems not to hinder throughput too much (max 2 Mbyte/s).
     * 3.6.5.1   Reduced version (pseudo code)
     * PACKETLENGTH=100000;
     * packets=1200;
     * byte[] data = new byte[packetlen*packets];
     * byte[] buffer = new byte[packetlen];
     * for (int i = 0; i < packets; i++)
     * {
     *   Buffer.BlockCopy(data, total_cnt, buffer, 0, packetlen);
     *   TE_USB_FX2_SetData(ref TE_USB_FX2_USBDevice, ref buffer, ref   packetlen, PI_EP8, 	TIMEOUT_MS,BUFFER_SIZE);
     *   total_cnt += packetlen; 
     * }
     * 3.6.5.2   Expanded version (code)
     * SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_START_RX, TIMEOUT_MS);
     * //ElapsedTime.Start(); //StopWatch start
     * Stopwatch stopWatch = new Stopwatch();
     * stopWatch.Start();
     * for (int i = 0; i < packets; i++)
     * {
     *   packetlen = PACKETLENGTH;
     *   Buffer.BlockCopy(data, total_cnt, buffer, 0, packetlen);
     *   if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SetData(ref  TE_USB_FX2_USBDevice, ref buffer, ref packetlen, PI_EP8, TIMEOUT_MS, 
     *   BUFFER_SIZE) == false) errors++;
     *   else total_cnt += packetlen;
     * }
     * //total_cnt += (packetlen * packets);
     * stopWatch.Stop();
     * 3.6.6   DataWrite size shall not be too small
     * The  reason is described in section 1.1.4 PacketSize.
     * PacketSize has also a strong influence on DataThroughput. If PacketSize is too small (512 byte for example) you can achieve 
     * very low data throughput (2.2 Mbyte/s) even if you use a large driver buffer (driver buffer size = 131,072 byte). 
     * See 6 TE_USB_FX2_CyUSB.dll:  Data Transfer Throughput Optimization.
     * 
     * 3.6.7   Parameters
     * 1. ref CyUSBDevice TE_USB-FX2_USBDevice
     * This parameter is passed by reference (ref). It points to the module selected by TE_USB_FX2_Open(). See pages 70-93 of 
     * CyUSB.NET.pdf (Cypress CyUSB .NET DLL Programmer's Reference)
     * 2. ref byte[] DataWrite
     * This parameter is passed by reference (ref). C# applications use directly TE_USB_FX2_CyUSB.dll based on CyUSB.dll. To avoid 
     * copying back and forth large amount of data between these two DLLs, data is passed by reference and not by value.
     * This parameter points to the byte array that contains the data to be written to buffer EP8 (0x08) of USB FX2 microcontroller. 
     * Data contained in EP8 are then read by the FPGA.
     * 3. ref int DataWriteLength
     * This parameter is passed by reference (ref). This parameter is the length (in bytes) of the previous byte array; 
     * it is the length of the packet read from FX2 USB endpoint EP6 (0x86). Normally it is PacketLength.
     * 4. int PipeNumber
     * This parameter is the value that identify the endpoint used for the data transfer. It is called PipeNumber because it 
     * identifies the buffer (pipe) used by the USB FX2 microcontroller.
     * 5. uint Timeout.
     * The unsigned integer value is the time in milliseconds assigned to the synchronous method XferData() of data transfer used by 
     * CyUSB.dll. 
     * Timeout is the time that is allowed to the function for sending/receiving the data packet passed to the function; this 
     * timeout shall be large enough to allow the data/command transmission/reception. Otherwise the transmission/reception will 
     * fail. See 1.1.2 Timeout Setting.
     * 6. int BufferSize
     * The integer value is the dimension (in bytes) of the driver buffer (SW) used in data transmission of a single endpoint 
     * (EP8 0x08 in this case); the total buffer size is the sum of all BufferSize of every endpoint used.
     * The BufferSize has a strong influence on DataThroughput. If  BufferSize is too small, DataThroughput can be 1/3-1/2 of 
     * the maximum value (from a maximum value of 24 Mbyte/s for write transactions to an actual value of 14 Mbyte/s). 
     * If BufferSize has a large value (a roomy buffer), the program shall be able to cope with the non-deterministic behavior of 
     * C# without losing packets.
     * 
     * 3.6.8   Return Value
     * 1. bool: logical type
     * This function returns true if it is able to write data to buffer EP8 within Timeout milliseconds. 
     * This function returns false otherwise.
     * 
     */

    public static bool TE_USB_FX2_SetData(ref CyUSBDevice TE_USB_FX2_USBDevice, ref byte[] DataWrite, ref int DataWriteLength, int PipeNo, uint Timeout, int BufferSize)
    //public static bool TE_USB_FX2_SetData(ref CyBulkEndPoint outEndpointPipeNo, ref byte[] DataWrite, ref int DataWriteLength)
    {
      bool bResultDataRead = false;
      byte PipeNoHex = 0x00;

      CyBulkEndPoint outEndpointPipeNo = null;
      //Shortest and more portable way to select the Address using the PipeNumber
      if (PipeNo == 8) PipeNoHex = 0x08;
      else PipeNoHex = 0x00;

      if ((TE_USB_FX2_USBDevice != null) && (PipeNoHex == 0x08))
      {
        outEndpointPipeNo = TE_USB_FX2_USBDevice.EndPointOf(PipeNoHex) as CyBulkEndPoint;
        outEndpointPipeNo.TimeOut = Timeout;

        //int MaxPacketSize= outEndpointPipeNo.MaxPktSize;
        //Console.WriteLine("MaxPacketSize {0} ", MaxPacketSize);

        //int XferSize1 = outEndpointPipeNo.XferSize;
        //Console.WriteLine("XferSize {0} ", XferSize1);

        //outEndpointPipeNo.XferMode = XMODE.DIRECT;

        outEndpointPipeNo.XferSize = BufferSize;// 131072;

        //calls the XferData function for bulk transfer(IN) in the cyusb.dll
        bResultDataRead = outEndpointPipeNo.XferData(ref DataWrite, ref DataWriteLength);
        //Console.WriteLine("bResultDataRead {0} ", bResultDataRead);

        //uint inUSBstatus1 = outEndpointPipeNo.UsbdStatus;
        //Console.WriteLine("UsbdStatus {0:X8} e ", inUSBstatus1);

        //uint inUSBstatus2 = outEndpointPipeNo.NtStatus;
        //Console.WriteLine("NtStatus {0:X8} e ", inUSBstatus2);

        if (bResultDataRead == true) return true;
        else return false;
      }
      else return false;
    }


  }
}
