# Firmware source for TE USB (FX2) equipped modules

## Requirements
To download and run "EZ-USB Development Kit" installation use prepare.bat

To build this firmware extra header and library files required
* C:\Cypress\USB\Target\Inc\Fx2.h
* C:\Cypress\USB\Target\Inc\fx2regs.h
* C:\Cypress\USB\Target\Inc\fx2sdly.h
* C:\Cypress\USB\Target\Lib\LP\EZUSB.LIB
* C:\Cypress\USB\Target\Lib\LP\USBJmpTb.OBJ
Theese files are part of "EZ-USB Development Kit" package
which can be downloaded from [http://www.cypress.com/?docID=6018]
Package should be installed to default folder (C:\Cypress\USB), otherwise
project options should be changed.

## Tools
* Compiler: Keil uVision V4.24
* Output file generator: C:\Cypress\USB\bin\Hex2bix.exe (Intel Hex file to EZ-USB Binary file conversion utility)
 please read Cypress AN45197 "Using the Hex2bix Conversion Utility"
 http://www.cypress.com/?rID=17627
 Hex2bix is a program used to convert a .hex file to a raw binary, A51, or IIC format. This application note describes how to use the Hex2bix conversion utility for successful file conversion.

## License
Firmware source distributed under MIT license. See license_mit.txt

## Binary Firmware Files

### TE_USB_FX2-v03.00.iic
Trenz Electronic FX2 USB microcontroller firmware file:
* VID/PID: 0x0BD0/0x0300
* device driver: Trenz Electronic (Cypress Semiconductor derivative)
* firmware version: 3.0
* file format: I2C (Inter-Integrated-Circuit EEPROM)

### TE_USB_FX2-v03.00.DW.iic
Trenz Electronic FX2 USB microcontroller firmware file (<strong>not recommended for new designs</strong>):
* VID/PID: 0x0547/0x1002
* device driver source: DEWESoft
* firmware version: 3.0
* file format: I2C (Inter-Integrated-Circuit EEPROM)
