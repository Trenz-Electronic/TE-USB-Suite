using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using CyUSB;
using TE_USB_FX2;

using System.Diagnostics;

namespace TE_USB_FX2_SampleApplication
{

  public enum FX2_Commands
  {
    READ_VERSION = 0x00,
    INITALIZE = 0xA0,
    READ_STATUS = 0xA1,
    WRITE_REGISTER = 0xA2,
    READ_REGISTER = 0xA3,
    RESET_FIFO_STATUS = 0xA4,
    FLASH_READ = 0xA5,
    FLASH_WRITE = 0xA6,
    FLASH_ERASE = 0xA7,
    EEPROM_READ = 0xA8,
    EEPROM_WRITE = 0xA9,
    GET_FIFO_STATUS = 0xAC,
    I2C_WRITE = 0xAD,
    I2C_READ = 0xAE,
    //I2C_BYTES = 0x0C,
    //I2C_MICROBLAZE_ADDRESS = 0x3F,
    POWER_ON = 0xAF,
    FLASH_WRITE_COMMAND = 0xAA,
    SET_INTERRUPT = 0xB0,
    GET_INTERRUPT = 0xB1
  };

  public enum MB_Commands
  {
    FX22MB_REG0_NOP = 0,
    FX22MB_REG0_GETVERSION = 1,
    FX22MB_REG0_START_TX = 2,
    FX22MB_REG0_START_RX = 3,
    FX22MB_REG0_STOP = 4,
    FX22MB_REG0_PING = 5
  };

  public enum FX2_Parameters
  {
      I2C_BYTES = 0x0C,
      MB_I2C_ADDRESS = 0x3F
  };

  class TE_USB_FX2_SampleApplication
  {

    static void DrawMenu()
    {
      //cout << endl;
      Console.WriteLine("TE_USB_FX2 DLL Example 1.0 ");
      Console.WriteLine("    1 - Get number of cards ");
      Console.WriteLine("    2 - Connect cardNo 0");
      Console.WriteLine("    3 - Connect cardNo 1");
      Console.WriteLine("    4 - Disconnect");
      Console.WriteLine("    5 - Get FX2 firmware version");
      Console.WriteLine("    6 - Get FPGA firmware version");
      Console.WriteLine("    7 - Get FX2 FIFO Status");
      Console.WriteLine("    8 - Reset FX2 FIFO Status");
      Console.WriteLine("    9 - Read high speed data (FPGA RX)");
      Console.WriteLine("    10 - Write high speed data (FPGA TX)");

      Console.WriteLine("    0 - Exit "); ;
    }

    static int GetFPGAstatus(ref CyUSBDevice TE_USB_FX2_USBDevice)
    {
      byte [] Command = new byte[64];
      byte [] Reply = new byte[64];
      int CmdLength = 64;
      int ReplyLength = 64;

      uint TIMEOUT_MS=1000;

      Command[0] = (byte)FX2_Commands.SET_INTERRUPT;
      Command[1] = (byte)FX2_Parameters.MB_I2C_ADDRESS;
      Command[2] = (byte)FX2_Parameters.I2C_BYTES;

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength,TIMEOUT_MS)==false)
      {
        //cout << "Error" << endl;
        Console.WriteLine("Error 1");
        return -1;
      }

      //Console.WriteLine("Reply {0}, {1} ,{2} ,{3}, {4}", Reply[0], Reply[1], Reply[2], Reply[3], Reply[4]);

      Command[0] = (byte)FX2_Commands.GET_INTERRUPT;	//read from interrupt data register
      //if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS)==false)
      //{
      //    //cout << "Error" << endl;
      //    Console.WriteLine("Error 2");
      //    return -1;
      //}

      int ItIsPassed = 0;

      Reply[0]=0;
      while (Reply[0] == 0)
      {
        //Console.WriteLine("Wait");
        //Console.WriteLine("Reply {0}, {1} ,{2} ,{3}, {4}", Reply[0], Reply[1], Reply[2], Reply[3], Reply[4]);
        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS)==false)
        {
          //cout << "Error" << endl;
          Console.WriteLine("Error 3");
          return -1;
        }
        ItIsPassed = (int)Reply[4];
      }

      return ItIsPassed;//(int)Reply[4]; //return data verification status
    }

    public static bool SendFPGAcommand(ref CyUSBDevice TE_USB_FX2_USBDevice, MB_Commands Command , uint Timeout)
    {

      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return false;
      }


      bool bResultCommand_FX22MB = false;
      byte[] cmd_FX22MB_REG0 = new byte[64];
      byte[] reply_FX22MB_REG0 = new byte[64];
      int cmd_length_FX22MB_REG0 = 64;
      int reply_length_FX22MB_REG0 = 64;

      //Command layout:
      //0xAD - Command type I2C write
      //0x3F - Address (I2C address of Microblaze)
      //0x0C - I2C_BYTES
      //0x00 - FX22MB_REG0_NOP
      //0x00 - FX22MB_REG0_NOP
      //0x00 - FX22MB_REG0_NOP
      //0x?? - Command

      //0x AD 3F 0C 00 00 00 ??
      cmd_FX22MB_REG0[0] = (byte)FX2_Commands.I2C_WRITE;
      cmd_FX22MB_REG0[1] = (byte)FX2_Parameters.MB_I2C_ADDRESS;
      cmd_FX22MB_REG0[2] = (byte)FX2_Parameters.I2C_BYTES;
      cmd_FX22MB_REG0[3] = (byte)MB_Commands.FX22MB_REG0_NOP;
      cmd_FX22MB_REG0[4] = (byte)MB_Commands.FX22MB_REG0_NOP;
      cmd_FX22MB_REG0[5] = (byte)MB_Commands.FX22MB_REG0_NOP;
      cmd_FX22MB_REG0[6] = (byte)Command;

      bResultCommand_FX22MB = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref cmd_FX22MB_REG0, ref cmd_length_FX22MB_REG0, ref cmd_FX22MB_REG0, ref reply_length_FX22MB_REG0, Timeout);

      //bResultCommand_FX22MB_REG0_START = inEndpointCmd.XferData(ref cmd_FX22MB_REG0_START, ref cmd_length_FX22MB_REG0_START);

      //Console.WriteLine("bResultCommand_FX22MB_REG0_START {0}", bResultCommand_FX22MB_REG0_START);

      //bResultReply_FX22MB_REG0_START = outEndpointCmd.XferData(ref reply_FX22MB_REG0_START, ref reply_length_FX22MB_REG0_START);
      //END

      //Console.WriteLine("bResultReply_FX22MB_REG0_START {0}", bResultReply_FX22MB_REG0_START);

      if (bResultCommand_FX22MB) return true;
      else return false;
    }

    static void GetNumberOfCards(ref USBDeviceList USBdevList)
    {
      //cout << endl << TE0300_ScanCards() << endl;
      int NumberOfCardAttached = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_ScanCards(ref USBdevList);
      Console.WriteLine("The number of card is {0} ", NumberOfCardAttached);
    }

    static void GetFX2version(CyUSBDevice TE_USB_FX2_USBDevice)
    {
      //byte Command[64], Reply[64];
      byte[] Command = new byte[64];
      byte[] Reply = new byte[64];
      int CmdLength = 64;
      int ReplyLength = 64;
      uint TIMEOUT_MS = 1000;

      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return;
      }

      UInt16 VID = TE_USB_FX2_USBDevice.VendorID;
      //Console.WriteLine("VID {0:X4} e ", VID);

      UInt16 PID = TE_USB_FX2_USBDevice.ProductID;
      //Console.WriteLine("PID {0:X4} e ", PID);


      Command[0] = (byte)FX2_Commands.READ_VERSION;
      //comand read FX2 version
      //Console.WriteLine("Command[0] {0:X2} ", Command[0]);

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == true)
      {

        if (ReplyLength >= 4)
        {
          //printf("Major version: %d \n", Reply[0]);
          //printf("Minor version: %d \n", Reply[1]);
          //printf("Device hi: %d \n", Reply[2]);
          //printf("Device lo: %d \n", Reply[3]);
          Console.WriteLine("Major version: {0}", Reply[0]);
          Console.WriteLine("Minor version: {0}", Reply[1]);
          Console.WriteLine("Device hi: {0}", Reply[2]);
          Console.WriteLine("Device lo: {0}", Reply[3]);
        }
      }
      else
        //cout << "Error" << endl;
        Console.WriteLine("Error");
    }

    static void GetFPGAversion(CyUSBDevice TE_USB_FX2_USBDevice)
    {

      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return;
      }

      //byte Command[64], Reply[64];
      byte[] Command = new byte[64];
      byte[] Reply = new byte[64];
      int CmdLength = 64;
      int ReplyLength = 64;

      uint TIMEOUT_MS = 1000;

      // byte MB_I2C_ADRESS = 0x3f;
      //byte I2C_BYTES = 12;

      Command[0] = (byte)FX2_Commands.SET_INTERRUPT;
      Command[1] = (byte)FX2_Parameters.MB_I2C_ADDRESS;
      Command[2] = (byte)FX2_Parameters.I2C_BYTES;

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
      {
        //cout << "Error" << endl;
        Console.WriteLine("Error Send Command SET INTERRUPT");
        //DrawMenu();
        return;
      }

      //Command[0] = (byte)FX2_Commands.GET_INTERRUPT;    //clear interrupt data register

      Command[0] = (byte)FX2_Commands.I2C_WRITE; //0xAD;//comand I2C_WRITE
      //Command[1] = (byte)FX2_Commands.I2C_MICROBLAZE_ADDRESS;
      //Command[2] = (byte)FX2_Commands.I2C_BYTES;
      Command[3] = (byte)0;
      Command[4] = (byte)0;
      Command[5] = (byte)0;
      Command[6] = (byte)MB_Commands.FX22MB_REG0_GETVERSION;//1; //get FPGA version

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
      {
        //cout << "Error" << endl;
        Console.WriteLine("Error Send Command Get FPGA Version");
        return;
      }

      Command[0] = (byte)FX2_Commands.GET_INTERRUPT; //0xB1;//comand GET_INTERRUPT

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == true)
      {
        if ((ReplyLength > 4) && (Reply[0] != 0))
        {
          //Console.WriteLine("INT# : {0}", Reply[0]);
          Console.WriteLine("Major version: {0}", Reply[1]);
          Console.WriteLine("Minor version: {0}", Reply[2]);
          Console.WriteLine("Release version: {0}", Reply[3]);
          Console.WriteLine("Build version: {0}", Reply[4]);
        }
      }
      else
        Console.WriteLine("Error, GET INTERRUPT");
    }

    static void GetFX2FifoStatus(CyUSBDevice TE_USB_FX2_USBDevice)
    {

      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return;
      }

      //byte Command[64], Reply[64];
      byte[] Command = new byte[64];
      byte[] Reply = new byte[64];
      int CmdLength = 64;
      int ReplyLength = 64;

      uint TIMEOUT_MS = 1000;

      Command[0] = (byte)FX2_Commands.GET_FIFO_STATUS;

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == true)
      {
        if (ReplyLength >= 4)
        {
          Console.WriteLine("EP2 FIFO CS: {0:X2}", Reply[0]);
          Console.WriteLine("EP4 FIFO CS: {0:X2}", Reply[1]);
          Console.WriteLine("EP6 FIFO CS: {0:X2}", Reply[2]);
          Console.WriteLine("EP8 FIFO CS: {0:X2}", Reply[3]);
          //Console.WriteLine("EP2 FIFO BCH: {0:X2}", Reply[4]);
          //Console.WriteLine("EP4 FIFO BCH: {0:X2}", Reply[5]);
          //Console.WriteLine("EP6 FIFO BCH: {0:X2}", Reply[6]);
          //Console.WriteLine("EP8 FIFO BCH: {0:X2}", Reply[7]);
        }
      }
      else
        //cout << "Error" << endl;
        Console.WriteLine("Error");
    }

    static void ResetFX2FifoStatus(CyUSBDevice TE_USB_FX2_USBDevice)
    {

      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return;
      }

      //cout << endl << "Resetting all FIFOs" << endl;
      Console.WriteLine("Resetting all FIFOs");
      byte[] Command = new byte[64];
      byte[] Reply = new byte[64];
      int CmdLength = 64;
      int ReplyLength = 64;

      uint TIMEOUT_MS = 100000;

      Command[0] = (byte)FX2_Commands.RESET_FIFO_STATUS;
      Command[1] = 0; //reset all fifos

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
        //cout << "Error" << endl;
        Console.WriteLine("Error Send Command Reset all fifos");

      Command[0] = (byte)FX2_Commands.INITALIZE; //0xA0;//comand SWITCH_MODE
      Command[1] = 1;//FIFO mode : is not in the documentation

      if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
        //cout << "Error" << endl;
        Console.WriteLine("Error Switch Mode Fifo Mode");
    }

    static void ReadDataFPGAIntegrity(CyUSBDevice TE_USB_FX2_USBDevice, int BUFFER_SIZE, int PACKETSNUMBER, int PACKETLENGTH, uint TIMEOUT_MS)
    {

      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return;
      }

      //int PACKETLENGTH = 51200;//102400;//102400;//102400;//1228800;//1638400;//512;

      //int PACKETSNUMBER = 500;

      //int BUFFER_SIZE = 131072;

      int packetlen = PACKETLENGTH;

      int packets = PACKETSNUMBER;

      //uint TIMEOUT_MS = 2000;
      int PI_EP6 = 6;

      uint verification=0;
      int test_cnt = 0;
      int errors = 0;

      bool bResultXfer = false;

      int total_cnt = 0;

      byte[] data = new byte[packetlen*packets];

      byte[] buffer = new byte[packetlen];

      //fixed (byte *buf = data)

      //allocate memory

      ResetFX2FifoStatus(TE_USB_FX2_USBDevice);

      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();

      //bResult_GetData_Start = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetData_Start(ref TE_USB_FX2_USBDevice, TIMEOUT_MS);
      SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_START_TX, TIMEOUT_MS); //starts test

      test_cnt = 0;
      total_cnt = 0;
      for (int i = 0; i < packets; i++)
      {
        //buf = &data[total_cnt];
        packetlen = PACKETLENGTH;
        //fixed (byte* buf = &data[total_cnt])
        //bResultXfer = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetDataP(ref inEndpointPipeNo, buf, ref packetlen, PI_EP6, TIMEOUT_MS);
        bResultXfer = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetData(ref TE_USB_FX2_USBDevice, ref buffer, ref packetlen, PI_EP6, TIMEOUT_MS,BUFFER_SIZE);
        Buffer.BlockCopy(buffer, 0, data, total_cnt, packetlen);
        if (bResultXfer == false)
        {
          //cout << "ERROR" << endl;
          Console.WriteLine("Error Get Data");
          //DrawMenu();
          //bResult_GetData_Stop = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetData_Stop(ref TE_USB_FX2_USBDevice, TIMEOUT_MS);
          //SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_STOP, TIMEOUT_MS);
          //return;
        }
        total_cnt += packetlen;

        //ResetFX2FifoStatus(TE_USB_FX2_USBDevice);

      }

      //bResult_GetData_Stop = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_GetData_Stop(ref TE_USB_FX2_USBDevice, TIMEOUT_MS);
      SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_STOP, TIMEOUT_MS);

      stopWatch.Stop();
      TimeSpan ts = stopWatch.Elapsed;

      packetlen = PACKETLENGTH;
      test_cnt=0;
      int total_cnt2 = packetlen;
      for (int i2 = 0; i2 < packets; i2++)
      {
        for (int j2 = 0; j2 < packetlen; j2 += 4)
        {
          //verification = (data[j+3]<<24) | (data[j+2]<<16) | (data[j+1]<<8) | data[j];
          verification = (uint)((data[total_cnt2 - packetlen + j2] << 24) | (data[total_cnt2 - packetlen + j2 + 1] << 16) | (data[total_cnt2 - packetlen + j2 + 2] << 8) | data[total_cnt - packetlen + j2 + 3]);
          //verification = (((uint)(data[j ]) * 256*256*256) + ((uint)(data[j + 1]) * 256*256) + ((uint)(data[j + 2]) * 256) + ((uint)(data[j+3 ])))/1;
          if (verification != test_cnt)
          {
            //				        if (printout) cout << "VERIFICATION ERROR" << endl;
            errors++;
            //				        break;
          }
          else
          {
            //				        if (printout) cout << "VERIFICATION ERROR" << endl;
            //errors++;
            //				        break;
          }

          test_cnt++;
        }
        total_cnt2 += packetlen;
        //Console.WriteLine(" total_cnt2 {0}", total_cnt2);
      }


      //Console.WriteLine("Read test passed");
      //Console.WriteLine(" {0:X}", data);
      Console.WriteLine(" Errors {0}", errors);

      string elapsedTime = String.Format("(0:0000)", ts.Milliseconds);
      int msTime = ts.Milliseconds;

      string elapsedTime2 = String.Format("(0:00)", ts.Seconds);
      int sTime = ts.Seconds;

      Console.WriteLine("TimeSpan {0} ", ts);

      Console.WriteLine("TimeSpan Seconds {0} ", sTime);

      Console.WriteLine("TimeSpan MilliSeconds {0} ", msTime);

      double total_cnt_Float = (double)total_cnt;

      msTime = sTime * 1000 + msTime;

      double msTime_Float = (double)msTime;

      // (total_cnt / (1024 * 1024 * (msTime / 1000)) ) = ( (total_cnt*1000) / (1024 * 1024 * (msTime)) )

      Console.WriteLine("Millisecond {0} ", msTime);

      double throughput_float = 0.0;

      throughput_float = (total_cnt_Float / (1024 * 1024 * (msTime_Float / 1000)));

      string throughput_string = throughput_float.ToString("0.00");

      if (errors >= 1) Console.WriteLine("memory->host data verification FAILED: {0} Byte ERRORS", errors);
      else Console.WriteLine("memory->host data verification PASSED!!!");

      Console.WriteLine("Transferred {0} kB in {1} ms = {2} MB/s", total_cnt / 1024, msTime, throughput_string);

      ResetFX2FifoStatus(TE_USB_FX2_USBDevice);
      //DrawMenu();
      return;
    }

    static void WriteDataFPGAIntegrity(CyUSBDevice TE_USB_FX2_USBDevice, int BUFFER_SIZE, int PACKETSNUMBER, int PACKETLENGTH, uint TIMEOUT_MS, uint GetStatusFPGAynFlag)
    {
      if (TE_USB_FX2_USBDevice == null)
      {
        Console.WriteLine("Error,no device is selected");
        return;
      }

      int errors = 0;

      //int PACKETLENGTH = 51200;//102400;//102400; //512;
      //int PACKETSNUMBER = 500; //200;
      //int BUFFER_SIZE = 131072;
      //uint TIMEOUT_MS = 50;
      int PI_EP8 = 8;

      int packetlen = PACKETLENGTH;//122880000;//2457600;// 65536; // 102400 * 24;// 65536;

      int packets = PACKETSNUMBER;

      int total_cnt = 0;

      byte[] data = new byte[packetlen*packets];  //packetlen*packets //
      //allocate memory

      byte[] buffer = new byte[packetlen];

      ResetFX2FifoStatus(TE_USB_FX2_USBDevice);

      for (uint j = 0; j < (packetlen * packets); j += 4)
      {
        data[j] = (byte)((0xFF000000 & total_cnt) >> 24); //(byte)(0);
        data[j + 1] = (byte)((0x00FF0000 & total_cnt) >> 16); //(byte)(0);
        data[j + 2] = (byte)((0x0000FF00 & total_cnt) >> 8); //(byte)(0);
        data[j + 3] = (byte)(0x000000FF & total_cnt); //(byte)(0);
        total_cnt++;
      }

      total_cnt = 0;

      //starts test
      SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_START_RX, TIMEOUT_MS);

      //ElapsedTime.Start(); //StopWatch start
      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();

      for (int i = 0; i < packets; i++)
      {
        packetlen = PACKETLENGTH;
        Buffer.BlockCopy(data, total_cnt, buffer, 0, packetlen);
        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SetData(ref TE_USB_FX2_USBDevice, ref buffer, ref packetlen, PI_EP8, TIMEOUT_MS, BUFFER_SIZE) == false)
          //if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SetData(ref outEndpointPipeNo, ref data, ref packetlen) == false)
        {
          //Console.WriteLine("Error");
          //return;
          errors++;
          //total_cnt = 10;
        }
        else
        {
          //Console.WriteLine("Exact");
          total_cnt += packetlen;
        }

      }
      //total_cnt += (packetlen * packets);
      stopWatch.Stop();
      TimeSpan ts = stopWatch.Elapsed;

      SendFPGAcommand(ref TE_USB_FX2_USBDevice, MB_Commands.FX22MB_REG0_STOP, TIMEOUT_MS);

      //the FPGA verify the data integrity of data transmitted

      int status = 0;

      if (GetStatusFPGAynFlag == 1)
      {
        status = GetFPGAstatus(ref TE_USB_FX2_USBDevice);
      }
      else
        status = 2;

      //Console.WriteLine("total_cnt {0}", total_cnt);

      string elapsedTime = String.Format("(0:0000)", ts.Milliseconds);
      int msTime = ts.Milliseconds;

      string elapsedTime2 = String.Format("(0:00)", ts.Seconds);
      int sTime = ts.Seconds;

      string elapsedTime3 = String.Format("(0:00)", ts.Minutes);
      int mTime = ts.Minutes;

      Console.WriteLine("TimeSpan {0} ", ts);

      Console.WriteLine("TimeSpan Minutes {0} ", mTime);

      Console.WriteLine("TimeSpan Seconds {0} ", sTime);

      Console.WriteLine("TimeSpan MilliSeconds {0} ", msTime);

      double total_cnt_Float = (double)total_cnt;

      msTime = mTime * 60 * 1000 + sTime * 1000 + msTime;

      double msTime_Float = (double)msTime;

      // (total_cnt / (1024 * 1024 * (msTime / 1000)) ) = ( (total_cnt*1000) / (1024 * 1024 * (msTime)) )

      Console.WriteLine("Millisecond {0} ", msTime);

      //int status = 1;
      if (status == 2) Console.WriteLine("host->memory  data verification Integrity SKIPPED!!!");
      if (status == 1) Console.WriteLine("host->memory  data verification Integrity PASSED!!!");
      else Console.WriteLine("host->memory  data verification Failed!!!");

      //if (errors >= 1) Console.WriteLine("host->memory data verification FAILED: {0} Packet ERRORS", errors);
      //else Console.WriteLine("host->memory  data verification PASSED!!!");

      double throughput_float = 0.0;

      throughput_float = (total_cnt_Float / (1024 * 1024 * (msTime_Float / 1000)));

      string throughput_string = throughput_float.ToString("0.00");

      Console.WriteLine("Transferred {0} kB in {1} ms = {2} MB/s", total_cnt / 1024, msTime, throughput_string);

      //Console.WriteLine("Write test passed");
      //DrawMenu();

      ResetFX2FifoStatus(TE_USB_FX2_USBDevice);

      return;
    }

    static void Main(string[] args)
    {
      CyUSBDevice TE_USB_FX2_USBDevice = null;
      USBDeviceList USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
      int m_sel = 13;


      while (m_sel != 0)
      {
        DrawMenu();
        //cin >> m_sel;
        string line = Console.ReadLine(); // Read string from console
        int value;
        int.TryParse(line, out value); // Try to parse the string as an integer
        m_sel = value;
        try
        {
          switch (m_sel)
          {
          case 1:
            GetNumberOfCards(ref USBdevList);
            break;
          case 2:
            if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Open(ref TE_USB_FX2_USBDevice, ref USBdevList, 0) == false)
              Console.WriteLine("Module is not connected!");
            //if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Open(ref TE_USB_FX2_USBDevice, ref USBdevList, 0) == true)
            else
            {
              Console.WriteLine("Module is connected!");
              Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
            }
            //TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_DisplayDriverInformation(ref TE_USB_FX2_USBDevice, ref USBdevList, 0);

            break;
          case 3:
            if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Open(ref TE_USB_FX2_USBDevice, ref USBdevList, 1) == false)
              Console.WriteLine("Module is not connected!");
            //if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Open(ref TE_USB_FX2_USBDevice, ref USBdevList, 1) == true)
            else
            {
              Console.WriteLine("Module is connected!");
              Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
            }
            //TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_DisplayDriverInformation(ref TE_USB_FX2_USBDevice,ref USBdevList, 1);

            break;
          case 4:
            TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Close(ref USBdevList);
            break;
          case 5:
            //Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
            GetFX2version(TE_USB_FX2_USBDevice);
            //Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
            break;
          case 6:
            //Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
            GetFPGAversion(TE_USB_FX2_USBDevice);
            //Console.WriteLine("USBdev {0} ", TE_USB_FX2_USBDevice);
            break;
          case 7:
            GetFX2FifoStatus(TE_USB_FX2_USBDevice);
            break;
          case 8:
            ResetFX2FifoStatus(TE_USB_FX2_USBDevice);
            break;
          case 9:
            //ReadDataFPGAIntegrity(TE_USB_FX2_USBDevice);

            Console.WriteLine("Write the Buffer Size desired for EP6 (power of 2 is better)");
            Console.WriteLine("Value suggested is 131072");

            string lineBufferSizeR = Console.ReadLine(); // Read string from console
            int BufferSizeR = 131072;
            int.TryParse(lineBufferSizeR, out BufferSizeR); // Try to parse the string as an integer

            Console.WriteLine("BufferSizeR {0} ", BufferSizeR);

            Console.WriteLine("Write the Packets Number desired");
            Console.WriteLine("Value suggested is 1200 if you are testing TE0320 or TE0630");
            Console.WriteLine("Value suggested is  600 if you are testing TE0300");

            string linePacketsNumberR = Console.ReadLine(); // Read string from console
            int PacketsNumberR = 5;
            int.TryParse(linePacketsNumberR, out PacketsNumberR); // Try to parse the string as an integer

            Console.WriteLine("PacketsNumberR {0} ", PacketsNumberR);

            Console.WriteLine("Write the Packet Length desired");
            Console.WriteLine("Value suggested is 102400");
            Console.WriteLine("if you are testing TE0320 or TE0630.");
            Console.WriteLine("Know issue: does not use 131072 or the test fail.");
            Console.WriteLine("Value suggested is  51200 if you are testing TE0300;");
            Console.WriteLine("if the value used is >51200 the test fails.");
            
            Console.WriteLine("In C# you are normally able to read even packet with length less than ");
            Console.WriteLine("512 byte AND it is what you implicitly do with point 5 (FX2 firmware version)");
            Console.WriteLine("6 (FPGA firmware version),7 (FX2 FIFO Status) BUT for this test ");
            Console.WriteLine("you must use 1024 or more for Packet Size value");

            string linePacketLengthR = Console.ReadLine(); // Read string from console
            int PacketLengthR = 51200;
            int.TryParse(linePacketLengthR, out PacketLengthR); // Try to parse the string as an integer

            Console.WriteLine("PacketLengthR {0} ", PacketLengthR);

            Console.WriteLine("Write the Timeout value desired, the integer is measured in milliseconds");
            Console.WriteLine("Value TimeOut (ms) > [PacketLength/DataThroughput ]+1 ms");
            Console.WriteLine("for high responsive computer");
            Console.WriteLine("DataThroughput value expected is >30 Mbyte/s, so with PacketLength=102400 byte,");
            Console.WriteLine("the value is 5-6 ms");
            Console.WriteLine("If the computer is not highly responsive you must set Timeout to large value :");
            Console.WriteLine("20,50,200,1000 ms (it depends on how much the computer lack real time behavior).");

            string lineTimeOutR = Console.ReadLine(); // Read string from console
            uint TimeOutR = 1000;
            uint.TryParse(lineTimeOutR, out TimeOutR); // Try to parse the string as an integer

            //Console.WriteLine("TimeOutR {0} ", TimeOutR);

            ReadDataFPGAIntegrity(TE_USB_FX2_USBDevice,BufferSizeR,PacketsNumberR,PacketLengthR,TimeOutR);

            break;
          case 10:
            //WriteDataFPGAIntegrity(TE_USB_FX2_USBDevice);

            Console.WriteLine("Write the Buffer Size desired for EP8 (power of 2 is better)");
            Console.WriteLine("Value suggested is 131072");

            string lineBufferSizeW = Console.ReadLine(); // Read string from console
            int BufferSizeW = 131072;
            int.TryParse(lineBufferSizeW, out BufferSizeW); // Try to parse the string as an integer

            Console.WriteLine("Write the Packets Number desired");
            Console.WriteLine("Value suggested is 1200 if you are testing TE0320 or TE0630");
            Console.WriteLine("Value suggested is  600 if you are testing TE0300");

            string linePacketsNumberW = Console.ReadLine(); // Read string from console
            int PacketsNumberW = 5;
            int.TryParse(linePacketsNumberW, out PacketsNumberW); // Try to parse the string as an integer

            Console.WriteLine("Write the Packet Length desired");
            Console.WriteLine("Value suggested is 102400");
            Console.WriteLine("Know issue: does not use 131072 or the test fail.");
            Console.WriteLine("Value suggested is  51200 if you are testing TE0300;");
            Console.WriteLine("if the value used is >51200 the test fails.");
            Console.WriteLine("In C# you are able to write even packet with length less than");
            Console.WriteLine("512 byte AND in this case you can also do this BUT");
            Console.WriteLine("in this case you are only able to test write data integrity");
            Console.WriteLine("");

            string linePacketLengthW = Console.ReadLine(); // Read string from console
            int PacketLengthW = 51200;
            int.TryParse(linePacketLengthW, out PacketLengthW); // Try to parse the string as an integer

            Console.WriteLine("Write the Timeout value desired, the integer is measured in milliseconds");
            Console.WriteLine("Value TimeOut (ms) > [PacketLength/DataThroughput ]+1 ms");
            Console.WriteLine("for high responsive computer");
            Console.WriteLine("DataThroughput value expected is >20 Mbyte/s, so with PacketLength=102400 byte,");
            Console.WriteLine("the value is 5-6 ms");
            Console.WriteLine("If the computer is not highly responsive you must set Timeout to large value :");
            Console.WriteLine("20,50,200,1000 ms (it depends on how much the computer lack real time behavior).");

            string lineTimeOutW = Console.ReadLine(); // Read string from console
            uint TimeOutW = 1000;
            uint.TryParse(lineTimeOutW, out TimeOutW); // Try to parse the string as an integer

            Console.WriteLine("You want make an integrity test on data writen on FPGA?");
            Console.WriteLine("1 for YES, 0 for NO");

            string lineGetStatusFPGAyn = Console.ReadLine(); // Read string from console
            uint GetStatusFPGAyn = 1000;
            uint.TryParse(lineGetStatusFPGAyn, out GetStatusFPGAyn); // Try to parse the string as an integer

            WriteDataFPGAIntegrity(TE_USB_FX2_USBDevice, BufferSizeW, PacketsNumberW, PacketLengthW, TimeOutW, GetStatusFPGAyn);

            break;
          }
        }
        catch (System.ObjectDisposedException)
        {
          Console.WriteLine("TE_USB_FX2_USBDevice Disposed: you have used the wrong procedure!");
          m_sel = 1;
        }
      }



      if (TE_USB_FX2_USBDevice != null)
        TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Close(ref USBdevList);

      return;
    }
  }
}
