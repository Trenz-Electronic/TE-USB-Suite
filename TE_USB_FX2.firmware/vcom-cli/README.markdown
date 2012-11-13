# FX2 firmware for Virtual COM and CLI.
This repository contains virtual COM project relevant to 
Trenz Electronic modules equipped with a USB microcontroller.
Project mased on GNU radio and usb_jtag projects.
Refer to AN-TE0002 for project details.

## Build project
Requirements:
* SDCC version 3.2.0 or higher
* make (tested with make from Xilinx EDK 13.2 distribution)

Build
* make clean
* make

## Driver
Project use example virtual COM port driver from Cypress AN58764.

## FX2 Firmware
FX2 firmware file in iic format can be found in 'ready_for_download' folder. 
To update FX2 firmware on your module:

* Put EEPROM switch on your module to "OFF" state.
* Connect module to PC, using USB cable.
* Install Cypress generic driver if needed.
* Put EEPROM switch on your module to "ON" state.
* Run Cypress USB Console.
* Go Options->EZ-USB Interface
* Press "Lg EEPROM" button
* Select VirtualCom.iic from firmware folder
* Wait till operation completed
* Reconnect module from USB
* Install Cypress Virtual COM port driver if needed

## FPGA interface
From FPGA side FX2 microcontroller configured to use synchronous Slave FIFO 
interface. Data from PC is going to EP2 FIFO. Data from FPGA should be writed 
to EP6 FIFO. Flags pins used to show state of this FIFO:

* Flag A - EP2 Empty Flag
* Flag B - EP6 Full Flag
* Flag C - EP6 Programmable Full Flag ('1' when less than 128 bytes free)
* Flag D - EP2 Programmable Full Flag ('1' when more than 128 bytes to read)

All signals configured to active high level.
