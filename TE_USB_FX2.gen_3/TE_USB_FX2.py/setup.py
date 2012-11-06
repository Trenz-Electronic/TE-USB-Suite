from distutils.core import setup
#from distutils.core import core
import py2exe

setup(windows=['te_usb_test.py'], data_files=[(".", ["TE_USB_FX2_CyAPI_64.dll"])])
