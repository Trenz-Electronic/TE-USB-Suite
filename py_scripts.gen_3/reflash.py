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
import sys
import os.path

from ttk import *
from ctypes import *
from binascii import *
from pkg_fx2def import *
import time
import platform
#-------------------------------------------------------------------------------
version = " v0.01 Beta"
# Define later used variables
usb_bitstream = None
fpga_bitstream = None
fx2dll_32_name = "TE_USB_FX2_API_C-32.dll"
fx2dll_64_name = "TE_USB_FX2_API_C-64.dll"
fx2dll = None
fpga_file_opened = 0
usb_file_opened = 0
complete = 0							# Progressbar variable
op_error = 0							# Error number
opened = 0		  						# handle flag

CardNumber = c_int(0)			   		# Card Number 0
DriverBufferSize = c_int(132072)		# Driver Buffer Size 132072
timeout_ms = c_ulong(1000)		  		# Timeout 1s
cmd = create_string_buffer(64)	  		# Buffer for command
reply = create_string_buffer(64)		# Buffer for reply
cmd_length = c_long(64)			 		# Command length always = 64
reply_length = c_long(64)		   		# Variable for reply length

def flash_erase():
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms
	
	print "Erasing Flash"
	cmd[0] = CMD_FX2_READ_STATUS
	for t in range(7):
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			print "ERROR: Can't call API function TE0300_SendCommand"
	if ord(reply[2]) != 0:
		print "Flash busy"
		return 1
	time.sleep(0.5)	# Sleep some time

	flash_busy = 0
	while flash_busy == 0:					# Flash should be busy after erase command
		cmd[0] = CMD_FX2_FLASH_ERASE		# Command
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			print "ERROR: Flash Erase> Can't call API function"
		cmd[0] = CMD_FX2_READ_STATUS		# Command
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			print "ERROR: Can't call API function TE0300_SendCommand"
		flash_busy = ord(reply[2])

	erase_complete = 0
	for t in range(70):
		time.sleep(0.5)
		complete = (t * 100) / 70 # progressbar
		print str(int(complete)) + "%\r",
		# testing Busy Flag
		cmd[0] = CMD_FX2_READ_STATUS
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			print "ERROR: Can't call API function TE0300_SendCommand"
		if ord(reply[2]) == 0:
			erase_complete = 1
			break
	# Time is over Check if we finish erasing
	if erase_complete == 0:
		print "ERROR: Flash busy after chip erase"
		return 1
	else:
		print "Erase complete"
		return 0

#-------------------------------------------------------------------------------
def flash_write(bitstream):
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms

	wr_block_max_size = 59	  			# maximum bytes to put in one write command
	spi_addr = 0						# From address 0
	wr_op_cnt = 0			   			# count cycles for progressbar
	
	offset = bitstream_size
	for o in range(bitstream_size):
		if(ord(bitstream[o]) == 0xFF):
			offset = o
			break
	
	if(offset == bitstream_size):
		print "Can't find starttup sequence in binary data"
		return 7
	else:
		print "Writing binary data from offset " + str(offset)

	while spi_addr < (bitstream_size - offset):	# cycle to the end of bitstream
		if ((bitstream_size - offset) - spi_addr) > wr_block_max_size: # can write 59
			wr_block_size = wr_block_max_size
		else:   						# data remainder is less than 59
			wr_block_size = (bitstream_size - offset) - spi_addr
		
		sector_rem = 0x0000ff - (spi_addr & 0x0000ff)   # sector remainder
	
		if sector_rem < wr_block_size and sector_rem != 0:  # cross 
			wr_block_size = sector_rem + 1; # write to the end of sector
		
		cmd[0] = CMD_FX2_FLASH_WRITE		# Set command
		cmd[1] = chr((spi_addr >> 16) & 0x00ff) # higgest part of addr
		cmd[2] = chr((spi_addr >> 8) & 0x00ff)  # high part of addr
		cmd[3] = chr(spi_addr & 0x00ff)		 # low part of addr
		cmd[4] = chr(wr_block_size)			 # size
	
		for wr_block_cnt in range(wr_block_size):   # copy data
			cmd[5 + wr_block_cnt] = chr(ord(bitstream[offset + spi_addr + wr_block_cnt]))

		if fx2dll.TE_USB_FX2_SendCommand( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
			print "ERROR: Can't call API function"
			return 1
		
		if wr_op_cnt % 100 == 0:	# update progressbar
			complete = (spi_addr / ((bitstream_size - offset) / 100))
			print str(int(complete)) + "%\r",
		
		for wr_block_cnt in range(wr_block_size): # compare reply with data
			if reply[wr_block_cnt] != cmd[5 + wr_block_cnt]:
				print "ERROR: Write failed at " + str(spi_addr + wr_block_cnt)
				print "Data / Reply:"
				for i in range(wr_block_size):
					print "@"+str(spi_addr + i)+ " 0x"+b2a_hex(cmd[5 + i]) + " | " + "0x"+b2a_hex(reply[i])
				return 2
				
		spi_addr += wr_block_size   # update address
		wr_op_cnt += 1			  # increment operation counter
	return 0

#-------------------------------------------------------------------------------
def power_on():
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms

	cmd[0] = CMD_FX2_POWER_ON   # Power ON FPGA
	cmd[1] = chr(1)			 # 1 = Turn ON
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function"
		return 1
	return 0
#-------------------------------------------------------------------------------
def check_done():
	cmd[0] = CMD_FX2_READ_STATUS
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function TE0300_SendCommand"
		return 1
	
	if ord(reply[3]) == 0:  # Check DONE
		return 2
	else:
		return 0
#-------------------------------------------------------------------------------
# Run
print "TE Reflash G3 " + version
if(platform.architecture()[0]=="64bit"):
	print "Loading dll for 64 bit system"
	fx2dll = windll.LoadLibrary(fx2dll_64_name)
else:
	print "Loading dll for 32 bit system"
	fx2dll = windll.LoadLibrary(fx2dll_32_name)
	
if len(sys.argv) != 2:
	print "ERROR: No bit file specified"
	print "usage: python reflash.py bitstream.bit"
	sys.exit()
	
if not os.path.isfile(sys.argv[1]):
	print "ERROR: File " + sys.argv[1] + " not exist"
	sys.exit()
	
print "Loading bitstream from " + sys.argv[1]
fpga_bin_file = open(sys.argv[1], 'rb')	# Open in binary mode
fpga_bitstream = fpga_bin_file.read()	# Read
fpga_bin_file.close()					# Close
bitstream_size = len(fpga_bitstream)	# Calculate size
print "Bitstream size " + str(bitstream_size)


#time_start = time.time() 				# Store start time
cards = fx2dll.TE_USB_FX2_ScanCards()   # Call ScanCards driver function
print "Found " + str(cards) + " card(s)"
if cards == 0:
	print "ERROR: No cards to connect"
	sys.exit()

if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms, DriverBufferSize) != 0: # Open and get handle
	print "ERROR: Failed to connect card"
	sys.exit()
else:
	print "Connected to card 1"

#	time_write_start = time.time()  	# remember start time

if flash_erase() == 0:
	print "Programming Flash"
	if flash_write(fpga_bitstream) == 0:
		print "Done"

		print "Turn on FPGA power"
		power_on()
	
		print "Checking DONE pin"
		if check_done() == 0:
			print "DONE pin is High"
		else:
			print "ERROR: DONE pin is not High"

#time_end = time.time()
#time_total = round((time_end - time_start),1)
#time_erase = round((time_write_start - time_start),1)
#time_programing = round((time_end - time_write_start),1)
#print "Total time " + str(time_total) + " s (erase " + str(time_erase) + " s / program " + str(time_programing) + " s)"
	
	
fx2dll.TE_USB_FX2_Close ()	  # close driver connection
print "Done"
	