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


CMD_FLASH_WRITE_COMMAND	= b'\xAA'
SPI_WREN = b'\x06'
SPI_WRSR = b'\x01'
SPI_RDSR1 = b'\x05'

CardNumber = c_int(0)			   		# Card Number 0
DriverBufferSize = c_int(132072)		# Driver Buffer Size 132072
timeout_ms = c_ulong(1000)		  		# Timeout 1s
cmd = create_string_buffer(64)	  		# Buffer for command
reply = create_string_buffer(64)		# Buffer for reply
cmd_length = c_long(64)			 		# Command length always = 64
reply_length = c_long(64)		   		# Variable for reply length

#-------------------------------------------------------------------------------
def flash_unlock():
	cmd[0] = CMD_FLASH_WRITE_COMMAND   	# Send command tp SPI
	cmd[1] = b'\x01'					# 1 byte command
	cmd[2] = b'\x01'					# Bytes to read
	cmd[3] = SPI_RDSR1					# Write status register
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function TE0300_SendCommand"
		return 1
	print "Status before 0x%02x" % (ord(reply[1]))
	cmd[0] = CMD_FLASH_WRITE_COMMAND   	# Send command tp SPI
	cmd[1] = b'\x01'					# 1 byte command
	cmd[2] = b'\x00'					# Bytes to read
	cmd[3] = SPI_WREN					# Write enable
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function TE0300_SendCommand"
		return 1
	cmd[0] = CMD_FLASH_WRITE_COMMAND   	# Send command tp SPI
	cmd[1] = b'\x02'					# 2 byte command
	cmd[2] = b'\x00'					# Bytes to read
	cmd[3] = SPI_WRSR					# Write status register
	cmd[4] = b'\x00'					# clear all bits
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function TE0300_SendCommand"
		return 1
	cmd[0] = CMD_FLASH_WRITE_COMMAND   	# Send command tp SPI
	cmd[1] = b'\x01'					# 1 byte command
	cmd[2] = b'\x01'					# Bytes to read
	cmd[3] = SPI_RDSR1					# Write status register
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API
		print "ERROR: Can't call API function TE0300_SendCommand"
		return 1
	print "Status after  0x%02x" % (ord(reply[1]))
#-------------------------------------------------------------------------------
# Run
print "TE Unlock G3 " + version
if(platform.architecture()[0]=="64bit"):
	print "Loading dll for 64 bit system"
	fx2dll = windll.LoadLibrary(fx2dll_64_name)
else:
	print "Loading dll for 32 bit system"
	fx2dll = windll.LoadLibrary(fx2dll_32_name)
	
	
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

	
flash_unlock()
fx2dll.TE_USB_FX2_Close ()	  # close driver connection
print "Done"
	