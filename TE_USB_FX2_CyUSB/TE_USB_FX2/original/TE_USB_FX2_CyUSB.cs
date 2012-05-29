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
    /// //This function take ( a null initialized or an already initialized) USB Device List
    /// //(USBDeviceList is a type defined in CyUSB DLL), (re-)create an USB Device List ,
    /// //search for USB Trenz Device and count them.
    /// //This function return the number of Trenz's USB Device attached to computer.
    /// //To use this function in an application program you must write
    /// //TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_ScanCards()
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
      //If exist at least an USB device that use the CYUSB.SYS driver,
      //I search and count the number of these devices that are of Trenz Electronic
      if (USBdevList.Count != 0)
      {
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

    /// <summary>
    /// ///////
    /// </summary>
    /// <param name="TE_USB_FX2_USBDevice"></param>
    /// <param name="USBdevList"></param>
    /// <param name="CardNumber"></param>
    /// <returns></returns>

    public static bool TE_USB_FX2_Open(ref CyUSBDevice TE_USB_FX2_USBDevice, ref USBDeviceList USBdevList, int CardNumber)
    {

      int CardCounted = 0;   // Trenz Board
      int DeviceNumber = 0;  // Cypress Board ( number >= TrenzBoard)

      int CypressDeviceNumber = 0;
      int TrenzDeviceNumber = 0;

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

    /// <summary>
    /// //////
    /// </summary>
    /// <param name="USBdevList"></param>
    /// <returns></returns>

    public static bool TE_USB_FX2_Close(ref USBDeviceList USBdevList)
    {
      if (USBdevList != null)
      {
        //Console.WriteLine("Close funziona?");
        USBdevList.Dispose();
        //Console.WriteLine("Close funziona A {0} ", USBdevList);
        //if (USBdevList == null)
        //Console.WriteLine("Close funziona");
        return true;
      }
      else
        //Console.WriteLine("Close non funziona B {0}" , USBdevList);
        return false;
    }


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
        //Select the endpoint of OUT number 1
        outEndpoint1 = TE_USB_FX2_USBDevice.EndPointOf(0x81) as CyBulkEndPoint;
        //Select the endpoint of IN number 1
        inEndpoint1 = TE_USB_FX2_USBDevice.EndPointOf(0x01) as CyBulkEndPoint;

        // Set the timeout
        outEndpoint1.TimeOut = Timeout;
        inEndpoint1.TimeOut = Timeout;

        //calls the XferData function for bulk transfer(IN) in the cyusb.dll
        bResultCommand = inEndpoint1.XferData(ref Command, ref CmdLength);
        //Console.WriteLine("bResultCommand  {0} ", bResultCommand);
        //Console.WriteLine("Command[0] {0:X2} ", Command[0]);
        //Console.WriteLine("CmdLength {0} ", CmdLength);

        //uint inUSBstatus1 = inEndpoint1.UsbdStatus;
        //Console.WriteLine("UsbdStatus {0:X8} e ", inUSBstatus1);

        //uint inUSBstatus2 = inEndpoint1.NtStatus;
        //Console.WriteLine("NtStatus {0:X8} e ", inUSBstatus2);

        if (bResultCommand == true)
        {
          //calls the XferData function for bulk transfer(OUT) in the cyusb.dll
          bResultReply = outEndpoint1.XferData(ref Reply, ref ReplyLength);
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
