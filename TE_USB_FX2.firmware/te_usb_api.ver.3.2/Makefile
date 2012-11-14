#-------------------------------------------------------------------------------
# Makefile for te_api
#-------------------------------------------------------------------------------
# Copyright (C) 2012 Trenz Electronic
#-------------------------------------------------------------------------------
# Permission is hereby granted, free of charge, to any person obtaining a 
# copy of this software and associated documentation files (the "Software"), 
# to deal in the Software without restriction, including without limitation 
# the rights to use, copy, modify, merge, publish, distribute, sublicense, 
# and/or sell copies of the Software, and to permit persons to whom the 
# Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included 
# in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
# OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
# IN THE SOFTWARE.
#
#-------------------------------------------------------------------------------
# Company: Trenz Electronics GmbH
# Engineer: Oleksandr Kiyenko (a.kienko@gmail.com)
#-------------------------------------------------------------------------------

LIBDIR=fx2
LIB=libfx2.lib

CC=sdcc
CFLAGS+=-mmcs51 --no-xinit-opt -I${LIBDIR}

CFLAGS+=--opt-code-size

AS=sdas8051
ASFLAGS+=-plosgff

LDFLAGS=--code-loc 0x0000 --code-size 0x1800
LDFLAGS+=--xram-loc 0x1800 --xram-size 0x0800
LDFLAGS+=-Wl '-b USBDESCSEG = 0xE100'
LDFLAGS+=-L ${LIBDIR}

%.rel : %.a51
	$(AS) $(ASFLAGS) $<

%.rel : %.c
	$(CC) -c $(CFLAGS) $(CPPFLAGS) $< -o $@

default: usb_te.iic usb_dw.iic

%.iic : %.hex
	c:\\Cypress\USB\bin\Hex2bix.exe -ir -f 0xC2 -m 0xF000 -c 0x1 -o $@ $<

usb_te.hex: vectors.rel fw.rel dscr_te.rel i2c.rel startup.rel te_api.rel spi.rel ${LIBDIR}/${LIB}
	$(CC) $(CFLAGS) $(LDFLAGS) -o $@ $+
	packihx $@ > .tmp.hex
	rm $@
	mv .tmp.hex $@

usb_dw.hex: vectors.rel fw.rel dscr_dw.rel i2c.rel startup.rel te_api.rel spi.rel ${LIBDIR}/${LIB}
	$(CC) $(CFLAGS) $(LDFLAGS) -o $@ $+
	packihx $@ > .tmp.hex
	rm $@
	mv .tmp.hex $@

${LIBDIR}/${LIB}:
	make -C ${LIBDIR}

dscr_te.rel: dscr_te.a51
dscr_dw.rel: dscr_dw.a51
i2c.rel: i2c.c i2c.h
fw.rel: fw.c i2c.h te_api.h fpga.h spi.h
te_api.rel: te_api.c te_api.h
spi.rel: spi.c spi.h

.PHONY: clean

clean:
	make -C ${LIBDIR} clean
	rm -f *.lst *.asm *.sym *.rel *.mem *.map *.rst *.hex *.iic *.lk *.omf *.cdb

release: usb_te.iic usb_dw.iic
	copy usb_te.iic ..\ready_for_download\current_te.iic
	copy usb_dw.iic ..\ready_for_download\current_dw.iic
	make -C ${LIBDIR} clean
	rm -f *.lst *.asm *.sym *.rel *.mem *.map *.rst *.hex *.iic *.lk *.omf *.cdb

