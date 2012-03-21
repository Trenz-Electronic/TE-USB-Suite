from distutils.core import setup
import py2exe

setup(windows=['ofut.py'], data_files=[(".", ["TE0300DLL.dll"])])
