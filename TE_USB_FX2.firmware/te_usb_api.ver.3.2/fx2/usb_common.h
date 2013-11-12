/* -*- c++ -*- */
/*-----------------------------------------------------------------------------
 * Common USB code for FX2
 *-----------------------------------------------------------------------------
 * Code taken from USRP2 firmware (GNU Radio Project), version 3.0.2,
 * Copyright 2003 Free Software Foundation, Inc.
 *-----------------------------------------------------------------------------
 * This code is part of usbserial. It's is free software; you can redistribute
 * it and/or modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 2 of the License,
 * or (at your option) any later version. usbjtag is distributed in the hope
 * that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.  You should have received a
 * copy of the GNU General Public License along with this program in the file
 * COPYING; if not, write to the Free Software Foundation, Inc., 51 Franklin
 * St, Fifth Floor, Boston, MA  02110-1301  USA
 *-----------------------------------------------------------------------------
 */

#ifndef _USB_COMMON_H_
#define _USB_COMMON_H_

#define	bRequestType	SETUPDAT[0]
#define	bRequest		  SETUPDAT[1]
#define	wValueL			  SETUPDAT[2]
#define	wValueH			  SETUPDAT[3]
#define	wIndexL			  SETUPDAT[4]
#define	wIndexH			  SETUPDAT[5]
#define	wLengthL		  SETUPDAT[6]
#define	wLengthH		  SETUPDAT[7]

#define MSB(x)	(((unsigned short) x) >> 8)
#define LSB(x)	(((unsigned short) x) & 0xff)

//It is defined in usb_common.c
extern volatile __bit _usb_got_SUDAV;

// Provided by user application to havdle serial line changes
void process_line_coding(void);
void usb_install_handlers (void);

/** Handles the setup package and the basic device requests like reading 
 *  descriptors, get/set confifuration etc. 
 */
void usb_handle_setup_packet (void);

// Macro to check if new setup data is available: it is syntactic sugar.
// See usb_common.c. 
// It is used in fw.c.
#define usb_setup_packet_avail()	_usb_got_SUDAV

#endif /* _USB_COMMON_H_ */
