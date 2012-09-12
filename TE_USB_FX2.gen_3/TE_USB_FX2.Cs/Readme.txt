The part that follows is taken from the manual UM-TE_USB_API.cs (September 2012).
This readme file is only for quick information and, in case of difference, the manual always prevails.

----------------------------------------

Requirements
When using TE_USB_FX2_CyUSB.dll API, a host computer should meet the following requirements:
Operating system: Microsoft Windows 2000, Microsoft Windows XP, Microsoft Windows Vista, Microsoft Windows 7
USB driver : Trenz Electronic USB FX2 driver
Interface: USB 2.0 host
.NET Framework version >= 4.0.30319
See your module user manual for dedicated driver installation instructions.

API Functions

Exported function list:
TE_USB_FX2_ScanCards()
TE_USB_FX2_Open()
TE_USB_FX2_Close()
TE_USB_FX2_SendCommand()
TE_USB_FX2_GetData()
TE_USB_FX2_SetData()

In order to provide a user interface for driver functions, dynamic link library TE_USB_FX2_CyUSB.dll and CyUSB.dll have been used. User program should load these libraries and initialize module connection to get access to API functions. To do this, you shall:
1. copy TE_USB_FX2_CyUSB.dll and CyUSB.dll to the project folder (for example 
TE-USB-Suite/TE_USB_FX2_SampleApplication/TE_USB_FX2_SampleApplication/);
2. open the C# project (double click the TE_USB_FX2_CyUSB_SampleApplication icon in the folder 
TE-USB-Suite/TE_USB_FX2_CyUSB_SampleApplication/);
3. open "Explore Solution" if it is not already open (Ctrl+W or left click "Visualize > Explore Solution");
4. in the right panel "Explore Solution", right click "Reference";
5. select "Add Reference". A new window (Add Reference) opens;
6. select the fourth sheet (Browse). The term "Look In" shall have automatically selected the correct folder 
(TE-USB-Suite/TE_USB_FX2_SampleApplication/). If is not so, you shall select the folder where you have copied the previous DLLs;
7. left click one of the two DLLs;
8. select OK;
9. repeat steps from 4 to 8 for the second DLL.
