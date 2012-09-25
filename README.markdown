# Trenz Electronic USB Suite
This repository contains projects and files relevant to Trenz Electronic modules equipped with a USB microcontroller.

## Linux_FUT
Linux version of DEWESoft Firmware Upgrade Tool. Based on libusb (http://www.libusb.org/).<br />
This application is compatible with both generation 2 and 3 of the TE USB FX2 software stack. <br /> 
For generation 2, please use VID/PID = 0x0547/0x1002. <br />
For generation 3, please use VID/PID = 0x0BD0/0x0300. <br />
New fork? Consider also libusbx (http://libusbx.org/).

## TE_USB_FX2.firmware
Firmware source files for Cypress EZ-USB FX2 microcontoller on Trenz Electronic modules. <br />
This application is compatible with both generation 2 and 3 of the TE USB FX2 software stack. <br />
For generation 2, please use VID/PID = 0x0547/0x1002. <br />
For generation 3, please use VID/PID = 0x0BD0/0x0300.

## TE_USB_FX2.gen_2<br />
**Not recommended for new designs**.<br />
- Open Firmware Upgrade Tool 2:<br />
  provides main functions to update EEPROM and Flash memory on TE modules based on Cypress EZ-USB FX2 microcontroller.
- Example programs (C language) which show API usage.

## TE_USB_FX2.gen_3
- Trenz Electronic C++ API<br />
  for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).
- Trenz Electronic C#  API<br />
  for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).
- Example programs (C++ and C# language) which show API usage.
- Open Firmware Upgrade Tool 3:<br />
  provides main functions to update EEPROM and Flash memory on TE modules based on Cypress EZ-USB FX2 microcontroller.
