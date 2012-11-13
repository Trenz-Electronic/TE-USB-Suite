SET C51BIN=C:\Keil\C51\BIN\
SET C51INC=C:\Keil\C51\INC\;c:\CYPRESS\USB\Target\Inc\;C:\Keil\C51\INC\
SET C51LIB=C:\Keil\C51\LIB\
SET CPU_TYPE=EZ-USB FX2 (CY7C68XXX)
SET CPU_VENDOR=Cypress Semiconductor
SET UV2_TARGET=dw
SET CPU_XTAL=0x02DC6C00
"C:\Keil\C51\BIN\C51.EXE" @fw.__i
"C:\Keil\C51\BIN\C51.EXE" @versions.__i
"C:\Keil\C51\BIN\A51.EXE" @dscr_dw._ia
"C:\Keil\C51\BIN\C51.EXE" @te_api.__i
"C:\Keil\C51\BIN\C51.EXE" @spi.__i
"C:\Keil\C51\BIN\C51.EXE" @i2c.__i
"C:\Keil\C51\BIN\BL51.EXE" @usb_dw.lnp
"C:\Keil\C51\BIN\OH51.EXE" "usb_dw" 
c:\cypress\usb\bin\hex2bix -i -f 0xC2 -o usb_dw.bin usb_dw.hex
c:\cypress\usb\bin\hex2bix -i -f 0xC2 -o usb_dw.iic usb_dw.hex
