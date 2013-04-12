# Open FirmWare Upgrade Tool
It provides main functions to update EEPROM and Flash memory on
TE modules based on Cypress EZ-USB FX2 microcontroller.

## Requirements
- operating system: Microsoft windows (32 or 64 bit)
- Python: 2.7
 - Windows 32 bit: Python 32 bit (e.g.: Python 2.7.3 Windows Installer)
	http://www.python.org/ftp/python/2.7.3/python-2.7.3.msi
 - Windows 64 bit: Python 64 bit (e.g.: Python 2.7.3 Windows **X86**-*64* Installer)
	http://www.python.org/ftp/python/2.7.3/python-2.7.3.amd64.msi
Note: Please be sure that you download correct version, as python.org can offer you
32 bit version by default.
- TE USB FX2 driver (generation 3)
- simplified C++ TE USB FX2 API (generation 3): TE_USB_FX2_API_C.dll (not officialy supported yet)

## Attention

On some computer configurations, weird behaviors can be experienced with the OpenFut instrument.

In this case, you shall change the paths in lines 74 and 75 of openfut.py to absolute paths with an r before the paths. The r escapes the \s (slashes).

From<br />
    fx2dll_32_name = "TE_USB_FX2_API_C-32.dll"<br />
to<br />
    fx2dll_32_name = r"C:\OpenFut\TE_USB_FX2_API_C-32.dll" <br />
or<br />
    fx2dll_32_name = r'C:\OpenFut\TE_USB_FX2_API_C-32.dll'
   
From<br />
    fx2dll_64_name = "TE_USB_FX2_API_C-64.dll"<br />
to<br />
    fx2dll_64_name = r"C:\OpenFut\TE_USB_FX2_API_C-64.dll"<br />
or<br />
    fx2dll_64_name = r'C:\OpenFut\TE_USB_FX2_API_C-64.dll'
      
The same problem may happen with open() function: in this case, you must force an absolute path in a similar way:<br /> 
for example open(r"C:\file2download\birstream.bit").
       
Another problem that may happen in some configurations is the convention call of ctype.
In this case you must change lines 621 and 623 from windll convention to cdll convention.

From<br />
    fx2dll = windll.LoadLibrary(fx2dll_32_name)<br />
to<br />
    fx2dll =   cdll.LoadLibrary(fx2dll_32_name)

From<br />
    fx2dll = windll.LoadLibrary(fx2dll_64_name)<br />
to<br />
    fx2dll =   cdll.LoadLibrary(fx2dll_64_name)
