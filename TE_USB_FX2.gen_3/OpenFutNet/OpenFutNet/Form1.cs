/*-------------------------------------------------------------------------------
 * Copyright (c) 2013 Trenz Electronic GmbH
 *-------------------------------------------------------------------------------
 * USB Firmware Upgrade Tool (FUT) and FPGA bitstream download tool for 
 * Trenz Electronic USB FX2 modules (based on Cypress EZ-USB FX2). 
 *-------------------------------------------------------------------------------
 * MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

//using Microsoft.Win32;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;

using System.Linq.Expressions;
using System.Reflection;

using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using System.Management;

using CyUSB;
using TE_USB_FX2;

using System.Diagnostics;

/* This program is a C# evolution of the program Python OpenFut for 3rd Generation Firmware.  
 * 
 * 
 * Info
 * 1)
 * TE USB FX2 module starts as a TE device (at power on) when
 * A) 3rd Generation Firmware is correctly stored in FX2 microcontroller's EEPROM;
 * B) the EEPROM switch is set to ON when the TE USB FX2 module is powered on.
 * 
 * TE USB FX2 module starts as a Cypress device (at power on) when
 * A) the EEPROM switch is set to OFF when the TE USB FX2 module is powered on.
 * 
 * TE USB FX2 module starts as a DEWESoft device (at power on) when
 * A) 2nd Generation Firmware is correctly stored in FX2 microcontroller's EEPROM;
 * B) the EEPROM switch is set to ON when the TE USB FX2 module is powered on.
 * 
 * 
 * 2)
 * Recovery Procedure.
 * TE USB FX2 module is used as a Cypress device and it is possible to programm into
 * FX2 microcontroller's EEPROM 2nd or 3rd Generation Firmware.
 * Procedure description. You should:
 * A) turn off the TE micromodule.
 * B) set the EEPROM switch to OFF.
 * C) turn on the TE micromodule
 * D) set the EEPROM switch to ON
 * E) install the Cypress USB Generic driver (if it is not already installed)
 * F) select the .icc file (FX2 microcontroller's Firmware of 2nd or 3rd Generation must be used) to 
 *    program into FX2 microcontoller's EEPROM using left button "Select .iic file or enter file path"
 * G) program the .icc file into EEPROM using right button "Program USB: write IIC EEPROM"
 * H) wait the end of OpenFutNet programming operation (the programming operation ends when buttons are released).
 * I) check the firmware version written if a 3rd Generation firmware has been selected at section F) section
 * 
 * 
 * Warning
 * 
 * 1)
 * For the Recovery Procedure, it is necessary to install the Cypress USB Generic driver.
 * On some computers, it could automatically install itself. On other computers, you must install
 * the version that can be founded on Trenz Electronic Web Site 
 * http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite/d2/recovery/d3/drivers.html.
 * 
 * 
 * 2)
 * To be able to use this program you must install the Trenz Electronic driver TE USB FX2 driver
 * 
 * 
 * 3) 
 * This program should be used with a single Trenz Electronic or Cypress device attached.
 * 
 * This program has been realized in this way for two reason:
 * A) the TE USB FX2 module are not identified by a unique serial number;
 * B) the program is more simple and intuitive to use.
 * 
 * In any case, if it is necessary a version of this program to use with more than one single micromodule,
 * it can be realized using this program as example (or better the alternative version under test explained), 
 * but it is strongly advised to find a way to uniquely identify the TE micromodule, before 
 * to write a new program.
 * 
 * 
 * 4)
 * Micromodule's EEPROM switch should be moved to ON to realize the FX2 microcontroller's 
 * EEPROM programming. 
 * 
 * For the Recovery Procedure (TE USB FX2 module turn on as Cypress device), the TE USB FX2 module must 
 * be powered on with EEPROM switch moved to OFF.
 * After this, the EEPROM switch must be set to ON; otherwise the EEPROM programming will fail.
 * Unfortunately, at this moment the TE USB FX2 module can't automatically warning you about 
 * user's oversight (EEPROM switch is left to OFF). 
 * 
 * 
 * 5)
 * At this moment, like the Python Open_FUT, the program use one single large array of byte for 
 * the FPGA's bitstream that should be written into the SPI Flash of the micromodule.
 * If a very old computer with very low RAM is used, this may affect the performance of the program;
 * we have not yet actually seen this behavior in our tests and we also doesn't expect a large hit 
 * on the performnance because the performance of this program are I/O bounded by TE USB FX2 module's 
 * SPI Flash.
 * 
 * At this moment, another version of OpenFutNet, using OpenFile and ReadBinary function to lessen 
 * the computer's memory used, is under investigation for the correct sizing of buffer to use.
 * In this version, a lower number of static variables will be used.
 * If you desire to realize a OpenFutNet program to write more than one single micromodule, this version will 
 * certainly better fits your needs. But, before considering this alternative version of the program, 
 * it is strongly advised to find a way to uniquely identify the TE micromodule.
 * 
 * 
 * 6)
 * In this version of the program, the FPGA bitstream maximum size is 128M. This is correct because it is
 * the maximum size of Flash mounted on TE USB FX2 modules programmed by this program.
 * 
 * 
 * Feature
 * 1)
 * With this program (OpenFutNet), you can download Trenz Electronic FX2 microcontroller Firmware 
 * (both 2nd and 3rd Generation) into the FX2 microcontroller's EEPROM.
 * 
 * This is realized using:
 * A)the Trenz Electronic .NET DLL 'TE_USB_FX2.dll';
 * B)the Cypress .NET DLL "CyUSB.dll".
 * 
 * The change of Firmware is immediatly effective because the new Firmware is also loaded 
 * into the FX2 microcontroller's RAM. To do this the Cypress .NET DLL "CyUSB.dll" is used.
 * If the 3rd Generation Firmware v3.02 is used, the Firmware RAM loading is correctly seen as a 
 * TE USB FX2 module detach/insertion cycle; it logically happens even if it isn't manually relaized.
 * 
 * 2)
 * OpenFutNet warns you about double (or more) TE USB FX2 module insertion if TE micromodules are seen as 
 * Cypress or Trenz Electronic device. The TE micromodules seen as DEWESoft device are not warned as
 * multiple insertions because they have another warning message.
 * 
 * 
 * 3)
 * OpenFutNet warns you about TE USB FX2 module insertion if TE USB FX2 module is seen as Cypress device.
 * OpenFutNet warns you that you will be able only to use Recovery Procedure for FX2 microcontroller's 
 * Firmware (EEPROM) and that you will not be able to program SPI Flash (FPGA).
 * 
 * 
 * 4)
 * OpenFutNet warns you about TE USB FX2 module insertion if TE USB FX2 module is seen as DEWESoft device.
 * OpenFutNet warns you that it can't write the TE micromodule. 
 * You should use a Recovery Procedure. See Info.
 * A) turn off the TE micromodule
 * B) follow the Recovery Firmware Boot  
 * 
 * 
 * 5)
 * OpenFutNet use TE_USB_FX2.dll to automatically obtains information about the 3rd Generation Firmware 
 * version written into FX2 microcontroller's EEPROM.
 * 
 * 
 * 6)
 * OpenFutNet use TE_USB_FX2.dll to automatically obtains information about the TE Reference Architecture 
 * (TE Reference Design based on Xilinx MicroBlaze soft processor and custom TE USB FX2 module) running onto 
 * Xilinx FPGA and written into SPI Flash.
 * 
 * 
 * 7)
 * OpenFutNet tries to guide you during EEPROM (FX2 microcontroller) or SPI Flash (FPGA) programming.
 * 
 * 8)
 * An informative Log is written during EEPROM and SPI Flash programming.
 */

namespace OpenFutNet
{
    //Derive the class Form1 from Form. 
    //It is a partial declaration; the other part is in Form1.Designer.cs
    public partial class Form1 : Form
    {
      
        /* bool flag: 
         * START 
         */

        //bool flag. It is true if the OpenFutProgram should retrieve SPI Flash Identifier
        bool bRetrieve_FlashID = false;

        //bool flag. It is true if a MCS Dummy Word Line is found
        bool bMCS_DummyWordLine = false;

        //bool flag. It is true if a MCS (Xilinx Flash type) Spartan3A Sync Word Line is found
        bool bMCSXilinxFlash_Spartan3A_SyncWordLine = false;

        //bool flag. It is true if a MCS (third-party SPI Flash type) Spartan3A Sync Word Line is found
        bool bMCSSPIFlash3rdParts_Spartan3A_SyncWordLine = false;

        //bool flag. It is true if a MCS (Xilinx Flash type) Spartan3E or Spartan6 Sync Word Line is found
        bool bMCSXilinxFlash_Spartan3Eor6_SyncWordLine = false;

        //bool flag. It is true if a MCS (third-party SPI Flash type) Spartan3E or Spartan6 Sync Word Line is found
        bool bMCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine = false;
                                        
        //bool flag. It is true if the information given in Log textBox is verbose
        private static bool bVerboseLogText = false;

        //bool flag. It is true if the user desires to clear the log text in the box below, 
        //before every programming operation
        private static bool bClearLogTextBeforeEveryProgrammingOperation = false;

        //bool flag: True if SPI Flash of TE USB FX2 module is already written during one run of BW2, false otherwise
        private static bool bFLASH_FPGA_AlreadyWritten = false;

        //bool flag: True if the bitstream file .bit or .mcs to write into SPI Flash of TE USB FX2 module is already "PreProcessed", false otherwise.
        //The .bit file must be stripped of header and .mcs file must be transformed in a 
        //.bit bitstream. In this version the byte array wr_fpga_bitsream is used.
        //If the .mcs file has been created for Xilinx Flash, wr_fpga_bitsream bit order is inverted. 
        //If the .mcs file has been created for third-party SPI Flash, wr_fpga_bitsream bit order is unchanged.
        private static bool bFileFpga_PreProcessed = false;

        //bool flag: True if the .mcs file selected has been created for Xilinx Flash and not for a
        //third-party SPI Flash.
        private static bool bMCSXilinxFlash = false;

        //bool flag: True if programming of SPI Flash ended. 
        private static bool bresult_SPI_Flash_Programming = false;

        //bool flag: True if the TE USB FX2 module inserted is seen as a DEWESoft device:
        //A) 2nd Generation Firmware is correctly stored in FX2 microcontroller's EEPROM;
        //B) the EEPROM switch is set to ON when the TE USB FX2 module is powered on. 
        //VID of DEWESoft found
        private static bool bVID_DEWESoft = false;
        //PID of DEWESoft found
        private static bool bPID_DEWESoft = false;

        //bool flag.
        //Unlike Digilent Board it is not possible to test if EEPROM switch has been actually set to ON.
        //This is a "fake" bool flag set to true when user confirms in a MessageBox that 
        //the EEPROM switch has been set to ON.
        private static bool bEEPROMSwitchSetToOn = false; 

        //bool flag: true if .iic file (FX2 microcontroller's Firmware) has been correctly writen by
        //CyUSB.dll into the FX2 microcontroller's RAM
        private static bool bResultLoadExternalRam = false;

        //bool flag: true if .iic file (FX2 microcontroller's Firmware) has been correctly writen by
        //CyUSB.dll into the FX2 microcontroller's EEPROM
        private static bool bResultLoadEEPROM = false;

        //bool flag: True if a .bit or a .mcs file has been selected by left button "Select *.bit or *.mcs file"
        //or a filepath is given in the FPGA filepath textbox
        private static bool bFileFpga_Selected = false;

        //bool flag: True if a .iic file has been selected by left button "Select *.iic file"
        //or a filepath is given in the USB filepath textbox
        private static bool bFileUSB_Selected = false;

        /* bool flag: 
         * STOP 
         */

        /* Progres bar: 
         * START 
         */

        //variable that go from 0 to 100 to inform the user about SPI Flash Write progression
        private static int percentComplete_FPGA_SPIFlashWrite = 0;
        //variable that go from 0 to 100 to inform the user about the maximum SPI Flash Write 
        //progression reached
        private static int highestPercentageReached_FPGA_SPIFlashWrite = 0;

        //variable that go from 0 to 100 to inform the user about SPI Flash Erase progression
        private static int percentComplete_FPGA_SPIFlashErase = 0;
        //variable that go from 0 to 100 to inform the user about the maximum SPI Flash Erase 
        //progression reached
        private static int highestPercentageReached_FPGA_SPIFlashErase = 0;

        //variable that go from 0 to 100 to inform the user about EEPROM write progression
        private static int percentComplete_USB_EEPROMWrite = 0;
        //variable that go from 0 to 100 to inform the user about the maximum EEPROM write 
        //progression reached
        //private static int highestPercentageReached_USB_EEPROMWrite = 0;

        /* Progress bar: 
         * STOP 
         */

        //size of bitstream to write on SPI Flash
        private static int fpga_bitstream_size = 0;

        //size of bitstream to write on SPI Flash, when realized from mcs to bit PreElaboration
        private static int fpga_bitstream_sizeMCS2BIT = 0;

        //it is the value used to strip the useless header of .bit file
        private static int offset = 0;

        //size of the bitstream (size of .iic file selected) to write on EEPROM (FX2 microcontroller)
        private static int usb_bitstream_size = 0;

        //"buffer" used to write the USB firmware *.icc file into EEPROM (FX2 microcontroller).
        //The FX2 microcontroller's EEPROM have a size of 64 Kbytes 
        //The .iic file is so small, that there is no intention to change this value.
        private static byte[] wr_usb_bitstream = new byte[64 * 1024];

        //"buffer" used to write the FPGA bitstream file into SPI Flash
        //The SPI Flash on TE USB FX2 module TE0300, TE0320 and TE0630 have 
        // a maximum size of 128 Mbytes.
        //The .bit or the .mcs file are not small, so an alternative version (using OpenFile and 
        //ReadBinary function and a true buffer of 4 Kbytes to 64 Kbytes) is on the way.
        private static byte[] wr_fpga_bitsream = new byte[(128 * 1024 * 1024)];

        //identify the type of .bit or .mcs file (FPGA bitstream file)
        //0) No FPGA bitsteam file selected
        //1) .bit Spartan3E or Spartan6
        //2) .mcs Xilinx SPI Flash Spartan3E or Spartan6
        //3) .mcs third-party SPI Flash Spartan3E or Spartan6
        //4) .bit Spartan3A
        //5) .mcs Xilinx SPI Flash Spartan3A
        //6) .mcs third-party SPI Flash Spartan3A
        //7) Wrong (bad formatted) file selected
        private static int file_type = 0;

        //identify the type of error realized during OpenFutRun
        private static int op_error = 0;

        //USB FX2 Trenz Electronic device selected
        CyUSBDevice TE_USB_FX2_USBDevice = null;

        //USB FX2 device selected: can be USB Cypress or Trenz Electronic
        CyFX2Device fx2 = null;

        //List of USB Cypress or Trenz Electronic connected
        USBDeviceList USBdevList = null;
     
        //PID and VID of Cypress device
        private static UInt16 PIDCypress = 0x8613;
        private static UInt16 VIDCypress = 0x04b4;

        //PID and VID of Trenz Electronic device
        private static UInt16 PIDTrenzElectronic = 0x0300;
        private static UInt16 VIDTrenzElectronic = 0x0bd0;

        //declaration used to create a wait behavior inside BackgrounWorker
        private static ManualResetEvent backgroundWakeEvent = new ManualResetEvent(false);

        //Query used to search a PlugAndPlay (PnP) event every 1 second:
        //it is used with OnPnPWMIEvent() to check if a DEWESoft device has been attached 
        //"Select * from Win32_PnPEntity where Availability = 12 or Availability = 11" has been also considered
        const string QUERY = @"select * from __InstanceOperationEvent within 1 where TargetInstance isa 'Win32_PnPEntity'";  // and TargetInstance.HardwareID isa 'USB\\VID_0547&PID_1002\\000' "; //and (TargetInstance.DriveType=2)";
        //Use of the query created above
        ManagementEventWatcher watcherDEWESoft = new ManagementEventWatcher(new WqlEventQuery(QUERY));

        //const string QUERY_Unknow_Device = @"select * from Win32_PnPEntity within 1 where ConfigManagerErrorCode <> 0";  // and TargetInstance.HardwareID isa 'USB\\VID_0547&PID_1002\\000' "; //and (TargetInstance.DriveType=2)";
        //Use of the query created above
        //ManagementEventWatcher watcher_TE_USB_FX2Gen3_or_GenericCypress_NotInstalled = new ManagementEventWatcher(new WqlEventQuery(QUERY_Unknow_Device));

        //TE API Commands used that should be sent to FX2 microcontroller
        //Some of thes TE API commands grants the ability to operate on FPGA and SPI Flash.
        //Some of these TE API commands require Microblaze "aid"

        // Commands definition
        /*
        #define EP1DATA_COUNT 0x40
        #define CMD_READ_VERSION 0x00
        #define CMD_SWITCH_MODE 0xA0
        #define CMD_READ_STATUS 0xA1
        #define CMD_WRITE_REGISTER 0xA2
        #define CMD_READ_REGISTER 0xA3
        #define CMD_RESET_FIFO_STATUS 0xA4
        #define CMD_FLASH_READ 0xA5
        #define CMD_FLASH_WRITE 0xA6
        #define CMD_FLASH_ERASE 0xA7
        #define CMD_SECTOR_ERASE 0xF7
        #define CMD_EEPROM_READ 0xA8
        #define CMD_EEPROM_WRITE 0xA9
        #define CMD_FX2_FLASH_WRITE_COMMAND 0xAA
        #define CMD_DEV_LOCK 0xBB
        #define CMD_START_TEST 0xBD
        #define CMD_I2C_WRITE_READ 0xAB
        #define CMD_GET_FIFO_STATUS 0xAC
        #define CMD_I2C_WRITE 0xAD
        #define CMD_I2C_READ 0xAE
        #define CMD_FPGA_POWER 0xAF
        #define CMD_SET_AUTORESPONSE 0xB0
        #define CMD_GET_AUTORESPONSE 0xB1
        #define CMD_FPGA_RESET 0xB2
        */

        private enum FX2_Commands
        {
            CMD_FX2_READ_VERSION = 0x00,      
            CMD_FX2_INITALIZE = 0xA0,         
            CMD_FX2_READ_STATUS = 0xA1,       
            CMD_FX2_WRITE_REGISTER = 0xA2,    
            CMD_FX2_READ_REGISTER = 0xA3,     
            CMD_FX2_RESET_FIFO_STATUS = 0xA4, 
            CMD_FX2_FLASH_READ = 0xA5,  
            CMD_FX2_FLASH_WRITE = 0xA6, 
            // CMD_FX2_SECTOR_ERASE = 0xF7, No longer used
            CMD_FX2_FLASH_ERASE = 0xA7, 
            CMD_FX2_EEPROM_READ = 0xA8, 
            CMD_FX2_EEPROM_WRITE = 0xA9, 
            CMD_FX2_GET_FIFO_STATUS = 0xAC, 
            CMD_FX2_I2C_WRITE = 0xAD, 
            CMD_FX2_I2C_READ = 0xAE, 
            //I2C_BYTES_SIZE = 0x0C,
            //I2C_MICROBLAZE_ADDRESS = 0x3F,
            CMD_FX2_POWER_ON = 0xAF, 
            CMD_FX2_FLASH_WRITE_COMMAND = 0xAA, 
            CMD_FX2_SET_INTERRUPT = 0xB0, 
            CMD_FX2_GET_INTERRUPT = 0xB1,
            CMD_FX2_FPGA_RESET = 0xB2
            //CMD_FX2_FLASH_WRITE_COMMAND = 0xAA
        };

        //TE API Commands parameter specific for the Microblaze based TE Reference Design
        private enum MB_Commands
        {
            FX22MB_REG0_NOP = 0,
            FX22MB_REG0_GETVERSION = 1,
            FX22MB_REG0_START_TX = 2,
            FX22MB_REG0_START_RX = 3,
            FX22MB_REG0_STOP = 4,
            FX22MB_REG0_PING = 5
        };

        //Parameter used with TE API Commands
        private enum FX2_Parameters
        {
            I2C_BYTES_SIZE = 0x0C,
            MB_I2C_ADDRESS = 0x3F
        };

        // byte array used for the inversion of bit order in a byte
        private static byte[] BitReverseTable ={
    0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0,
    0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
    0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8,
    0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
    0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4,
    0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
    0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec,
    0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
    0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2,
    0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
    0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea,
    0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
    0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6,
    0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
    0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee,
    0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
    0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1,
    0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
    0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9,
    0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
    0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5,
    0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
    0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed,
    0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
    0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3,
    0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
    0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb,
    0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
    0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7,
    0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
    0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef,
    0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff};


        /* String START */      

        //String containing VID, it is used to write on textBox_VID 
        private static String VID_String = String.Empty;
        //String containing PID, it is used to write on textBox_PID
        private static String PID_String = String.Empty;

        //String that is used to store the Latest Major Version of FX2 USB microcontroller firmware
        private static String LatestMajorVersionFW_String = String.Empty;
        //String that is used to store the Latest Minor Version of FX2 USB microcontroller firmware
        private static String LatestMinorVersionFW_String = String.Empty;

        //String that is used to store the Latest Major Version of reference FPGA project (MicroBlaze)
        private static String LatestMajorVersionFPGA_String = String.Empty;
        //String that is used to store the Latest Minor Version of reference FPGA project (MicroBlaze)
        private static String LatestMinorVersionFPGA_String = String.Empty;
        //String that is used to store the Latest Release Version of reference FPGA project (MicroBlaze)
        private static String LatestReleaseVersionFPGA_String = String.Empty;
        //String that is used to store the Latest Build Version of reference FPGA project (MicroBlaze)
        private static String LatestBuildVersionFPGA_String = String.Empty;

        //FPGA is running a reference project (Microblaze based): Yes/No
        private static String SystemTypeFPGAFlash_Text = String.Empty;

        //String that is used to store the Firmware Type of FX2 USB microcontroller: 
        //Generation 2, Generation 3 or Recovery (Cypress).
        //The selection is based on PID and VID of USB device attached
        private static String FirmwareTypeUSB_Text = String.Empty;

        //String that is used to store the Driver Type of FX2 USB microcontroller: 
        //Generation 2, Generation 3 or Recovery (Cypress).
        //The selection is based on PID and VID of USB device attached
        private static String DriverType_TextBox_Text = String.Empty;

        //String that is used to store the full filepath of *.bit or *.mcs file to download on SPI Flash
        private static String FPGAFile_FilePath = String.Empty;

        //String that is used to store the full filepath of *.iic file to download on EEPROM
        private static String USBFile_FilePath = String.Empty;

        //String that is used to store the different operation used for FPGA/SPI Flash proggramming:
        //1) Analyze the bitstream
        //2) Erase the SPI Flash
        //3) Write the SPI Flash
        //4) Check the DONE PIN
        private static String TextLine = String.Empty;

        //String that is used to store the contents of the log information: info, warnings and errors.
        //Some info, warnings and errors are displayed only if Verbose Flag (bVerboseLogText) is checked.
        private static String LogTextLine = String.Empty;

        //String that is used to store the current Status information: it is displayed in the left corner of GUI. 
        private static String StatusTextLine = String.Empty;

        //String that is used to store the message that a DEWESoft USB device has been attached 
        String DEWESoftDeviceAttached_String = "The TE USB FX2 module starts as a DEWESoft USB Device." + "\r\n" + "This happens when EEPROM switch is set to ON when the TE USB FX2 module is attached to USB port (or more generally when the TE USB FX2 module is powered on with EEPROM switch ON) but the 2nd Generation Firmware is stored in the EEPROM of FX2 microcontroller." + "\r\n" + "If the DEWESoft driver is installed you are able to find this device in Windows 'Device Manager' under 'Universal Serial Bus controller'. Otherwise you are able to find this device in Windows 'Device Manager' under 'Unknow Device'." + "\r\n" + "If you desire to change the FX2 Firmware from 2nd Generation to 3rd Generation (or even if you want to simply upgrade the 2nd Generation FX2 Firmware version) you must start a Recovery Procedure. " + "\r\n" + "Recovery Procedure: you should use a power off/on cycle with EEPROM switch to OFF to start with VID and PID of a Cypress Generic device. If the computer does't see the Cypress Generic device you must install the 'Cypress Generic device' driver found on Trenz Electronic web site in the recovery area." + "\r\n";

        //String that is used to store the OpenFutNet version
        String OpenFutNETversion = "v 1.0.3";
        /*String  STOP*/

        /*Initialization of Form1 */
        public Form1()
        {
            //It is a function automatically generated by Windows Form Designer.
            //It is defined in Form1.Designer.cs
            //It initialize GUI (Form Form1) Components
            InitializeComponent();

            //This function is used at Form Form1 initialization to avoid to run 2 times the
            //BackGroundWorker (BW1_FPGA_SPIFlash or BW2_CypressUSB_I2CEEPROM) during the firt 
            //programming operation.
            //This behavior seems due to Form1.Designer.cs initialiaziation (automatically generated).
            InitializeComponent_OnlyASingleInstanceOfBackGroundWorker4EveryNewMustRun();
            //LogTextScrollDown();

        }

        //This function is used at Form Form1 initialization to avoid to run 2 times the
        //BackGroundWorker (BW1_FPGA_SPIFlash or BW2_CypressUSB_I2CEEPROM) during the firt 
        //programming operation.
        //This behavior seems due to Form1.Designer.cs initialiaziation (automatically generated).
        private void InitializeComponent_OnlyASingleInstanceOfBackGroundWorker4EveryNewMustRun()
        {
            //Firt method
            //It doesn't work; it doesn't avoid the first double RunWorkerCompletedEvent
            //BW1_FPGA_SPIFlash.Dispose();
            //BW2_CypressUSB_I2CEEPROM.Dispose();

            //Second method: it works
            //Avoid the problem of double RunWorkerCompletedEvent (aka double BW1_FPGA_SPIFlash run) 
            //during the first programming operation of SPI Flash
            BW1_FPGA_SPIFlash.DoWork -= new DoWorkEventHandler(BW1_FPGA_SPIFlash_DoWork);
            BW1_FPGA_SPIFlash.ProgressChanged -= new ProgressChangedEventHandler(BW1_FPGA_SPIFlash_ProgressChanged);
            BW1_FPGA_SPIFlash.WorkerReportsProgress = true;
            BW1_FPGA_SPIFlash.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW1_FPGA_SPIFlash_RunWorkerCompleted);
            BW1_FPGA_SPIFlash.WorkerSupportsCancellation = true;

            //Second method: it works
            //Avoid the problem of double RunWorkerCompletedEvent (aka double BW2_CypressUSB_I2CEEPROM run) 
            //during the first programming operation of IIC EEPROM of FX2 microcontroller
            BW2_CypressUSB_I2CEEPROM.DoWork -= new DoWorkEventHandler(BW2_CypressUSB_I2CEEPROM_DoWork);
            BW2_CypressUSB_I2CEEPROM.ProgressChanged -= new ProgressChangedEventHandler(BW2_CypressUSB_I2CEEPROM_ProgressChanged);
            BW2_CypressUSB_I2CEEPROM.WorkerReportsProgress = true;
            BW2_CypressUSB_I2CEEPROM.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted);
            BW2_CypressUSB_I2CEEPROM.WorkerSupportsCancellation = true;

            //Initialize the value of some Control of the Form Form1.
            LatestMajorVersionFW_textBox.Text = "Not yet retrieved";
            LatestMinorVersionFW_textBox.Text = "Not yet retrieved";
            LatestMajorVersionFPGA_textBox.Text = "Not yet retrieved";
            LatestMinorVersionFPGA_textBox.Text = "Not yet retrieved";

        }

        //This function is called when the form Form1 is closed aka when Program OpenFutNet is closed.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            //The resources of PnpQuery used to search PnP connection/disconnection of DEWESoft device(s) 
            //are disposed
            //ManagementEventWatcher watcherDEWESoft = new ManagementEventWatcher(new WqlEventQuery(QUERY));
            watcherDEWESoft.Stop();
            watcherDEWESoft.EventArrived -= new EventArrivedEventHandler(OnPnPWMIEvent);
            watcherDEWESoft.Dispose();

            //watcher_TE_USB_FX2Gen3_or_GenericCypress_NotInstalled.Stop();
            //watcher_TE_USB_FX2Gen3_or_GenericCypress_NotInstalled.EventArrived -= new EventArrivedEventHandler(OnPnPWMIEvent);
            //watcher_TE_USB_FX2Gen3_or_GenericCypress_NotInstalled.Dispose();

            //The resource used by Trenz Electronic/Cypress device(s) are disposed
            USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
            if (USBdevList != null)
            {
                USBdevList.DeviceRemoved -= USBdevList_DeviceRemoved;
                USBdevList.DeviceAttached -= USBdevList_DeviceAttached;
                USBdevList.Dispose();
            }

            //The BackGroundWorker BW1_FPGA_SPIFlash used for FPGA and SPI Flash is disposed
            //Maybe is necessary to add -new Events
            BW1_FPGA_SPIFlash.Dispose();

            //The BackGroundWorker  BW2_CypressUSB_I2CEEPROM used for FX2 microcontroller EEPROM is disposed
            //Maybe is necessary to add -new Events
            BW2_CypressUSB_I2CEEPROM.Dispose();         
        }

        //Function called when the event "Form1 is loading" happens.
        private void Form1_Load(object sender, EventArgs e)
        {
            //"Trick that is used to preserve the default log text that exists at initialization time.
            LogTextLine += "Info, warnings and errors are reported in this log." + "\r\n" + "\r\n" +
                           "This program is a C# evolution of the program Python OpenFut for 3rd Generation Firmware." + "\r\n" + "\r\n";
            // textBox_LogText.Text + "\r\n";

            //This is a group of code written inside two using statements 
            //It checks if a DEWESoft device is already attached when the Form1 
            //(OpenFutNet programm) is loaded.
            //If it is loaded, it inform the user that he/she should detach the TE USB FX2 module
            //and follow a Recovery Procedure
            // TO DO: at this moment, the code doesn't show the message because the Verbose Flag (bVerboseLogText) is false when the progam starts.
            // TO DO: Verbose Flag should be transformed in a varible that can mantains its own status when the program is closed
            using (System.Management.ManagementClass PnPClass = new ManagementClass("Win32_PnPEntity"))
            //Win32_USBController no because DEWESoft device is not seen as a Windows USBdevice (at least if DEWESoft driver is not installed)
            //With  Win32_PnPEntity the DEWESoft device can be found even if DEWESoft driver is not installed
            {             
                using (System.Management.ManagementObjectCollection PnPCollection = PnPClass.GetInstances())               
                {
                    foreach (System.Management.ManagementObject usb in PnPCollection)
                    {

                        string deviceId = usb["deviceid"].ToString();
                        
                        int vidIndex = deviceId.IndexOf("VID_");
                        string startingAtVid = deviceId.Substring(vidIndex + 4); // + 4 to remove "VID_"                    
                        string vid = startingAtVid.Substring(0, 4); // vid is four characters long
                        
                        if (vid.Equals("0547"))
                        {
                            textBox_VID.Text = "0x" + vid;
                            bVID_DEWESoft = true;
                        }

                        /*
                        if (vid.Equals("0BD0"))
                        {
                            textBox_VID.Text = "0x" + vid;
                            bVID_TE_PnP = true;
                        }

                        if (vid.Equals("04B4"))
                        {
                            textBox_VID.Text = "0x" + vid;
                            bVID_Cy_PnP = true;
                        }
                        */
                        int pidIndex = deviceId.IndexOf("PID_");
                        string startingAtPid = deviceId.Substring(pidIndex + 4); // + 4 to remove "PID_"                    
                        string pid = startingAtPid.Substring(0, 4); // pid is four characters long
                        
                        if (pid.Equals("1002"))
                        {
                            textBox_PID.Text = "0x" + pid;
                            //bVID_DEWESoft = true;
                        }                 
                    }                   
                }
            }

            textBox_LogText.Text = LogTextLine;
            LogTextScrollDown();
            textBox_LogText.Update();

            if (bVID_DEWESoft || bPID_DEWESoft)
            {
                DriverType_TextBox.Text = "DEWESoft device";
                FirmwareTypeUSB.Text = "Trenz Electronic Gen2";
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                LogTextLine += "---A DEWESoft device is already inserted when OpenFut starts to run--" + "\r\n";
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                LogTextLine += "INFO: The TE USB FX2 module starts as a DEWESoft Device: this happens when EEPROM switch is set to ON when the TE USB FX2 module is attached to USB port (or more generally when the TE USB FX2 module is powered on with EEPROM switch ON)." + "\r\n";
                LogTextLine += "INFO: The TE USB FX2 module runs the 2nd generation firmware (TE_USB_FX2 Gen 2)" + "\r\n";
                LogTextLine += "WARNING: You can't write a new firmware inside the EEPROM (even if EEPROM switch is set to ON)" + "\r\n";
                LogTextLine += "WARNING: You can't write a new FPGA bitstream inside SPI Flash." + "\r\n";
                LogTextLine += "WARNING: You should start a Recovery Procedure Boot to change the firmware of FX2 microcontroller" + "\r\n";
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                FirmwareTypeUSB.Text = "TE_USB_FX2 Gen2";
                DriverType_TextBox.Text = "It couldn't be retieved";
                LatestMajorVersionFW_String = "It couldn't be retieved";
                LatestMinorVersionFW_String = "It couldn't be retieved";
                if (bVerboseLogText == true)
                {
                    LogTextLine += DEWESoftDeviceAttached_String + "\r\n" + "\r\n";
                    MessageBox.Show(DEWESoftDeviceAttached_String);
                }
            }
            textBox_LogText.Text = LogTextLine;
            LogTextScrollDown();
            textBox_LogText.Update();
            /*
            textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
            textBox_LogText.ScrollToCaret();
            textBox_LogText.Refresh();
            */

            //FUNDAMENTAL: without these 3 instructions, it doesn't seems to work  
            //A new USB device list is created
            backgroundWakeEvent.WaitOne(200);          
            USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
            backgroundWakeEvent.WaitOne(200);

            UInt16 PID = 0;
            UInt16 VID = 0;

            //If the number of Cypress/Trenz Electronic device are more than one
            if (USBdevList.Count != 0)
            {
                //Create a single device instance
                CyFX2Device fx2test = USBdevList[0] as CyFX2Device;
                PID = fx2test.ProductID;
                VID = fx2test.VendorID;

                //If more than one Trenz Electronic and/or Cypress Device
                if (USBdevList.Count > 1)
                {
                    MessageBox.Show("In this version the program must be used with a single Trenz Electronic or Cypress module attached. You must remove Cypress and/or Trenz micromodule(s) until only one module remains. This must be the module that you desire to write.");
                }

                //If a single Trenz Electronic Device is attached
                else if ((USBdevList.Count == 1) && (PID == PIDCypress) && (VID == VIDCypress))
                {
                    PID_String = "0x8613";
                    VID_String = "0x04B4";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "---A Cypress device is already inserted when OpenFut start to run--" + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "INFO: The TE USB FX2 module starts as a Cypress Device: this happens when EEPROM switch is set to OFF when the TE USB FX2 module is attached to USB port (or more generally when the TE USB FX2 module is powered on with EEPROM switch OFF)." + "\r\n";
                    LogTextLine += "INFO: Generic Cypress USB Driver used for the recovery of Trenz Electronic Firmware (TE_USB_FX2 Gen3) for FX2 microcontroller" + "\r\n";
                    LogTextLine += "INFO: RECOVERY PROCEDURE: you can write a new firmware inside the EEPROM (if EEPROM switch is set to ON)" + "\r\n";
                    LogTextLine += "INFO: RECOVERY PROCEDURE: you can't write a new FPGA bitstream inside SPI Flash." + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                    FirmwareTypeUSB.Text = "Cypress used for Recovery Procedure";
                    DriverType_TextBox.Text = "Generic Cypress USB Driver";
                    //MessageBox.Show("The VID and PID used are for normal Cypress device; if you are starting a recovery procedure, it is correct. Otherwise, you should use a power off/on cycle with EEPROM switch to ON to start with VID and PID of a Trenz Electronic device. If the computer does't see the Trenz Electronic device you must install the 'Trenz Electronic USB FX2' driver." );
                    //bEEPROMSwitchSetToOn = true;                     
                    //LogTextLine += "Minor version: " + Reply1[1].ToString() + "\r\n";
                    //LatestMajorVersionFW_textBox.Text = Reply[0].ToString();
                    //LatestMinorVersionFW_textBox.Text = Reply[1].ToString();
                    LatestMajorVersionFW_String = "Recovery Procedure";
                    LatestMinorVersionFW_String = "Recovery Procedure";
                    LogTextScrollDown();

                }
                // if a single Cypress Device is attached
                else if ((USBdevList.Count == 1) && (PID == PIDTrenzElectronic) && (VID == VIDTrenzElectronic))
                {
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "---A Trenz Electronic device is already inserted when OpenFut start to run--" + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "INFO: The TE USB FX2 module starts as a Trenz Electronic Device: this happens when EEPROM switch is set to ON when the TE USB FX2 module is attached to USB port (or more generally when the TE USB FX2 module is powered on with EEPROM switch ON)." + "\r\n";
                    LogTextLine += "INFO: Trenz Electronic TE_USB_FX2 driver used for normal work with TE USB FX2 module" + "\r\n";
                    LogTextLine += "INFO: You can write a new firmware inside the EEPROM (if EEPROM switch is set to ON)" + "\r\n";
                    LogTextLine += "INFO: You can write a new FPGA bitstream inside SPI Flash." + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    FirmwareTypeUSB.Text = "TE_USB_FX2 Generation 3";
                    DriverType_TextBox.Text = "TE USB FX2 Driver Gen 3";
                    LatestMajorVersionFW_String = "To be retrieved";
                    LatestMinorVersionFW_String = "To be retrieved";
                    LogTextScrollDown();
                }

                //bVID_DEWESoft = false;
                //bPID_DEWESoft = false;

                //A new device list is created: it is moved outside of RefreshInformationUIThread() 
                backgroundWakeEvent.WaitOne(500);
                USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
                backgroundWakeEvent.WaitOne(500);

                //This function refresh the information in the GUI about the SINGLE device attached.
                RefreshInformationUIThread();
                textBox_LogText.Update();
                LogTextScrollDown();
            }

            //Query used to search a PlugAndPlay (PnP) event every 1 second:
            //it is used with OnPnPWMIEvent() callback function to check if a 
            //DEWESoft device has been attached. 
            /**///const string QUERY = @"select * from __InstanceOperationEvent within 1 where TargetInstance isa 'Win32_PnPEntity'";  // and TargetInstance.HardwareID isa 'USB\\VID_0547&PID_1002\\000' "; //and (TargetInstance.DriveType=2)";
            //"Select * from Win32_PnPEntity where Availability = 12 or Availability = 11" has been also considered           
            //Use of the query string QUERY created above
            /**///ManagementEventWatcher watcherDEWESoft = new ManagementEventWatcher(new WqlEventQuery(QUERY));

            //The two lines of code (commented out) (below) are moved at class level because 
            //watcherDEWESoft should be disposed using watcherDEWESoft.Dispose() when Form1 close (OpenFunNet program exit).
            /*Code commented out *///const string QUERY = @"select * from __InstanceOperationEvent within 1 where TargetInstance isa 'Win32_PnPEntity'";  // and TargetInstance.HardwareID isa 'USB\\VID_0547&PID_1002\\000' "; //and (TargetInstance.DriveType=2)";
            /*Code commented out*///ManagementEventWatcher watcherDEWESoft = new ManagementEventWatcher(new WqlEventQuery(QUERY));
            
            //The two lines of code below are used to start the repeating query (every 1 second) and 
            //to capture only DEWESoft PnP event: attachment and removing
            watcherDEWESoft.EventArrived += new EventArrivedEventHandler(OnPnPWMIEvent);
            watcherDEWESoft.Start();


            //SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode <> 0
            //const string QUERY_Unknow_Device = @"SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode <> 0";  // and TargetInstance.HardwareID isa 'USB\\VID_0547&PID_1002\\000' "; //and (TargetInstance.DriveType=2)";
            /*Code commented out*/
            //ManagementEventWatcher watcherDEWESoft = new ManagementEventWatcher(new WqlEventQuery(QUERY_Unknow_Device));
            //watcher_TE_USB_FX2Gen3_or_GenericCypress_NotInstalled.EventArrived += new EventArrivedEventHandler(OnPnPWMIEvent);
            //watcher_TE_USB_FX2Gen3_or_GenericCypress_NotInstalled.Start();

            //App_PnP_Callback evHandler = new App_PnP_Callback(PnP_Event_Handler);

            //Trenz Electronic/Cypress device attached event is elaborated 
            //by callback function USBdevList_DeviceAttached
            //USBdevList.DeviceAttached.Start() is not required
            USBdevList.DeviceAttached += new EventHandler(USBdevList_DeviceAttached);

            //Trenz Electronic/Cypress device removed event is elaborated 
            //by callback function USBdevList_DeviceRemoved
            //USBdevList.DeviceRemoved.Start() is not required
            USBdevList.DeviceRemoved += new EventHandler(USBdevList_DeviceRemoved);

            textBox_LogText.Update();
            //Scroll down Logtext
            LogTextScrollDown();
            RefreshInformationUIThread();
            /* TEST CODE, TO COMMENT OUT unless you need for test purpose
            String regstryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (Microsoft.Win32.RegistryKey keyName = Registry.LocalMachine.OpenSubKey(regstryKey))
            {
                foreach (string subKey in keyName.GetSubKeyNames())
                {
                    using (RegistryKey subKey1 = keyName.OpenSubKey(subKey))
                    {
                        LogTextLine += ((subKey1.GetValue("DisplayName")) );
                        //Response.Write(subKey1.GetValue("DisplayName"));
                        
                        textBox_LogText.Update();
                        LogTextScrollDown();     
                    }
                }
            }
            RefreshInformationUIThread();
            */

        }

        //This group of a function and a delegate is used to obtain a Thread Safe Update
        //of the GUI (Property of Control components), in particular they are used in the 
        //function OnPnPWMIEvent().
        private delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);
        public static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate(SetControlPropertyThreadSafe), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }
        }
        /* These two functions commented out shoud do the same functionality but they seem to not work/compile
         * They require using
         * System.Linq.Expressions;
         * System.Reflection;

        private delegate void SetPropertyThreadSafeDelegate<TResult>(Control @this, Expression<Func<TResult>> property, TResult value);

        public static void SetPropertyThreadSafe<TResult>(this Control @this, Expression<Func<TResult>> property, TResult value)
        {
            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

            if (propertyInfo == null ||
                !@this.GetType().IsSubclassOf(propertyInfo.ReflectedType) ||
                @this.GetType().GetProperty(propertyInfo.Name, propertyInfo.PropertyType) == null)
            {
                throw new ArgumentException("The lambda expression 'property' must reference a valid property on this Control.");
            }

            if (@this.InvokeRequired)
            {
                @this.Invoke(new SetPropertyThreadSafeDelegate<TResult>(SetPropertyThreadSafe), new object[] { @this, property, value });
            }
            else
            {
                @this.GetType().InvokeMember(propertyInfo.Name, BindingFlags.SetProperty, null, @this, new object[] { value });
            }
        }
        */

        //This function is called if a PnP event is retrieved by the query QUERY realized
        //by watcherDEWESoft every 1 second: it is used to find the insertion and detachment of DEWESoft device 
        private void OnPnPWMIEvent(object sender, EventArrivedEventArgs e)
        {
            PropertyData p = e.NewEvent.Properties["TargetInstance"];
            if (p != null)
            {
                ManagementBaseObject mbo = p.Value as ManagementBaseObject;

                //SetControlPropertyThreadSafe(textBox_VID, "Text", "0x00000");

                string deviceId = mbo["deviceid"].ToString();
                //Console.WriteLine(deviceId);

                int vidIndex = deviceId.IndexOf("VID_");
                string startingAtVid = deviceId.Substring(vidIndex + 4); // + 4 to remove "VID_"                    
                string vid = startingAtVid.Substring(0, 4); // vid is four characters long
                //Console.WriteLine("VID: " + vid);
                if (vid.Equals("0547"))
                {
                    VID_String = vid;
                    //textBox_VID_Text = vid;

                    //NOT Thread Safe, but it works 95% of the times: 
                    //it has been changed with the Thread Safe SetControlPropertyThreadSafe
                    /*
                    this.Invoke((MethodInvoker)delegate {
                        textBox_VID.Text = "0x0547";
                        DriverType_TextBox.Text = "DEWESoft device";
                        FirmwareTypeUSB.Text = "TE FX2 Firmware Gen 2";
                        LatestMajorVersionFW_textBox.Text = "It is not possible to retrieve";
                        LatestMinorVersionFW_textBox.Text = "It is not possible to retrieve";
                        String temp = "The TE USB FX2 module starts as a DEWESoft USB Device." + "\r\n" + "This happens when EEPROM switch is set to ON when the TE USB FX2 module is attached to USB port (or more generally when the TE USB FX2 module is powered on with EEPROM switch ON) but the 2nd Generation Firmware is stored in the EEPROM of FX2 microcontroller." + "\r\n" + "If the DEWESoft driver is installed you are able to find this device in Windows 'Device Manager' under 'Universal Serial Bus controller'. Otherwise you are able to find this device in Windows 'Device Manager' under 'Unknow Device'." + "\r\n" + "If you desire to change the FX2 Firmware from 2nd Generation to 3rd Generation (or even if you want to simply upgrade the 2nd Generation FX2 Firmware version) you must start a Recovery Procedure. " + "\r\n" + "Recovery Procedure: you should use a power off/on cycle with EEPROM switch to OFF to start with VID and PID of a Cypress Generic device. If the computer does't see the Cypress Generic device you must install the 'Cypress Generic device' driver found on Trenz Electronic web site in the recovery area." + "\r\n";
                        LogTextLine += temp + "\r\n";
                        MessageBox.Show(temp);
                    }); // runs on UI thread
                    */

                    /* Thread Safe, it should work 100% of the times */
                    SetControlPropertyThreadSafe(textBox_VID, "Text", "0x0547");
                    SetControlPropertyThreadSafe(DriverType_TextBox, "Text", "DEWESoft device");
                    SetControlPropertyThreadSafe(FirmwareTypeUSB, "Text", "TE USB FX2 Firmware Gen 2");

                    SetControlPropertyThreadSafe(LatestMajorVersionFW_textBox, "Text", "It is not possible to retrieve");
                    SetControlPropertyThreadSafe(LatestMinorVersionFW_textBox, "Text", "It is not possible to retrieve");
                    //String temp = "The TE USB FX2 module starts as a DEWESoft USB Device." + "\r\n" + "This happens when EEPROM switch is set to ON when the TE USB FX2 module is attached to USB port (or more generally when the TE USB FX2 module is powered on with EEPROM switch ON) but the 2nd Generation Firmware is stored in the EEPROM of FX2 microcontroller." + "\r\n" + "If the DEWESoft driver is installed you are able to find this device in Windows 'Device Manager' under 'Universal Serial Bus controller'. Otherwise you are able to find this device in Windows 'Device Manager' under 'Unknow Device'." + "\r\n" + "If you desire to change the FX2 Firmware from 2nd Generation to 3rd Generation (or even if you want to simply upgrade the 2nd Generation FX2 Firmware version) you must start a Recovery Procedure. " + "\r\n" + "Recovery Procedure: you should use a power off/on cycle with EEPROM switch to OFF to start with VID and PID of a Cypress Generic device. If the computer does't see the Cypress Generic device you must install the 'Cypress Generic device' driver found on Trenz Electronic web site in the recovery area." + "\r\n";
                    LogTextLine += DEWESoftDeviceAttached_String + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                    SetControlPropertyThreadSafe(textBox_LogText, "Text", LogTextLine);

                    /* Cross thread: it is not thread safe
                    textBox3.SelectionStart = textBox3.Text.Length;
                    textBox3.ScrollToCaret();
                    textBox3.Refresh();
                     */ 

                    /* Impossible to use because MessageBox is a type, not a varaiable
                    SetControlPropertyThreadSafe(MessageBox, "Show", temp);
                    */                  
                    
                    //MessageBox.Show(DEWESoftDeviceAttached_String);

                    //TO DO: a more robust method to use this flag.
                    bVID_DEWESoft = !(bVID_DEWESoft);
                }

                int pidIndex = deviceId.IndexOf("PID_");
                string startingAtPid = deviceId.Substring(pidIndex + 4); // + 4 to remove "PID_"                    
                string pid = startingAtPid.Substring(0, 4); // pid is four characters long
                //Console.WriteLine("PID: " + pid);
                if (pid.Equals("1002"))
                {
                    PID_String = pid;
                    SetControlPropertyThreadSafe(textBox_PID, "Text", "0x1002");
                    //textBox_PID_Text = pid;
                    /* Not thread safe
                    this.Invoke((MethodInvoker)delegate
                    {
                        textBox_PID.Text = "0x1002";
                    }); // runs on UI thread
                    bVID_DEWESoft = true;
                    */
                    //TO DO: a more robust method to use this flag.
                    bPID_DEWESoft = !(bPID_DEWESoft);
                }      
            }
        }

        /* Summary
        Event handler (callback function) for USB device removal
        */
        //TO DO: (DateTime.Today).ToString() could not work, search a better solution.
        void USBdevList_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;

            if ((usbEvent.ProductID == PIDCypress) && (usbEvent.VendorID == VIDCypress))
            {
                PID_String = "0x8613";
                VID_String = "0x04B4";

                if ((bResultLoadExternalRam == true) )
                {
                    //bResultLoadExternalRam = false; NO
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "INFO: Cypress device is removed: " + (DateTime.Today).ToString() + " ." + "\r\n";
                        LogTextLine += "INFO: It is a side effect of programming FX2 microcontroller RAM with Trenz Electronic firmware v3.02: the device is seen as removed even if it is not phisically removed" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    }
                }
                else
                {
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "INFO: Cypress device is removed: " + (DateTime.Today).ToString() + " ." + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    }
                }
                
            }
            else if ((usbEvent.ProductID == PIDTrenzElectronic) && (usbEvent.VendorID == VIDTrenzElectronic))
            {

                if ((bResultLoadExternalRam == true))
                {
                    //bResultLoadExternalRam = false; NO
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "INFO: Trenz Electronic device is removed: " + (DateTime.Today).ToString() + "\r\n";
                        LogTextLine += "INFO: It is a side effect of programming FX2 microcontroller RAM with Trenz Electronic firmware v3.02: the device is seen as removed even if it is not phisically removed" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    }
                }
                else
                {
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "INFO: Trenz Electronic device is removed: " + (DateTime.Today).ToString() + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    }
                }
            }
            else
            {
                if (bVerboseLogText == true)
                {
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "ERROR: Unspecified Electronic device is removed" + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                }
            }

            textBox_LogText.Text = LogTextLine;
            LogTextScrollDown();
            /*
            textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
            textBox_LogText.ScrollToCaret();
            textBox_LogText.Refresh();
            */
        
        }

        /* Summary
           Event handler (callback function) for new device attach
       */
        void USBdevList_DeviceAttached(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;


            if ((usbEvent.ProductID == PIDCypress) && (usbEvent.VendorID == VIDCypress))
            {
                //PID_String = "0x8613";
                //VID_String = "0x04B4";
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                LogTextLine += "INFO: Cypress device is attached: " + (DateTime.Today).ToString() + "\r\n";               
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                
            }
            else if ((usbEvent.ProductID == PIDTrenzElectronic) && (usbEvent.VendorID == VIDTrenzElectronic))
            {

                if ((bResultLoadExternalRam == true) )
                {
                    bResultLoadExternalRam = false;
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "INFO: Trenz Electronic device is attached: " + (DateTime.Today).ToString() + "\r\n";
                        LogTextLine += "INFO: It is a side effect of programming FX2 microcontroller RAM with Trenz Electronic firmware v3.02: the device is seen as removed even if it is not phisically removed" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    }
                }
                else
                {
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "INFO: Trenz Electronic device is attached: " + (DateTime.Today).ToString() + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    }
                }
                
            }
            else
            {
                if (bVerboseLogText == true)
                {
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "ERROR: Insertion of an Unspecified Electronic device " + "\r\n";
                    LogTextLine += "Insetion happens " + (DateTime.Today).ToString() + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n";
                }
            }

            //A new USB device list is created
            backgroundWakeEvent.WaitOne(500);
            USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
            backgroundWakeEvent.WaitOne(500);
 
            //Refresh the information displayed on the GUI (Form1) about the Device inserted
            RefreshInformationUIThread();           
        }

        //This function refresh the information displayed on the GUI (Form1) about the Device inserted
        //As the name warns, it run on the UIThread
        //TO DO: add another Backgroundworker to move the SendCommand section to a background thread;
        //in this way the GUI will avoid the last remaining possible reason to freeze.
        private void RefreshInformationUIThread()
        {
            //These three lines of code has been moved outside of this function to lessen/avoid GUI freezing
            //backgroundWakeEvent.WaitOne(1000);
            //USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
            //backgroundWakeEvent.WaitOne(1000);

            // Reset the text in the result label.
            //resultLabel.Text = String.Empty;
            UInt16 PID = 0;
            UInt16 VID = 0;

            //Verify that one Cypress or Trenz Electronic device has been attached (inserted)
            if (USBdevList.Count != 0)
            {
                //Modify this code if multiple USB device should be connected
                CyFX2Device fx2test = USBdevList[0] as CyFX2Device;
                PID = fx2test.ProductID;
                VID = fx2test.VendorID;
                //If more than one Cypress/TE device has been attached the program warns you that only one attached device is expected
                if (USBdevList.Count > 1)
                {
                    MessageBox.Show("In this version the program must be used with a single Trenz Electronic or Cypress module attached. You must remove Cypress and/or Trenz Electronic micromodules until only one module remains. This must be the module that you desire to write.");
                }
                //FirmwareTypeUSB.Text = "No Cypress or TE device, maybe you are using an old Firmware.";
                //DriverType_TextBox.Text = "Undefined or DEWESoft";
                //MessageBox.Show("The VID and PID used are for normal Cypress device; if you are starting a recovery procedure, it is correct. Otherwise use a power on/off cycle with EEPROM switch to ON to start with VID and PID of a Trenz Electronic device");
            }

            //If a Cypress Device is attached and it is the first time (bEEPROMSwitchSetToOn = false) that the program register this device a Recovery procedure is advised
            if ((PID == PIDCypress) && (VID == VIDCypress) && (!bEEPROMSwitchSetToOn))
            {
                FirmwareTypeUSB.Text = "Cypress used for Recovery Procedure";
                DriverType_TextBox.Text = "Cypress USB Generic Driver";


                if (bVerboseLogText == true)
                {
                    LogTextLine += "INFO: Cypress USB Generic Driver used for recovery" + "\r\n";
                    MessageBox.Show("The VID and PID used are for normal Cypress device; if you are starting a recovery procedure, it is correct. Otherwise, you should use a power off/on cycle with EEPROM switch to ON to start with VID and PID of a Trenz Electronic device. If the computer does't see the Trenz Electronic device you must install the 'Trenz Electronic USB FX2' driver.");
                }
                LatestMajorVersionFW_String = "Recovery Procedure";
                LatestMinorVersionFW_String = "Recovery Procedure";
                PID_String = "0x8613";
                VID_String = "0x04B4";

                LatestMajorVersionFW_textBox.Text = LatestMajorVersionFW_String;
                LatestMinorVersionFW_textBox.Text = LatestMinorVersionFW_String;
                textBox_LogText.Text = LogTextLine;
                LogTextScrollDown();
                /*
                textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
                textBox_LogText.ScrollToCaret();
                textBox_LogText.Refresh();
                */ 
                toolStripStatusLabel1.Text = StatusTextLine;
                LatestMajorVersionFPGA_textBox.Text = LatestMajorVersionFPGA_String;
                LatestMinorVersionFPGA_textBox.Text = LatestMinorVersionFPGA_String;
                LatestReleaseVersionFPGA_textBox.Text = LatestReleaseVersionFPGA_String;
                LatestBuildVersionFPGA_textBox.Text = LatestBuildVersionFPGA_String;
                textBox_PID.Text = PID_String;
                textBox_VID.Text = VID_String;

            }

            //If a Trenz Electronic Device is attached it is possible to try to use FX2 Command to retrieve information about the TE Device 
            //USB FX2 Firmware version and if a reference FPGA project has been loaded
            else if (((PID == PIDTrenzElectronic) && (VID == VIDTrenzElectronic)) || bEEPROMSwitchSetToOn)
            {
                
                FirmwareTypeUSB.Text = "TE USB FX2 Gen3";
                DriverType_TextBox.Text = "Trenz Electronic USB FX2 Device Driver";

                //Send Command Section
                //TO DO: add another Backgroundworker to move the SendCommand section to a background thread;
                //in this way the GUI will avoid the last remaining possible reason to freeze.

                TE_USB_FX2_USBDevice = USBdevList[0] as CyUSBDevice;
                backgroundWakeEvent.WaitOne(1000);
                
                if (bVerboseLogText == true)
                {
                    LogTextLine += "Checking the use of Trenz Electronic Reference Design: START" + "\r\n";
                    LogTextScrollDown();
                } 
                
                byte[] Command = new byte[64];
                byte[] Reply = new byte[64];
                int CmdLength = 64;
                int ReplyLength = 64;

                uint TIMEOUT_MS = 1000;

                Command[0] = (byte)FX2_Commands.CMD_FX2_SET_INTERRUPT;
                Command[1] = (byte)FX2_Parameters.MB_I2C_ADDRESS;
                Command[2] = (byte)FX2_Parameters.I2C_BYTES_SIZE;

                if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
                {                 
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "WARNING/INFO: Can't call API function TE_USB_FX2_SendCommand + SET_INTERRPUPT" + "\r\n";
                        LogTextLine += "WARNING/INFO: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                    }
                    StatusTextLine = "WARNING/INFO: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                    //worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                    
                }

                //Command[0] = (byte)FX2_Commands.CMD_FX2_GET_INTERRUPT;    //clear interrupt data register

                Command[0] = (byte)FX2_Commands.CMD_FX2_I2C_WRITE;
                //Command[1] = (byte)FX2_Parameters.MB_I2C_ADDRESS;
                //Command[2] = (byte)FX2_Parameters.I2C_BYTES_SIZE;
                Command[3] = (byte)0;
                Command[4] = (byte)0;
                Command[5] = (byte)0;
                Command[6] = (byte)MB_Commands.FX22MB_REG0_GETVERSION; //get FPGA version

                if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
                {
                    //cout << "Error" << endl;
                    //Console.WriteLine("Error Send Command Get FPGA Version");
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "WARNING/INFO: Can't call API function TE_USB_FX2_SendCommand + FX22MB_REG0_GETVERSION" + "\r\n";
                        LogTextLine += "WARNING/INFO: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                    }               
                    StatusTextLine = "WARNING/INFO: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                    //worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                    
                    LatestMajorVersionFPGA_textBox.Text = "It is impossible to retrieve";
                    LatestMinorVersionFPGA_textBox.Text = "It is impossible to retrieve";
                }

                Command[0] = (byte)FX2_Commands.CMD_FX2_GET_INTERRUPT; //0xB1;//comand CMD_FX2_GET_INTERRUPT

                if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == true)
                {
                    if ((ReplyLength > 4) && (Reply[0] != 0))
                    {
                        
                        SystemTypeFPGAFlash.Text = "Yes";
                        
                        if (bVerboseLogText == true)
                        {
                            LogTextLine += "INFO: Trenz Electronic Reference Design based on MicroBlaze soft processor is used" + "\r\n";
                            LogTextLine += "INFO: Major version: " + Reply[1].ToString() + "\r\n";
                            LogTextLine += "INFO: Minor version: " + Reply[2].ToString() + "\r\n";
                            LogTextLine += "INFO: Release version: " + Reply[3].ToString() + "\r\n";
                            LogTextLine += "INFO: Build version: " + Reply[4].ToString() + "\r\n";
                        }      
                    
                        LatestMajorVersionFPGA_String = Reply[1].ToString();
                        LatestMinorVersionFPGA_String = Reply[2].ToString();
                        LatestReleaseVersionFPGA_String = Reply[3].ToString();
                        LatestBuildVersionFPGA_String = Reply[4].ToString();                      

                    }
                    else
                    {
                        if (bVerboseLogText == true)
                            LogTextLine += "INFO: Custom project not based on TE Reference Architecture" + "\r\n";
                        SystemTypeFPGAFlash.Text = "No, Custom project not based on TE Reference Architecture";
                        //LogTextLine += "Major version: " + "it doesn't exist" + "\r\n";
                        //LogTextLine += "Minor version: " + "it doesn't exist" + "\r\n";
                        //LogTextLine += "Release version: " + "it doesn't exist" + "\r\n";
                        //LogTextLine += "Build version: " + "it doesn't exist" + "\r\n";
                    }
                }
                else
                {
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "INFO: Custom project not based on TE Reference Architecture" + "\r\n";
                        LogTextLine += "INFO: Warning: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";                       
                        LogTextLine += "WARNING/INFO: Can't call API function TE_USB_FX2_SendCommand + FX22MB_REG0_GETVERSION" + "\r\n";

                    }
                    SystemTypeFPGAFlash.Text = "No, Custom project not based on TE Reference Architecture";
                    StatusTextLine = "Warning: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                    //worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                }
                if (bVerboseLogText == true)
                {
                    LogTextLine += "Checking the use of Trenz Electronic Reference Design :STOP" + "\r\n";
                    LogTextLine += "Checking the use of Trenz Electronic TE FX2 Firmware :START" + "\r\n";
                }
                //label3.Text = TextLine;
                textBox_LogText.Text = LogTextLine;
                LogTextScrollDown();
                /*
                textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
                textBox_LogText.ScrollToCaret();
                textBox_LogText.Refresh();
                */
                toolStripStatusLabel1.Text = StatusTextLine;
                LatestMajorVersionFPGA_textBox.Text = LatestMajorVersionFPGA_String;
                LatestMinorVersionFPGA_textBox.Text = LatestMinorVersionFPGA_String;
                LatestReleaseVersionFPGA_textBox.Text = LatestReleaseVersionFPGA_String;
                LatestBuildVersionFPGA_textBox.Text = LatestBuildVersionFPGA_String;

                textBox_PID.Text = PID_String;
                textBox_VID.Text = VID_String;

                byte[] Command1 = new byte[64];
                byte[] Reply1 = new byte[64];
                //int CmdLength = 64;
                //int ReplyLength = 64;
                //uint TIMEOUT_MS = 1000;

                Command1[0] = (byte)FX2_Commands.CMD_FX2_READ_VERSION;
                //comand read FX2 version
                //Console.WriteLine("Command[0] {0:X2} ", Command[0]);

                if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command1, ref CmdLength, ref Reply1, ref ReplyLength, TIMEOUT_MS) == true)
                {
                    if (ReplyLength >= 4)
                    {
                        if (bVerboseLogText == true)
                        {
                            LogTextLine += "INFO: TE FX2 Firmware Generation 3 stored in FX2 microcontroller EEPROM" + "\r\n";
                            LogTextLine += "INFO: Major version: " + Reply1[0].ToString() + "\r\n";
                            LogTextLine += "INFO: Minor version: " + Reply1[1].ToString() + "\r\n";
                        }

                        LatestMajorVersionFW_String = Reply1[0].ToString();
                        LatestMinorVersionFW_String = Reply1[1].ToString();

                        VID_String = "0x" + (TE_USB_FX2_USBDevice.VendorID).ToString("X4");
                        PID_String = "0x" + (TE_USB_FX2_USBDevice.ProductID).ToString("X4");
                    }
                    else
                    {
                        if (bVerboseLogText == true)
                            LogTextLine += "ERROR: TE FX2 Firmware Generation 3 is not stored in FX2 microcontroller EEPROM" + "\r\n";

                        LatestMajorVersionFW_String = "ERROR";
                        LatestMinorVersionFW_String = "ERROR";

                    }

                }

                else
                //cout << "Error" << endl;
                //Console.WriteLine("Error");
                {
                    if (bVerboseLogText == true)
                        LogTextLine += "ERROR: TE FX2 Firmware Generation 3 is not stored in FX2 microcontroller EEPROM" + "\r\n";
                                      
                    LatestMajorVersionFW_String = "ERROR";
                    LatestMinorVersionFW_String = "ERROR";                  
                }

                if (bVerboseLogText == true)
                {
                    LogTextLine += "Checking the use of Trenz Electronic TE_USB_FX2 firmware: STOP" + "\r\n";                  
                }
                
                textBox_LogText.Text = LogTextLine;
                LogTextScrollDown();
                /*
                textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
                textBox_LogText.ScrollToCaret();
                textBox_LogText.Refresh();
                */ 
                toolStripStatusLabel1.Text = StatusTextLine;

                textBox_VID.Text = VID_String;
                textBox_PID.Text = PID_String;

                LatestMajorVersionFW_textBox.Text = LatestMajorVersionFW_String;
                LatestMinorVersionFW_textBox.Text = LatestMinorVersionFW_String;
            }
        }

        // This function is used to obtains the Friendly Name of the host's Operating System
        private static string GetOSFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }

        //This function has been removed because is never used, but it could be useful in the future
        /*public void PnP_Event_Handler(IntPtr pnpEvent, IntPtr hRemovedDevice)
        {
            if (pnpEvent.Equals(CyConst.DBT_DEVICEREMOVECOMPLETE))
            {

                //USBdevList.Remove(hRemovedDevice);
                // Other removal event handling
                //USBdevList.USBdevList_DeviceAttached(ref USBdevList);
            }

            if (pnpEvent.Equals(CyConst.DBT_DEVICEARRIVAL))
            {
                //USBdevList.Add();
                // Other arrival event handling    
            }                
        
        }*/

        //SOURCE of this fundamental function: http://blogs.msdn.com/b/heikkiri/archive/2012/07/17/hex-string-to-corresponding-byte-array.aspx
        //This function is used convert Hex string value in Byte array.
        //The normal functions used by Windows for the same works are not suitable because they create
        //something similar to 0A040E05 instead of A4E5
        private static byte[] ConvertToByteArray(string value)
        {
            byte[] bytes = null;
            if (String.IsNullOrEmpty(value))
                bytes = null;
            else
            {
                int string_length = value.Length;
                int character_index = (value.StartsWith("0x", StringComparison.Ordinal)) ? 2 : 0; // Does the string define leading HEX indicator '0x'. Adjust starting index accordingly.               
                int number_of_characters = string_length - character_index;

                bool add_leading_zero = false;
                if (0 != (number_of_characters % 2))
                {
                    add_leading_zero = true;

                    number_of_characters += 1;  // Leading '0' has been striped from the string presentation.
                }

                bytes = new byte[number_of_characters / 2]; // Initialize our byte array to hold the converted string.

                int write_index = 0;
                if (add_leading_zero)
                {
                    bytes[write_index++] = FromCharacterToByte(value[character_index], character_index);
                    character_index += 1;
                }

                for (int read_index = character_index; read_index < value.Length; read_index += 2)
                {
                    byte upper = FromCharacterToByte(value[read_index], read_index, 4);
                    byte lower = FromCharacterToByte(value[read_index + 1], read_index + 1);

                    bytes[write_index++] = (byte)(upper | lower);
                }
            }

            return bytes;
        }

        //Function used inside the function ConvertToByteArray
        private static byte FromCharacterToByte(char character, int index, int shift = 0)
        {
            byte value = (byte)character;
            if (((0x40 < value) && (0x47 > value)) || ((0x60 < value) && (0x67 > value)))
            {
                if (0x40 == (0x40 & value))
                {
                    if (0x20 == (0x20 & value))
                        value = (byte)(((value + 0xA) - 0x61) << shift);
                    else
                        value = (byte)(((value + 0xA) - 0x41) << shift);
                }
            }
            else if ((0x29 < value) && (0x40 > value))
                value = (byte)((value - 0x30) << shift);
            else
                throw new InvalidOperationException(String.Format("Character '{0}' at index '{1}' is not valid alphanumeric character.", character, index));

            return value;
        }

        //This function reverse the bit order of a byte
        private static byte ReverseWithLookupTable(byte toReverse)
        {
            return BitReverseTable[toReverse];
        }


        /* BW1_FPGA_SPIFlash background worker: START */
        
        //This is the asyncronous function called to acually do the FPGA SPI Flash erase/writing work
        //This function internally calls SPI_Flash_Programming() function
        private void BW1_FPGA_SPIFlash_DoWork(object sender, DoWorkEventArgs e)
        {

            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation
            // to the Result property of the DoWorkEventArgs
            // object. This is will be available to the 
            // RunWorkerCompleted eventhandler.
            /*for exampl, e.Result = ComputeFibonacci((int)e.Argument, worker, e);*/

            e.Result = SPI_Flash_Programming( worker, e);         
            
        }

        // This event handler (callback function) updates the progress bar of FPGA SPI Flash
        private void BW1_FPGA_SPIFlash_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label3.Text = TextLine;
            textBox_LogText.Text = LogTextLine;
            textBox_LogText.Update();
            LogTextScrollDown();
            /*
            textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
            textBox_LogText.ScrollToCaret();
            textBox_LogText.Refresh();
            */
            toolStripStatusLabel1.Text = StatusTextLine;
            //LatestMajorVersionFPGA_textBox.Text = LatestMMversionFPGA_String;
            //LatestMinorVersionFPGA_textBox.Text = ReleaseBuildVersion_String;
            LatestMajorVersionFPGA_textBox.Text = LatestMajorVersionFPGA_String;
            LatestMinorVersionFPGA_textBox.Text = LatestMinorVersionFPGA_String;
            LatestReleaseVersionFPGA_textBox.Text = LatestReleaseVersionFPGA_String;
            LatestBuildVersionFPGA_textBox.Text = LatestBuildVersionFPGA_String;
            SystemTypeFPGAFlash.Text = SystemTypeFPGAFlash_Text;
        }

        // This event handler (callback function) deals with the results of the
        // background operation for FPGA SPI Flash.
        private void BW1_FPGA_SPIFlash_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW1_FPGA_SPIFlash_RunWorkerCompleted);
                worker.DoWork -= new DoWorkEventHandler(BW1_FPGA_SPIFlash_DoWork);
                worker.Dispose();

            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                //resultLabel.Text = "Canceled";
                MessageBox.Show("Canceled: probably, you have not selected a bitstrream file for the Flash.");
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW1_FPGA_SPIFlash_RunWorkerCompleted);
                worker.DoWork -= new DoWorkEventHandler(BW1_FPGA_SPIFlash_DoWork);
                worker.Dispose();

            }
            else
            {   
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW1_FPGA_SPIFlash_RunWorkerCompleted);
                worker.DoWork -= new DoWorkEventHandler(BW1_FPGA_SPIFlash_DoWork);
                worker.Dispose();              
            }

            // Enable the Start button.
            button_ProgFpgaStart.Enabled = true;
            button_ProgrUSBStart.Enabled = true;
            button_ProgFpgaFilePathSelection.Enabled = true;
            button_ProgUSBFilePathSelection.Enabled = true;

            // Disable the Start button until 
            // the asynchronous operation is done.          
            button_RefereshInformation.Enabled = true;
            checkBox_VerboseLogText.Enabled = true;
            checkBox_Retrieve_Flash_ID.Enabled = true;
            checkBox_ClearLogText4everyProgrammingOperation.Enabled = true;

            // Disable all flags that should be disabled
            bFileFpga_Selected = false;
            bFileFpga_PreProcessed = false;
            bFLASH_FPGA_AlreadyWritten = false;
            
            bMCSXilinxFlash = false;

            textBox_FPGA_Bitstream_File_Path.ReadOnly = false;

            //bresult_SPI_Flash_Programming = false;

            // Disable the Cancel button.
            //cancelAsyncButton.Enabled = false;
        }

        //This event handler (callback function) deals with button click event of button 
        //'Select *.bit or *.mcs file, or enter file path' aka button_ProgFpgaFilePathSelection
        //It set bool flag bFileFpga_Selected to true
        private void button_ProgFpgaFilePathSelection_Click(object sender, EventArgs e)
        {
            StatusTextLine = String.Empty;

            bMCSXilinxFlash = false;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Title = "Select file to download . . .";
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.DefaultExt = "bitstream (*.bit)";
            openFileDialog1.Filter = "MCS (*.mcs) | *.mcs|bitstream (*.bit) | *.bit"; 
            // example "bit files (*.bit)|*.bit|All files (*.*)|*.*";

            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {                        
               FPGAFile_FilePath = openFileDialog1.FileName;

               textBox_FPGA_Bitstream_File_Path.Text = FPGAFile_FilePath;

               bFileFpga_Selected = true;
            }
        }

        //This event handler (callback function) deals with button click event of button 
        //'Program FPGA: write SPI Flash' aka button_ProgFpgaStart
        //It set bool flag bFileFpga_Selected to true if FPGAFile_FilePath.Length > 0
        /* A more robust check should be used */
        private void button_ProgFpgaStart_Click(object sender, EventArgs e)
        {
            /* TO DO: a more robust check should be used*/
            if (FPGAFile_FilePath.Length > 0)
                bFileFpga_Selected = true;

            //Add Event handlers (calback functions)
            BW1_FPGA_SPIFlash.DoWork += new DoWorkEventHandler(BW1_FPGA_SPIFlash_DoWork);
            BW1_FPGA_SPIFlash.ProgressChanged += new ProgressChangedEventHandler(BW1_FPGA_SPIFlash_ProgressChanged);
            BW1_FPGA_SPIFlash.WorkerReportsProgress = true;
            BW1_FPGA_SPIFlash.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW1_FPGA_SPIFlash_RunWorkerCompleted);
            //backgroundWorker1.RunWorkerAsync();
            BW1_FPGA_SPIFlash.WorkerSupportsCancellation = true;

            textBox_FPGA_Bitstream_File_Path.ReadOnly = true;

            progressBar1.Value = (int)0;
            progressBar1.Enabled = true;
            highestPercentageReached_FPGA_SPIFlashWrite = 0;

            // Disable the Start button until 
            // the asynchronous operation is done.
            button_ProgFpgaStart.Enabled = false;
            button_ProgFpgaFilePathSelection.Enabled = false;
            button_ProgrUSBStart.Enabled = false;
            button_ProgUSBFilePathSelection.Enabled = false;
            button_RefereshInformation.Enabled = false;
            checkBox_VerboseLogText.Enabled = false;
            checkBox_Retrieve_Flash_ID.Enabled = false;
            checkBox_ClearLogText4everyProgrammingOperation.Enabled = false;

            // Start the asynchronous operation.
            if (!BW1_FPGA_SPIFlash.IsBusy)
                BW1_FPGA_SPIFlash.RunWorkerAsync();
            else
                MessageBox.Show("Can't run the worker twice!");            
        }

        /******SPI_Flash_Programming****/
        // This is the method that does the actual work. For this
        // example, it computes a Fibonacci number and
        // reports progress as it does its work.
        bool SPI_Flash_Programming(BackgroundWorker worker, DoWorkEventArgs e)
        {

            //bFileFpga_Selected = false;
            bFileFpga_PreProcessed = false;
            bFLASH_FPGA_AlreadyWritten = false;

            bMCSXilinxFlash = false;
            bEEPROMSwitchSetToOn = false;

            bMCS_DummyWordLine = false;
            bMCSXilinxFlash_Spartan3A_SyncWordLine = false;
            bMCSSPIFlash3rdParts_Spartan3A_SyncWordLine = false;
            bMCSXilinxFlash_Spartan3Eor6_SyncWordLine = false;
            bMCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine = false;


            if (bClearLogTextBeforeEveryProgrammingOperation == true)
                LogTextLine = String.Empty;

            if (USBdevList == null)
            {
                throw new ArgumentException(
                    "At least one TE USB FX2 module must be attached");
            }

            // Abort the operation if the user has canceled.
            // Note that a call to CancelAsync may have set 
            // CancellationPending to true just after the
            // last invocation of this method exits, so this 
            // code will not have the opportunity to set the 
            // DoWorkEventArgs.Cancel flag to true. This means
            // that RunWorkerCompletedEventArgs.Cancelled will
            // not be set to true in your RunWorkerCompleted
            // event handler. This is a race condition.
            while (true)
            {
                if (worker.CancellationPending || (!bFileFpga_Selected))
                {                  
                    e.Cancel = true;
                    return bresult_SPI_Flash_Programming;
                }

                //1st Step: open the bitstream file (.bit or .mcs) selected by FPGAFile_FilePath
                if (bFileFpga_Selected && !(bFileFpga_PreProcessed) && (!bFLASH_FPGA_AlreadyWritten))
                {
                    /*try
                    {*/
                        using (FileStream fpga_bitstream_file = new FileStream(FPGAFile_FilePath, FileMode.Open))
                        //using (FileStream fpga_bitstream = new FileStream(FPGAFile_FilePath, FileMode.Open))                    
                        {

                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "\r\n";
                                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                                LogTextLine += "SPI Flash Programming operation: it also changes Project running on FPGA" + "\r\n";
                                LogTextLine += "INFO: The operation STARTS " + (DateTime.Today).ToString() + "\r\n";
                                LogTextLine += "INFO: OpenFutNET version running on the host: " + OpenFutNETversion + "\r\n";
                                LogTextLine += "INFO: Operating system of the host:" + GetOSFriendlyName() + "\r\n";
                                LogTextLine += "INFO: .NET version running on the host: " + Environment.Version.ToString() + "\r\n";
                                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                            }
                            else
                            {
                                LogTextLine += "\r\n";
                                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                            }

                            StatusTextLine = "Bitstream file for FPGA is open and preprocessed: START";
                            if (bVerboseLogText == true)
                                LogTextLine += "Bitstream file for FPGA is open and preprocessed: START" + "\r\n";
                            else
                                LogTextLine += "\r\n";


                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);

                            //start a timer to measure the time required by FPGA Bistream elaboration
                            Stopwatch stopWatchFpgaBitstreamElaboration = new Stopwatch();
                            stopWatchFpgaBitstreamElaboration.Start();
                            fpga_bitstream_size = (int)fpga_bitstream_file.Length;

                            /*try
                            {*/
                                using (BinaryReader fpga_bitstream = new BinaryReader(fpga_bitstream_file))
                                {
                                    //The program creates a byte array with the size of .bit (or .mcs) file selected
                                    //TO DO: read the file as blocks of 4-16 Kbyte
                                    byte[] wr_fpga_bitsream2 = new byte[fpga_bitstream_size];

                                    //The program reads the entire content of .bit (or .mcs) file selected
                                    //TO DO: read the file as blocks of 4-16 Kbyte
                                    wr_fpga_bitsream2 = fpga_bitstream.ReadBytes(fpga_bitstream_size);

                                    //The program transform the the byte array in a string usefull for MCS file elaboration
                                    string sourceMCS = (System.Text.Encoding.ASCII.GetString(wr_fpga_bitsream2)).Replace("-", string.Empty);
                                    //The program transform the the byte array in a string usefull for BIT file elaboration
                                    string sourceBIT = (BitConverter.ToString(wr_fpga_bitsream2)).Replace("-", string.Empty);
                                    
                                    //Code usefull to test: start
                                    //(BitConverter.ToString(resultB)).Replace("-", string.Empty);
                                    //BitConverter.ToString(msgb); .BIT
                                    //System.Text.Encoding.ASCII.GetString(msgb); No
                                    //System.Text.Encoding.UTF8.GetString(msgb); yes, mcs

                                    //MessageBox.Show("BIT " + msgb);
                                    //MessageBox.Show("MCS " + msgb);  
                                    //Code usefull to test: end

                                    //Bit file match declaration
                                    //This match store the Spartan's dummy word line found inside .bit bitstream 
                                    System.Text.RegularExpressions.Match match_BIT_DummyWordLine = null;
                                    //This match store the Spartan3E and Spartan6's SyncWord found inside .bit bitstream
                                    System.Text.RegularExpressions.Match match_BIT_Spartan3Eor6_SyncWordLine = null;
                                    //This match store the Spartan3A's SyncWord found inside .bit bitstream
                                    System.Text.RegularExpressions.Match match_BIT_Spartan3A_SyncWordLine = null;

                                    //MCS file match declaration
                                    //This match store the Spartan's dummy word line found inside .mcs bitstream
                                    System.Text.RegularExpressions.Match match_MCS_DummyWordLine = null;
                                    //MCS file Spartan3A
                                    //This match store the Spartan3A's SyncWordLine CaseA found inside .mcs bitstream (MCS Xilinx SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseA = null;
                                    //This match store the Spartan3A's SyncWordLine CaseA found inside .mcs bitstream (MCS third party SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseA = null;
                                    //This match store the Spartan3A's SyncWordLine CaseB found inside .mcs bitstream (MCS Xilinx SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseB = null;
                                    //This match store the Spartan3A's SyncWordLine CaseB found inside .mcs bitstream (MCS third party SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseB = null;
                                    //MCS file Spartan3E or Spartan6
                                    //This match store the Spartan3E (and Spartan6) SyncWordLine CaseA found inside .mcs bitstream (MCS Xilinx SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseA = null;
                                    //This match store the Spartan3E (and Spartan6) SyncWordLine CaseA found inside .mcs bitstream (MCS third party SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseA = null;
                                    //This match store the Spartan3E (and Spartan6) SyncWordLine CaseB found inside .mcs bitstream (MCS Xilinx SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseB = null;
                                    //This match store the Spartan3E (and Spartan6) SyncWordLine CaseB found inside .mcs bitstream (MCS third party SPI Flash)
                                    System.Text.RegularExpressions.Match match_MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseB = null;

                                    //This string identify the Spartan3A's SyncWord pattern inside .bit bitstream 
                                    string sPattern_BIT_Spartan3A_SyncWord = "(AA99){1}";

                                    //This string identify the Spartan3E and Spartan6's dummy pattern (inside .bit bitstream) that is before the Sync word 
                                    // "(FFFF){1,64}" + "(AA99){1}"
                                    string sPatternBIT_Spartan3A_DummyWord = "(FFFF){1,64}";

                                    //This string identify the Spartan3E and Spartan6's SyncWord pattern inside .bit bitstream 
                                    string sPattern_BIT_Spartan3Eor6_SyncWord = "(AA995566){1}";

                                    //This string identify the Spartan3E and Spartan6's dummy pattern (inside .bit bitstream) that is before the Sync word 
                                    // "(FFFFFFFF){1,32}" + "(AA995566){1}"
                                    string sPatternBIT_Spartan3Eor6_DummyWord = "(FFFFFFFF){1,32}";     

                                    // Bit file regular expression match serach
                                    //This match store the Spartan's dummy word line found inside .bit bitstream
                                    match_BIT_DummyWordLine = System.Text.RegularExpressions.Regex.Match(sourceBIT, (sPatternBIT_Spartan3Eor6_DummyWord), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    //This match store the Spartan3E and Spartan6's Sync Word line found inside .bit bitstream
                                    match_BIT_Spartan3A_SyncWordLine = System.Text.RegularExpressions.Regex.Match(sourceBIT, (sPatternBIT_Spartan3A_DummyWord + sPattern_BIT_Spartan3A_SyncWord), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    //This match store the Spartan3A's Sync Word line found inside .bit bitstream
                                    match_BIT_Spartan3Eor6_SyncWordLine = System.Text.RegularExpressions.Regex.Match(sourceBIT, (sPatternBIT_Spartan3Eor6_DummyWord + sPattern_BIT_Spartan3Eor6_SyncWord), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                                                  
                                    //sync line "(\\.){1}:{1}[a-zA-Z0-9]{8}(FFFF){1,7}(AA99){1}"
                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3A + sPattern CheckValue;
                                    string MCS_DummyWordLine = "[a-zA-Z0-9]{8}(FFFF){8}[a-zA-Z0-9]{2}";

                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3A repeated 1 to 7 times+ sPattern SyncWordMCSXilinxFlash3ADSP;
                                    string MCSXilinxFlash_Spartan3A_SyncWordLine_CaseA = "[a-zA-Z0-9]{8}(FFFF){1,7}(5599){1}";
                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3A repeated 1 to 7 times+ sPattern SyncWordMCSSPIFlash3rdParts3ADSP;                          
                                    string MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseA = "[a-zA-Z0-9]{8}(FFFF){1,7}(AA99){1}";

                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3A repeated 8 times+ sPattern SyncWordMCSXilinxFlash3ADSP;                       
                                    string MCSXilinxFlash_Spartan3A_SyncWordLine_CaseB = "[a-zA-Z0-9]{8}(5599){1}";
                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3A repeated 8 times+ sPattern SyncWordMCSSPIFlash3rdParts3ADSP;                            
                                    string MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseB = "[a-zA-Z0-9]{8}(AA99){1}";

                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3Eor6 repeated 1 to 3 times+ sPattern SyncWordMCSXilinxFlash3Eor6;
                                    string MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseA = "[a-zA-Z0-9]{8}(FFFFFFFF){1,3}(5599AA66){1}";
                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3Eor6 repeated 1 to 3 times+ sPattern SyncWordMCSSPIFlash3rdParts3Eor6;                   
                                    string MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseA = "[a-zA-Z0-9]{8}(FFFFFFFF){1,3}(AA995566){1}";
                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3Eor6 repeated 3 times+ sPattern SyncWordMCSXilinxFlash3Eor6;
                                    string MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseB = "[a-zA-Z0-9]{8}(5599AA66){1}";
                                    //sPattern MCSAddressLengthLine + sPattern DummyWordSpartan3Eor6 repeated 3 times+ sPattern SyncWordMCSSPIFlash3rdParts3Eor6;
                                    string MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseB = "[a-zA-Z0-9]{8}(AA995566){1}";

                                    //this is the separator of MCS lines
                                    string[] stringSeparators = new string[] { ":" };
                                    string[] resultMCS;

                                    // sourceMCS is subdivided in resultMCS using stringSeparator ":"
                                    // TO DO: instead of sourceMCS is possible to use the first 5000-10000 character
                                    resultMCS = sourceMCS.Split(stringSeparators, StringSplitOptions.None);

                                    //this is the current MCS line number
                                    int lineMCSnumber = 0;

                                    //this is the last MCS dummy line number
                                    int lineDummy = 0;

                                    //For each MCS line the program search
                                    foreach (string lineMCS in resultMCS)
                                    {
                                        //This match store the Spartan's dummy word line found inside .mcs bitstream
                                        match_MCS_DummyWordLine = System.Text.RegularExpressions.Regex.Match(lineMCS, MCS_DummyWordLine, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        
                                        match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseA = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSXilinxFlash_Spartan3A_SyncWordLine_CaseA, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseA = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseA, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseB = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSXilinxFlash_Spartan3A_SyncWordLine_CaseB, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseB = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseB, System.Text.RegularExpressions.RegexOptions.IgnoreCase);


                                        match_MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseA = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseA, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        match_MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseA = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseA, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        match_MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseB = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseB, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        match_MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseB = System.Text.RegularExpressions.Regex.Match(lineMCS, MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseB, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                        //if Dummy word line is found the corresponding flag is asserted (true) and the dummy line is asiigned to the current line
                                        if (match_MCS_DummyWordLine.Success)
                                        {
                                            bMCS_DummyWordLine = true;
                                            lineDummy = lineMCSnumber;
                                        }

                                        //If a dummy word line is found in the prevoius (Xilinx Flash type) MCS line and SyncWordLine CaseB for Spartan3A in the current MCS line is found, the file is identified as a MCS File for Spartan3A (Xilinx Flash type)
                                        else if (((bMCS_DummyWordLine == true) && (lineDummy == lineMCSnumber - 1)) && (match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseB.Success))
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSXilinxFlash_Spartan3A_SyncWordLine = true;
                                            break;
                                        }
                                        //If SyncWordLine CaseA for Spartan3A in the current MCS line is found, the file is identified as a MCS File for Spartan3A (Xilinx Flash type)
                                        else if (match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseA.Success)
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSXilinxFlash_Spartan3A_SyncWordLine = true;
                                            break;
                                        }
                                        //If a dummy word line is found in the prevoius (third party SPI Flash type) MCS line and SyncWordLine_CaseB for Spartan3A in the current MCS line is found, the file is identified as a MCS File for Spartan3A (third party SPI Flash type)
                                        else if (((bMCS_DummyWordLine == true) && (lineDummy == lineMCSnumber - 1)) && (match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseB.Success))
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSSPIFlash3rdParts_Spartan3A_SyncWordLine = true;
                                            break;
                                        }
                                        //If SyncWordLine CaseA for Spartan3A in the current MCS line is found, the file is identified as a MCS File for Spartan3A (third party SPI Flash type)
                                        else if (match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseA.Success)
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSSPIFlash3rdParts_Spartan3A_SyncWordLine = true;
                                            break;
                                        }
                                        //If a dummy word line is found in the prevoius (Xilinx Flash type) MCS line and SyncWordLine_CaseB for Spartan3E (or Spartan6) in the current MCS line is found, the file is identified as a MCS File for Spartan3E or 6 (Xilinx Flash type)
                                        else if (((bMCS_DummyWordLine == true) && (lineDummy == lineMCSnumber - 1)) && (match_MCSXilinxFlash_Spartan3Eor6_SyncWordLine_CaseB.Success))
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSXilinxFlash_Spartan3Eor6_SyncWordLine = true;
                                            break;
                                        }
                                        //If SyncWordLine CaseA for Spartan3E (or Spartan6) in the current MCS line is found, the file is identified as a MCS File for Spartan3E or 6 (Xilinx Flash type)
                                        else if (match_MCSXilinxFlash_Spartan3A_SyncWordLine_CaseA.Success)
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSXilinxFlash_Spartan3Eor6_SyncWordLine = true;
                                            break;
                                        }
                                        //If a dummy word line is found in the prevoius (third party SPI Flash type) MCS line and SyncWordLine_CaseB for Spartan3E (or Spartan6) in the current MCS line is found, the file is identified as a MCS File for Spartan3E or 6 (third party SPI Flash type)
                                        else if (((bMCS_DummyWordLine == true) && (lineDummy == lineMCSnumber - 1)) && (match_MCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine_CaseB.Success))
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine = true;
                                            break;
                                        }
                                        //If SyncWordLine CaseA for Spartan3E (or Spartan6) in the current MCS line is found, the file is identified as a MCS File for Spartan3E or 6 (third party SPI Flash type)
                                        else if (match_MCSSPIFlash3rdParts_Spartan3A_SyncWordLine_CaseA.Success)
                                        {
                                            bMCS_DummyWordLine = true;
                                            bMCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine = true;
                                            break;
                                        }
                                        //In any other case bMCS_DummyWordLine = false
                                        else
                                        {
                                            bMCS_DummyWordLine = false;
                                        }
                                        
                                        //At every iteration increment the current MCS line number.
                                        lineMCSnumber += 1;
                                    }

                                    //TEST CODE, comment out only for test
                                    //resultLabel.Text = (matchSyncWordMCSSPIFlash3rdParts.Index).ToString();

                                    //If a BIT Sync Word line (Spartan3 or 6) is found on the bit file, the file is identified as BIT file for Spartan 3 or 6
                                    if (match_BIT_DummyWordLine.Success && match_BIT_Spartan3Eor6_SyncWordLine.Success && (!(sourceBIT[0].Equals('3'))) && (!(sourceBIT[1].Equals('A'))))
                                    {
                                        //BIT file bitstream Spartan3 or Spartan6
                                        file_type = 1; //# BIT file
                                        //This code is used to estimate the header offset to use for stripping bitstream of non binary data
                                        offset = (match_BIT_DummyWordLine.Index) / 2;	 
                                        //resultLabel.Text =offset.ToString();
                                        bMCSXilinxFlash = false;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: (Case 1) The file selected seems a valid bit file created for FPGA Spartan 3 or Spartan 6. It could be flashed on Trenz Electronic micromodules";
                                        //it is not possible to directly programming in Flash a BIT file with other instruments (iMPACT or Digilent Adept), only with this one. In iMPACT you must first generate the MCS file for iMPACT or Digilent Adept." + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //If a Xilinx MCS Sync Word line (Spartan3 or 6) is found on the mcs file, the file is identified as Xilinx MCS file for Spartan 3 or 6
                                    else if ((bMCS_DummyWordLine && bMCSXilinxFlash_Spartan3Eor6_SyncWordLine) && ((sourceMCS[0].Equals(':'))))
                                    {
                                        //MCS file bitstream XILINX Spartan3 or Spartan6                            
                                        file_type = 2; //# MCS file
                                        //It is assumed that MCS has not description header 
                                        offset = 0;	                                                                      
                                        bMCSXilinxFlash = true;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: (Case 2) The file selected seems a valid MCS file (for FPGA Spartan 3 or Spartan 6) created for Xilinx Flash. It could be flashed on Trenz Electronic micromodules" + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //If a third party MCS Sync Word line (Spartan3 or 6) is found on the bit file, the file is identified as 3rd party MCS file for Spartan 3 or 6
                                    else if (bMCS_DummyWordLine && bMCSSPIFlash3rdParts_Spartan3Eor6_SyncWordLine && ((sourceMCS[0].Equals(':'))))
                                    {
                                        //MCS file bitstream 3rd party SPI Flash Spartan3 or Spartan6 
                                        file_type = 3; //# MCS file 
                                        //It is assumed that MCS has not description header                       
                                        offset = 0;
                                        bMCSXilinxFlash = false;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: (Case 3) The file selected seems a valid MCS file (for FPGA Spartan 3 or Spartan 6) created for third-party SPI Flash. It could be flashed on Trenz Electronic micromodules" + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //If a BIT Sync Word line (Spartan3A) is found on the bit file, the file is identified as BIT file for Spartan 3A. To be sure of the identification SyncWordLine for Spartan3E (or 6) should not be found.
                                    else if (match_BIT_DummyWordLine.Success && match_BIT_Spartan3A_SyncWordLine.Success && (!(match_BIT_Spartan3Eor6_SyncWordLine.Success)) && (!(sourceBIT[0].Equals('3'))) && (!(sourceBIT[1].Equals('A'))))
                                    {
                                        //bit file bitstream Spartan 3A
                                        file_type = 4;
                                        //This code is used to estimate the header offset to use for stripping bitstream of non binary data
                                        offset = (match_BIT_DummyWordLine.Index) / 2;	//# BIT file                                  
                                        
                                        bMCSXilinxFlash = false;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: (Case 4) The file selected seems a valid bit file created for FPGA Spartan3A DSP. It could be flashed on Trenz Electronic micromodules." + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //If a Xilinx MCS Sync Word line (Spartan3A) is found on the mcs file, the file is identified as Xilinx MCS file for Spartan3A
                                    else if (((bMCS_DummyWordLine && bMCSXilinxFlash_Spartan3A_SyncWordLine ) && ((sourceMCS[0].Equals(':')))))
                                    {
                                        //MCS file bitstream Xilinx Flash Spartan3A
                                        //It is assumed that MCS has not description header
                                        offset = 0;	//# MCS file                                
                                        file_type = 5;
                                        bMCSXilinxFlash = true;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO (Case 5) The file selected seems a valid MCS (for FPGA Spartan3A DSP ) file created for Xilinx Flash. It could be flashed on Trenz Electronic micromodules." + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //If a third party MCS Sync Word line (Spartan3A) is found on the bit file, the file is identified as 3rd party MCS file for Spartan3A
                                    else if (((bMCS_DummyWordLine && bMCSSPIFlash3rdParts_Spartan3A_SyncWordLine) && ((sourceMCS[0].Equals(':')))))        
                                    {
                                        //MCS file bitstream 3rd party SPI Flash  Spartan3 or Spartan6 
                                        //It is assumed that MCS has not description header            
                                        offset = 0;	//# MCS file 
                                        file_type = 6;
                                        bMCSXilinxFlash = false;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: (Case 6) The file selected seems a valid MCS file (for FPGA Spartan3A DSP) created for third-party SPI Flash. It could be flashed on Trenz Electronic micromodules." + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //If no one of the condition above is true the file is identified as an unknow file bitstream
                                    //TO DO: 
                                    //1) add a flag to force programming operation even if the file is not identified
                                    //2) add a flag to force (or not) bit inversion in the elaboration of the unidentified file bitstream
                                    //3) add a flag or 2-3 radio button to force "interpretation/elaboration" ad bit/mcs/bin file bitstream
                                    //4) add a pop-up that show the result of elaboration on the first 400-500 byte before to actually program SPI Flash and FPGA
                                    else
                                    {                    
                                        offset = 0;
                                        file_type = 7;
                                        bMCSXilinxFlash = false;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "ERROR/WARNING: (Case 7) The file selected seems an invalid BIT or MCS file. Check the option used and if it is for FPGA Spartan3/6/3A. Maybe is only a header in an MCS that should not exist." + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }

                                    //This flag is asserted because the FPGA/Flash file is selected and identified.
                                    bFileFpga_Selected = true;
                                    //This flag is not yet asserted because the Flash and the FPGA are not yet written.
                                    bFLASH_FPGA_AlreadyWritten = false;

                                    //END 1st step: identification of file type

                                    //2nd step start: 
                                    //If bit file identified in 1st step, the header is stripped away from .bit file selected 
                                    //If mcs file identified in 1st step, extraction of .bit bitstream from mcs file;
                                    //if the MCS file selected is of Xilinx Flash type every data byte (.bit bitstream byte extracted) 
                                    //must be bit reversed

                                    //Bit file identified in 1st step
                                    if ((file_type == 1) || (file_type == 4))
                                    {                     
                                        //BIT FILE: the header is stripped away                                                            
                                        for (int i = 0; i < (fpga_bitstream_size - offset); i++)
                                        {
                                            wr_fpga_bitsream[i] = (wr_fpga_bitsream2[i + offset]);
                                        }
                                        for (int i = fpga_bitstream_size - offset + 1; i < fpga_bitstream_size; i++)
                                        {
                                            wr_fpga_bitsream[i] = (byte)0;
                                        }
                                        //This flag is asserted because the pre-elaboration for bit bitstream is done.
                                        bFileFpga_PreProcessed = true;
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: Bitstream File is stripped of the header; the stripped bitstream could be flashed on Trenz Electronic micromodules" + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    }
                                    //Mcs file identified in 1st step
                                    else if ((file_type == 2) || (file_type == 5) || (file_type == 3) || (file_type == 6))
                                    {                              
                                        if (bVerboseLogText == true)
                                            LogTextLine += "INFO: The MCS HEX file must be converted into a bit file before the MCS file can be flashed on SPI Flash File" + "\r\n";
                                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                        
                                        //See below
                                        int[] DataRecord = new int[16];
                                        int RecordDatByteNumber = 0;
                                        int high_addressfield = 0;
                                        int low_addressfield = 0;
                                        int addressfield = 0;                                   
                                        byte[] address = new byte[8];

                                        /*
                                         * http://www.sbprojects.com/knowledge/fileformats/intelhex.php
                                        '00' = Data Record
                                        '01' = End Of File Record
                                        '02' = Extended Segment Address Record
                                        '03' = Start Segment Address Record
                                        '04' = Extended Linear Address Record
                                        '05' = Start Linear Address Record.
                                        */
                                        /*
                                        :ll aaaa tt[ d  d  d  d dddddddddddddddddddddddddddd] cc.
                                     
                                        012 3456 78  9 10 11 12 -------non-existent---------
                                        :02 0000 04  0  0  0  0                               FA.

                                        ¦02 is the number of data bytes in the record.
                                        ¦0000 is the (lower) address field. For the extended linear address record, this field is always 0000. 
                                        ¦04 is the record type 04 (an extended linear address record). 
                                        ¦0000 is the upper 16 bits of the address.
                                        ¦FA is the checksum of the record and is calculated as
                                            01h + NOT(02h + 00h + 00h + 04h + 00h + 00h)= 01h + NOT(06h) = 01h + F9h = FA. 

                            16 byte + 1 check byte:  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16  check byte
                                        :10 0000 00 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF  00.
                                        :10 0010 00 55 99 AA 66 0C 85 00 E0 04 00 8C 85 60 14 8C 82  DA.
                                         */
                             
                                        //Test Code: it starts here
                                        /*
                                        byte[] msgb2 = new byte[2000];
                                        for (int i = 0; i < 2000; i++)
                                        {        
                                            msgb2[i] = (byte) (sourceMCS[i]);
                                            //ReverseWithLookupTable(wr_fpga_bitsream[i]);  
                                        }                                        

                                        /////string resultToSee2 = (BitConverter.ToString(msgb2)).Replace("-", string.Empty);
                                        //System.Text.Encoding.UTF8.GetString(msgb).Replace("-", string.Empty);
                                        //(BitConverter.ToString(msgb)).Replace("-", string.Empty);
                                        //System.Text.Encoding.ASCII.GetString(msgb); No
                                        string resultToSee2 =  System.Text.Encoding.UTF8.GetString(msgb2); //.Replace("-", string.Empty);
                                        //System.Text.Encoding.UTF8.GetString(msgb); yes, mcs

                                        MessageBox.Show("This is the first part of the bitstream that will be written in the SPI Flash of 3rd Party on Trenz Electronic micromodule" + "\r\n" + resultToSee2);
                                        */
                                        //Test Code: it ends here

                                        //TO DO: verify the checksum code of every MCS line here or before
                                        //Probably is better before using resultMCS instead of sourceMCS

                                        //TO DO: change the code below from sourceMCS to resultMCS

                                        //":" start of a new MCS line
                                        for (int i = 0; i < sourceMCS.Length; i++)
                                        {
                                            if ((sourceMCS[i]).Equals(':'))
                                            {

                                                addressfield = high_addressfield + low_addressfield;
                                                //sanity check 
                                                if ((sourceMCS[i + 7]).Equals('0'))
                                                {
                                                    //Local stringbuilder used to transform the .mcs file line in a int number       
                                                    //number of d inside the line :ll aaaa tt[dddddddddddddddddddddddddddddddd] cc.
                                                    System.Text.StringBuilder sb_rdbn = new System.Text.StringBuilder();

                                                    sb_rdbn.Append(sourceMCS[i + 1]);
                                                    sb_rdbn.Append(sourceMCS[i + 2]);

                                                    //The program hypothesizes to extract this number of .bit content from the .mcs file line
                                                    //number of d inside the line :ll aaaa tt[dddddddddddddddddddddddddddddddd] cc.
                                                    RecordDatByteNumber = int.Parse(sb_rdbn.ToString(), System.Globalization.NumberStyles.HexNumber);

                                                    /* //Test Code
                                                    MessageBox.Show(RecordDatByteNumber.ToString());
                                                    */

                                                    //Local stringbuilder used to transform the .mcs file line in a int number 
                                                    System.Text.StringBuilder sb_la = new System.Text.StringBuilder();

                                                    sb_la.Append(sourceMCS[i + 3]);
                                                    sb_la.Append(sourceMCS[i + 4]);
                                                    sb_la.Append(sourceMCS[i + 5]);
                                                    sb_la.Append(sourceMCS[i + 6]);

                                                    //The aaaa inside the line :ll aaaa tt[dddddddddddddddddddddddddddddddd] cc.
                                                    low_addressfield = int.Parse(sb_la.ToString(), System.Globalization.NumberStyles.HexNumber);
                                                    //The aaaa inside the line :ll aaaa tt[dddddddddddddddddddddddddddddddd] cc.
                                                    //plus
                                                    //the (dddd << 16) inside of a previous line :02 0000 04 dddd cc. (record type = 04)
                                                    addressfield = high_addressfield + low_addressfield;
                                                    //MessageBox.Show(low_addressfield.ToString());                                              

                                                    
                                                    //if((record_type == 4) )             
                                                    //¦04 is the record type 04 (an extended linear address record). 
                                                    //¦0000 is the address field. For the extended linear address record, this field is always 0000. 
                                                    if ((sourceMCS[i + 8]).Equals('4')
                                                        && ((sourceMCS[i + 3]).Equals('0'))
                                                        && ((sourceMCS[i + 4]).Equals('0'))
                                                        && ((sourceMCS[i + 5]).Equals('0'))
                                                        && ((sourceMCS[i + 6]).Equals('0')))
                                                    {
                                                        System.Text.StringBuilder sb_ha = new System.Text.StringBuilder();

                                                        sb_ha.Append(sourceMCS[i + 9]);
                                                        sb_ha.Append(sourceMCS[i + 10]);
                                                        sb_ha.Append(sourceMCS[i + 11]);
                                                        sb_ha.Append(sourceMCS[i + 12]);

                                                        //the dddd  inside of a current line :02 0000 04 dddd cc. (record type = 04)
                                                        high_addressfield = int.Parse(sb_ha.ToString(), System.Globalization.NumberStyles.HexNumber);
                                                        //(dddd << 16)
                                                        high_addressfield = high_addressfield << 16;
                                                        //MessageBox.Show(low_addressfield.ToString());
                                                        //MessageBox.Show(high_addressfield.ToString());                            
                                                        addressfield = high_addressfield + low_addressfield;
                                                        //MessageBox.Show("haf " + high_addressfield.ToString());
                                                    }
                                                    //else if((record_type == 0)) 
                                                    //'00' = Data Record
                                                    else if ((sourceMCS[i + 8]).Equals('0'))
                                                    {
                                                        String s_converted_data = "0x";
                                                        System.Text.StringBuilder sb_data = new System.Text.StringBuilder();

                                                        /* //test code
                                                        MessageBox.Show(RecordDatByteNumber.ToString());
                                                        byte[] temp_array = new byte[RecordDatByteNumber];
                                                        MessageBox.Show("rdbn " + RecordDatByteNumber.ToString());
                                                        */

                                                        //sourceMCS is organized in nibble, so RecordDatByteNumber * 2 instead of RecordDatByteNumber
                                                        //All the data in the line are appended to string builder sb_data
                                                        for (int i1 = 0; i1 < RecordDatByteNumber * 2; i1++)
                                                        {
                                                            sb_data.Append(sourceMCS[i + 9 + i1]);
                                                        }

                                                        /* //test code
                                                        if (sourceMCS[i + 9 + RecordDatByteNumber * 2].Equals('.'))
                                                            MessageBox.Show("Hello");
                                                        */
                                                        
                                                        //Sting builder is converted in a string
                                                        s_converted_data = sb_data.ToString();

                                                        //This function is used convert Hex string value s_converted_data in Byte array temp_array.
                                                        //The normal functions used by Windows for the same works are not suitable because they create
                                                        //something similar to 0A040E05 instead of A4E5
                                                        byte[] temp_array = ConvertToByteArray(s_converted_data);
                                                        
                                                        /* //test code
                                                        string resultToSee2 = (BitConverter.ToString(temp_array)).Replace("-", string.Empty);
                                                        MessageBox.Show(resultToSee2);
                                                        */

                                                        //For every byte in the .mcs line the bit reversed (or not) byte is written in the byte array wr_fpga_bitsream 
                                                        for (int i2 = 0; i2 < RecordDatByteNumber; i2++)
                                                        {
                                                            //if the MCS file selected is of Xilinx Flash type every data byte must be bit reversed
                                                            if (bMCSXilinxFlash)
                                                            {
                                                                wr_fpga_bitsream[addressfield + i2] = ReverseWithLookupTable(temp_array[i2]);
                                                            }
                                                            //if the MCS file selected is of third-party SPI Flash type every data byte must be not bit reversed
                                                            else
                                                            {
                                                                wr_fpga_bitsream[addressfield + i2] = temp_array[i2];
                                                            }
                                                            // Increment the size of the bitstream (MCS 2 BIT) 
                                                            //fpga_bitstream_sizeMCS2BIT += 1; 
                                                            //The line above is correct only for *.mcs file without "holes"
                                                            //For example if the *.mcs file has bitstream + user data. 
                                                        }
                                                        fpga_bitstream_sizeMCS2BIT = addressfield + RecordDatByteNumber;
                                                        //Using the line above OpenFutNet supports the *.mcs file with "holes".
                                                        //For example if the *.mcs file has bitstream + user data.
                                                        //Thanks to the Trenz Electronic Fpga User 
                                                        //Alejandro for the recommendation

                                                    }
                                                    //else if((record_type == 1))
                                                    //'01' = End Of File Record
                                                    else if ((sourceMCS[i + 8]).Equals('1'))
                                                    {
                                                        //End of a file
                                                        // add something as a message to test if necessary
                                                        break;
                                                    }                                   
                                                }
                                                else
                                                {
                                                    //Sanity check failed: the mcs is wrong
                                                    MessageBox.Show("The MCS file is corrupted");
                                                    break;
                                                }     
                                            }
                                            //else (aka sourceMCS[i]) is not to Equals(':')) it is not the start of a new MCS line 
                                        }
                                        //The extraction of .bit bitstream from .mcs file is ended
                                        //Assign the value of the size of .bit bitstream extracted to the variable used to read the bitstream buffer
                                        fpga_bitstream_size = fpga_bitstream_sizeMCS2BIT;
                                    }

                                    if (bVerboseLogText == true)
                                        LogTextLine += "INFO: MCS HEX file has been converted into a Byte Array" + "\r\n";
                                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);

                                    /* //Test code
                                    byte[] msgb2a = new byte[2000];
                                    for (int i = 0; i < 2000; i++)
                                    {
                                        msgb2a[i] = (wr_fpga_bitsream[i]);
                                        //ReverseWithLookupTable(wr_fpga_bitsream[i]);
                                    }

                                    string resultToSee3 = (BitConverter.ToString(msgb2a)).Replace("-", string.Empty);
                                    //System.Text.Encoding.UTF8.GetString(msgb).Replace("-", string.Empty);
                                    //(BitConverter.ToString(msgb)).Replace("-", string.Empty);
                                    //System.Text.Encoding.ASCII.GetString(msgb); No

                                    //System.Text.Encoding.UTF8.GetString(msgb); yes, mcs

                                    MessageBox.Show("This is the first part of the bitstream that will be written in the SPI Flash of 3rd Party on Trenz Electronic micromodule" + "\r\n" + resultToSee3);
                                    */

                                    //Close the binary read
                                    fpga_bitstream.Close();
                                    //Close the selected .bit or .mcs file
                                    fpga_bitstream_file.Close();
                                    //stop the timer
                                    stopWatchFpgaBitstreamElaboration.Stop();

                                    TimeSpan ts = stopWatchFpgaBitstreamElaboration.Elapsed;

                                    string elapsedTime = String.Format("(0:0000)", ts.Milliseconds);
                                    int msTime = ts.Milliseconds;

                                    string elapsedTime2 = String.Format("(0:00)", ts.Seconds);
                                    int sTime = ts.Seconds;

                                    string elapsedTime3 = String.Format("(0:00)", ts.Minutes);
                                    int mTime = ts.Minutes;

                                    msTime = mTime * 60 * 1000 + sTime * 1000 + msTime;

                                    if (bVerboseLogText == true)
                                        LogTextLine += "INFO: Bitstream file for FPGA is open and preprocessed in " + msTime.ToString() + " milliseconds" + "\r\n";

                                    highestPercentageReached_FPGA_SPIFlashErase = -1;
                                    highestPercentageReached_FPGA_SPIFlashWrite = -1;
                                    percentComplete_FPGA_SPIFlashWrite = 0;
                                    percentComplete_FPGA_SPIFlashErase = 0;
                                                                    
                                    StatusTextLine = "Bitstream file for FPGA is open and preprocessed: STOP";

                                    //This flag is asserted because the pre-elaboration for bit bitstream is done.
                                    bFileFpga_PreProcessed = true;
                                    bFLASH_FPGA_AlreadyWritten = false;

                                    if (bVerboseLogText == true)
                                        LogTextLine += "Bitstream file for FPGA is open and preprocessed: STOP" + "\r\n";
                                    //worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    //label3.Text = TextLine;
                                    //textBox3.Text = LogTextLine;
                                    //toolStripStatusLabel1.Text = StatusTextLine;
                                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                }
                            //}
                            /*catch (Exception ex)
                            {
                                MessageBox.Show("Error: Could not binary read the file selected. Original error: " + ex.Message);
                            }*/
                        }
                    //}
                    /*catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }*/
                }

                //3rd step: write the .bit bitstream (selected .bit file or extracted .bit file from the selected .mcs file) in the SPI Flash after a connection to TE device and an SPI Flash erasing
                if (bFileFpga_Selected && bFileFpga_PreProcessed && (!bFLASH_FPGA_AlreadyWritten))
                {
                    //Timeout time for the TE API Command that will be transmitted to EP0 of FX2 USB microcontroller 
                    //Timeout 1s
                    uint timeout_ms = 1000;

                    //bFileFpga_Selected && bFileFpga_PreProcessed
                    bool opened = true;                  

                    //Byte array of command to send to FX2 USB microcontroller
                    byte[] Command = new byte[64];
                    //Byte array of reply from the FX2 USB microcontroller
                    byte[] Reply = new byte[64];
                    int CmdLength = 64;
                    int ReplyLength = 64;

                    //No error until this point
                    op_error = 0;

                    //CmdLength = 4;
                    //ReplyLength = 4;
               
                    // Consider to move this code in a C# console program or in a Python script and to use
                    // a power off/on cycle after the use of this program/script. 
                    //bool bRetrieve_FlashID = true;
                    //Retrieve SPI Flash Idenifier
                    if (bRetrieve_FlashID == true)
                    {
                        /*
                         * Function in TE USB FX2 Generation 3 firmware can be found at page
                         * https://wiki.trenz-electronic.de/display/TEUSB/SPI+Flash+Commands  
                         * void spi_command(BYTE CmdLen, unsigned char *CmdData, BYTE RdLen, unsigned char *RdData)
                         * {
                         *      volatile unsigned char spi_count, rd_buff;// pr_address;

                         *      OED = 0x73;       // 0b01110011; => use FPGA_POWER to obtain OED=0x03;
                         *      FPGA_POWER = 0;   //power off fpga
        
                         *      FLASH_ENABLE;                 //assert chip select
                         *      //Write command
                         *      spi_count = CmdLen;
                         *      if (spi_count > 64) spi_count = 64;
                         *      while (spi_count > 0)
                         *      {
                         *          putcSPI(*CmdData); //send read command
                         *          CmdData++;
                         *          spi_count = spi_count - 1;
                         *      }
                         *
                         *      //Read response
                         *      spi_count = RdLen;
                         *      if (spi_count > 64) spi_count = 64;
                         *      while (spi_count > 0)
                         *      {
                         *          rd_buff = getcSPI();
                         *          *RdData = rd_buff;
                         *          RdData++;
                         *          spi_count = spi_count - 1;
                         *      }
                         *      FLASH_DISABLE;
                         *}

                        /*                                  
                         * Another similar function used with vcom firmware (not TE USB FX2 Generation 3 firmware) is the following                                    
                         * void process_flash_id(void)
                         * {                     
                         *  BYTE mid, did, uid;            
                         *  char msg[] = "IDCODE ??????";
                         *  busy_polling();
                         *  FLASH_ENABLE; // assert chip select
                         *  putcSPI(SPI_RDID); // get ID command 
                         *  mid = getcSPI();   
                         *  did = getcSPI();
                         *  uid = getcSPI(); 
                         *  FLASH_DISABLE; // negate chip select
                         *  msg[7] = hex_char_h(mid);          
                         *  msg[8] = hex_char_l(mid);    
                         *  msg[9] = hex_char_h(did);
                         *  msg[10] = hex_char_l(did);
                         *  msg[11] = hex_char_h(uid);
                         *  msg[12] = hex_char_l(uid);  
                         *  print_ep6_string(msg);    
                         *  //if((mid == 0xEF) && (did == 0x40) && (uid == 0x17))
                         *  // print_ep6_string("W25Q64FV");
                         *  //if((mid == 0x20) && (did == 0x20) && (uid == 0x16))   
                         *  // print_ep6_string("M25P32");   
                         * }
                         */

                        /* ----FPGA power off----
                         * To get full control on SPI bus firmware disable FPGA power supply 
                         * before this operation. 
                         * 
                         * FPGA_POWER = 0;
                         *  
                         * -----------putcSPI(SPI_RDID); // get ID command-------------------------
                         * busy_polling(); => 
                         * // See https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/te_usb_api.ver.3.2/te_api.cOn 
                         * // some modules busy_polling() cause API error =>
                         * // => better to do it from software 
                         * FLASH_ENABLE; // assert chip select
                         * putcSPI(SPI_RDID); // get ID command
                         * mid = getcSPI();
                         * did = getcSPI();
                         * uid = getcSPI();
                         * FLASH_DISABLE; // negate chip select
                         * msg[7] = hex_char_h(mid);
                         * msg[8] = hex_char_l(mid);
                         * msg[9] = hex_char_h(did);
                         * msg[10] = hex_char_l(did);
                         * msg[11] = hex_char_h(uid);
                         * msg[12] = hex_char_l(uid);
                         * 
                         * After each flash command, use "power on" command and "fpga reset" 
                         * command to run FPGA. => "fpga reset" is, at current time broken
                         * 
                         * ----FPGA power on----
                         * 
                         * FPGA_POWER=1;
                         *  
                         * ---fpga_reset-----
                         * This command also disconnect FX2 microcontroller from SPI bus 
                         * to allow FPGA boot from SPI Flash.
                         * OED = 0x03; // Configure PS_ON, PROG as outputs; MOSI, CCLK, CSO_B areno longer output
                         * 
                         * The "fpga reset" command execute FPGA reset sequence by driving FPGA 
                         * PROG_B pin low and high after some delay. 
                         * FPGA_PROG = 0;
                         * SYNCDELAY; SYNCDELAY; SYNCDELAY;
                         * FPGA_PROG = 1;
                         * 
                         */

                        //byte[] Command = new byte[64];
                        //byte[] Reply = new byte[64];
                        CmdLength = 64;
                        ReplyLength = 64;

                        /*
                         * ----FPGA power off----
                         * To get full control on SPI bus firmware disable FPGA power supply 
                         * before this operation. 
                         * 
                         * FPGA_POWER = 0;
                         */
                        Command[0] = (byte)FX2_Commands.CMD_FX2_POWER_ON;   //# Power OFF FPGA
                        Command[1] = (byte)(Convert.ToChar(0)); //chr(0)    //# 0 = Turn Off
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)                     
                        {               
                            if (bVerboseLogText == true)
                                LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_POWER_ON" + "\r\n";
                            else
                                LogTextLine += " STOP. " + "ERROR: it is not possible to power off and the power on the FPGA" + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                            StatusTextLine = "ERROR: it is not possible to power on the FPGA";
                            op_error = 5;
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            return false;
                        }

                        byte[] Command1 = new byte[64];
                        byte[] Reply1 = new byte[64];
                        int CmdLength1 = 4;
                        int ReplyLength1 = 64;

                        byte[] Command2 = new byte[64];
                        byte[] Reply2 = new byte[64];
                        int CmdLength2 = 64;
                        int ReplyLength2 = 64;

                        //To use the firmware function spi_command() you need to use a indirection
                        Command2[0] = (byte)FX2_Commands.CMD_FX2_FLASH_WRITE_COMMAND;
                        Command2[1] = (byte)0x01; //Numeber of SPI commands used by spi_command(): putcSPI(SPI_RDID);                  
                        Command2[2] = (byte)0x03; //Number of SPI bytes as reply: mid = 0x20 did = 0x20 uid = 0x16
                        Command2[3] = (byte)0x9F; //(byte)SPI_Commands.CMD_SPI_RDID; // SPI_RDID 0x9F ≡ get ID command

                        Command1[0] = Command2[0];
                        Command1[1] = Command2[1];
                        Command1[2] = Command2[2];
                        Command1[3] = Command2[3];
                        Command1[4] = (byte)0;
                        Command1[5] = (byte)0;
                        Command1[6] = (byte)0;
   
                        /*
                         * -----------putcSPI(SPI_RDID); // get ID command-------------------------
                         * busy_polling(); => 
                         * // See https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/te_usb_api.ver.3.2/te_api.cOn 
                         * // some modules busy_polling() cause API error =>
                         * // => better to do it from software 
                         * FLASH_ENABLE; // assert chip select
                         * putcSPI(SPI_RDID); // get ID command
                         * mid = getcSPI();
                         * did = getcSPI();
                         * uid = getcSPI();
                         * FLASH_DISABLE; // negate chip select
                         * msg[7] = hex_char_h(mid);
                         * msg[8] = hex_char_l(mid);
                         * msg[9] = hex_char_h(did);
                         * msg[10] = hex_char_l(did);
                         * msg[11] = hex_char_h(uid);
                         * msg[12] = hex_char_l(uid);
                         */

                        /*
                         * OED = 0x73;       // 0b01110011; => use FPGA_POWER to obtain OED=0x03;
                         * FPGA_POWER = 0;   //power off fpga 
                         * FLASH_ENABLE;     //assert chip select
                         * 
                         */
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command1, ref CmdLength1, ref Reply1, ref ReplyLength1, 5000) == true)
                        {
                            LogTextLine += "SPI Flash IDCODE 0x" + Reply1[1].ToString("x") + " 0x"
                                + Reply1[2].ToString("x") + " 0x" + Reply1[3].ToString("x") + "\r\n";
                        }
                        else
                        {
                            LogTextLine += "SPI Flash IDCODE 0x" + Reply1[1].ToString("x") + " 0x"
                                + Reply1[2].ToString("x") + " 0x" + Reply1[3].ToString("x") + "\r\n";

                        }

                        CmdLength = 64;
                        ReplyLength = 64;

                        /* Fpga reset and Power ON 
                         * After each flash command, use "power on" command and "fpga reset" 
                         * command to run FPGA. => "fpga reset" is, at current time, is not 
                         * available in TE USB FX2 microcontroller reference firmware
                         * https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/te_usb_api.ver.3.2/te_api.c
                         */

                        /* This part will be introduced if CMD_FPGA_RESET of 
                         * https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/te_usb_api.ver.3.2/te_api.c
                         * will be changed to reflect the same code of vcom reference firmware, 
			             * (https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/vcom-asyn/te_vcom.c
			             * or https://github.com/Trenz-Electronic/TE-USB-Suite/blob/master/TE_USB_FX2.firmware/vcom-cli/te_vcom.c)		
                        // FPGA_PROG = 0; //FX2_PROG_B = 0;
                        // SYNCDELAY; SYNCDELAY; SYNCDELAY;
                        // FPGA_PROG = 1; //FX2_PROG_B = 1;
                         * The "fpga reset" command execute FPGA reset sequence by driving FPGA 
                         * PROG_B pin low and high after some delay. 
                         * FPGA_PROG = 0;
                         * SYNCDELAY; SYNCDELAY; SYNCDELAY;
                         * FPGA_PROG = 1; 
                         */
                        /* //CMD_FX2_FPGA_RESET to add if CMD_FPGA_RESET of TE USB FX2 module's reference firmware is changed to vcom firmware
                         * Command[0] = (byte)FX2_Commands.CMD_FX2_FPGA_RESET;
                         * if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)                    
                         * {                        
                         *      if (bVerboseLogText == true)                        
                         *          LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_FPGA_RESET" + "\r\n";                    
                         *      else    
                         *          LogTextLine += " STOP. " + "ERROR: it is not possible to reset the FPGA" + "\r\n";                          
                         *      LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";                
                         *      StatusTextLine = "ERROR: it is not possible to power on the FPGA";              
                         *      op_error = 0; //It in not considered an error but only a warning                       
                         *      worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);                        
                         *      //return false;          
                         * }
                         */

                        /* 
                         * ----FPGA power on----
                         * 
                         * FPGA_POWER=1;
                         */
                        Command[0] = (byte)FX2_Commands.CMD_FX2_POWER_ON;   //# Power ON FPGA
                        Command[1] = (byte)(Convert.ToChar(1)); //chr(1)    //# 1 = Turn On => OED = 0x03; 
                        /*IMPORTANT: //# 1 = Turn On => OED = 0x03;  
                         * OED = 0x03; // Configure PS_ON, PROG as outputs; MOSI, CCLK, CSO_B areno longer output
                         * This command also disconnect FX2 microcontroller from SPI bus 
                         * to allow FPGA boot from SPI Flash. 
                         */
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)
                        {
                            if (bVerboseLogText == true)
                                LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_POWER_ON" + "\r\n";
                            else
                                LogTextLine += " STOP. " + "ERROR: it is not possible to power off and the power on the FPGA" + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                            StatusTextLine = "ERROR: it is not possible to power on the FPGA";
                            op_error = 5;
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            return false;
                        }
                    }
                                      
                    //The program reports to the user that SPI Flash erasing is starting
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "SPI Programming: START" + "\r\n";
                    }
                    else
                    {
                        LogTextLine += "SPI Flash erasing: START.....";
                    }
                                                 
                    if (bVerboseLogText == true)
                        LogTextLine += "INFO: Bitstream size " + fpga_bitstream_size.ToString() + " bytes" + "\r\n";

                    //3rd step section A: connect to TE device
                    //Call ScanCards driver function: the function returns the number of TE device attached
                    //Python equivalent cards = fx2dll.TE_USB_FX2_ScanCards() 
                    int cards = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_ScanCards(ref USBdevList);                  
                    
                    if (bVerboseLogText == true)
                        LogTextLine += "INFO: Found " + cards.ToString() + " card(s)" + "\r\n";
                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                    if (cards == 0)
                    {                      
                        StatusTextLine = "Error: no card found";
                        
                        if (bVerboseLogText == true)
                            LogTextLine += "ERROR: No cards to connect" + "\r\n";
                        else
                            LogTextLine += " STOP. " + " ERROR: No cards to connect" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                        //Type of error = 3
                        op_error = 3;
                        file_type = 0;
                        //other zeroing
                        return bresult_SPI_Flash_Programming;
                    }

                    //# Timeout 1s //Timeout time for the TE API Command that will be transmitted to EP0 of FX2 USB microcontroller 
                    timeout_ms = 1000;
                    //if op_error == 0:   # No errors in past
                    //if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms, DriverBufferSize) != 0: 
                    //Open the TE device //# Connect to card
                    if ((op_error != 0) || (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_Open(ref TE_USB_FX2_USBDevice, ref USBdevList, 0) == false))
                    {                       
                        StatusTextLine = "Error: failed to connect card";
                        if (bVerboseLogText == true)
                            LogTextLine += "ERROR: Failed to connect card" + "\r\n";
                        else
                            LogTextLine += " STOP. " + "ERROR: Failed to connect card" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                        //Type of error = 4
                        op_error = 4;
                        file_type = 0;
                        //other zeroing if needed
                        return bresult_SPI_Flash_Programming;
                    }
                    else
                    {
                        //No error until this point: scanned the TE device and open correctly a TE device attached
                        opened = true;                     
                        if (bVerboseLogText == true)
                            LogTextLine += "INFO: Connected to card 1" + "\r\n";
                    }
                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);

                    //3rd step section B: erase the SPI Flash
                    //# Erase SPI Flash if no errors until now
                    if ((op_error == 0) || opened) 
                    {
                        //sectors2erase = (fpga_bitstream_size >> 16) + 1 # full sectors + remainder
                        //#spi_erase_sectors(sectors2erase)
                                              

                        // # full sectors + remainder
                        int sector2erase = (fpga_bitstream_size >> 16) + 1;
                        
                        if (bVerboseLogText == true)
                            LogTextLine += "SPI Flash erasing: START" + "\r\n";
                        //.set("Erasing")   # Update operation label
                        StatusTextLine = "SPI Flash erasing";

                        //Testing if SPI Flash is busy
                        //CmdLength = 64;
                        //ReplyLength = 64;
                        Command[0] = (byte)FX2_Commands.CMD_FX2_READ_STATUS;                      
                        for (int t = 0; t < 8; t++)
                        {

                            if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)
                            //fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API                               
                            {
                                //LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand" + "\r\n";

                                if ((Reply[2]) != 0)
                                {
                                    //op.set("Error")
                                    //printlog("ERROR: Flash busy")
                                    StatusTextLine = "Error: Flash busy";
                                    if (bVerboseLogText == true)
                                        LogTextLine += "ERROR: Flash busy" + "\r\n";
                                    else
                                        LogTextLine += " STOP" + "ERROR: Flash busy" + "\r\n";
                                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                                    op_error = 6;
                                    return false;
                                }
                            }

                        }                         
                        if (bVerboseLogText == true)
                            LogTextLine += "Bulk erasing: START" + "\r\n";
                        //int flash_busy = 0;
                        TextLine = "FPGA SPI Flash erasing progress";
                        
                        /*
                        int flash_busy = 0;
                        //Testing if go busy: it seems to not work in C#
                        
                        while (flash_busy == 0)	 //# Flash should be busy after erase command
		                {
                            Command[0] = (byte)FX2_Commands.CMD_FX2_FLASH_ERASE;		//# Command
		                    if( TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)
                                //fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			                    //printlog("ERROR: Can't call API function TE_USB_FX2_SendCommand")
			                    //op.set("Error")   
                            {
			                    op_error = 5;
                            }
                            

                            Command[0] = (byte)FX2_Commands.CMD_FX2_READ_STATUS;		//# Command
		                    if( TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)
                                //fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			                    //printlog("ERROR: Can't call API function TE_USB_FX2_SendCommand")
			                    //op.set("Error")
                            {
			                    op_error = 5;
                            }
		       
                            flash_busy = Reply[2];
                        }
                        */

                        //# Chip erase time is 15-45 seconds
                        int erase_complete = 0;
                        highestPercentageReached_FPGA_SPIFlashWrite = -1;

                        //Create and start a timer to measure how much time is required to erase the SPI Flash
                        Stopwatch stopWatchBulkErase = new Stopwatch();
                        stopWatchBulkErase.Start();
                        //If no errors until now start to erase the SPI Flash
                        if (op_error == 0) 
                        {
                            //# Command erase all the SPI Flash
                            Command[0] = (byte)FX2_Commands.CMD_FX2_FLASH_ERASE;		
                            if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, 2000) == false)
                            //fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
                            //printlog("ERROR: Can't call API function TE_USB_FX2_SendCommand")
                            //op.set("Error")  
                            {
                                //Type of error = 5
                                op_error = 5;
                                StatusTextLine = "Error: it is not possible to erase the SPI Flash";
                                if (bVerboseLogText == true)
                                    LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_FLASH_ERASE" + "\r\n";
                                else
                                    LogTextLine += " STOP. " + " ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_FLASH_ERASE" + "\r\n";
                                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                return false;
                            }
                            //# Chip erase time is 15-45 seconds
                            for (int t = 0; t < 45; t++)
                            {
                                                         
                                //completeErase = (t * 100) / 45; //# progressbar

                                TextLine = "FPGA SPI Flash erasing progress";

                                percentComplete_FPGA_SPIFlashErase = (t * 100) / 45;

                                if (percentComplete_FPGA_SPIFlashErase > highestPercentageReached_FPGA_SPIFlashErase)
                                {
                                    highestPercentageReached_FPGA_SPIFlashErase = percentComplete_FPGA_SPIFlashErase;
                                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                }                        

                                // Wait 1 second
                                backgroundWakeEvent.WaitOne(1000);

                                //System.Threading.Thread.Sleep(1000); NO

                                //System.Threading.WaitHandle.WaitOne(1000);  NO

                                //# testing Busy Flag
                                Command[0] = (byte)FX2_Commands.CMD_FX2_READ_STATUS;
                                //# Command CMD_FX2_READ_STATUS

                                if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)
                                //fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
                                //printlog("ERROR: Can't call API function TE_USB_FX2_SendCommand")
                                //op.set("Error")    
                                {
                                    op_error = 5;
                                    StatusTextLine = "Error: it is not possible to erase the SPI Flash";
                                    if (bVerboseLogText == true)
                                        LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_READ_STATUS" + "\r\n";
                                    else
                                        LogTextLine += " STOP. " + "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_READ_STATUS" + "\r\n";
                                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    return false;
                                }

                                if ((Reply[2]) == 0)
                                {
                                    //SPI Flash erase completed succesfully
                                    erase_complete = 1;
                                    percentComplete_FPGA_SPIFlashErase = 100;
                                    highestPercentageReached_FPGA_SPIFlashErase = 100;
                                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    backgroundWakeEvent.WaitOne(1000);
                                    break;
                                }

                            }
                        }

                        TextLine = "FPGA SPI Flash writing progress";

                        //Stop the timer
                        stopWatchBulkErase.Stop();
                        TimeSpan ts = stopWatchBulkErase.Elapsed;

                        string elapsedTime = String.Format("(0:0000)", ts.Milliseconds);
                        int msTime = ts.Milliseconds;

                        string elapsedTime2 = String.Format("(0:00)", ts.Seconds);
                        int sTime = ts.Seconds;

                        string elapsedTime3 = String.Format("(0:00)", ts.Minutes);
                        int mTime = ts.Minutes;

                        if (bVerboseLogText == true)
                            LogTextLine += "INFO: SPI Flash Bulk erasing have taken " + mTime.ToString() + ":" + sTime.ToString() + ":" + msTime.ToString() + " m:s:ms" + "\r\n";

                        //# Check if the SPI Flash has been erased

                        //If SPI Flash erasing is failed
                        if (erase_complete == 0)
                        {
                            //Type of error = 6
                            op_error = 6;
                            
                            StatusTextLine = "ERROR: SPI Flash erase failed";
                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "ERROR: Flash busy after chip erase" + "\r\n";
                                LogTextLine += "Bulk Erasing: STOP" + "\r\n";
                                LogTextLine += "SPI Flash erasing: STOP" + "\r\n";
                            }
                            else
                                LogTextLine += " STOP. " + "ERROR: Flash busy after chip erase." + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            return false;

                        }
                        //If SPI Flash erasing is succesful
                        else
                        {
                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "Bulk erasing: STOP" + "\r\n";
                                LogTextLine += "SPI Flash erasing: STOP" + "\r\n";
                            }
                            else
                                LogTextLine += " STOP. SUCCESS: SPI Flash erased" + "\r\n";                               
                        }
                    }

                    //3rd step section C: write the .bit bitstream (selected .bit file or extracted from the selected .mcs file) in the SPI Flash
                    //# Write to flash
                    if (op_error == 0)  //# No errors in past
                    {
                        TextLine = "FPGA SPI Flash writing progress";
                        
                        percentComplete_FPGA_SPIFlashWrite = 0;
                        highestPercentageReached_FPGA_SPIFlashWrite = -1;
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);

                        int spi_addr = 0;			  //# From address 0
                        int wr_block_max_size = 59;	  //# maximum bytes to put in one write command
                        int wr_block_size = 0;        //# block size initialization
                        int wr_op_cnt = 0;			  //# count cycles for progressbar
                        int sector_rem = 0;           //# remainder

                        //Create and start a timer to measure the time necessary to write the SPI Flash
                        Stopwatch stopWatchSPIFlashWrite = new Stopwatch();
                        stopWatchSPIFlashWrite.Start();

                        if (bVerboseLogText == true)    
                            LogTextLine += "SPI Flash programming : START" + "\r\n";
                        else
                            LogTextLine += "SPI Flash programming : START.....";

                        //# cycle to the end of .bit bitstream
                        while ((spi_addr < fpga_bitstream_size))	                   
                        {
                            //printlog("Programming")
                            StatusTextLine = "SPI Flash programming";
                            //# Set command
                            Command[0] = (byte)FX2_Commands.CMD_FX2_FLASH_WRITE;
                            // Multiple of 59 
                            if ((fpga_bitstream_size - spi_addr) > wr_block_max_size)
                                wr_block_size = wr_block_max_size; //: # can write 59
                            else //# data remainder is less than 59
                                wr_block_size = ((int)fpga_bitstream_size) - spi_addr;
                            //# sector remainder
                            sector_rem = 0x0000ff - (spi_addr & 0x0000ff);   
                            if (sector_rem < wr_block_max_size && sector_rem != 0) // # cross 
                                wr_block_size = sector_rem + 1; //# write to the end of sector
                          
                            //chr Convert.ToChar(myAsciiValue);                           
                            Command[1] = (byte)(Convert.ToChar((spi_addr >> 16) & 0x00ff)); //# higgest part of addr
                            Command[2] = (byte)(Convert.ToChar((spi_addr >> 8) & 0x00ff));  //# high part of addr
                            Command[3] = (byte)(Convert.ToChar(spi_addr & 0x00ff));		    //# low part of addr
                            Command[4] = (byte)(Convert.ToChar(wr_block_size));			    // # size
                         
                            for (int wr_block_cnt = 0; wr_block_cnt < wr_block_size; wr_block_cnt++)   //# copy data
                            {
                                Command[5 + wr_block_cnt] = wr_fpga_bitsream[spi_addr + wr_block_cnt];                               
                            }

                            if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)
                            {                            
                                if (bVerboseLogText == true)
                                    LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_FLASH_WRITE" + "\r\n";
                                else
                                    LogTextLine += " STOP. " + "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_FLASH_WRITE" + "\r\n";
                                
                                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                                StatusTextLine = "Error: it is not possible to write the SPI FLASH";  //# Update operation label                    
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                op_error = 5;
                                return false;
                            }

                            for (int wr_block_cnt = 0; wr_block_cnt < wr_block_size; wr_block_cnt++)   //# copy data
                            {                              
                                if (Reply[wr_block_cnt] != Command[5 + wr_block_cnt])
                                {
                                    if (bVerboseLogText == true)
                                        LogTextLine += "ERROR: Write operation failed at SPI Flash address " + (spi_addr + wr_block_cnt).ToString() + "\r\n";
                                    else
                                        LogTextLine += " STOP. " + "ERROR: Write operation failed at SPI Flash address " + (spi_addr + wr_block_cnt).ToString() + "\r\n";
                                                
                                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                                    StatusTextLine = "Error: it is not possible to write the SPI Flash";
                                    worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                                    op_error = 5;
                                    return false;
                                }
                            }

                            percentComplete_FPGA_SPIFlashWrite = (spi_addr / (((int)fpga_bitstream_size) / 100));
                            if (percentComplete_FPGA_SPIFlashWrite > highestPercentageReached_FPGA_SPIFlashWrite)
                            {
                                highestPercentageReached_FPGA_SPIFlashWrite = percentComplete_FPGA_SPIFlashWrite;
                                //if (percentComplete_FPGA_SPIFlashWrite == 99)
                                //    percentComplete_FPGA_SPIFlashWrite = 100;
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                            }
                            spi_addr += wr_block_size;  // # update address
                            wr_op_cnt += 1;			    // # increment operation counter			
                        }

                        //This flag is asserted because the procedure to erase and write SPI Flash is ended
                        bFLASH_FPGA_AlreadyWritten = true;
                        //bFileFpga_Selected = true;

                        StatusTextLine = "SPI Flash programming completed";

                        //Stop timer
                        stopWatchSPIFlashWrite.Stop();
                        TimeSpan ts = stopWatchSPIFlashWrite.Elapsed;

                        string elapsedTime = String.Format("(0:0000)", ts.Milliseconds);
                        int msTime = ts.Milliseconds;

                        string elapsedTime2 = String.Format("(0:00)", ts.Seconds);
                        int sTime = ts.Seconds;

                        string elapsedTime3 = String.Format("(0:00)", ts.Minutes);
                        int mTime = ts.Minutes;

                        if (bVerboseLogText == true)
                        {
                            LogTextLine += "INFO: SPI Flash Write have taken " + mTime.ToString() + ":" + sTime.ToString() + ":" + msTime.ToString() + " m:s:ms" + "\r\n";
                            LogTextLine += "INFO: SUCCESS, SPI Flash programmed" + "\r\n";
                            LogTextLine += "SPI Flash programming : STOP" + "\r\n";                       
                        }

                        //LogTextLine += "SPI Flash Write: STOP" + "\r\n";
                        
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                        //LogTextLine += "bFLASH_FPGA_AlreadyWritten " + bFLASH_FPGA_AlreadyWritten.ToString() + "\r\n";
                        //LogTextLine += "bFileFpga_Selected " + bFileFpga_Selected.ToString() + "\r\n";
                        //LogTextLine += "percentComplete_FPGA_SPIFlashWrite " + percentComplete_FPGA_SPIFlashWrite.ToString() + "\r\n";
                    }

                }

                //4th step: the SPI Flash is used to program the FPGA (DONE PIN Check)
                if (bFileFpga_Selected && bFileFpga_PreProcessed && (bFLASH_FPGA_AlreadyWritten))
                {
                   
                    byte[] Command = new byte[64];
                    byte[] Reply = new byte[64];
                    int CmdLength = 64;
                    int ReplyLength = 64;               
                    uint timeout_ms = 1000;

                    //# No errors until now
                    if (op_error == 0)     
                    {
                        //FUNDAMENTAL: added to avoid DONE PIN Check failure even when FPGA configuration should be successful
                        backgroundWakeEvent.WaitOne(1500);
                        
                        if (bVerboseLogText == true)
                            LogTextLine += "INFO: FPGA's power cycle; power off followed by a power on. The FPGA is turned off and on." + "\r\n";                      
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);

                        Command[0] = (byte)FX2_Commands.CMD_FX2_POWER_ON;   //# Power ON FPGA
                        Command[1] = (byte)(Convert.ToChar(1)); //chr(1)    //# 1 = Turn ON
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)                     
                        {               
                            if (bVerboseLogText == true)
                                LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand " + "CMD_FX2_POWER_ON" + "\r\n";
                            else
                                LogTextLine += " STOP. " + "ERROR: it is not possible to power off and the power on the FPGA" + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                            StatusTextLine = "ERROR: it is not possible to power on the FPGA";
                            op_error = 5;
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            return false;
                        }
                    }

                    //# No errors until now
                    if (op_error == 0)   
                    {
                        StatusTextLine = "Power On: Done pin checking";
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);

                        //# Wait for boot: during this sleep time the FPGA is configured by the content (.bit file with header stripped or .mcs file converted in a .bit bitstream) of thirSPI Flash.
                        //time.sleep(6)
                        //FUNDAMENTAL: the value is 6 seconds instead of 1 or 2 added to avoid problem with some computer configuration (DONE PIN Check failure even when the FPGA should be correcly configured by SPI Flash content; if the TE module is turned off and then on it work correctly)
                        backgroundWakeEvent.WaitOne(6000);
                          
                        if (bVerboseLogText == true)
                            LogTextLine += "Checking DONE pin: START" + "\r\n";
                        
                        //Read DONE PIN Status: DONE PIN status can be found in Reply[4]
                        Command[0] = (byte)FX2_Commands.CMD_FX2_READ_STATUS;   //# READ STATUS
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)                          
                        {             
                            if (bVerboseLogText == true)
                                LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand + CMD_FX2_READ_STATUS" + "\r\n";
                            else
                                LogTextLine += " STOP. " + "ERROR: it is not possible to check the DONE pin" + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                
                            StatusTextLine = "ERROR: it is not possible to check the DONE pin";
                            //Error Type = 5
                            op_error = 5;
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            
                            return false;
                        }
                        //# Check DONE PIN Status
                        else if (Reply[4] == 0) //"ERROR: DONE pin is not High")   
                        {                                         
                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "WARNING: DONE pin is not High" + "\r\n";
                                LogTextLine += "WARNING: SPI Flash Programming End Status is Uncertain" + "\r\n";
                            }
                            else
                            {
                                LogTextLine += " STOP. " + "WARNING: SPI Flash programmed succesfully but DONE pin is not High" + "\r\n";
                                LogTextLine += "WARNING: SPI Flash programmed succesfully but the FPGA seems to have uncorrectly readback the bitstream stored inside SPI Flash." + "\r\n" + 
                                               "INFO: A power off/on cycle of the TE USB FX2 module is advised. After this, the TE USB FX2 module should start correctly. Otherwise (but unlikely), you should rewrite the SPI Flash." + "\r\n" +
                                               "INFO: If the power off/on cycle doesn't work, the *.bit or *.mcs file selected could be wrong; they could be prepared for a different FPGA device." + "\r\n";
                            }
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                            StatusTextLine = "WARNING: SPI Flash programmed succesfully but DONE pin is not High";
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            return false;
                        }

                        else //INFO: DONE pin is High (SUCCESS: SPI Flash has been correctly readback by FPGA)
                        {
                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "INFO: DONE pin is High (SUCCESS: SPI Flash has been correctly readback by FPGA)" + "\r\n";
                                //LogTextLine += "DONE" + "\r\n";
                                LogTextLine += "INFO: SUCCESS, SPI Flash has been correctly Programmed and Readback by FPGA" + "\r\n";
                                LogTextLine += "Checking DONE pin: STOP" + "\r\n";                   
                            }
                            else
                            {
                                LogTextLine += " STOP. " + "SUCCESS: SPI Flash programmed." + "\r\n";
                                LogTextLine += "INFO: DONE pin is High (SUCCESS: SPI Flash has been correctly readback by FPGA)." + "\r\n";
                            }
                            
                            // # Update operation label                            
                            StatusTextLine = "SUCCESS: SPI Flash programmed and checked using DONE pin.";

                            //complete = 100 # Show it 100%
                            percentComplete_FPGA_SPIFlashWrite = 100;
                            highestPercentageReached_FPGA_SPIFlashWrite = percentComplete_FPGA_SPIFlashWrite;                        
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                        }                        
                        //bFLASH_FPGA_AlreadyWritten = true;
                    }

                    
                    //# No errors in past
                    if (op_error == 0)   
                    {     
                        //sleep for 1.5 seconds
                        backgroundWakeEvent.WaitOne(1500);
                        
                        if (bVerboseLogText == true)
                            LogTextLine += "INFO: FPGA's power cycle: power off followed by a power on. The FPGA is turned off and on." + "\r\n";
                        
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);

                        //Power On FPGA: "FPGA's power cycle: power off followed by a power on"
                        Command[0] = (byte)FX2_Commands.CMD_FX2_POWER_ON;   //# Power ON FPGA
                        Command[1] = (byte)(Convert.ToChar(1)); //chr(1)    //# 1 = Turn ON
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, timeout_ms) == false)                        
                        {
                            //Error type = 5
                            op_error = 5;
                            if (bVerboseLogText == true)
                                LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand + CMD_FX2_POWER_ON" + "\r\n";
                            StatusTextLine = "Error: it is not possible to power on the FPGA";                            
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                        }


                        file_type = 0;
                        //other zeroing if necessary                                      
                    }

                    //If there is no error until now, retrieve FPGA Version
                    if (op_error == 0)   
                    {
                        //This sleep time of 5 second is fundamental
                        backgroundWakeEvent.WaitOne(5000);
                        
                        //TE_USB_FX2_USBDevice = USBdevList[0] as CyUSBDevice;
                        //Form1.GetFPGAversion(TE_USB_FX2_USBDevice);

                        //byte Command[64], Reply[64];
                        //Command = new byte[64];
                        //Reply = new byte[64];
                        CmdLength = 64;
                        ReplyLength = 64;

                        uint TIMEOUT_MS = 1000;

                        //Set an interrupt
                        Command[0] = (byte)FX2_Commands.CMD_FX2_SET_INTERRUPT;
                        Command[1] = (byte)FX2_Parameters.MB_I2C_ADDRESS; //byte MB_I2C_ADRESS = 0x3f; MICROBLAZE's I2C ADDRESS
                        Command[2] = (byte)FX2_Parameters.I2C_BYTES_SIZE; //byte I2C_BYTES_SIZE = 12;
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
                        {                          
                            if (bVerboseLogText == true)
                                LogTextLine += "WARNING: Can't call API function TE_USB_FX2_SendCommand + SET_INTERRPUPT" + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";                            
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                            return true;
                        }
                         
                        if (bVerboseLogText == true)
                            LogTextLine += "Checking the use of Trenz Electronic Reference Design :START" + "\r\n";

                        //Write I2C at MICROBLAZE's I2C ADDRESS (MB_I2C_ADRESS) the MicroBalze Command Get Version of reference project running on FPGA (MB_Commands.FX22MB_REG0_GETVERSION)
                        Command[0] = (byte)FX2_Commands.CMD_FX2_I2C_WRITE; //0xAD;//comand CMD_FX2_I2C_WRITE
                        //Command[1] = (byte)FX2_Commands.MB_I2C_ADDRESS;
                        //Command[2] = (byte)FX2_Commands.I2C_BYTES_SIZE;
                        Command[3] = (byte)0;
                        Command[4] = (byte)0;
                        Command[5] = (byte)0;
                        Command[6] = (byte)MB_Commands.FX22MB_REG0_GETVERSION;//1; //get version of reference project running on FPGA
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == false)
                        {
                            //Instead of reference project a custom project or blinking led project run on TE USB FX2 module
                            SystemTypeFPGAFlash.Text = "No, Custom project not based on TE Reference Architecture";
                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "WARNING: Can't call API function TE_USB_FX2_SendCommand + FX22MB_REG0_GETVERSION" + "\r\n";
                                //StatusTextLine = "Warning: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashErase);
                            }
                            return true;
                        }
                        //else a reference project is running on the FPGA of TE USB FX2 module

                        // Retrieve the desired data and clear interrupt data register
                        Command[0] = (byte)FX2_Commands.CMD_FX2_GET_INTERRUPT; //0xB1;
                        if (TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command, ref CmdLength, ref Reply, ref ReplyLength, TIMEOUT_MS) == true)
                        {
                            //If the FPGA is running a reference project (Microblaze based)
                            if ((ReplyLength > 4) && (Reply[0] != 0))
                            {                          
                                SystemTypeFPGAFlash_Text = "Yes";
                                if (bVerboseLogText == true)
                                {
                                    LogTextLine += "INFO: Trenz Electronic Reference Design based on MicroBlaze soft processor is used" + "\r\n";
                                    LogTextLine += "INFO: Major version: " + Reply[1].ToString() + "\r\n";
                                    LogTextLine += "INFO: Minor version: " + Reply[2].ToString() + "\r\n";
                                    LogTextLine += "INFO: Release version: " + Reply[3].ToString() + "\r\n";
                                    LogTextLine += "INFO: Build version: " + Reply[4].ToString() + "\r\n";
                                }
                               
                                LatestMajorVersionFPGA_String = Reply[1].ToString();
                                LatestMinorVersionFPGA_String = Reply[2].ToString();
                                LatestReleaseVersionFPGA_String = Reply[3].ToString();
                                LatestBuildVersionFPGA_String = Reply[4].ToString();
                                // Show the retrieved data in the GUI
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);                               
                            }
                            else
                            {
                                //If the FPGA is running a custom project (Microblaze based or not), for example ledblink
                                SystemTypeFPGAFlash_Text = "No, Custom project not based on TE Reference Architecture";
                                if (bVerboseLogText == true)
                                {
                                    LogTextLine += "INFO: Custom project not based on TE Reference Architecture" + "\r\n";                            
                                }
                                LatestMajorVersionFPGA_String = "NO";
                                LatestMinorVersionFPGA_String = "NO";
                                LatestReleaseVersionFPGA_String = "NO";
                                LatestBuildVersionFPGA_String = "NO";
                                // Show (in the GUI) that there is no data to retrieve
                                worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                            }
                        }
                        else
                        {
                            //If the FPGA is running a custom project (Microblaze based or not), for example ledblink
                            SystemTypeFPGAFlash_Text = "No, Custom project not based on TE Reference Architecture";
                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "INFO: Custom project not based on TE Reference Architecture" + "\r\n";
                                LogTextLine += "WARNING: Can't call API function TE_USB_FX2_SendCommand + FX22MB_REG0_GETVERSION" + "\r\n";
                                //StatusTextLine = "Warning: it is not possible to retrieve version of Trenz Electronic System flashed on the FPGA; maybe you have flashed a Custom Client System not based on Trenz Electronic FPGA";
                            }
                            LatestMajorVersionFPGA_String = "NO";
                            LatestMinorVersionFPGA_String = "NO";
                            LatestReleaseVersionFPGA_String = "NO";
                            LatestBuildVersionFPGA_String = "NO";
                            // Show (in the GUI) that there is no data to retrieve
                            worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                        }
                        // End or reference project checking
                        if (bVerboseLogText == true)
                        {
                            LogTextLine += "Checking the use of Trenz Electronic Reference Design :STOP" + "\r\n";
                     
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                            LogTextLine += "SPI Flash Programming operation: it also changes Project running on FPGA" + "\r\n";
                            LogTextLine += "The operation STOPS " + (DateTime.Today).ToString() + "\r\n";
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                        }
                        else
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                        worker.ReportProgress(percentComplete_FPGA_SPIFlashWrite);
                    }
                }

                //zeroing file type
                file_type = 0;
                //bFileFpga_Selected = false;
                //other zeroing

                return bresult_SPI_Flash_Programming;

            }

        }

        //TO DO, a more robust event handler should be used if the filepath is manually inserted instead of dialog box
        //Event handler (callback function) for text changes of full file path of file .bit or .mcs bitstream (Fpga/Flash)
        private void textBox_FPGA_Bitstream_File_Path_TextChanged(object sender, EventArgs e)
        {
            FPGAFile_FilePath = textBox_FPGA_Bitstream_File_Path.Text;
            //LogTextLine = String.Empty;
            StatusTextLine = String.Empty;

            bFileFpga_Selected = true;

            //bMCSXilinxFlash = false;
        }

        /* BW1_FPGA_SPIFlash background worker STOP */

        /* BW2_CypressUSB_I2CEEPROM bacground worker START */

        //Event handler (callback function) for Click of button "Start IIC programming"
        private void button_ProgrUSBStart_Click(object sender, EventArgs e)
        {
            
            UInt16 PID = 0;
            UInt16 VID = 0;

            //Reset of flags
            bFileFpga_Selected = false;
            bFileFpga_PreProcessed = false;
            bFLASH_FPGA_AlreadyWritten = false;

            bMCSXilinxFlash = false;
            bEEPROMSwitchSetToOn = false;

            // Disable the Start button until 
            // the asynchronous operation is done.
            button_ProgFpgaStart.Enabled = false;
            button_ProgFpgaFilePathSelection.Enabled = false;
            button_ProgrUSBStart.Enabled = false;
            button_ProgUSBFilePathSelection.Enabled = false;
            button_RefereshInformation.Enabled = false;
            checkBox_VerboseLogText.Enabled = false;
            checkBox_Retrieve_Flash_ID.Enabled = false;
            checkBox_ClearLogText4everyProgrammingOperation.Enabled = false;

            //If file path is not null, a .iic file is cosidered selected.
            if (USBFile_FilePath.Length > 0)
                bFileUSB_Selected = true;

            //Create a list of Cypress or Trenz Electronic device
            USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);

            //If at least a Cypress or Trenz Electronic device is attached
            if (USBdevList.Count != 0)
            {
                //cast the first device
                CyFX2Device fx2test = USBdevList[0] as CyFX2Device;
                PID = fx2test.ProductID;
                VID = fx2test.VendorID;
                //If more than one Cypress or Trenz Electronic device is used you should remove all device except one
                if (USBdevList.Count > 1)
                {
                    MessageBox.Show("In this version the program must be used with a single Trenz Electronic or Cypress module attached. You must remove Cypress and or Trenz Electronic micromodule(s) until only one module remains. This must be the module that you desire to write.");
                }
                FirmwareTypeUSB_Text = "No Cypress, Trenz Elecrtonic or DEWESoft device, maybe you are using an corrupted Firmware.";
                DriverType_TextBox_Text = "Undefined";
            }

            //If a Cypress device is attached a recovery procedure (boot) should be used
            if ((PID == PIDCypress) && (VID == VIDCypress) && (!bEEPROMSwitchSetToOn))
            {
                FirmwareTypeUSB_Text = "Cypress used for Recovery Procedure";
                DriverType_TextBox_Text = "Cypress USB Generic Driver";
                //MessageBox.Show("The VID and PID used are for normal Cypress device; you must move the EEPROM switch to ON (if the switch is not already ON) and click again the 'Program USB' button");

                PID_String = "0x8613";
                VID_String = "0x04B4";

                //A pop-up appears to remember to the user that EEPROM switch should be
                DialogResult rc = MessageBox.Show("The VID and PID used are for normal Cypress device; this is correct for a recovery procedure. For the current 3 TE USB FX2 modules the EEPROM switch is ENABLED when set to ON. You must move the EEPROM switch to ON (if the switch is not already ON) and click 'YES' button if you desire to continue with the Recovery Procedure", "EEPROM switch/check to ON", MessageBoxButtons.YesNo);
                if (rc == DialogResult.Yes)
                {
                    //Recovery boot is accepted by the user

                    //The Background worker BW2 (used to program the EEPROM with iic firmware file) starts 
                    InitializeAndStartBW2_CypressUSB_I2CEEPROM();

                    //Unlike Digilent Board it is not possible to test if EEPROM switch has been actually set to ON.
                    //This is a "fake" bool flag set to true when user confirms in a MessageBox that 
                    //the EEPROM switch has been set to ON.
                    bEEPROMSwitchSetToOn = true;
                }
                else if (rc == DialogResult.No)
                {
                    //Recovery boot is refused by the user

                    //Unlike Digilent Board it is not possible to test if EEPROM switch has been actually set to ON.
                    //This is a "fake" bool flag set to true when user confirms in a MessageBox that 
                    //the EEPROM switch has been set to ON.
                    bEEPROMSwitchSetToOn = false;
                }
                /*
                if (rc == DialogResult.Cancel)
                {
                    //e.Cancel = true;
                    bEEPROMSwitchSetToOn = false;
                }
                */

                //bEEPROMSwitchSetToOn = true;             
            }

            //If a TE device is attached it is possible to start the Background Worker BW2 to program the EEPROM with selected .iic firmware file
            else if (((PID == PIDTrenzElectronic) && (VID == VIDTrenzElectronic)) || bEEPROMSwitchSetToOn)
            {
                //The Background worker BW2 (used to program the EEPROM with iic firmware file) starts 
                InitializeAndStartBW2_CypressUSB_I2CEEPROM();        
            }
            //If is not a Cypress or TE device
            else
            {       
                MessageBox.Show("Error, wrong VID and PID. Move the EEPROM switch to OFF and use a power on/ power off cycle. After this, you will probably receive another message to move the EEPROM switch. After this, click again the USB Start Program Button and this time should run.");
                FirmwareTypeUSB_Text = "No Cypress, Trenz Elecrtonic or DEWESoft device, maybe you are using an corrupted Firmware.";
                DriverType_TextBox_Text = "Undefined"; 
            }
        }

        //This event handler (callback function) deals with button click event of button 
        //'Select *.iic file' aka button_ProgUSBFilePathSelection
        private void button_ProgUSBFilePathSelection_Click(object sender, EventArgs e)
        {
            Stream myStreamUSBI2CEEPROM = null;
            OpenFileDialog openFileDialog2 = new OpenFileDialog();

            openFileDialog2.Title = "Select file to download . . .";
            openFileDialog2.InitialDirectory = "c:\\";
            openFileDialog2.DefaultExt = "I2C (*.iic)";
            openFileDialog2.Filter = "I2C (*.iic) | *.iic";

            openFileDialog2.FilterIndex = 1;
            openFileDialog2.RestoreDirectory = true;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    USBFile_FilePath = openFileDialog2.FileName;

                    textBox_USB_Firmware_IIC_File_Path.Text = USBFile_FilePath;                  

                    if ((myStreamUSBI2CEEPROM = openFileDialog2.OpenFile()) != null)
                    {
                        //using (FileStream fpga_bitstream = new FileStream(FPGAFile_FilePath, FileMode.Open))

                        using (myStreamUSBI2CEEPROM)
                        {

                            usb_bitstream_size = (int)myStreamUSBI2CEEPROM.Length;
                            // aka fpga_bitstream.Length;
                            //USING, maybe is better
                            BinaryReader usb_bitstream = new BinaryReader(myStreamUSBI2CEEPROM);
                            byte[] wr_usb_bitstream2 = new byte[usb_bitstream_size];
                            wr_usb_bitstream2 = usb_bitstream.ReadBytes(usb_bitstream_size);
                            for (int i = 0; i < usb_bitstream_size; i++)
                                wr_usb_bitstream[i] = wr_usb_bitstream2[i];

                            bFileUSB_Selected = true;
                        }

                        myStreamUSBI2CEEPROM.Close();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }

            }
            

        }

        private void InitializeAndStartBW2_CypressUSB_I2CEEPROM()
        {
            FirmwareTypeUSB_Text = "TE FX2 Firmware Gen 3";
            DriverType_TextBox_Text = "Trenz Electronic USB FX2 Device Driver";
            BW2_CypressUSB_I2CEEPROM.DoWork += new DoWorkEventHandler(BW2_CypressUSB_I2CEEPROM_DoWork);
            BW2_CypressUSB_I2CEEPROM.ProgressChanged += new ProgressChangedEventHandler(BW2_CypressUSB_I2CEEPROM_ProgressChanged);
            BW2_CypressUSB_I2CEEPROM.WorkerReportsProgress = true;

            BW2_CypressUSB_I2CEEPROM.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted);
            //backgroundWorker1.RunWorkerAsync();

            BW2_CypressUSB_I2CEEPROM.WorkerSupportsCancellation = true;

            progressBar2.Value = (int)0;
            progressBar2.Enabled = true;
            //highestPercentageReached_USB_EEPROMWrite = 0;


            // Disable the Start button until 
            // the asynchronous operation is done.
            button_ProgFpgaStart.Enabled = false;
            button_ProgFpgaFilePathSelection.Enabled = false;
            button_ProgrUSBStart.Enabled = false;
            button_ProgUSBFilePathSelection.Enabled = false;

            textBox_USB_Firmware_IIC_File_Path.ReadOnly = true;

            // Start the asynchronous operation.
            BW2_CypressUSB_I2CEEPROM.RunWorkerAsync();
        }

        private void BW2_CypressUSB_I2CEEPROM_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            //BackgroundWorker worker = new BackgroundWorker();
            //worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            //worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            //worker.RunWorkerAsync(dumpDate);


            // Assign the result of the computation
            // to the Result property of the DoWorkEventArgs
            // object. This is will be available to the 
            // RunWorkerCompleted eventhandler.
            /*e.Result = ComputeFibonacci((int)e.Argument, worker, e);*/

            e.Result = I2CEEPROM_Programming(worker, e);


        }

        private void BW2_CypressUSB_I2CEEPROM_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
            //progressBar3.Value = e.ProgressPercentage;
            //resultLabel.Text = TextLine;
            //label3.Text = TextLine;
            textBox_LogText.Text = LogTextLine;
            textBox_LogText.Update();
            //MessageBox.Show(textBox3.GetLineFromCharIndex(textBox3.SelectionStart()).ToString());
            LogTextScrollDown();
            /*
            textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
            textBox_LogText.ScrollToCaret();
            textBox_LogText.Refresh();
            */
            toolStripStatusLabel1.Text = StatusTextLine;

            textBox_VID.Text = VID_String;
            textBox_PID.Text = PID_String;

            LatestMajorVersionFW_textBox.Text = LatestMajorVersionFW_String;
            LatestMinorVersionFW_textBox.Text = LatestMinorVersionFW_String;

            FirmwareTypeUSB.Text = FirmwareTypeUSB_Text;
            DriverType_TextBox.Text = DriverType_TextBox_Text;



        }

        private void BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            // First, handle the case where an exception was thrown.

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted);
                worker.DoWork -= new DoWorkEventHandler(BW2_CypressUSB_I2CEEPROM_DoWork);
                worker.Dispose();

            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                //resultLabel.Text = "Canceled";
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted);
                worker.DoWork -= new DoWorkEventHandler(BW2_CypressUSB_I2CEEPROM_DoWork);
                worker.Dispose();

            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                //resultLabel2.Text = e.Result.ToString();
                RefreshInformationUIThread();

                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(BW2_CypressUSB_I2CEEPROM_RunWorkerCompleted);
                worker.DoWork -= new DoWorkEventHandler(BW2_CypressUSB_I2CEEPROM_DoWork);
                worker.Dispose();
            }

            // Enable the Start button.
            button_ProgFpgaStart.Enabled = true;
            button_ProgrUSBStart.Enabled = true;
            button_ProgFpgaFilePathSelection.Enabled = true;
            button_ProgUSBFilePathSelection.Enabled = true;

           
            button_RefereshInformation.Enabled = true;
            checkBox_VerboseLogText.Enabled = true;
            checkBox_Retrieve_Flash_ID.Enabled = true;
            checkBox_ClearLogText4everyProgrammingOperation.Enabled = true;

            textBox_USB_Firmware_IIC_File_Path.ReadOnly = false;
            // Disable the Cancel button.
            //cancelAsyncButton.Enabled = false;

        }

        /******Cypress USB EEPROM Programming****/
        //# USB Lg EEPROM programming 
        //This is the method that does the actual work.
        bool I2CEEPROM_Programming(BackgroundWorker worker, DoWorkEventArgs e)
        {
            // USBDeviceList USBdevList,
            // ref CyUSBDevice TE_USB_FX2_USBDevice
            // The parameter n must be >= 0 and <= 91.
            // Fib(n), with n > 91, overflows a long.  

            if (bClearLogTextBeforeEveryProgrammingOperation == true)
                LogTextLine = String.Empty;

            if (USBdevList == null)
            {
                throw new ArgumentException(
                    "At least one TE USB FX2 module must be attached");
            }

            bool result = false;
            bool opened = false;

            // Abort the operation if the user has canceled.
            // Note that a call to CancelAsync may have set 
            // CancellationPending to true just after the
            // last invocation of this method exits, so this 
            // code will not have the opportunity to set the 
            // DoWorkEventArgs.Cancel flag to true. This means
            // that RunWorkerCompletedEventArgs.Cancelled will
            // not be set to true in your RunWorkerCompleted
            // event handler. This is a race condition.

            if (worker.CancellationPending)
            {
                //TextLine = "line 1063";
                e.Cancel = true;
                return result;
            }
            else if (bFileUSB_Selected)
            {
                //TextLine = "line 1069";
                //timeout_ms = c_ulong(1000)		  # Timeout 1s
                //CardNumber = c_int(0)			   # Card Number 0
                //DriverBufferSize = c_int(132072)	# Driver Buffer Size 132072
                uint timeout_ms = 1000;
                //int CardNumber = 0;
                //int DriverBufferSizeWrite = 131072;
                //int DriverBufferSizeRead = 131072;

                byte[] Command = new byte[64];
                byte[] Reply = new byte[64];
                int CmdLength = 64;
                int ReplyLength = 64;

                UInt16 PID = 0;
                UInt16 VID = 0;

                op_error = 0;

                //Stopwatch stopWatch = new Stopwatch();
                //stopWatch.Start();
                //printlog("Programming EEPROM")
                //fpga_bitstream_size = len(usb_bitstream)			 # Calculate size
                //printlog("Firmware size " + str(fpga_bitstream_size))
                if (bVerboseLogText == true)
                {
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "Programming FX2 microcontroller EEPROM and RAM using I2C" + "\r\n";
                    LogTextLine += "INFO: The operation STARTS " + (DateTime.Today).ToString() + "\r\n";
                    LogTextLine += "INFO: OpenFutNET version running on the host: " + OpenFutNETversion  + "\r\n";
                    LogTextLine += "INFO: Operating system of the host:" + GetOSFriendlyName() + "\r\n";
                    LogTextLine += "INFO: .NET version running on the host: " + Environment.Version.ToString() + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                }
                
                if (bVerboseLogText == false)
                    //LogTextLine += "\r\n" + "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                    LogTextLine += "FX2 microcontroller EEPROM programming: START.....";
                else
                    LogTextLine += "FX2 microcontroller EEPROM programming: START" + "\r\n";

                if (bVerboseLogText == true)
                    LogTextLine += "Firmware size " + usb_bitstream_size.ToString() + " bytes" + "\r\n";

                if (USBFile_FilePath.Length > 0)
                    bFileUSB_Selected = true;

                USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);

                if (USBdevList.Count != 0)
                {
                    fx2 = USBdevList[0] as CyFX2Device;
                    PID = fx2.ProductID;
                    VID = fx2.VendorID;
                    if (USBdevList.Count > 1)
                    {
                        MessageBox.Show("In this version the program must be used with a single Trenz Electronic or Cypress module attached. You must remove Cypress and/or Trenz Electronic micromodule(s) until only one module remains. This must be the module that you desire to write.");
                    }
                }

                if (((USBdevList.Count == 1) && (PID == PIDCypress) && (VID == VIDCypress) && bEEPROMSwitchSetToOn) || (((USBdevList.Count == 1) && (PID == PIDTrenzElectronic) && (VID == VIDTrenzElectronic))))
                {
                    if (bVerboseLogText == true)
                    {
                        LogTextLine += "INFO: VID and PID identify a Cypress Device: therefore EEPROM will be programmed using Cypress DLL 'CyUSB.dll'" + "\r\n";
                        LogTextLine += "INFO: Found " + (USBdevList.Count).ToString() + " card(s)" + "\r\n";
                        LogTextLine += "INFO: Connected to card 1" + "\r\n";
                    }

                    if ((USBdevList.Count == 1) && (PID == PIDCypress) && (VID == VIDCypress) && bEEPROMSwitchSetToOn)
                    {
                        PID_String = "0x8613";
                        VID_String = "0x04B4";
                    }
                    else
                    {
                        //PID and VID of Trenz Electronic device     
                        PID_String = "0x0300";
                        VID_String = "0x0BD0";         
                    }

                    opened = true;
 
                    if ((op_error == 0) && (opened == true))  //# No errors in past
                    {
                        StatusTextLine = "Programming FX2 microcontroller EEPROM";
                        if (bVerboseLogText == true)
                            LogTextLine += "Writing firmware to EEPROM: START" + "\r\n";
                        bResultLoadEEPROM = fx2.LoadEEPROM(USBFile_FilePath, true);

                        worker.ReportProgress(percentComplete_USB_EEPROMWrite);
                        if (bResultLoadEEPROM == false)
                        {
                            StatusTextLine = "Error: it is not possible to write EEPROM.";

                            if (bVerboseLogText == true)
                            {
                                LogTextLine += "ERROR: Can't call API function TE_USB_FX2_SendCommand + CMD_FX2_EEPROM_WRITE" + "\r\n";
                                LogTextLine += "INFO/WARNING: probably, you should move the EEPROM switch to ON" + "\r\n";
                            }
                            else
                                LogTextLine += " STOP. " + "ERROR: it is not possible to write the EEPROM. Probably, you should move the EEPROM switch to ON" + "\r\n";
                            op_error = 5;
                            LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                            worker.ReportProgress(percentComplete_USB_EEPROMWrite);                          
                            return false;
                        }
                        else
                        {
                            backgroundWakeEvent.WaitOne(750);
                            percentComplete_USB_EEPROMWrite = 100;
                            backgroundWakeEvent.WaitOne(750);
                            if (bVerboseLogText == true)
                                LogTextLine += "Writing firmware to EEPROM: STOP" + "\r\n";
                            else
                                LogTextLine += " STOP. " + "SUCCESS: FX2 microcontroller EEPROM programmed." + "\r\n";
                            StatusTextLine = "SUCCESS: FX2 microcontroller EEPROM programmed.";
                            worker.ReportProgress(percentComplete_USB_EEPROMWrite);
                            //break;
                        }
                    }
                }

               
            }             
            else          
            {
                StatusTextLine = "Error: file not selected";
                if (bVerboseLogText == true)
                    LogTextLine += "ERROR: File not selected" + "\r\n";
                op_error = 6;
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                return false;
                //op.set("Error")
                //printlog("ERROR: File not selected")          
            }

              
            if ((op_error == 0) || (bResultLoadEEPROM))           
            {
            
                //backgroundWakeEvent.WaitOne(5000);
                    
                if (bVerboseLogText == true)
                    LogTextLine += "FX2 microcontroller RAM Programming: START" + "\r\n";
                else
                    LogTextLine += "FX2 microcontroller RAM Programming: START.....";

                bResultLoadExternalRam = fx2.LoadExternalRam(USBFile_FilePath);
                    
                if (bResultLoadExternalRam == true)
                {
                    StatusTextLine = "SUCCESS: FX2 microcontroller EEPROM and RAM programmed";
                        
                    if (bVerboseLogText == true)
                    {                        
                        LogTextLine += "FX2 microcontroller RAM Programming: STOP" + "\r\n";
                        LogTextLine += "INFO: Programming FX2 microcontroller RAM with TE FX2 firmware v3.02 it is equivalent to remove and insert the module." + "\r\n";
                        LogTextLine += "INFO: Programming FX2 microcontroller RAM with TE FX2 firmware version earlier than v3.02 it is NOT equivalent to remove and inssert the module:" +"\r\n" 
                                        +"in this case you should click the button 'Refresh information about FPGA and FX2 microcontroller' to obtain the last value of firmware version" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n";
                        LogTextLine += "Programming EEPROM and RAM of FX2 microcontroller using I2C" + "\r\n";
                        LogTextLine += "INFO: The operation STOPS " + (DateTime.Today).ToString() + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";                
                    }
                    else
                    {
                        LogTextLine += " STOP. " + "SUCCESS: FX2 microcontroller RAM programmed" + "\r\n";
                        LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                        //LogTextLine += "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                        
                    }
                }
                else
                {
                    StatusTextLine = "Warning: RAM has not been written";
                    if (bVerboseLogText == true)
                        LogTextLine += "Warning: RAM has not been written" + "\r\n";
                    else
                        LogTextLine += " STOP. " + "Warning: it is not possible to write the RAM. You should power off/on cycle the TE USB FX2 module to load the RAM with EEPROM firmware." + "\r\n";
                    LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                    worker.ReportProgress(percentComplete_USB_EEPROMWrite);           
                }
         
                if (USBdevList != null)
                {
                        USBdevList.DeviceRemoved -= USBdevList_DeviceRemoved;
                        USBdevList.DeviceAttached -= USBdevList_DeviceAttached;
                        USBdevList.Dispose();           
                }

                //backgroundWakeEvent.WaitOne(6000);
                backgroundWakeEvent.WaitOne(500);
                USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
                USBdevList.DeviceAttached += new EventHandler(USBdevList_DeviceAttached);
                USBdevList.DeviceRemoved += new EventHandler(USBdevList_DeviceRemoved);
                TE_USB_FX2_USBDevice = USBdevList[0] as CyUSBDevice;
                //FX2MicronctrollerRamProgrammed = true;
                
                backgroundWakeEvent.WaitOne(500);

                //IF Trenz Electronic, non Cypress o DEWESoft
                /*
                while (bResultLoadExternalRam == true  ) 
                {
                    textBox_PID.Text = PID_String;
                    textBox_VID.Text = VID_String;



                    backgroundWakeEvent.WaitOne(1000);
                    Command1[0] = (byte)FX2_Commands.CMD_FX2_READ_VERSION;
                    //bSendCommand = false;
                    //if TE_USB_FX2_USBDevice.
                    bSendCommand = TE_USB_FX2.TE_USB_FX2.TE_USB_FX2_SendCommand(ref TE_USB_FX2_USBDevice, ref Command1, ref CmdLength1, ref Reply1, ref ReplyLength1, TIMEOUT_MS);
                    backgroundWakeEvent.WaitOne(1000);
                    if ( bSendCommand )
                    {                  
                        if (ReplyLength1 >= 4)             
                        {
                            bResultLoadExternalRam = false;
                            break;
                        }                 
                        else                    
                        {                  
                    
                        }     
                    }
                }
                */
                       
                worker.ReportProgress(percentComplete_USB_EEPROMWrite);
                backgroundWakeEvent.WaitOne(1000);
                worker.ReportProgress(percentComplete_USB_EEPROMWrite);                 
                backgroundWakeEvent.WaitOne(1000);                

                //LogTextLine += "Programming RAM of FX2 microcontroller: STOP" + "\r\n";
                worker.ReportProgress(percentComplete_USB_EEPROMWrite);

                return bResultLoadEEPROM;
                       
            }
            else
            {
                StatusTextLine = "Error: EEPROM has not been written";               
                if (bVerboseLogText == true)
                    LogTextLine += "ERROR: EEPROM has not been written" + "\r\n";
                else
                    LogTextLine += " STOP. " + "ERROR: it is not possible to write the EEPROM." + "\r\n";
                LogTextLine += "-----------------------------------------------------------------------------------------------------------------" + "\r\n" + "\r\n";
                worker.ReportProgress(percentComplete_USB_EEPROMWrite);
                result = false;
                return result;
            }    

            //return result;
        }

        private void textBox_USB_Firmware_IIC_File_Path_TextChanged(object sender, EventArgs e)
        {
            USBFile_FilePath = textBox_USB_Firmware_IIC_File_Path.Text;
            //LogTextLine = String.Empty;
            StatusTextLine = String.Empty;

            bFileUSB_Selected = true;
        }

        /* BW2_CypressUSB_I2CEEPROM bacground worker STOP */
                       
        
        /*Help Button : START */

        private void Form1_HelpButtonClicked(Object sender, CancelEventArgs e)
        {
            //TO DO
            /*
            System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "Cancel", e.Cancel);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "HelpButtonClicked Event");
             * */

        }

        private void button_ShowHelpHtml_Click(object sender, EventArgs e)
        {
            //var localPath = Server.MapPath("/OpenFutNet/help1.html");
            //string host = HttpContext.Current.Request.Url.Host;

            //https://wiki.trenz-electronic.de/display/TEUSB/OpenFutNet

            String test_String= "https://wiki.trenz-electronic.de/display/TEUSB/OpenFutNet";

            System.Windows.Forms.Help.ShowHelp(button_ShowHelpHtml, test_String);

            /*
            Uri test = GetAbsoluteUrlForLocalFile("help.txt");
            //"../Debug/Help1.html"
            String test_String = test.ToString();

            test_String = test_String.Replace("file:///", "");

            test_String = test_String.Replace("/", "\\");

            System.Windows.Forms.Help.ShowHelp(button_ShowHelpHtml, test_String);
             */ 

        }

        private static Uri GetAbsoluteUrlForLocalFile(string path)
        {
            var fileUri = new Uri(path, UriKind.RelativeOrAbsolute);

            if (fileUri.IsAbsoluteUri)
            {
                return fileUri;
            }
            else
            {
                var baseUri = new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);

                return new Uri(baseUri, fileUri);
            }
        }

        /*Help button : STOP */

        //Scroll down LogText function
        private void LogTextScrollDown()
        {
            textBox_LogText.Update();
            textBox_LogText.SelectionStart = textBox_LogText.Text.Length;
            textBox_LogText.ScrollToCaret();
            textBox_LogText.Refresh();
        }

        /* Log Text button and checkBox: START */

        //Clear LogText
        private void button_ClearLogtext_Click(object sender, EventArgs e)
        {
            LogTextLine = String.Empty;
            textBox_LogText.Text = LogTextLine;
            // changed

        }

        //Refresh Information
        private void button_RefereshInformation_Click(object sender, EventArgs e)
        {
            backgroundWakeEvent.WaitOne(500);
            USBdevList = new USBDeviceList(CyConst.DEVICES_CYUSB);
            backgroundWakeEvent.WaitOne(500);
            bResultLoadEEPROM = false;
            RefreshInformationUIThread();
        }

        //Clear LogText before every programming operation
        private void checkBox_ClearLogText4everyProgrammingOperation_CheckedChanged(object sender, EventArgs e)
        {
            
            bClearLogTextBeforeEveryProgrammingOperation = checkBox_ClearLogText4everyProgrammingOperation.Checked;
        }

        //Verbose LogText
        private void checkBox_VerboseLogText_CheckedChanged(object sender, EventArgs e)
        {
            bVerboseLogText = checkBox_VerboseLogText.Checked;
        }

        //Retrieve Flash ID
        private void checkBox_Retrieve_Flash_ID_CheckedChanged(object sender, EventArgs e)
        {
            bRetrieve_FlashID = checkBox_Retrieve_Flash_ID.Checked;
        }
   
        /* Log Text button and checkBox STOP */       
    }
}
