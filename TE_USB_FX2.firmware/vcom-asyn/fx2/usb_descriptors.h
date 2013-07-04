/* -*- c++ -*- */
/*-----------------------------------------------------------------------------
 * USB descriptor references
 *-----------------------------------------------------------------------------
 * Code taken from USRP2 firmware (GNU Radio Project), version 3.0.2,
 * Copyright 2003 Free Software Foundation, Inc.
 *-----------------------------------------------------------------------------
 * This code is part of usbjtag. usbjtag is free software; you can redistribute
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
/* Remove warning 196: pointer target lost const qualifier
extern __xdata const char high_speed_device_descr[];
extern __xdata const char high_speed_devqual_descr[];
extern __xdata const char high_speed_config_descr[];

extern __xdata const char full_speed_device_descr[];
extern __xdata const char full_speed_devqual_descr[];
extern __xdata const char full_speed_config_descr[];
*/
extern __xdata char high_speed_device_descr[];
extern __xdata char high_speed_devqual_descr[];
extern __xdata char high_speed_config_descr[];

extern __xdata char full_speed_device_descr[];
extern __xdata char full_speed_devqual_descr[];
extern __xdata char full_speed_config_descr[];


extern __xdata unsigned char nstring_descriptors;
extern __xdata char * __xdata string_descriptors[];

/*
 * We patch these locations with info read from the usrp config eeprom
 */
extern __xdata char usb_desc_hw_rev_binary_patch_location_0[];
extern __xdata char usb_desc_hw_rev_binary_patch_location_1[];
extern __xdata char usb_desc_hw_rev_ascii_patch_location_0[];
extern __xdata char usb_desc_serial_number_ascii[];
