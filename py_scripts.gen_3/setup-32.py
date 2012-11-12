from distutils.core import setup
#from distutils.core import core
import py2exe

setup(windows=['ofut.py'], data_files=[(".", ["TE_USB_FX2_API_C-32.dll"])])
