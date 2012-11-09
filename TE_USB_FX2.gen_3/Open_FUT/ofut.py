# -*- coding: UTF-8 -*-
#-------------------------------------------------------------------------------
# Copyright (c) 2011 Trenz Electronic GmbH
#-------------------------------------------------------------------------------
# This file is Firmware Upgrade Tool for Trenz Electronic modules based on
# Cypress USB-EZ FX2 chip
#------------------------------------------------------------------------------- 
# Author: Alexander Kienko <a.kienko@gmail.com>
#-------------------------------------------------------------------------------
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>.
#-------------------------------------------------------------------------------
# THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED 
# WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
# MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO 
# EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
# SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
# PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
# OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
# WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
# OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
# OF THE POSSIBILITY OF SUCH DAMAGE.
#-------------------------------------------------------------------------------
# Import used packages
from Tkinter import *
from tkMessageBox import *
from tkFileDialog import askopenfilename
from ttk import *
from ctypes import *
from binascii import *
from pkg_fx2def import *
import time
import tkFont
import zipfile
import platform
#-------------------------------------------------------------------------------
version = " v0.03 Beta"
root = Tk() # Create main window
root.title("Open Firmware Upgrade Tool"+version)	# Header of main window
fpga_file_opt = fpga_options = {}	 # Array of file open dialog fpga_options
fpga_options['defaultextension'] = '' 
fpga_options['filetypes'] = [('bin files', '.bin'), ('all files', '.*')]
fpga_options['initialdir'] = '.'			 # Current dir
fpga_options['initialfile'] = 'fpga.bin'	 # default file name
fpga_options['parent'] = root
fpga_options['title'] = 'Select FPGA bitstream file' # title
usb_file_opt = usb_options = {}	 # Array of file open dialog usb_options
usb_options['defaultextension'] = '' 
usb_options['filetypes'] = [('bin files', '.bin'), ('all files', '.*')]
usb_options['initialdir'] = '.'			 # Current dir
usb_options['initialfile'] = 'usb.bin'	  # default file name
usb_options['parent'] = root
usb_options['title'] = 'Select USB firmware file' # title
fwu_file_opt = fwu_options = {}	 # Array of file open dialog fwu_options
fwu_options['defaultextension'] = '' 
fwu_options['filetypes'] = [('fwu files', '.fwu'), ('all files', '.*')]
fwu_options['initialdir'] = '.'			 # Current dir
fwu_options['initialfile'] = 'v.fwu'	  # default file name
fwu_options['parent'] = root
fwu_options['title'] = 'Select firmware update file' # title
# Define later used variables
usb_bitstream = None
fpga_bitstream = None
fx2dll_32_name = "TE_USB_FX2_API_C-32.dll"
fx2dll_64_name = "TE_USB_FX2_API_C-64.dll"
fx2dll = None
fpga_file_opened = 0
usb_file_opened = 0
complete = 0		# Progressbar variable
op_error = 0		# Error number
op = StringVar()	# Current operation name
op.set("")		  # No operation at start
#-----m_handle = c_int(0) # create variable to handle
opened = 0		  # handle flag
#-------------------------------------------------------------------------------
def bitswap(orig_byte): # Function swap bits in byte
	return sum(1<<(8-1-i) for i in range(8) if orig_byte>>i&1)
#-------------------------------------------------------------------------------
def printlog(logmsg):
	log_text.config(state = NORMAL)	 # Enable for editing
	log_text.insert(END, logmsg + '\n') # Add to the end of text
	log_text.config(state = DISABLED)   # Disable editing
	log_text.yview(END)				 # scroll down to the end
#-------------------------------------------------------------------------------
def fpga_bitfile_select():   # Call file select dialog
	global fpga_bitstream
	global fpga_file_opened
	fpga_bitfile_name = askopenfilename(**fpga_file_opt) # File select dialog
	fpga_file_text.config(state = NORMAL)	   # Enable for editing
	fpga_file_text.delete(1.0, END)			 # Erase input field
	fpga_file_text.insert(END, fpga_bitfile_name)  #Put file name in input field
	fpga_file_text.config(state = DISABLED)	 # Disable editing
	if fpga_bitfile_name:
		fpga_bin_file = open(fpga_bitfile_name, 'rb')	# Open in binary mode
		fpga_bitstream = fpga_bin_file.read()			# Read
		fpga_bin_file.close()							# Close
		fpga_file_opened = 1
	else:										   # No filename defined
		printlog("ERROR: Can't open file")
		op.set("Error")	 # Show error
		op_error = 1		# Signalling
	return 0
#-------------------------------------------------------------------------------
def usb_bitfile_select():   # Call file select dialog
	global usb_bitstream
	global usb_file_opened
	usb_bitfile_name = askopenfilename(**usb_file_opt)  # File select dialog
	usb_file_text.config(state = NORMAL)			# Enable for editing
	usb_file_text.delete(1.0, END)				  # Erase input field
	usb_file_text.insert(END, usb_bitfile_name)  # Put file name in input field
	usb_file_text.config(state = DISABLED)		  # Disable editing
	if usb_bitfile_name:
		usb_bin_file = open(usb_bitfile_name, 'rb') # Open in binary mode
		usb_bitstream = usb_bin_file.read()		 # Read
		usb_bin_file.close()						# Close
		usb_file_opened = 1
	else:										   # No filename defined
		printlog("ERROR: Can't open file")
		op.set("Error")							 # Show error
		op_error = 1								# Signaling error
	return 0
#-------------------------------------------------------------------------------
def fwu_file_select():   # Call file select dialog
	global usb_bitstream
	global fpga_bitstream
	global usb_file_opened
	global fpga_file_opened
	fwu_file_name = askopenfilename(**fwu_file_opt) # File select dialog
	fwu_file_text.config(state = NORMAL)		# Enable for editing
	fwu_file_text.delete(1.0, END)			  # Erase input field
	fwu_file_text.insert(END, fwu_file_name)	# Put file name in input field
	fwu_file_text.config(state = DISABLED)	  # Disable editing
	if zipfile.is_zipfile(fwu_file_name):	   # Check if file is really zip
		printlog("Reading fwu from " + fwu_file_name)
		fwu_file = file(fwu_file_name, 'rb')	# define zip file
		zip_file = zipfile.ZipFile(fwu_file, 'r')   # operate as zip
		usb_bitstream = zip_file.read("usb.bin", 'rb')  # read usb.bin from zip
		printlog("USB  Firmware size " + str(len(usb_bitstream)))
		usb_file_opened = 1
		fpga_bitstream = zip_file.read("fpga.bin", 'rb')	# read fpga.bin
		printlog("FPGA Bitstream size " + str(len(fpga_bitstream)))
		fpga_file_opened = 1	 
		zip_file.close()		# close zip
	else:					   # not zip file
		printlog("ERROR: File " + fwu_file_name + " is not valid FWU file")
		op.set("Error")
		op_error = 1
	return 0
#-------------------------------------------------------------------------------
def spi_erase_sectors(sectors2erase):
	global op_error
	global op
	global complete
	global root
	#---global m_handle
	global opened
	global fx2dll
	printlog("Erasing SPI Flash")
	op.set("Erasing")   # Update operation label
	cmd = create_string_buffer(64)	  # Buffer for command
	reply = create_string_buffer(64)	# Buffer for reply
	cmd[0] = CMD_FX2_SECTOR_ERASE	   # Command 
	#cmd_length = c_int(64)			  # Command length always = 64
	cmd_length = c_long(64)			 # Command length always = 64
	reply_length = c_long(64)		   # Variable for reply length
	timeout_ms = c_ulong(1000)		  # Timeout 1s
	printlog(str(sectors2erase) + " Sectors to erase")
	for sector in range(sectors2erase):
		complete = (sector * 100) / sectors2erase # progressbar
		progressbar["value"] = complete
		root.update()				   # Redrive to update progressbar
		cmd[1] = chr(sector)			# sector to erase (convert to 8 bit)
		#-- TE0300_SendCommand(unsigned int, byte*, int	   ,byte*	   , int*			   , int);
		#-- TE0300_SendCommand(m_handle	, cmd  , cmd_length,byref(reply), byref(reply_length), timeout_ms)
		#--if fx2dll.TE0300_SendCommand(m_handle, cmd, cmd_length, 
		#--byref(reply), byref(reply_length), timeout_ms) != 0:	# call API
		# you must open and select
		#		 TE_USB_FX2_SendCommand ( byte* Command, long CmdLength, byte* Reply, long ReplyLength, unsigned long Timeout);
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			op.set("Error")
			printlog("ERROR: Can't call API function TE0300_SendCommand")
			op_error = 5
			break
	if op_error == 0:
		printlog("Erase complete")

def spi_erase_bulk():
	global op_error
	global op
	global complete
	global root
	global opened
	global fx2dll
	printlog("Erasing SPI Flash")
	op.set("Erasing")   # Update operation label
	cmd = create_string_buffer(64)	  # Buffer for command
	reply = create_string_buffer(64)	# Buffer for reply
	cmd_length = c_long(64)			 # Command length always = 64
	reply_length = c_long(64)		   # Variable for reply length
	timeout_ms = c_ulong(1000)		  # Timeout 1s

	cmd[0] = CMD_FX2_READ_STATUS
	for t in range(7):
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			op.set("Error")
			printlog("ERROR: Can't call API function TE0300_SendCommand")
	if ord(reply[2]) != 0:
		op.set("Error")
		printlog("ERROR: Flash busy")
		op_error = 6
	
	printlog("Bulk erase")
	
	flash_busy = 0
	while flash_busy == 0:					# Flash should be busy after erase command
		cmd[0] = CMD_FX2_FLASH_ERASE		# Command
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			op.set("Error")
			printlog("ERROR: Can't call API function TE0300_SendCommand")
			op_error = 5
		cmd[0] = CMD_FX2_READ_STATUS		# Command
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			op.set("Error")
			printlog("ERROR: Can't call API function TE0300_SendCommand")
			op_error = 5
		flash_busy = ord(reply[2])
		
	# Chip erase time 15-30s
	erase_complete = 0
	if op_error == 0:   # No errors in past
		for t in range(35):
			time.sleep(1)
			complete = (t * 100) / 35 # progressbar
			progressbar["value"] = complete
			root.update()				   # Redrive to update progressbar
			# testing Busy Flag
			cmd[0] = CMD_FX2_READ_STATUS
			if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
				op.set("Error")
				printlog("ERROR: Can't call API function TE0300_SendCommand")
				op_error = 5
			if ord(reply[2]) == 0:
				erase_complete = 1
				break
	# Check if we finish erasing
	if erase_complete == 0:
		op.set("Error")
		printlog("ERROR: Flash busy after chip erase")
		op_error = 6
	else:
		printlog("Erase complete");
		
#-------------------------------------------------------------------------------
def eeprom_program():	  # USB Lg EEPROM programming 
	global usb_bitstream
	global op_error
	global op
	global complete
	global root
	global opened
	global usb_file_opened

	timeout_ms = c_ulong(1000)		  # Timeout 1s
	CardNumber = c_int(0)			   # Card Number 0
	DriverBufferSize = c_int(132072)	# Driver Buffer Size 132072
	
	op_error = 0
	time_op_start = time.time() # Store start time
	if usb_file_opened == 1:
		printlog("Programming EEPROM")
		bitstream_size = len(usb_bitstream)			 # Calculate size
		printlog("Firmware size " + str(bitstream_size))
	else:
		op.set("Error")
		printlog("ERROR: File not selected")
		op_error = 6
	# Load dll
	if op_error == 0:   # No errors in past
		#fx2dll = windll.LoadLibrary(fx2dll_name)
		cards = fx2dll.TE_USB_FX2_ScanCards()   # Call ScanCards driver function
		printlog("Found " + str(cards) + " card(s)")
		if cards == 0:
			op.set("Error")
			printlog("ERROR: No cards to connect")
			op_error = 3
	# Connect to card
	if op_error == 0:   # No errors in past
		#--if fx2dll.TE0300_Open(byref(m_handle), 0) != 0: # Open and get handle
		# int TE_USB_FX2_Open (  int CardNumber, unsigned long TimeOut, int DriverBufferSize);
		#if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms, DriverBufferSize) != 0: # Open and get handle
		if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms, DriverBufferSize) != 0: # Open and get handle
			op.set("Error")
			printlog("ERROR: Failed to connect card")
			op_error = 4
		else:
			opened = 1
			printlog("Connected to card 1")
	# Write
	if op_error == 0:   # No errors in past
		op.set("Programming")			   # Update operation label
		progressbar["value"] = 0			# Reset progressbar
		cmd = create_string_buffer(64)	  # create buffers for API call
		reply = create_string_buffer(64)	
		cmd_length = c_long(64)
		reply_length = c_long(64)
		timeout_ms = c_ulong(1000)
		printlog("Writing bitstream to EEPROM")
		wr_block_max_size = 32	  # write by 32 bytes per transfer
		cmd[0] = CMD_FX2_EEPROM_WRITE
		wr_op_cnt = 0
		for i2c_addr in range(0, bitstream_size, wr_block_max_size):
			if (bitstream_size - i2c_addr) > wr_block_max_size:
				i2c_size = wr_block_max_size			# usual transfer = 32
			else:
				i2c_size = bitstream_size - i2c_addr	# last chunk
			cmd[1] = chr((i2c_addr >> 8) & 0x00ff)	  # high part of addr
			cmd[2] = chr(i2c_addr & 0x00ff)			 # low part of addr
			cmd[3] = chr(i2c_size)					  # size
			for j in range(i2c_size):
				cmd[4 + j] = usb_bitstream[i2c_addr + j]
			#--if fx2dll.TE0300_SendCommand(m_handle, cmd, cmd_length, 
			#--byref(reply), byref(reply_length), timeout_ms) != 0:
			if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
				op.set("Error")
				printlog("ERROR: Can't call API function")
				op_error = 5
			if wr_op_cnt % 10 == 0:	# update progressbar
				complete = (i2c_addr / (bitstream_size / 100))
				progressbar["value"] = complete
				root.update()
			wr_op_cnt += 1;
	if op_error == 0:   # No errors in past	
		printlog("EEPROM Write complete")
		op.set("Done")			  # Update operation label
		progressbar["value"] = 100  # Show it 100%
		root.update()			   # Rewrite
	if opened == 1:
		#fx2dll.TE0300_Close(byref(m_handle))	# close driver connection
		fx2dll.TE_USB_FX2_Close ()  # close driver connection
		opened = 0
#-------------------------------------------------------------------------------
def spi_program():	  # SPI Flash programming 
	global fpga_bitstream
	global op_error
	global op
	global complete
	global root
	global m_handle
	global opened
	global fpga_file_opened
	global fx2dll

	#timeout_ms = c_ulong(1000)		  # Timeout 1s
	CardNumber = c_int(0)			   # Card Number 0
	DriverBufferSize = c_int(132072)	# Driver Buffer Size 132072
	#DriverBufferSize = c_int(65536)	# Driver Buffer Size 132072
	
	op_error = 0
	time_op_start = time.time() # Store start time
	if fpga_file_opened == 1:
		printlog("SPI Programming")
		bitstream_size = len(fpga_bitstream)			 # Calculate size
		printlog("Bitstream size " + str(bitstream_size))
	else:
		op.set("Error")
		printlog("ERROR: File not selected")
		op_error = 6
	# Load dll
	if op_error == 0:   # No errors in past
		#fx2dll = windll.LoadLibrary(fx2dll_name)
		cards = fx2dll.TE_USB_FX2_ScanCards()   # Call ScanCards driver function
		printlog("Found " + str(cards) + " card(s)")
		if cards == 0:
			op.set("Error")
			printlog("ERROR: No cards to connect")
			op_error = 3
	# Connect to card
	timeout_ms = c_ulong(1000)		  # Timeout 1s
	if op_error == 0:   # No errors in past
		if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms, DriverBufferSize) != 0: # Open and get handle
			op.set("Error")
			printlog("ERROR: Failed to connect card")
			op_error = 4
		else:
			opened = 1
			printlog("Connected to card 1")

	cmd = create_string_buffer(64)	  # Buffer for command
	reply = create_string_buffer(64)	# Buffer for reply
	cmd_length = c_long(64)			 # Command length always = 64
	reply_length = c_long(64)		   # Variable for reply length
	 
	# Erase Flash
	if op_error == 0:   # No errors in past
		sectors2erase = (bitstream_size >> 16) + 1 # full sectors + remainder
		#spi_erase_sectors(sectors2erase)
		spi_erase_bulk()
			
	# Write to flash
	if op_error == 0:   # No errors in past
		op.set("Preparing")		 # Update operation label
		progressbar["value"] = 0	# Reset progressbar
		wr_bitstream = create_string_buffer(bitstream_size) # Create buffer
		printlog("Prepare write buffer")	# We have to swap bits before write
		for i in range(bitstream_size):
			wr_bitstream[i] = chr(bitswap(ord(fpga_bitstream[i])))   # swap
			if i % 10000 == 0:  # update progressbar, but not often
				complete = (i * 100) / bitstream_size
				progressbar["value"] = complete
				root.update()
		printlog("Programming")
		op.set("Programming")	   # Update operation label
		progressbar["value"] = 0	# Reset progressbar
		time_write_start = time.time()  # remember start time
		wr_block_max_size = 59	  # maximum bytes to put in one write command
		spi_addr = 0				# From address 0
		wr_op_cnt = 0			   # count cycles for progressbar
		cmd[0] = CMD_FX2_FLASH_WRITE	# Set command
		while spi_addr < bitstream_size:	# cycle to the end of bitstream
			if (bitstream_size - spi_addr) > wr_block_max_size: # can write 59
				wr_block_size = wr_block_max_size
			else:   # data remainder is less than 59
				wr_block_size = bitstream_size - spi_addr
			sector_rem = 0x0000ff - (spi_addr & 0x0000ff)   # sector remainder
			if sector_rem < wr_block_max_size and sector_rem != 0:  # cross 
				wr_block_size = sector_rem + 1; # write to the end of sector
			cmd[1] = chr((spi_addr >> 16) & 0x00ff) # higgest part of addr
			cmd[2] = chr((spi_addr >> 8) & 0x00ff)  # high part of addr
			cmd[3] = chr(spi_addr & 0x00ff)		 # low part of addr
			cmd[4] = chr(wr_block_size)			 # size
			for wr_block_cnt in range(wr_block_size):   # copy data
				cmd[5 + wr_block_cnt] = wr_bitstream[spi_addr + wr_block_cnt]
			if fx2dll.TE_USB_FX2_SendCommand( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
				op.set("Error") # Update operation label
				printlog("ERROR: Can't call API function TE0300_SendCommand")
				op_error = 5
			if wr_op_cnt % 100 == 0:	# update progressbar
				complete = (spi_addr / (bitstream_size / 100))
				progressbar["value"] = complete
				root.update()
			# Check readback
			for wr_block_cnt in range(wr_block_size): # compare reply with data
				if reply[wr_block_cnt] != cmd[5 + wr_block_cnt]:
					op.set("Error")
					printlog("ERROR: Write failed at " + str(spi_addr + wr_block_cnt))
					print "Data / Reply:"
					for i in range(wr_block_size):
						print "@"+str(spi_addr + i)+ " 0x"+b2a_hex(cmd[5 + i]) + " | " + "0x"+b2a_hex(reply[i])
					op_error = 5
					break
			if op_error != 0:
				break
			spi_addr += wr_block_size   # update address
			wr_op_cnt += 1			  # increment operation counter
	if op_error == 0:   # No errors in past
		printlog("Write complete")
		printlog("Total time "+ str(round((time.time() - time_op_start))) 
		+ " seconds")
		printlog("Turn on FPGA")
		cmd[0] = CMD_FX2_POWER_ON   # Power ON FPGA
		cmd[1] = chr(1)			 # 1 = Turn ON
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			printlog("ERROR: Can't call API function TE0300_SendCommand")
			op.set("Error")
			op_error = 5
	if op_error == 0:   # No errors in past
		complete = 0
		progressbar["value"] = 0	
		root.update()   # Redraw
		time.sleep(1)   # Wait for boot
		printlog("Checking DONE pin")
		cmd[0] = CMD_FX2_READ_STATUS
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			printlog("ERROR: Can't call API function TE0300_SendCommand")
			op.set("Error")
			op_error = 5
		if ord(reply[3]) == 0:  # Check DONE
			printlog("ERROR: DONE pin is not High")
		else:
			printlog("DONE pin is High")
			op.set("Done")			  # Update operation label
			complete = 100			  # Show it 100%
			progressbar["value"] = 100  # Show it 100%
			root.update()			   # Rewrite
	if opened == 1:
		fx2dll.TE_USB_FX2_Close ()	  # close driver connection
		opened = 0
		printlog("Done")
#-------------------------------------------------------------------------------
def update_both():
	printlog("Full firmware upgrade")
	eeprom_program()
	spi_program()
#-------------------------------------------------------------------------------
# GUI
#-------------------------------------------------------------------------------
#Define font for input fields and logs
iffont = tkFont.Font(family = "Helvetica", size = 8, weight = "normal")
# Create widgets
# Row 0 - FPGA
fpga_frame = Frame(root)
fpga_file_label = Label(fpga_frame, text = "FPGA bitstream bin file",width = 25)
fpga_file_text = Text(fpga_frame, font = iffont, width = 50, height = 1)
fpga_btn_select = Button(fpga_frame, text = "...", width = 10, 
command = fpga_bitfile_select)
fpga_btn_prog = Button(fpga_frame, text = "Program FPGA", width = 24,
command = spi_program)
# Row 1 - USB
usb_frame = Frame(root)
usb_file_label = Label(usb_frame, text = "USB firmware bin file", width = 25)
usb_file_text = Text(usb_frame,font = iffont, width = 50, height = 1)
usb_btn_select = Button(usb_frame,text = "...", width = 10, 
command = usb_bitfile_select)
usb_btn_prog = Button(usb_frame, text = "Program USB", width = 24, 
command = eeprom_program)
# Row 2 - FWU
fwu_frame = Frame(root)
fwu_file_label = Label(fwu_frame, text = "Firmware upgrade file", width = 25)
fwu_file_text = Text(fwu_frame,font = iffont, width = 50, height = 1)
fwu_btn_select = Button(fwu_frame,text = "...", width = 10, command = fwu_file_select)
fwu_btn_both = Button(fwu_frame, text = "BOTH", width = 7, command = update_both)
fwu_btn_usb = Button(fwu_frame, text = "USB", width = 7, command = eeprom_program)
fwu_btn_fpga = Button(fwu_frame, text = "FPGA", width = 7, command = spi_program)

# Row 3 - Operation
op_frame = Frame(root)
op_label = Label(op_frame, textvariable = op, width = 25)
progressbar = Progressbar(op_frame, orient = HORIZONTAL, length = 430, 
mode = 'determinate')
# Row 4 - Log
log_frame = Frame(root)
log_text = Text(log_frame,font = iffont)
log_scroll = Scrollbar(log_frame)
# Placing widgets
fpga_frame.pack(side = TOP, fill = X, expand = 1)
usb_frame.pack(side = TOP, fill = X, expand = 1)
fwu_frame.pack(side = TOP, fill = X, expand = 1)
log_frame.pack(side = TOP, fill = BOTH, expand = 1)
op_frame.pack(side = TOP, fill = X, expand = 1)
# Row 0 - FPGA
fpga_file_label.pack(side = LEFT)
fpga_btn_prog.pack(side = RIGHT)
fpga_btn_select.pack(side = RIGHT)
fpga_file_text.pack(side = RIGHT, fill = X, expand = 1)
usb_file_text.config(state = DISABLED)  # Edit by user is disabled
# Row 1 - USB
usb_file_label.pack(side = LEFT)
usb_btn_prog.pack(side = RIGHT)
usb_btn_select.pack(side = RIGHT)
usb_file_text.pack(side = RIGHT, fill = X, expand = 1)
usb_file_text.config(state = DISABLED)  # Edit by user is disabled
# Row 2 - FWU
fwu_file_label.pack(side = LEFT)
fwu_btn_both.pack(side = RIGHT)
fwu_btn_usb.pack(side = RIGHT)
fwu_btn_fpga.pack(side = RIGHT)
fwu_btn_select.pack(side = RIGHT)
fwu_file_text.pack(side = RIGHT, fill = X, expand = 1)
fwu_file_text.config(state = DISABLED)  # Edit by user is disabled
# Row 3 - Operation
op_label.pack(side = LEFT)
progressbar.pack(side = RIGHT, fill = X, expand = 1)
# Row 4 Log
log_scroll.pack(side = RIGHT, fill = Y, expand = 0)
log_text.pack(side = LEFT, fill = BOTH, expand = 1)
log_scroll.config(command = log_text.yview) # connect scroll to text
log_text.config(yscrollcommand = log_scroll.set)	# coonect test to scroll
log_text.config(state = DISABLED)   # Edit by user is disabled
# Run
if(platform.architecture()[0]=="64bit"):
	fx2dll = windll.LoadLibrary(fx2dll_64_name)
else:
	fx2dll = windll.LoadLibrary(fx2dll_32_name)
root.mainloop()
