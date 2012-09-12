# Trenz Electronic USB Suite
This repository contains projects and files relevant to Trenz Electronic modules equipped with a USB microcontroller.

## Linux_FWUT
Linux version of FirmWare Upgrade Tool. Based on libusb.<br />
Please find the documentation here (generation 2): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html

## Open_FWUT
Python version of FirmWare Upgrade Tool. Based on Windows API (TE0300DLL.dll).<br />
Please find the documentation here (generation 2): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html

## TE_USB_FX2.firmware
Firmware source files for Cypress EZ-USB FX2 microcontoller on Trenz Electronic modules.

## TE_USB_FX2_CyAPI<br />TE_USB_FX2_CyAPI_SampleApplication
Trenz Electronic C++ API for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).<br />
With sample application.
* Folder "TE_USB_FX2_CyAPI/" contains a Microsoft Visual Studio 2010 project for Trenz Electronic C++ API
* Folder "TE_USB_FX2_CyAPI_SampleApplication/" contains a Microsoft Visual Studio 2010 project for a Trenz Electronic C++ API sampple application
* Folder "TE_USB_FX2_CyAPI_SampleApplication/executable-32/" contains files executable on 32-bit Microsoft Windows operating systems. "Microsoft Visual C++ 2010 Redistributable Package (x86)" (vcredist_x86.exe) shall be installed and can be downloaded from<br />http://www.microsoft.com/en-us/download/details.aspx?id=5555 .
* Folder "TE_USB_FX2_CyAPI_SampleApplication/executable-64/" contains files executable on 64-bit Microsoft Windows operating systems. "Microsoft Visual C++ 2010 Redistributable Package (x64)" (vcredist_x64.exe) shall be installed and can be downloaded from<br />http://www.microsoft.com/en-us/download/details.aspx?id=14632 .

Please find the documentation here (generation 3): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html

## TE_USB_FX2_CyUSB<br />TE_USB_FX2_CyUSB_SampleApplication
Trenz Electronic C#  API for FPGA module series equipped with FX2 USB microcontroller (Trenz Electronic firmware and drivers).<br />
With sample applications.
* Folder "TE_USB_FX2_CyUSB/" contains a Microsoft Visual Studio 2010 project for Trenz Electronic C# API
* Folder "TE_USB_FX2_CyUSB_SampleApplication/" contains a Microsoft Visual Studio 2010 project for a Trenz Electronic C# API sampple application
* Folder "TE_USB_FX2_CyUSB_SampleApplication/executable/" contains files executable on Microsoft Windows operating systems. "Microsoft .NET Framework 4" (vcredist_x64.exe) shall be installed.<br />
  The "Standalone Installer" (dotNetFx40_Full_x86_x64.exe) can be downloaded from<br />http://www.microsoft.com/en-us/download/details.aspx?id=17718 .<br />
  The "Web Installer" (dotNetFx40_Full_setup.exe) can be downloaded from<br />http://www.microsoft.com/en-us/download/details.aspx?id=17851 .

Please find the documentation here (generation 3): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html

## archive/TE0300_API_Example<br />archive/TE0320_API_Example
**Not recommended for new designs**.<br />
Example programs (C language) which show API usage. These programs:
* get FX2 firmware information (version and status)
* get FPGA information
* run memory tests
* run USB transfer tests

Please find the documentation here (generation 2): http://www.trenz-electronic.de/download/d0/Trenz_Electronic/d1/TE-USB-Suite.html