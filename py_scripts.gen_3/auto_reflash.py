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
#-------------------------------------------------------------------------------
fpga_file_opened = 0
usb_file_opened = 0
complete = 0							# Progressbar variable
op_error = 0							# Error number
opened = 0		  						# handle flag
#-------------------------------------------------------------------------------
CardNumber = c_int(0)			   		# Card Number 0
DriverBufferSize = c_int(132072)		# Driver Buffer Size 132072
timeout_ms = c_ulong(1000)		  		# Timeout 1s
cmd = create_string_buffer(64)	  		# Buffer for command
reply = create_string_buffer(64)		# Buffer for reply
cmd_length = c_long(64)			 		# Command length always = 64
reply_length = c_long(64)		   		# Variable for reply length
#-------------------------------------------------------------------------------
progress_cnt = 0
progress_sym = "-\\|/"
#-------------------------------------------------------------------------------
PACKET_LEN = 102400
packets = 600
data = create_string_buffer(PACKET_LEN * packets)
packet = create_string_buffer(PACKET_LEN)
packetlen = c_long(PACKET_LEN)
#-------------------------------------------------------------------------------
def flash_erase():
	global fx2dll
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms
	global complete
	
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
	for t in range(90):			# 45 sec (Max erase time for W25Q128 is 40 sec)
		time.sleep(0.5)
		complete = (t * 100) / 90 # progressbar
		print str(int(complete)) + "%\r",
		# testing Busy Flag
		cmd[0] = CMD_FX2_READ_STATUS
		reply[2] = chr(1)
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
		return 0

#-------------------------------------------------------------------------------
def flash_write(bitstream):
	global fx2dll
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms

	wr_block_max_size = 59	  			# maximum bytes to put in one write command
	spi_addr = 0						# From address 0
	wr_op_cnt = 0			   			# count cycles for progressbar
	bitstream_size = len(bitstream)		# Calculate size

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
				#print "Data / Reply:"
				#for i in range(wr_block_size):
				#	print "@"+str(spi_addr + i)+ " 0x"+b2a_hex(cmd[5 + i]) + " | " + "0x"+b2a_hex(reply[i])
				return 2
				
		spi_addr += wr_block_size   # update address
		wr_op_cnt += 1			  # increment operation counter
	return 0

#-------------------------------------------------------------------------------
def power_on():
	global fx2dll
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
	global fx2dll
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms

	cmd[0] = CMD_FX2_READ_STATUS
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function TE0300_SendCommand"
		return 1
	
#	if ord(reply[3]) == 0:  # Check DONE
	if ord(reply[4]) == 0:  # Check DONE
		return 2
	else:
		return 0

def get_mb_ver():
	global fx2dll
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms

	# Configure autoresponce engine
	cmd[0] = CMD_FX2_SET_INTERRUPT
	cmd[1] = MB_I2C_ADRESS
	cmd[2] = I2C_BYTES
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	# Read autoresponce (to clear buffer)
	cmd[0] = CMD_FX2_GET_INTERRUPT
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1
	#-------------------------------------------------------------------------------
	# Get Microblaze firmware version
	# Send command to start I2C read of Microblaze FW version 
	cmd[0] = CMD_FX2_I2C_WRITE
	cmd[1] = MB_I2C_ADRESS
	cmd[2] = I2C_BYTES
	cmd[3] = chr(0)
	cmd[4] = chr(0)
	cmd[5] = chr(0)
	cmd[6] = CMD_MB_GETVERSION
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	# Pull till Microblaze reply
	cmd[0] = CMD_FX2_GET_INTERRUPT
	reply[0] = chr(0)	# in this byte FX2 will return number of received I2C packets
	while reply[0] == chr(0):
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
			print "ERROR: Can't call API function"
			return 1

	if(ord(reply[1]) == 0xFF and ord(reply[2]) == 0xFF and ord(reply[3]) == 0xFF and ord(reply[4]) == 0xFF):
		return 1
	else:
		print "* Microblaze Firmware v. " + str(ord(reply[1])) + "." + str(ord(reply[2])) + " r" + str(ord(reply[3])) + " Build " + str(ord(reply[4]))
	return 0

def run_int_test():
	global fx2dll
	global cmd
	global cmd_length
	global reply
	global reply_length
	global timeout_ms
	global progress_sym
	global progress_cnt
	global packet
	global packetlen
	
	#get_mb_ver()
	#-------------------------------------------------------------------------------
	# Run Memory test
	# Send command to start Microblaze Memory test
	print "SDRAM Memory test"
	print "* Send memory test command"
	cmd[0] = CMD_FX2_I2C_WRITE
	cmd[1] = MB_I2C_ADRESS
	cmd[2] = I2C_BYTES
	cmd[3] = chr(0)
	cmd[4] = chr(0)
	cmd[5] = chr(0)
	cmd[6] = CMD_MB_TEST
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	# Read autoresponce (to clear buffer)
	print "* Read echo"
	cmd[0] = CMD_FX2_GET_INTERRUPT
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	# Read autoresponce with test result
	pull_cnt = 0
	# Pull till Microblaze reply
	print "* Pull for test result"
	cmd[0] = CMD_FX2_GET_INTERRUPT
	reply[0] = chr(0)	# in this byte FX2 will return number of received I2C packets
	while reply[0] == chr(0):
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
			print "ERROR: Can't call API function"
			return 1
		pull_cnt = pull_cnt + 1
		if((pull_cnt % 1000) == 0):
			pull_cnt = 0
			print progress_sym[progress_cnt] + "\r",
			if(progress_cnt == 3):
				progress_cnt = 0
			else:
				progress_cnt += 1
			
	if(ord(reply[4]) == 1):
		print "Memory test PASSED!!!"
	else:
		if(ord(reply[4]) == 2):
			print "ERROR: Memory test FAILED!"
			return 2
		else:
			print "ERROR: Unexpected result of memory test"
			return 3
	
	#############################################################
	print "Starting Host -> FX2 -> FPGA (w) test"
	print "* Resetiing FX2 FIFOs"
	cmd[0] = CMD_FX2_RESET_FIFO_STATUS
	cmd[1] = chr(0)
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1
	# Send command to start test 
	print "* Send Start RX"
	cmd[0] = CMD_FX2_I2C_WRITE
	cmd[1] = MB_I2C_ADRESS
	cmd[2] = I2C_BYTES
	cmd[3] = chr(0)
	cmd[4] = chr(0)
	cmd[5] = chr(0)
	cmd[6] = CMD_MB_START_RX
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	# Read autoresponce (to clear buffer)
	cmd[0] = CMD_FX2_GET_INTERRUPT
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	print "* Sending..."
	pull_cnt = 0
	for i in range(600):
		for j in range(102400):
			packet[j] = data[i*102400 + j]
		if fx2dll.TE_USB_FX2_SetData(byref(packet), packetlen) != 0:
			print "ERROR: Can't write packet " + str(i) + " to endpoint"
			return 1
		pull_cnt = pull_cnt + 1
		if((pull_cnt % 20) == 0):
			pull_cnt = 0
			print progress_sym[progress_cnt] + "\r",
			if(progress_cnt == 3):
				progress_cnt = 0
			else:
				progress_cnt += 1

	print "* Send Stop"
	cmd[0] = CMD_FX2_I2C_WRITE
	cmd[1] = MB_I2C_ADRESS
	cmd[2] = I2C_BYTES
	cmd[3] = chr(0)
	cmd[4] = chr(0)
	cmd[5] = chr(0)
	cmd[6] = CMD_MB_STOP
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1
	
	print "* Read echo"
	cmd[0] = CMD_FX2_GET_INTERRUPT
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1
	
	cmd[0] = CMD_FX2_GET_INTERRUPT
	reply[0] = chr(0)	# in this byte FX2 will return number of received I2C packets
	print "* Pull status..."
	pull_cnt = 0
	while reply[0] == chr(0):
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
			print "ERROR: Can't call API function"
			return 1
		pull_cnt = pull_cnt + 1
		if((pull_cnt % 1000) == 0):
			pull_cnt = 0
			print progress_sym[progress_cnt] + "\r",
			if(progress_cnt == 3):
				progress_cnt = 0
			else:
				progress_cnt += 1

	if(ord(reply[4]) == 1):
		print "host->memory data verification PASSED!"
	else:
		print "ERROR: host->memory data verification FAILED!"
		# Passed test indication
		cmd[0] = CMD_FX2_I2C_WRITE
		cmd[1] = MB_I2C_ADRESS
		cmd[2] = I2C_BYTES
		cmd[3] = chr(0)
		cmd[4] = chr(0)
		cmd[5] = chr(0)
		cmd[6] = CMD_MB_ERROR
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
			print "ERROR: Can't call API function"
			return 1
		return 3

	#############################################################
	print "Starting FPGA -> FX2 -> Host (r) test"
	print "* Resetiing FX2 FIFOs"
	cmd[0] = CMD_FX2_RESET_FIFO_STATUS
	cmd[1] = chr(0)
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1

	print "* Send Start TX"
	cmd[0] = CMD_FX2_I2C_WRITE
	cmd[1] = MB_I2C_ADRESS
	cmd[2] = I2C_BYTES
	cmd[3] = chr(0)
	cmd[4] = chr(0)
	cmd[5] = chr(0)
	cmd[6] = CMD_MB_START_TX
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		return 1
		
	rx_errors = 0;
	pull_cnt = 0
	print "* Reseiving..."
	for i in range(packets):
		if fx2dll.TE_USB_FX2_GetData(byref(packet), packetlen) != 0:
			print "ERROR: Can't read packet " + str(i) + " from endpoint"
			return 1
		for j in range(PACKET_LEN):
			if(packet[j] != data[i*PACKET_LEN + j]):
				rx_errors += 1;
		pull_cnt = pull_cnt + 1
		if((pull_cnt % 20) == 0):
			pull_cnt = 0
			print progress_sym[progress_cnt] + "\r",
			if(progress_cnt == 3):
				progress_cnt = 0
			else:
				progress_cnt += 1

	if rx_errors != 0:
		print "memory->host data verification FAILED: "+str(rx_errors)+" ERRORS"
		# Failed test indication
		cmd[0] = CMD_FX2_I2C_WRITE
		cmd[1] = MB_I2C_ADRESS
		cmd[2] = I2C_BYTES
		cmd[3] = chr(0)
		cmd[4] = chr(0)
		cmd[5] = chr(0)
		cmd[6] = CMD_MB_ERROR
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
			print "ERROR: Can't call API function"
			return 1
		return 2
	else:
		print "memory->host data verification PASSED!!!"
		# Passed test indication
		cmd[0] = CMD_FX2_I2C_WRITE
		cmd[1] = MB_I2C_ADRESS
		cmd[2] = I2C_BYTES
		cmd[3] = chr(0)
		cmd[4] = chr(0)
		cmd[5] = chr(0)
		cmd[6] = CMD_MB_PASSED
		if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
			print "ERROR: Can't call API function"
			return 1
		return 0

#-------------------------------------------------------------------------------
#-------------------------------------------------------------------------------
# Main
#-------------------------------------------------------------------------------

print "=== TE Auto Reflash G3 " + version + " ==="
if(platform.architecture()[0]=="64bit"):
	print "* Loading dll for 64 bit system"
	fx2dll = windll.LoadLibrary(fx2dll_64_name)
else:
	print "* Loading dll for 32 bit system"
	fx2dll = windll.LoadLibrary(fx2dll_32_name)
	
if len(sys.argv) != 2:
	print "ERROR: No bit file specified"
	print "usage: python reflash.py bitstream.bit"
	sys.exit()
	
if not os.path.isfile(sys.argv[1]):
	print "ERROR: File " + sys.argv[1] + " not exist"
	sys.exit()
	
print "* Loading bitstream from " + sys.argv[1]
fpga_bin_file = open(sys.argv[1], 'rb')	# Open in binary mode
fpga_bitstream = fpga_bin_file.read()	# Read
fpga_bin_file.close()					# Close
bitstream_size = len(fpga_bitstream)	# Calculate size
print "* Bitstream size " + str(bitstream_size)
#-------------------------------------------------------------------------------

print "* Filling testdata buffer..."
progress_cnt = 0
progress_sym = "-\\|/"
total_cnt = PACKET_LEN * packets / 4
for j in range(total_cnt):
	data[j * 4 + 0] = chr((j & 0xFF000000) >> 24)
	data[j * 4 + 1] = chr((j & 0x00FF0000) >> 16)
	data[j * 4 + 2] = chr((j & 0x0000FF00) >>  8)
	data[j * 4 + 3] = chr((j & 0x000000FF))
	if(j % PACKET_LEN == 0):
		print progress_sym[progress_cnt] + "\r",
		if(progress_cnt == 3):
			progress_cnt = 0
		else:
			progress_cnt += 1
print "Waiting for module connection..."
#------------------------------------------------------------------------------
# Main loop
#------------------------------------------------------------------------------
while 1:
	connected = 0
	cards = fx2dll.TE_USB_FX2_ScanCards()   # Call ScanCards driver function
	#print "Found " + str(cards) + " card(s)"
	if cards != 0:
		for card in range(cards):
			CardNumber = c_int(card)
			if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms, DriverBufferSize) != 0: # Open and get handle
				print "ERROR: Failed to connect card " + str(card)
			else:
				connected = 1
				cmd[0] = CMD_FX2_DEV_LOCK
				cmd[1] = chr(1)					# Lock
				if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
					print "ERROR: Can't call API function SendCommand"
					break

				if ord(reply[0]) == 0x22:
					print "* Working with card " + str(card)
					cmd[0] = CMD_FX2_READ_VERSION
					if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
						print "ERROR: Can't call API function SendCommand"
						break
					else:
						print "* FX2 firmware Ver " + str(ord(reply[0])) + "." + str(ord(reply[1]))
						time.sleep(0.7)	# Sleep some time
						################################################
#						if run_int_test() == 0:
#							print "Done"
#						else:
#							print "ERROR: Can't run test"
						################################################
						print "Erasing Flash"
						if flash_erase() == 0:
							print "* Erase complete"
							print "Programming Flash"
							if flash_write(fpga_bitstream) == 0:
								print "* Done"
								print "* Turn on FPGA power"
								power_on()
								time.sleep(5)	# Sleep some time
								print "* Checking DONE pin"
								if check_done() == 0:
									print "* DONE pin is High"
									time.sleep(2)	# Sleep some time
									print "* Read Microblaze firmware version"
									mb_ver = get_mb_ver()
									if mb_ver == 0:
										#print "* Start Memory test"
										if run_int_test() == 0:
											print "Done"
										else:
											print "ERROR: Internal test failed"
									else:
										print "ERROR: Can't read MB version"
								else:
									print "ERROR: DONE pin is not High"
			
			if connected == 1:
				fx2dll.TE_USB_FX2_Close ()	  # close driver connection
	print progress_sym[progress_cnt] + "\r",
	if(progress_cnt == 3):
		progress_cnt = 0
	else:
		progress_cnt += 1
	time.sleep(0.7)	# Sleep some time
#------------------------------------------------------------------------------
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

	