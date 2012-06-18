In order to provide a user interface for driver functions, dynamic link library TE_USB_FX2_CyAPI.dll and CyAPI.dll have been used. 
The API for 32 and 64 bit operating systems are located in two different folders:
TE-USB-Suite/TE_USB_FX2_CyAPI_SampleApplication/TE_USB_FX2_CyAPI_SampleApplication/DLL32/
TE-USB-Suite/TE_USB_FX2_CyAPI_SampleApplication/TE_USB_FX2_CyAPI_SampleApplication/DLL64/
These folders come from the folder FileToExportForApplication in the project folder TE-USB-Suite/TE_USB_FX2_CyAPI/.
FileToExportForApplication/ contains TE_USB_FX2_CyAPI.h, CyAPI.h and two folders; DLL32/ and DLL64/. DLL32/ and DLL64/ folders contain files with the same name but compiled respectively for 32 or 64 bit operating systems.
You shall select 32 or 64 bit for the compilation. To do this you shall:
1. in “Explore Solution” panel (top right window), right click the second line (between “Solution” and “External Dependencies”)
2. select “Properties”
3. left click "Configuration Management" (top right)
4. in "Active Solution Platform", select Win32 or x64
5. click “Close”
6. click “OK”
To create a program, you shall copy these files to the project folder. 
User programs should load these libraries and initialize module connection to get access to API functions. To do this, you shall:
1. copy TE_USB_FX2_CyAPI.h, TE_USB_FX2_CyAPI.dll, TE_USB_FX2_CyAPI.lib, CyAPI.h and CyAPI.lib to the project folder (for example 
TE-USB-Suite/TE_USB_FX2_CyAPI_SampleApplication/TE_USB_FX2_CyAPI_SampleApplication/);
2. open the C++ project (double click the TE_USB_FX2_CyAPI_SampleApplication icon in the folder 
TE-USB-Suite/TE_USB_FX2_CyAPI_SampleApplication/);
3. open "Explore Solution" if it is not already open (Ctrl +W or left click "Visualize>Explore Solution");
4. in the right panel "Explore Solution", right click "Header File";
5. select Add. A new window (Add Header File) opens;
6. the term "Look In" shall have automatically selected the correct folder 
(TE-USB-Suite/TE_USB_FX2_CyAPI_SampleApplication/TE_USB_FX2_CyAPI_SampleApplication/). 
If it is not so, you shall select the folder where you have copied the previous DLLs and header files;
7. left click one of the two header files (CyAPI.h or TE_USB_FX2_CyAPI.h);
8. select OK;
9. repeat steps from 4 to 8 for the second header file;
10. in the right panel "Explore Solution", right click "Resource File";
11. select Add. A new window (Add Resource File) opens;
12. the term "Look In" shall have automatically selected the correct folder 
(TE-USB-Suite/TE_USB_FX2_CyAPI_SampleApplication/TE_USB_FX2_CyAPI_SampleApplication/). 
If is not so, you shall select the folder where you have copied the previous DLLs and header files;
13. left click one of the three DLL files (TE_USB_FX2_CyAPI.dll, TE_USB_FX2_CyAPI.lib or CyAPI.lib);
14. select OK;
15. repeat steps from 10 to 14 for the second and third DLL file.