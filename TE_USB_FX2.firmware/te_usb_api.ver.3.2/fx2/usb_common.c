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

#include "usb_common.h"
#include "fx2regs.h"
#include "syncdelay.h"
#include "fx2utils.h"
#include "isr.h"
#include "usb_descriptors.h"
#include "usb_requests.h"

//extern __xdata char str0[];
//extern __xdata char str1[];
//extern __xdata char str2[];
//extern __xdata char str3[];
//extern __xdata char str4[];
//extern __xdata char str5[];

//This bit is a flag for the arrival of a SETUP USB PACKET.
//Global variable set by SUDAV isr (_usb_got_SUDAV, usb SetUp Data AVailable). 
//In usb_common.h is redefined using a macro as usb_setup_packet_avail(). It is syntactic sugar.
volatile __bit _usb_got_SUDAV;

unsigned char	_usb_config = 0;
unsigned char	_usb_alt_setting = 0;	// FIXME really 1/interface

__xdata unsigned char *current_device_descr;
__xdata unsigned char *current_devqual_descr;
__xdata unsigned char *current_config_descr;
__xdata unsigned char *other_config_descr;

//volatile __xdata BYTE LineCode[7] = {0x60,0x09,0x00,0x00,0x00,0x00,0x08};

//static void setup_descriptors (void){
void setup_descriptors (void){
	if (USBCS & bmHSM){		// high speed mode
		current_device_descr  = high_speed_device_descr;
		current_devqual_descr = high_speed_devqual_descr;
		current_config_descr  = high_speed_config_descr;
		other_config_descr    = full_speed_config_descr;
	}
	else {
		current_device_descr  = full_speed_device_descr;
		current_devqual_descr = full_speed_devqual_descr;
		current_config_descr  = full_speed_config_descr;
		other_config_descr    = high_speed_config_descr;
	}
}

//SUDAV ISR
/*SUDAV ISR should set a flag ( _usb_got_SUDAV) and return. 
In the main polling loop (aka superloop while(1) in fw.c) the flag change is detected by 
"if(usb_setup_packet_avail()"  (it is equal to "if(_usb_got_SUDAV)"). 
If _usb_got_SUDAV=1, the usb_handle_setup_packet() in fw.c (and defined in usbcommon.c) process the data from SETUP USB PACKET.  
Remember that the 8051 runs "slowly" compared to the USB bus.  You just don't have time to do a big switch statement 
inside the ISR and in any case is not normally good practice to put "heavy" function inside an ISR. 
The ISR should exit and then "jump" to the desired "heavy" function (if any).*/
static void isr_SUDAV (void) __interrupt{
	clear_usb_irq ();
	_usb_got_SUDAV = 1;
}

static void isr_USBRESET (void) __interrupt{
	clear_usb_irq ();
	setup_descriptors ();
}

static void
isr_HIGHSPEED (void) __interrupt{
	clear_usb_irq ();
	setup_descriptors ();
}

void usb_install_handlers (void){
	setup_descriptors ();	    // ensure that they're set before use

	hook_uv (UV_SUDAV,     (unsigned short) isr_SUDAV);
	//used in "if(usb_setup_packet_avail())" inside the superloop while(1) of fw.c
	
	hook_uv (UV_USBRESET,  (unsigned short) isr_USBRESET);
	hook_uv (UV_HIGHSPEED, (unsigned short) isr_HIGHSPEED);
	USBIE = bmSUDAV | bmURES | bmHSGRANT;
}

// On the FX2 the only plausible endpoints are 0, 1, 2, 4, 6, 8
// This doesn't check to see that they're enabled
unsigned char plausible_endpoint (unsigned char ep){
	ep &= ~0x80;	// ignore direction bit
	
	if (ep > 8)
		return 0;

	if (ep == 1)
		return 1;

	return (ep & 0x1) == 0;	// must be even
}

// return pointer to control and status register for endpoint.
// only called with plausible_endpoints

__xdata volatile unsigned char *
epcs (unsigned char ep)
{
	if (ep == 0x01)		// ep1 has different in and out CS regs
		return (__xdata volatile unsigned char *)EP1OUTCS;

	if (ep == 0x81)
		return (__xdata volatile unsigned char *)EP1INCS;

	ep &= ~0x80;			// ignore direction bit

	if (ep == 0x00)		// ep0
		return (__xdata volatile unsigned char *)EP0CS;
		
	return (__xdata volatile unsigned char *)(EP2CS + (ep >> 1));	// 2, 4, 6, 8 are consecutive
}

void usb_handle_setup_packet (void)
{
	_usb_got_SUDAV = 0;
	switch (bRequest){
		case RQ_GET_INTERFACE:
			EP0BUF[0] = _usb_alt_setting;	// FIXME app should handle
			EP0BCH = 0;
			EP0BCL = 1;
		break;

		case RQ_GET_DESCR:
			switch (wValueH){
				case DT_DEVICE:
					SUDPTRH = MSB (current_device_descr);
					SUDPTRL = LSB (current_device_descr);
				break;
				case DT_DEVQUAL:
					SUDPTRH = MSB (current_devqual_descr);
					SUDPTRL = LSB (current_devqual_descr);
				break;
				case DT_CONFIG:
					SUDPTRH = MSB (current_config_descr);
					SUDPTRL = LSB (current_config_descr);
				break;
				case DT_OTHER_SPEED:
					SUDPTRH = MSB (other_config_descr);
					SUDPTRL = LSB (other_config_descr);
				break;
				case DT_STRING:
					if (wValueL >= nstring_descriptors)
						fx2_stall_ep0 ();
					else {
						__xdata char *p = string_descriptors[wValueL];
						SUDPTRH = MSB (p);
						SUDPTRL = LSB (p);
					}
				break;
				default:
					fx2_stall_ep0 ();	// invalid request
				break;
			}
		break;

		case RQ_GET_STATUS:
			switch (bRequestType & bmRT_RECIP_MASK){
				case bmRT_RECIP_DEVICE:
					EP0BUF[0] = 0;
					EP0BUF[1] = 0;
					EP0BCH = 0;
					EP0BCL = 2;
				break;
				case bmRT_RECIP_INTERFACE:
					EP0BUF[0] = 0;
					EP0BUF[1] = 0;
					EP0BCH = 0;
					EP0BCL = 2;
				break;
				case bmRT_RECIP_ENDPOINT:
					if (plausible_endpoint (wIndexL)){
						EP0BUF[0] = *epcs (wIndexL) & bmEPSTALL;
						EP0BUF[1] = 0;
						EP0BCH = 0;
						EP0BCL = 2;
					}
					else
						fx2_stall_ep0 ();
				break;
				default:
					fx2_stall_ep0 ();
				break;
			}
		break;
		
		case RQ_SET_CONFIG:
			IOE &= ~(1 << 6);
			_usb_config = wValueL;		// FIXME app should handle
		break;

		case RQ_SET_INTERFACE:
			_usb_alt_setting = wValueL;	// FIXME app should handle
		break;

		case RQ_CLEAR_FEATURE:
			switch (bRequestType & bmRT_RECIP_MASK){
				//case bmRT_RECIP_DEVICE:
					//switch (wValueL){
					//	case FS_DEV_REMOTE_WAKEUP:
					//	default:
					//		fx2_stall_ep0 ();
					//}
				//break;
				case bmRT_RECIP_ENDPOINT:
					if (wValueL == FS_ENDPOINT_HALT && plausible_endpoint (wIndexL)){
						*epcs (wIndexL) &= ~bmEPSTALL;
						fx2_reset_data_toggle (wIndexL);
					}
					else
						fx2_stall_ep0 ();
				break;
				default:
					fx2_stall_ep0 ();
				break;
			}
		break;

		case RQ_SET_FEATURE:
			//switch (bRequestType & bmRT_RECIP_MASK){
				//case bmRT_RECIP_DEVICE:
					//switch (wValueL){
						//case FS_TEST_MODE:
						// hardware handles this after we complete SETUP phase handshake
						//break;

						//case FS_DEV_REMOTE_WAKEUP:
						//default:
							fx2_stall_ep0 ();
						//break;
					//}
				//break;
			//}
		break;

/*		case RQ_GET_LINE_CODING:
			SUDPTRCTL = 0x01;
			EP0BUF[0] = LineCode[0]; 
			EP0BUF[1] = LineCode[1]; 
			EP0BUF[2] = LineCode[2]; 
			EP0BUF[3] = LineCode[3]; 
			EP0BUF[4] = LineCode[4]; 
			EP0BUF[5] = LineCode[5]; 
			EP0BUF[6] = LineCode[6]; 
			EP0BCH = 0x00;
			SYNCDELAY;
			EP0BCL = 7;
			SYNCDELAY;
			while (EP0CS & 0x02);
				SUDPTRCTL = 0x00;
		break;		
		
		case RQ_SET_LINE_CODING:
			SUDPTRCTL = 0x01;
			EP0BCL = 0x00;
			SUDPTRCTL = 0x00;
			while (EP0BCL != 7);
			SYNCDELAY;
			LineCode[0] = EP0BUF[0]; 
			LineCode[1] = EP0BUF[1]; 
			LineCode[2] = EP0BUF[2]; 
			LineCode[3] = EP0BUF[3]; 
			LineCode[4] = EP0BUF[4]; 
			LineCode[5] = EP0BUF[5]; 
			LineCode[6] = EP0BUF[6];
			//process_line_coding();
		break;
*/			
		//case RQ_SET_CONTROL_STATE:
		//break;
	}
	EP0CS |= bmHSNAK;
}
