## OpenFutNet 
C# version of Open_FUT.<br />
This program is a C# evolution of the program Python OpenFut for 3rd Generation Firmware.<br />
Based on TE USB FX2 API for .NET ( managed C++ and C# for example).<br />

TO DO: add .bin file support for FPGA bitstream. <br />

TO DO: add type of FPGA selection (add manual selection; automatic selection is already realized). <br />

TO DO: change the memory instantiation. <br />

##Info
1)<br />
TE USB FX2 module starts as a TE device (at power on) when:<br />
A) 3rd Generation Firmware is correctly stored in FX2 microcontroller's EEPROM;<br />
B) the EEPROM switch is set to ON when the TE USB FX2 module is powered on.<br />

TE USB FX2 module starts as a Cypress device (at power on) when:<br />
A) the EEPROM switch is set to OFF when the TE USB FX2 module is powered on.<br />

TE USB FX2 module starts as a DEWESoft device (at power on) when:<br />
A) 2nd Generation Firmware is correctly stored in FX2 microcontroller's EEPROM;<br />
B) the EEPROM switch is set to ON when the TE USB FX2 module is powered on.<br />


2)<br />
Recovery Procedure.<br />
TE USB FX2 module is used as a Cypress device and it is possible to programm into FX2 microcontroller's EEPROM 2nd or 3rd Generation Firmware.<br />
Procedure description. You should:<br />
A) turn off the TE micromodule;<br />
B) set the EEPROM switch to OFF;<br />
C) turn on the TE micromodule;<br />
D) set the EEPROM switch to ON;<br />
E) install the Cypress USB Generic driver (if it is not already installed);<br />
F) select the .icc file (FX2 microcontroller's Firmware of 2nd or 3rd Generation must be used) to program into FX2 microcontoller's EEPROM using left button "Select .iic file or enter file path";<br />
G) program the .icc file into EEPROM using right button "Program USB: write IIC EEPROM";<br />
H) wait the end of OpenFutNet programming operation (the programming operation ends when buttons are released);<br />
I) check the firmware version written if a 3rd Generation firmware has been selected at section F).<br />
 
 
##Warning
 
1)<br />
For the Recovery Procedure, it is necessary to install the Cypress USB Generic driver.<br />
On some computers, it could automatically install itself. On other computers, you must install the version that can be founded on Trenz Electronic Web Site<br /> 
http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite/d2/recovery/d3/drivers.html.<br />
 
 
2)<br />
To be able to use this program you must install the Trenz Electronic driver TE USB FX2 driver.<br />
 
 
3)<br /> 
This program should be used with a single Trenz Electronic or Cypress device attached.<br />

This program has been realized in this way for two reason:<br />
A) the TE USB FX2 module are not identified by a unique serial number;<br />
B) the program is more simple and intuitive to use.<br />

In any case, if it is necessary a version of this program to use with more than one single micromodule, it can be realized using this program as example (or better the alternative version under test explained), but it is strongly advised to find a way to uniquely identify the TE micromodule, before to write a new program.<br />

 
4)
Micromodule's EEPROM switch should be moved to ON to realize the FX2 microcontroller's EEPROM programming.<br />

For the Recovery Procedure (TE USB FX2 module turn on as Cypress device), the TE USB FX2 module must be powered on with EEPROM switch moved to OFF.<br />
After this, the EEPROM switch must be set to ON; otherwise the EEPROM programming will fail.<br />
Unfortunately, at this moment the TE USB FX2 module can't automatically warning you about user's oversight (EEPROM switch is left to OFF). <br />

 
5)
At this moment, like the Python Open_FUT, the program use one single large array of byte for the FPGA's bitstream that should be written into the SPI Flash of the micromodule.<br />
If a very old computer with very low RAM is used, this may affect the performance of the program; we have not yet actually seen this behavior in our tests and we also doesn't expect a large hit  on the performnance because the performance of this program are I/O bounded by TE USB FX2 module's SPI Flash.<br />

At this moment, another version of OpenFutNet, using OpenFile and ReadBinary function to lessen 
the computer's memory used, is under investigation for the correct sizing of buffer to use.
In this version, a lower number of static variables will be used.
If you desire to realize a OpenFutNet program to write more than one single micromodule, this version will 
certainly better fits your needs. But, before considering this alternative version of the program, 
it is strongly advised to find a way to uniquely identify the TE micromodule.<br />


6)
In this version of the program, the FPGA bitstream maximum size is 128M. This is correct because it is
the maximum size of Flash mounted on TE USB FX2 modules programmed by this program.<br />
 
 
##Feature
1)
With this program (OpenFutNet), you can download Trenz Electronic FX2 microcontroller Firmware 
(both 2nd and 3rd Generation) into the FX2 microcontroller's EEPROM.<br />

This is realized using:
A)the Trenz Electronic .NET DLL 'TE_USB_FX2.dll';
B)the Cypress .NET DLL "CyUSB.dll".<br />

The change of Firmware is immediatly effective because the new Firmware is also loaded 
into the FX2 microcontroller's RAM. To do this the Cypress .NET DLL "CyUSB.dll" is used.
If the 3rd Generation Firmware v3.02 is used, the Firmware RAM loading is correctly seen as a 
TE USB FX2 module detach/insertion cycle; it logically happens even if it isn't manually relaized.<br />

2)
OpenFutNet warns you about double (or more) TE USB FX2 module insertion if TE micromodules are seen as 
Cypress or Trenz Electronic device. The TE micromodules seen as DEWESoft device are not warned as
multiple insertions because they have another warning message.<br />


3)
OpenFutNet warns you about TE USB FX2 module insertion if TE USB FX2 module is seen as Cypress device.
OpenFutNet warns you that you will be able only to use Recovery Procedure for FX2 microcontroller's 
Firmware (EEPROM) and that you will not be able to program SPI Flash (FPGA).<br />


4)
OpenFutNet warns you about TE USB FX2 module insertion if TE USB FX2 module is seen as DEWESoft device.
OpenFutNet warns you that it can't write the TE micromodule. 
You should use a Recovery Procedure:
A) turn off the TE micromodule;
B) follow the Recovery Firmware Boot. <br />  
 

5)
OpenFutNet use TE_USB_FX2.dll to automatically obtains information about the 3rd Generation Firmware 
version written into FX2 microcontroller's EEPROM. <br />
 
  
6)
OpenFutNet use TE_USB_FX2.dll to automatically obtains information about the TE Reference Architecture (TE Reference Design based on Xilinx MicroBlaze soft processor and custom TE USB FX2 module) running onto Xilinx FPGA and written into SPI Flash. <br />
 
 
7)
OpenFutNet tries to guide you during EEPROM (FX2 microcontroller) or SPI Flash (FPGA) programming.<br />
 
8)
An informative Log is written during EEPROM and SPI Flash programming.<br />
