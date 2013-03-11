# Trenz Electronic USB Suite
This repository contains projects and files relevant to Trenz Electronic modules equipped with a USB microcontroller.

## Linux_FUT
Linux version of DEWESoft Firmware Upgrade Tool. Based on libusb (http://www.libusb.org/).<br />
This application is compatible with both generation 2 and 3 of the TE USB FX2 software stack. <br /> 
For generation 2, please use VID/PID = 0x0547/0x1002. <br />
For generation 3, please use VID/PID = 0x0BD0/0x0300. <br />
New fork? Consider also libusbx (http://libusbx.org/).

## TE_USB_FX2.firmware <br /> (contains TE API firmware alias TE API commands)
Firmware source files for Cypress EZ-USB FX2 microcontoller on Trenz Electronic modules. <br />
This application is compatible with both generation 2 and 3 of the TE USB FX2 software stack. <br />
For generation 2, please use VID/PID = 0x0547/0x1002. <br />
For generation 3, please use VID/PID = 0x0BD0/0x0300. <br />
TE API firmware (alias TE API commands) are defined here. <br /> 
They must not be confused with TE API USB of Generation 2 and 3: one of the TE API USB (SendCommand) is used to send one command to MicroBlaze and FX2 microcontroller. The command used is defined in the firmware project.

## TE_USB_FX2.gen_2 <br /> (contains TE API USB Generation 2, some examples and Open_Fut tool) <br />
**Not recommended for new designs**.<br />
- Open Firmware Upgrade Tool 2:<br />
  provides main functions to update EEPROM and Flash memory on TE modules based on Cypress EZ-USB FX2 microcontroller.
- Example programs (C language) which show API usage.

## TE_USB_FX2.gen_3 <br /> (contains TE API USB Generation 3, some examples and Open_Fut tool)
- Trenz Electronic C++ API<br />
  for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).
- Trenz Electronic C#  API<br />
  for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).
- Example programs (C++ and C# language) which show API usage.
- Open Firmware Upgrade Tool 3:<br />
  provides main functions to update EEPROM and Flash memory on TE modules based on Cypress EZ-USB FX2 microcontroller.
