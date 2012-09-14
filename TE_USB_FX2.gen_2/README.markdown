﻿# Trenz Electronic USB Suite
This repository contains projects and files relevant to Trenz Electronic modules equipped with a USB microcontroller.

## Open_FWUT
Python version of FirmWare Upgrade Tool.<br /> Based on Windows API (TE0300DLL.dll).<br />
Please find the documentation here (generation 2): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html .

Open Firmware Upgrade Tool.<br />
This software provided main functions to update EEPROM and Flash content on
TE modules based on Cypress USB-EZ FX2 chip.

Requirements:
Python >= 2.7

## TE_USB_FX2.gen_2/TE0300_API_Example<br />TE_USB_FX2.gen_2/TE0320_API_Example
**Not recommended for new designs**.<br />
Example programs (C language) which show API usage. These programs:
* get FX2 firmware information (version and status)
* get FPGA information
* run memory tests
* run USB transfer tests

Please find the documentation here (generation 2): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html .