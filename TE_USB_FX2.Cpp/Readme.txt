The part that follows is taken from the manual UM-TE_USB_API.cpp (September 2012).
This readme file is only for quick information and, in case of difference, the manual always prevails.

----------------------------------------

Requirements
When using TE_USB_FX2_CyAPI.dll API, a host computer should meet the following requirements:
Operating system: Microsoft Windows 2000, Microsoft Windows XP, Microsoft Windows Vista, Microsoft Windows 7
USB driver: Trenz Electronic USB FX2 driver
Interface: USB 2.0 host
C++ Run Time: it is contained in 
Microsoft Visual C++ 2010 x64 Redistributable Setup: vcredist_x64.exe for 64 bit.
Microsoft Visual C++ 2010 x86 Redistributable Setup: vcredist_x86.exe for 32 bit
See your module user manual for dedicated driver installation instructions.


API Functions

Exported function list:
TE_USB_FX2_ScanCards()
TE_USB_FX2_Open()
TE_USB_FX2_Close()
TE_USB_FX2_SendCommand()
TE_USB_FX2_GetData_InstanceDriverBuffer()
TE_USB_FX2_GetData()
TE_USB_FX2_SetData_InstanceDriverBuffer()
TE_USB_FX2_SetData()

In order to provide a user interface for driver functions, dynamic link library TE_USB_FX2_CyAPI.dll and CyAPI.lib have been used. 
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
13. left click TE_USB_FX2_CyAPI.dll
Note: To compile the software project, only the TE_USB_FX2_CyAPI.dll dynamic library is strictly necessary; TE_USB_FX2_CyAPI.lib or CyAPI.lib static libraries are optional.
14. select OK;
15. optional: repeat steps from 10 to 14, replacing TE_USB_FX2_CyAPI.dll with TE_USB_FX2_CyAPI.lib and CyAPI.lib at step 13.


Visual Studio project settings
Visual Studio project files *.sln
( TE_USB_FX2_CyUSB.sln and TE_USB_FX2_CyUSB_SampleApplication.sln)
can be opened as follows:
1. right click *.sln file;
2. select “Open with”;
3. select “Microsoft Visual C++ 2010 Express” or “Microsoft Visual Studio 2010” (the latter is used if Visual Studio 2010 Professional is installed).
If Visual Studio 2010 Express is used to compile 64 bit C++ programs, Microsoft Windows SDK 7.1 must be installed after the installation of Visual Studio 2010 Express.
After opening the project file, you must select the correct settings, in particular if you use the version of the code from GitHub (or create a new software project ) instead of precompiled software project. The settings for the 32 and 64 bit case are different.
You must follow this procedure.
1. Open the project
2. Wait the end of the parsing (it is shown at lower left with a white “Ready”).
3. Right-click “Solution 'TE_USB_FX2_CyAPI_SampleApplication'” under “Solution Explorer”.
4. A new window pop up (“Solution 'TE_USB_FX2_CyAPI_SampleApplication'” Property Pages”).
5. Select “Configuration Properties”.
6. Left-click “Configuration Manager...”.
7. A new window pop up (“Configuration Manager”).
8. For “Active solution configuration” select “Release”.
9. For “Active solution platform” select “Win32” (“x64” for 64 bit case)
If “x64” does not exist you must create this option with <Edit>.
10. If are not already selected in the table, select “Release” for “Configuration” and “Win32” for “Platform” (Build must also selected with a “v” shown).
11. Left click “Close”.
12. The window “Configuration Manager” is closed.
13. Verify that “Win32” (“x64”) is selected for “Platform”.
14. Verify that “Release” is selected for “Configuration”.
15. In the window “Solution 'TE_USB_FX2_CyAPI_SampleApplication'” Property Pages” select “Apply” and then “Ok”.
16. The window “Solution 'TE_USB_FX2_CyAPI_SampleApplication'” Property Pages” is closed.
17. Right-click “TE_USB_FX2_CyAPI_SampleApplication” under “Solution Explorer”.
18. Select “Configuration Properties” then “General”.
19. a)“Platform Toolset” must be selected “v100” for 32 bit (both Express and Professional) and for 64 bit professional.
b)“Platform Toolset” must be selected “Windows7.1SDK” for 64 bit Express.
20. Select “Configuration Properties” then “C/C++”, then “Preprocessor”.
21. Select “Preprocessor Definitions” must be left clicked.
22. Left click the black arrow pointing toward the bottom and then select <Edit>.
23. A new window pop up (“Preprocessor Definitions”).
24. Add “WIN32” and then click return.
25. Add “NDEBUG” and then click return.
26. Add “_CONSOLE” and then click return.
27. Select “OK”.
28. The window “Preprocessor Definitions” is closed.
29. Select “Configuration Properties” then “C/C++”, then “Linker”.
30. Select “Input”, then “Ignore specific default libraries”.
31. Left click the black arrow pointing toward the bottom and then select <Edit>.
32. A new window pop up (“Ignore Specific Default Libraries”).
33. Add “libcmt.lib” and then click return.
34. Select “OK”.
35. Select “Input”, then “Additional Dependencies”.
36. Left click the black arrow pointing toward the bottom and then select <Edit>.
37. A new window pop up (“Additional Dependencies”).
38. Add “setupapi.lib” and then click return.
39. Add “CyApi.lib” and then click return.
40. Select “OK”
41. Select “debugging”, then “Generate Debug”.
42. Left click the black arrow pointing toward the bottom and then select “Yes(/DEBUG)”.
43. Click “Apply” and then “OK”.

Use of pdb file (symbolic debugging)
You can choose to use the pdb file for a debugging based on symbol.
If you compile using pdb file, the compilation is more slow.
If you want deactivate the service or change the directory of pdb files used you must follow this procedure:
1. select “Debug” in the project open
2. select “Options and Settings...” in the list open
3. a new window pop up (“Options”)
4. select “Debugging” then “Symbols”
5. a)(symbolic debugging information is used) select the “Symbol file (.pdb) locations:” you can choose “Microsoft Symbol Server” or a directory of your choice 
b)(symbolic debugging information is not used) deselect “Microsoft Symbol Server” and do not write an alternative directory (left a blank space)
In this case, in step of compilation you are informed that Visual Studio is unable to charge the symbols of various DLLs


