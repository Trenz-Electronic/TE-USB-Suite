;;------------------------------------------------------------------------------
;; Copyright (C) 2012 Trenz Electronic
;; 
;; Permission is hereby granted, free of charge, to any person obtaining a 
;; copy of this software and associated documentation files (the "Software"), 
;; to deal in the Software without restriction, including without limitation 
;; the rights to use, copy, modify, merge, publish, distribute, sublicense, 
;; and/or sell copies of the Software, and to permit persons to whom the 
;; Software is furnished to do so, subject to the following conditions:
;; 
;; The above copyright notice and this permission notice shall be included 
;; in all copies or substantial portions of the Software.
;; 
;; THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
;; OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
;; FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
;; AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
;; LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
;; FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
;; IN THE SOFTWARE.
;;------------------------------------------------------------------------------
.module usb_descriptors
VID			= 0x0547	; DEWESoft = 0x0547
PID			= 0x1002	; Sample Device = 0x1002

VERSION		= 0x0000	; Product Version
USB_VER		= 0x0200	; Support USB version 2.00 
USB_ATTR	= 0x80		; Bus powered, not self-powered, no remote wakeup
FTD_ATTR	= 0x001C	; Set USB version, use version string, enable suspend PD
MAX_POWER	= 250		; need 2*250 mA max
EP_SIZE		= 512		; Max packet size for Data EP
DEV_CLASS	= 0x00		; Device class

DSCR_DEVICE			=   1		; Descriptor type: Device
DSCR_CONFIG			=   2		; Descriptor type: Configuration
DSCR_STRING			=   3		; Descriptor type: String
DSCR_INTRFC			=   4		; Descriptor type: Interface
DSCR_ENDPNT			=   5		; Descriptor type: Endpoint
DSCR_DEVQUAL		=   6		; Descriptor type: Device Qualifier
		
DSCR_DEVICE_LEN		= 18
DSCR_CONFIG_LEN		=  9
DSCR_INTRFC_LEN		=  9
DSCR_ENDPNT_LEN		=  7
DSCR_DEVQUAL_LEN	= 10
		
ET_CONTROL			=   0		; Endpoint type: Control
ET_ISO				=   1		; Endpoint type: Isochronous
ET_BULK				=   2		; Endpoint type: Bulk
ET_INT				=   3		; Endpoint type: Interrupt
;;------------------------------------------------------------------------------
;;;		external ram data
;;------------------------------------------------------------------------------
.area USBDESCSEG	(XDATA)
;;------------------------------------------------------------------------------
.even		; descriptors must be 2-byte aligned for SUDPTR{H,L} to work
_high_speed_device_descr::
	.db		DSCR_DEVICE_LEN
	.db		DSCR_DEVICE
_dscr_usbver::
	.db		<USB_VER		; Specification version (LSB)
	.db		>USB_VER		; Specification version (MSB)
	.db		DEV_CLASS		; device class 
	.db		0x00			; device subclass
	.db		0x00			; device protocol
	.db		64			   	; bMaxPacketSize0 for endpoint 0
_dscr_vidpidver::
	.db		<VID			; idVendor
	.db		>VID			; idVendor
	.db		<PID			; idProduct
	.db		>PID			; idProduct
	.db		<VERSION		; bcdDevice
	.db		>VERSION		; bcdDevice
_dscr_strorder::
	.db		SI_VENDOR		; iManufacturer (string index)
	.db		SI_PRODUCT	   	; iProduct (string index)
	.db		SI_SERIAL		; iSerial number (string index)
	.db		1				; bNumConfigurations
;;------------------------------------------------------------------------------
;;; descriptors used when operating at high speed (480Mb/sec)
;;------------------------------------------------------------------------------
.even
_high_speed_devqual_descr::
	.db		DSCR_DEVQUAL_LEN
	.db		DSCR_DEVQUAL
	.db		<USB_VER		; bcdUSB (LSB)
	.db		>USB_VER		; bcdUSB (MSB)
	.db		0x00			; bDeviceClass
	.db		0x00			; bDeviceSubClass
	.db		0x0			 	; bDeviceProtocol
	.db		64			   	; bMaxPacketSize0
	.db		1				; bNumConfigurations (one config at 12Mb/sec)
	.db		0				; bReserved
;;------------------------------------------------------------------------------
.even
_high_speed_config_descr::		
	.db		DSCR_CONFIG_LEN
	.db		DSCR_CONFIG
	.db		<(_high_speed_config_descr_end - _high_speed_config_descr) ; LSB
	.db		>(_high_speed_config_descr_end - _high_speed_config_descr) ; MSB
	.db		1				; bNumInterfaces
	.db		1				; bConfigurationValue
	.db		0				; iConfiguration
_dscr_attrpow::
	.db		USB_ATTR		; bmAttributes
	.db		MAX_POWER		; bMaxPower 
;;------------------------------------------------------------------------------
;; Interface Descriptor
	.db		DSCR_INTRFC_LEN
	.db		DSCR_INTRFC
	.db		0				; bInterfaceNumber (zero based)
	.db		0				; bAlternateSetting
	.db		6				; bNumEndpoints
	.db		0xFF			; bInterfaceClass (vendor specific)
	.db		0x00			; bInterfaceSubClass (vendor specific)
	.db		0x00			; bInterfaceProtocol (vendor specific)
	.db		SI_PRODUCT	   	; iInterface (description)
;;------------------------------------------------------------------------------
;; EP1OUT EP Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x01			;; EP1 OUT
	.db		ET_BULK			;; Endpoint type
	.db	 	<64			  	;; wMaxPacketSize (LSB)
	.db	 	>64			  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP1IN EP Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x81			;; EP1 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<64			  	;; wMaxPacketSize (LSB)
	.db	 	>64			  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP2 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x82			;; EP2 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP6 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x86			;; EP6 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP4 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x84			;; EP6 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP8 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x08			;; EP8 OUT
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
_high_speed_config_descr_end:				   

;;==============================================================================
;;; descriptors used when operating at full speed (12Mb/sec)
;;------------------------------------------------------------------------------
.even
_full_speed_device_descr::		
	.db		DSCR_DEVICE_LEN
	.db		DSCR_DEVICE
	.db		<USB_VER		; Specification version (LSB)
	.db		>USB_VER		; Specification version (MSB)
	.db		DEV_CLASS		; device class (vendor specific)
	.db		0x00			; device subclass (vendor specific)
	.db		0x00			; device protocol (vendor specific)
	.db		64			   	; bMaxPacketSize0 for endpoint 0
	.db		<VID			; idVendor
	.db		>VID			; idVendor
	.db		<PID			; idProduct
	.db		>PID			; idProduct
	.db		<VERSION		; bcdDevice
	.db		>VERSION		; bcdDevice
	.db		SI_VENDOR		; iManufacturer (string index)
	.db		SI_PRODUCT	   	; iProduct (string index)
	.db		SI_SERIAL		; iSerial number (None)
	.db		1				; bNumConfigurations
;;------------------------------------------------------------------------------
.even
_full_speed_devqual_descr::
	.db		DSCR_DEVQUAL_LEN
	.db		DSCR_DEVQUAL
	.db		<USB_VER		; bcdUSB
	.db		>USB_VER		; bcdUSB
	.db		0x00			; bDeviceClass
	.db		0x00			; bDeviceSubClass
	.db		0x00			; bDeviceProtocol
	.db		64			   	; bMaxPacketSize0
	.db		1				; bNumConfigurations (one config at 480Mb/sec)
	.db		0				; bReserved
;;------------------------------------------------------------------------------
_full_speed_config_descr::		
	.db		DSCR_CONFIG_LEN
	.db		DSCR_CONFIG
	.db		<(_full_speed_config_descr_end - _full_speed_config_descr) ; LSB
	.db		>(_full_speed_config_descr_end - _full_speed_config_descr) ; MSB
	.db		1				; bNumInterfaces
	.db		1				; bConfigurationValue
	.db		0				; iConfiguration
	.db		USB_ATTR		; bmAttributes
	.db		MAX_POWER		; bMaxPower
;;------------------------------------------------------------------------------
;; Interface descriptor
	.db		DSCR_INTRFC_LEN
	.db		DSCR_INTRFC
	.db		0				; bInterfaceNumber (zero based)
	.db		0				; bAlternateSetting
	.db		6				; bNumEndpoints
	.db		0xFF			; bInterfaceClass (vendor specific)
	.db		0x00			; bInterfaceSubClass (vendor specific)
	.db		0x00			; bInterfaceProtocol (vendor specific)
	.db		SI_PRODUCT	    ; iInterface (description)
;;------------------------------------------------------------------------------
;; EP1OUT EP Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x01			;; EP1 OUT
	.db		ET_BULK			;; Endpoint type
	.db	 	<64			  	;; wMaxPacketSize (LSB)
	.db	 	>64			  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP1IN EP Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x81			;; EP1 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<64			  	;; wMaxPacketSize (LSB)
	.db	 	>64			  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP2 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x82			;; EP2 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP6 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x86			;; EP6 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP4 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x84			;; EP6 IN
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
;; EP8 Descriptor
	.db		DSCR_ENDPNT_LEN	;; Descriptor length
	.db		DSCR_ENDPNT		;; Descriptor type
	.db		0x08			;; EP8 OUT
	.db		ET_BULK			;; Endpoint type
	.db	 	<512		  	;; wMaxPacketSize (LSB)
	.db	 	>512		  	;; wMaxPacketSize (MSB)
	.db		0x00			;; Polling interval
;;------------------------------------------------------------------------------
_full_speed_config_descr_end:		

;;==============================================================================
;;;		String descriptors
;;------------------------------------------------------------------------------
_nstring_descriptors::
	.db		(_string_descriptors_end - _string_descriptors) / 2

_string_descriptors::
	.db		<str0, >str0
	.db		<str1, >str1
	.db		<str2, >str2
	.db		<str3, >str3
_string_descriptors_end:

;;------------------------------------------------------------------------------
SI_NONE = 0
;; str0 contains the language ID's.
.even
_str0::
str0:   
	.db		str0_end - str0
	.db		DSCR_STRING
	.db		0
	.db		0
	.db		<0x0409		  ; magic code for US English (LSB)
	.db		>0x0409		  ; magic code for US English (MSB)
str0_end:
;;------------------------------------------------------------------------------
SI_VENDOR = 1
.even
_str1::
str1:   
	.db		str1_end - str1
	.db		DSCR_STRING
    .db   'D, 0
    .db   'e, 0
    .db   'w, 0
    .db   'e, 0
    .db   's, 0
    .db   'o, 0
	.db   'f, 0
    .db   't, 0
str1_end:
;;------------------------------------------------------------------------------
SI_PRODUCT = 2
.even
_str2::
str2:   
	.db		str2_end - str2
	.db		DSCR_STRING
    .db   'D, 0
    .db   'E, 0
    .db   'W, 0
    .db   'E, 0
    .db   'S, 0
    .db   'o, 0
    .db   'f, 0
    .db   't, 0
    .db   ' , 0
    .db   'U, 0
    .db   'S, 0
    .db   'B, 0      
str2_end:
;;------------------------------------------------------------------------------
SI_SERIAL = 3
.even
_str3::
str3:   
	.db		str3_end - str3
	.db		DSCR_STRING
	.db		'1, 0
	.db		'0, 0
	.db		'1, 0
str3_end:
;;==============================================================================
