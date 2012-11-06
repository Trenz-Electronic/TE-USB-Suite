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
from ctypes import *
from binascii import *
from pkg_fx2def import *
import time
import platform
import sys
#-------------------------------------------------------------------------------
version = " v0.01 Beta"
fx2dll_32_name = "TE_USB_FX2_CyAPI_32.dll"
fx2dll_64_name = "TE_USB_FX2_CyAPI_64.dll"
fx2dll = None
#-------------------------------------------------------------------------------
# CLI
#-------------------------------------------------------------------------------
# Run
if(platform.architecture()[0]=="64bit"):
	fx2dll = windll.LoadLibrary(fx2dll_64_name)
else:
	fx2dll = windll.LoadLibrary(fx2dll_32_name)

cards = fx2dll.TE_USB_FX2_ScanCards()   # Call ScanCards driver function
print "Found " + str(cards) + " card(s)"
if cards == 0:
	print "ERROR: No cards to connect"
	sys.exit()

timeout_ms = c_ulong(1000)			# Timeout 1s
CardNumber = c_int(0)				# Card Number 0
DriverBufferSize = c_int(132072)	# Driver Buffer Size 132072
if fx2dll.TE_USB_FX2_Open(CardNumber, timeout_ms) != 0: # Open and get handle
	print "ERROR: Failed to connect card"
	sys.exit()
print "Connected to card 1"

cmd = create_string_buffer(64)	  # create buffers for API call
reply = create_string_buffer(64)	
cmd_length = c_long(64)
reply_length = c_long(64)
timeout_ms = c_ulong(1000)
# Configure autoresponce engine
cmd[0] = CMD_FX2_SET_INTERRUPT
cmd[1] = MB_I2C_ADRESS
cmd[2] = I2C_BYTES
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()

# Read autoresponce (to clear buffer)
cmd[0] = CMD_FX2_GET_INTERRUPT
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()
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
	sys.exit()

# Pull till Microblaze reply
cmd[0] = CMD_FX2_GET_INTERRUPT
reply[0] = chr(0)	# in this byte FX2 will return number of received I2C packets
while reply[0] == chr(0):
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		sys.exit()

print "Microblaze Firmware v. " + str(ord(reply[1])) + "." + str(ord(reply[2])) + " r" + str(ord(reply[3])) + " Build " + str(ord(reply[4]))
#-------------------------------------------------------------------------------
PACKET_LEN = 102400
packets = 600
data = create_string_buffer(PACKET_LEN * packets)
packet = create_string_buffer(PACKET_LEN)
#-------------------------------------------------------------------------------
# Host -> FX2 -> FPGA (w) test
#RX_PACKET_LEN = 102400
RX_PACKET_LEN = 1024
#rx_packets = 600
rx_packets = 600
rx_packetlen = c_long(RX_PACKET_LEN)

print "Filling buffer..."
progress_cnt = 0
#progress_sym = "->|<"
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

print "Resetiing FX2 FIFOs"
cmd[0] = CMD_FX2_RESET_FIFO_STATUS
cmd[1] = chr(0)
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()

# Send command to start test 
print "Send Start RX"
cmd[0] = CMD_FX2_I2C_WRITE
cmd[1] = MB_I2C_ADRESS
cmd[2] = I2C_BYTES
cmd[3] = chr(0)
cmd[4] = chr(0)
cmd[5] = chr(0)
cmd[6] = CMD_MB_START_RX
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()

# Read autoresponce (to clear buffer)
cmd[0] = CMD_FX2_GET_INTERRUPT
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()
# Response from "Start RX" command

time_write_start = time.time()  # remember start time

for i in range(rx_packets):
	for j in range(RX_PACKET_LEN):
		packet[j] = data[i*RX_PACKET_LEN + j]
	if fx2dll.TE_USB_FX2_SetData(byref(packet), rx_packetlen) != 0:
		print "ERROR: Can't write packet " + str(i) + " to endpoint"
		sys.exit()

time_write_end = time.time()  # remember end time

#SendFPGAcommand(handle,FX22MB_REG0_STOP); //stops test
print "Send Stop"
cmd[0] = CMD_FX2_I2C_WRITE
cmd[1] = MB_I2C_ADRESS
cmd[2] = I2C_BYTES
cmd[3] = chr(0)
cmd[4] = chr(0)
cmd[5] = chr(0)
cmd[6] = CMD_MB_STOP
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()
#GetFPGAstatus(handle)
# Read autoresponce (to clear buffer)
print "Read dummy"
cmd[0] = CMD_FX2_GET_INTERRUPT
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()

# Read autoresponce with test result
# Pull till Microblaze reply
cmd[0] = CMD_FX2_GET_INTERRUPT
reply[0] = chr(0)	# in this byte FX2 will return number of received I2C packets
print "Pull status"
while reply[0] == chr(0):
	if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
		print "ERROR: Can't call API function"
		sys.exit()

if(ord(reply[4]) == 1):
	print "host->memory data verification PASSED!"
else:
	print "host->memory data verification FAILED!"
	sys.exit()

elapsed_time = time_write_end - time_write_start
print "Transferred "+str(total_cnt/1024)+" kB in "+str(elapsed_time)+" s = "+str(total_cnt/(elapsed_time*1024*1024))+" MB/s\r\n"
#-------------------------------------------------------------------------------
print "Done"
sys.exit()
#-------------------------------------------------------------------------------
#-------------------------------------------------------------------------------
# FPGA -> FX2 -> Host (r) test
TX_PACKET_LEN = 102400
tx_packets = 600
tx_packetlen = c_long(TX_PACKET_LEN)

print "Resetiing FX2 FIFOs"
cmd[0] = CMD_FX2_RESET_FIFO_STATUS
cmd[1] = chr(0)
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()

print "Send Start TX"
cmd[0] = CMD_FX2_I2C_WRITE
cmd[1] = MB_I2C_ADRESS
cmd[2] = I2C_BYTES
cmd[3] = chr(0)
cmd[4] = chr(0)
cmd[5] = chr(0)
cmd[6] = CMD_MB_START_TX
if fx2dll.TE_USB_FX2_SendCommand ( byref(cmd), cmd_length, byref(reply), reply_length, timeout_ms) != 0:	# call API	
	print "ERROR: Can't call API function"
	sys.exit()

time_write_start = time.time()  # remember start time

rx_errors = 0;
for i in range(tx_packets):
	if fx2dll.TE_USB_FX2_GetData(byref(packet), tx_packetlen) != 0:
		print "ERROR: Can't read packet " + str(i) + " from endpoint"
		sys.exit()
	for j in range(TX_PACKET_LEN):
		if(packet[j] != data[i*TX_PACKET_LEN + j]):
			rx_errors += 1;
	
time_write_end = time.time()  # remember end time
elapsed_time = time_write_end - time_write_start

if rx_errors != 0:
	print "memory->host data verification FAILED: "+str(rx_errors)+" ERRORS"
	sys.exit()
else:
	print "memory->host data verification PASSED!!!"

print "Transferred "+str(total_cnt/1024)+" kB in "+str(elapsed_time)+" s = "+str(total_cnt/(elapsed_time*1024*1024))+" MB/s"
#-------------------------------------------------------------------------------
print "Done"
	