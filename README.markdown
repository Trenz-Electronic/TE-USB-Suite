# Trenz Electronic USB Suite
This repository contains projects and files relevant to Trenz Electronic modules equipped with a USB microcontroller.

## Linux_FWUT
Linux version of FirmWare Upgrade Tool. Based on libusb.<br />
This application is compatible with both generation 2 and 3 of the TE USB FX2 software stack. <br /> 
For generation 2, please use VID/PID = 0x0547/0x1002. <br />
For generation 3, please use VID/PID = 0x0BD0/0x0300. <br />

## TE_USB_FX2.firmware
Firmware source files for Cypress EZ-USB FX2 microcontoller on Trenz Electronic modules. <br />
This application is compatible with both generation 2 and 3 of the TE USB FX2 software stack. <br />
For generation 2, please use VID/PID = 0x0547/0x1002. <br />
For generation 3, please use VID/PID = 0x0BD0/0x0300. <br />

## TE_USB_FX2.gen_2<br />
**Not recommended for new designs**.<br />
- Open FirmWare Upgrade Tool.<br />
This software provided main functions to update EEPROM and Flash content on TE modules based on Cypress USB-EZ FX2 chip.
- Example programs (C language) which show API usage.

## TE_USB_FX2.gen_3
- Trenz Electronic C++ API for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).<br />
- Trenz Electronic C#  API for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).<br />
- Example programs (C++ and C# language) which show API usage. 