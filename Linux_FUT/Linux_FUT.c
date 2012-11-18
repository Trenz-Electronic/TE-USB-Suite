/*
    Copyright 2012 Carl Zeiss SMT GmbH

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/* use:
gcc -g -Wall `pkg-config libusb-1.0 --cflags --libs` main.c -o main

To compile the software you have to install the libusb-1.0
 */
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <libusb.h>
#include <string.h>
#include <getopt.h>

// TE USB FX2: generation 2.0
// #define  VENDOR_ID   0x0547
// #define  PRODUCT_ID  0x1002

// TE USB FX2: generation 2.5 and 3.0
#define  VENDOR_ID   0x0BD0
#define  PRODUCT_ID  0x0300

#define USAGE												\
		"Usage:\n" 										\
		"	sudo %s OPTIONS\n"								\
		"	$Id: Linux_FUT.c 15598 2012-03-20 10:29:26Z xykhe $ $HeadURL: https://svn.zeiss.org/tme/projects/33xx/trunk/pob/sw/linux/tools/trenzusb/trenzprogrammer.c $\n"								\
		"\n"											\
		"	This tools is used to program the fpga and eeprom on the Trenz Electronic TE0320\n"\
		"	The two necessary files are usb.bin and fpga.bin. They are normaly packed \n" 	\
		"	together in a file with the extension \"fwu\". This file is a zip-file.\n"	\
		"	to unzip it us unzip \"file.fwu\". The usb.bin is delivered by the company \n"	\
		"	Trenz Electronic. The fpga.bin is generated with \"Xilinx ISE\"\n\n"			\
		"	Device Ids of the usb (lsusb):\n"						\
		"		Vendor Id: 0x0547\n"							\
		"		Device Id: 0x1002\n"							\
		"\n"											\
		"	OPTIONS\n"									\
		"		-e FILE\n"								\
		"			Write the file named FILE to the eeprom and verify it.\n"	\
		"			Without this option the eeprom is left untouched\n\n"		\
		"		-E FILE\n"								\
		"			Compares the data from FILE to the eeprom data.\n\n"		\
		"		-f FILE\n"								\
		"			Write the file named FILE to the fpga.\n"			\
		"			Without this option the fpga is left untouched\n\n"		\
		"		-F FILE\n"								\
		"			Compares the data from FILE to the fpga data.\n\n"		\
		"		-b BUS_NUMBER\n"							\
		"			Number of the usb bus to which the device is conected (lsusb).\n\n"\
		"		-a DEVICE_NUMBER\n"							\
		"			Number of the device on the usb bus (lsusb).\n\n"		

/*  FX2 Commands definition */
#define CMD_FX2_READ_VERSION        	((unsigned char) 0x00)
#define CMD_FX2_INITALIZE           	((unsigned char) 0xA0)
#define CMD_FX2_READ_STATUS		((unsigned char) 0xA1)
#define CMD_FX2_WRITE_REGISTER		((unsigned char) 0xA2)
#define CMD_FX2_READ_REGISTER		((unsigned char) 0xA3)
#define CMD_FX2_RESET_FIFO_STATUS	((unsigned char) 0xA4)
#define CMD_FX2_FLASH_READ		((unsigned char) 0xA5)
#define CMD_FX2_FLASH_WRITE		((unsigned char) 0xA6)
#define CMD_FX2_FLASH_ERASE		((unsigned char) 0xA7)
#define CMD_FX2_SECTOR_ERASE		((unsigned char) 0xF7)
#define CMD_FX2_EEPROM_READ		((unsigned char) 0xA8)
#define CMD_FX2_EEPROM_WRITE		((unsigned char) 0xA9)
#define CMD_FX2_GET_FIFO_STATUS		((unsigned char) 0xAC)
#define CMD_FX2_I2C_WRITE		((unsigned char) 0xAD)
#define CMD_FX2_I2C_READ		((unsigned char) 0xAE)
#define CMD_FX2_POWER_ON		((unsigned char) 0xAF)
#define CMD_FX2_FLASH_WRITE_COMMAND	((unsigned char) 0xAA)
#define CMD_FX2_SET_INTERRUPT		((unsigned char) 0xB0)
#define CMD_FX2_GET_INTERRUPT		((unsigned char) 0xB1)


#define USB_BLOCK_SIZE	64
#define ARRAY_ELEMENTS(x)	(sizeof(x)/sizeof(*x))

#define CMDMSG(fmt, ...)								\
do {											\
        fprintf(stdout, "%s[%d] --> " fmt "\n", __FILE__, __LINE__, ## __VA_ARGS__); 	\
} while(0)

#define XERROR(fmt, ...)                                                \
do {                                                                    \
        fprintf(stderr, "ERROR: %s[%d] " fmt "\n", __FILE__, __LINE__, ## __VA_ARGS__); \
        goto xerror;                                                    \
} while(0)

#define CHECK_USB(xpr)                          \
do {                                            \
        int _s = (xpr);                         \
        if (_s < 0) XERROR("%s => %d (%s)", #xpr, _s, -_s < ARRAY_ELEMENTS(usb_errmsgs) ? usb_errmsgs[-_s] : "unknown"); \
} while(0)

#define CHECK_B(xpr)                          \
do {                                            \
        int _s = (xpr);                         \
        if (_s == 0) XERROR("%s => false", #xpr ); \
} while(0)

#define CHECK_P(xpr)                            \
do {                                            \
        void *_p = (xpr);                       \
        if (!_p) XERROR("%s => NULL", #xpr);    \
} while(0)

#define CHECK_I(xpr)                            \
do {                                            \
        int _i = (xpr);                       \
        if (_i < 0) XERROR("%s => %d", #xpr, _i);  \
} while(0)

typedef enum
{
	OFF = 0,
	ON
} Switch;

struct libusb_device_handle *usbDeviceHandle;

static const char *const usb_errmsgs[] = {
	/*0*/ "LIBUSB_SUCCESS",

	/** Input/output error */
	/*-1*/ "LIBUSB_ERROR_IO",

	/** Invalid parameter */
	/*-2*/ "LIBUSB_ERROR_INVALID_PARAM",

	/** Access denied (insufficient permissions) */
	/*-3*/ "LIBUSB_ERROR_ACCESS",

	/** No such device (it may have been disconnected) */
	/*-4*/ "LIBUSB_ERROR_NO_DEVICE",

	/** Entity not found */
	/*-5*/ "LIBUSB_ERROR_NOT_FOUND",

	/** Resource busy */
	/*-6*/ "LIBUSB_ERROR_BUSY",

	/** Operation timed out */
	/*-7*/ "LIBUSB_ERROR_TIMEOUT",

	/** Overflow */
	/*-8*/ "LIBUSB_ERROR_OVERFLOW",

	/** Pipe error */
	/*-9*/ "LIBUSB_ERROR_PIPE",

	/** System call interrupted (perhaps due to signal) */
	/*-10*/ "LIBUSB_ERROR_INTERRUPTED",

	/** Insufficient memory */
	/*-11*/ "LIBUSB_ERROR_NO_MEM",

	/** Operation not supported or unimplemented on this platform */
	/*-12*/ "LIBUSB_ERROR_NOT_SUPPORTED",
};

/* "swaptab[i]" is the value of "i" with the bits reversed. */
static const unsigned char swaptab[256] = {
  0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0,
  0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
  0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8,
  0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
  0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4,
  0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
  0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec,
  0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
  0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2,
  0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
  0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea,
  0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
  0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6,
  0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
  0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee,
  0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
  0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1,
  0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
  0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9,
  0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
  0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5,
  0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
  0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed,
  0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
  0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3,
  0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
  0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb,
  0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
  0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7,
  0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
  0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef,
  0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff,
};

/** Prototypes of the functions	*/
void printPoint (void);
void switchFpgaPower (Switch theSwitch);
void fpgaEraseBulk(void);
void eepromCheck (char *theDataFile);
void eepromProgram (char *theDataFile);
void fpgaCheck (char *theDataFile);
void fpgaProgram (char *theDataFile);
libusb_device *findDevice (uint8_t busNumber, uint8_t deviceAddress);



void printPoint (void)
{
	static int x = 0;
	static int sign = 0;
	static unsigned char signTab[4] = { '\\', '|', '/', '-' };

	if (x++ < 100) 
		return;

	/** Print the sign	*/
	printf("%c", signTab[sign]);
	printf("%c", 8); /** Backspace */
	fflush(stdout);

	/** Calculate the variables for the next run */
	x = 0;
	sign = sign == 4 ? 0 : sign + 1;
}

void switchFpgaPower (Switch theSwitch)
{
	unsigned char	aCmdBuffer[USB_BLOCK_SIZE];
	int actual_length;

	/** Clean Buffer	*/
	memset(aCmdBuffer, 0, sizeof(aCmdBuffer));

	if (theSwitch == ON)
	{
		CMDMSG("Switch fpga power on");
		aCmdBuffer[1] = 1;	/** <- Parameter 0 = OFF */
	} else
	{
		CMDMSG("Switch fpga power off");
		aCmdBuffer[1] = 0;	/** <- Parameter 0 = OFF */
	}

	aCmdBuffer[0] = CMD_FX2_POWER_ON; /** ON with Parameter 0 means OFF. It looks strange, but not my fault :-) */

	/** Send the command 	*/
	CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
		2, &actual_length, 1000) );

	CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, aCmdBuffer, 
			USB_BLOCK_SIZE, &actual_length, 1000) );

	/** OK */
	return;

xerror:
	printf("\n");
	exit(1);
}

void fpgaEraseBulk(void)
{
	unsigned char	aCmdBuffer[USB_BLOCK_SIZE];
	int 	actual_length;

	/** Switch the fpga Power off*/
	switchFpgaPower(OFF);

	CMDMSG("Erase fpga");

	/** Insert Command	*/
	memset(aCmdBuffer, 0, sizeof(aCmdBuffer));
	aCmdBuffer[0] = CMD_FX2_FLASH_ERASE;

	CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
		1, &actual_length, 1000) );

	/** Erase the fpga	*/
	for (;;)
	{
		memset(aCmdBuffer, 0, sizeof(aCmdBuffer));
		aCmdBuffer[0] = CMD_FX2_READ_STATUS;

		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
			1, &actual_length, 1000) );

		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, aCmdBuffer, 
			USB_BLOCK_SIZE, &actual_length, 1000) );

		if (aCmdBuffer[2] == (char) 0)
			break;

		printPoint();
	}

	printf("%c", 8); /** Backspace */

	/** OK */
	return;

xerror:
	printf("\n");
	exit(1);
}

void eepromCheck (char *theDataFile)
{
	FILE *fp;
	unsigned char aCmdBuffer[USB_BLOCK_SIZE];
	unsigned char aReplyBuffer[USB_BLOCK_SIZE];
	int aBlockSize;
	unsigned int aEepromAddr;
	int actual_length;
	int wrBlockSize;

	/** Open data file	*/
	CMDMSG("Open data file: %s", theDataFile);
	if (NULL == (fp = fopen(theDataFile, "r")))
	{
		 XERROR("Can't open file %s", theDataFile);
	}
	
	/** Switch the fpga Power off*/
	switchFpgaPower(OFF);

#define MAX_PAYLOAD	0x10

	/** check data	*/
	CMDMSG("Read eeprom data back and check");
	for (aEepromAddr = 0;;)
	{
		memset(aCmdBuffer, 0, sizeof(aCmdBuffer));

		/** Insert command in buffer	*/
		aCmdBuffer[0] = CMD_FX2_EEPROM_READ;

		/** Initialize the amount if payload size */
		wrBlockSize = MAX_PAYLOAD;

		/** Read data from file	*/
		if (0 == (aBlockSize = fread(&(aCmdBuffer[4]), 1, wrBlockSize, fp)))
		{
			break;
		}

		/** if we are reached the end of file, maybe there are o many data as we expected. */
		wrBlockSize = aBlockSize;

		/** Insert address in the buffer */
		aCmdBuffer[1] = (aEepromAddr >> 8) & 0xff;
		aCmdBuffer[2] = aEepromAddr & 0xff;

		/** Insert the actual block size	*/
		aCmdBuffer[3] = wrBlockSize;

		/** Bulk transfer	*/
		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
			4, &actual_length, 1000) );

		printPoint();

		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, aReplyBuffer, 
			USB_BLOCK_SIZE, &actual_length, 1000) );

		/** Check the data	*/
		if (memcmp(&aCmdBuffer[4], aReplyBuffer, wrBlockSize))
		{
			XERROR("Eeprom data corrupt");
		}

		/** increment the fpga address */
		aEepromAddr += aBlockSize;
	}

	printf("%c", 8); /** Backspace */
	CMDMSG("Eeprom data OK");

	/** Switch the fpga Power on*/
	switchFpgaPower(ON);

	/** Close data file */
	fclose(fp);

	/** OK */
	return;

xerror:
	printf("\n");
	exit(1);
}

void eepromProgram (char *theDataFile)
{
	FILE *fp;
	unsigned char aCmdBuffer[USB_BLOCK_SIZE];
	unsigned char aReplyBuffer[USB_BLOCK_SIZE];
	int aBlockSize;
	unsigned int aEepromAddr;
	int actual_length;
	int wrBlockSize;

	/** Open data file	*/
	CMDMSG("Open data file: %s", theDataFile);
	if (NULL == (fp = fopen(theDataFile, "r")))
	{
		 XERROR("Can't open file %s", theDataFile);
	}
	
	/** Switch the fpga Power off*/
	switchFpgaPower(OFF);

#define MAX_PAYLOAD	0x10

	/** Transfere data	*/
	CMDMSG("Send eeprom data");
	for (aEepromAddr = 0;;)
	{
		memset(aCmdBuffer, 0, sizeof(aCmdBuffer));

		/** Insert command in buffer	*/
		aCmdBuffer[0] = CMD_FX2_EEPROM_WRITE;

		/** Initialize the amount if payload size */
		wrBlockSize = MAX_PAYLOAD;

		/** Read data from file	*/
		if (0 == (aBlockSize = fread(&(aCmdBuffer[4]), 1, wrBlockSize, fp)))
		{
			break;
		}

		/** if we are reached the end of file, maybe there are o many data as we expected. */
		wrBlockSize = aBlockSize;

		/** Insert address in the buffer */
		aCmdBuffer[1] = (aEepromAddr >> 8) & 0xff;
		aCmdBuffer[2] = aEepromAddr & 0xff;

		/** Insert the actual block size	*/
		aCmdBuffer[3] = wrBlockSize;

		/** Bulk transfer	*/
		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
			wrBlockSize + 4, &actual_length, 1000) );

		printPoint();

		/** Insert command in buffer	*/
		aCmdBuffer[0] = CMD_FX2_EEPROM_READ;

		/** Read back the answere	*/
		memset(aReplyBuffer, 0, sizeof(aReplyBuffer));

		aReplyBuffer[2] = (aEepromAddr >> 8) & 0xff;
		aReplyBuffer[3] = aEepromAddr & 0xff;

		/** Insert the actual block size	*/
		aReplyBuffer[4] = wrBlockSize;

		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, aReplyBuffer, 
			USB_BLOCK_SIZE, &actual_length, 1000) );

		/** increment the eeprom address */
		aEepromAddr += aBlockSize;
	}

	/** Switch the fpga Power on*/
	switchFpgaPower(ON);

	/** Close data file */
	fclose(fp);

	/** OK */
	return;

xerror:
	printf("\n");
	exit(1);
}

void fpgaCheck (char *theDataFile)
{
	FILE *fp;
	unsigned char aCmdBuffer[USB_BLOCK_SIZE];
	unsigned char aFileBuffer[USB_BLOCK_SIZE];
	unsigned char aReplyBuffer[USB_BLOCK_SIZE];
	int aBlockSize;
	unsigned int afpgaAddr;
	int actual_length;
	int sectorRem;
	int wrBlockSize;

	/** Open data file	*/
	CMDMSG("Open data file: %s", theDataFile);
	if (NULL == (fp = fopen(theDataFile, "r")))
	{
		 XERROR("Can't open file %s", theDataFile);
	}

	switchFpgaPower(OFF);
	
#undef MAX_PAYLOAD
#define MAX_PAYLOAD	0x20

	/** Transfere data	*/
	CMDMSG("Check fpga data");
	for (afpgaAddr = 0;;)
	{
		memset(aCmdBuffer, 0, sizeof(aCmdBuffer));

		/** Insert command in buffer	*/
		aCmdBuffer[0] = CMD_FX2_FLASH_READ;

		/** Initialize the amount if payload size */
		wrBlockSize = MAX_PAYLOAD;

		/** calculate if we reached the sector limit */
		sectorRem = 0xffff - (afpgaAddr & 0xffff);
		if ((sectorRem < MAX_PAYLOAD) && (sectorRem != 0))
		{
			wrBlockSize = sectorRem +1;
		}

		/** Read data from file	*/
		if (0 == (aBlockSize = fread(aFileBuffer, 1, wrBlockSize, fp)))
		{
			break;
		}

		/** if we are reached the end of file, maybe there are o many data as we expected. */
		wrBlockSize = aBlockSize;

		/** Swaping bits	*/
		int i;
		for (i = 0; i < wrBlockSize; i++)
		{
			aFileBuffer[i] = swaptab[aFileBuffer[i]];
		}

		/** Insert address in the buffer */
		aCmdBuffer[1] = (afpgaAddr >> 16) & 0xff;
		aCmdBuffer[2] = (afpgaAddr >> 8) & 0xff;
		aCmdBuffer[3] = afpgaAddr & 0xff;

		/** Insert the actual block size	*/
		aCmdBuffer[4] = wrBlockSize;

		/** Bulk transfer	*/
		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
			5, &actual_length, 1000) );

		printPoint();

		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, aReplyBuffer, 
			USB_BLOCK_SIZE, &actual_length, 1000) );

		/** Check the data	*/
		if (memcmp(aFileBuffer, aReplyBuffer, wrBlockSize) )
		{
			XERROR("Fpga data corrupt");
		}

		/** increment the fpga address */
		afpgaAddr += aBlockSize;
	}

	printf("%c", 8); /** Backspace */
	CMDMSG("Fpga data OK");

	/** Switch the fpga Power on*/
	switchFpgaPower(ON);

	/** Close data file */
	fclose(fp);

	/** OK */
	return;

xerror:
	printf("\n");
	exit(1);
}

void fpgaProgram (char *theDataFile)
{
	FILE *fp;
	unsigned char aCmdBuffer[USB_BLOCK_SIZE];
	unsigned char aReplyBuffer[USB_BLOCK_SIZE];
	int aBlockSize;
	unsigned int afpgaAddr;
	int actual_length;
	int sectorRem;
	int wrBlockSize;

	/** Open data file	*/
	CMDMSG("Open data file: %s", theDataFile);
	if (NULL == (fp = fopen(theDataFile, "r")))
	{
		 XERROR("Can't open file %s", theDataFile);
	}
	
#undef MAX_PAYLOAD
#define MAX_PAYLOAD	0x10

	/** Transfere data	*/
	CMDMSG("Send fpga data");
	for (afpgaAddr = 0;;)
	{
		memset(aCmdBuffer, 0, sizeof(aCmdBuffer));

		/** Insert command in buffer	*/
		aCmdBuffer[0] = CMD_FX2_FLASH_WRITE;

		/** Initialize the amount if payload size */
		wrBlockSize = MAX_PAYLOAD;

		/** calculate if we reached the sector limit */
		sectorRem = 0xffff - (afpgaAddr & 0xffff);
		if ((sectorRem < MAX_PAYLOAD) && (sectorRem != 0))
		{
			wrBlockSize = sectorRem +1;
		}

		/** Read data from file	*/
		if (0 == (aBlockSize = fread(&(aCmdBuffer[5]), 1, wrBlockSize, fp)))
		{
			break;
		}

		/** if we are reached the end of file, maybe there are o many data as we expected. */
		wrBlockSize = aBlockSize;

		/** Swaping bits	*/
		int i;
		for (i = 5; i < sizeof(aCmdBuffer); i++)
		{
			unsigned char tmp = swaptab[aCmdBuffer[i]];
			aCmdBuffer[i] = tmp;
		}

		/** Insert address in the buffer */
		aCmdBuffer[1] = (afpgaAddr >> 16) & 0xff;
		aCmdBuffer[2] = (afpgaAddr >> 8) & 0xff;
		aCmdBuffer[3] = afpgaAddr & 0xff;

		/** Insert the actual block size	*/
		aCmdBuffer[4] = wrBlockSize;

		/** Bulk transfer	*/
		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_OUT | 1, aCmdBuffer, 
			5 + wrBlockSize, &actual_length, 1000) );

		printPoint();

		CHECK_USB( libusb_bulk_transfer(usbDeviceHandle, LIBUSB_ENDPOINT_IN | 1, aReplyBuffer, 
			USB_BLOCK_SIZE, &actual_length, 1000) );

		if (memcmp(&(aCmdBuffer[5]), aReplyBuffer, wrBlockSize) )
		{
			 XERROR("Fpga data corrupt");
		}

		/** increment the fpga address */
		afpgaAddr += wrBlockSize;
	}

	printf("%c", 8); /** Backspace */

	/** Switch the fpga Power on*/
	switchFpgaPower(ON);

	/** Close data file */
	fclose(fp);

	/** OK */
	return;

xerror:
	printf("\n");
	exit(1);
}

libusb_device *findDevice (uint8_t busNumber, uint8_t deviceAddress) 
{
	libusb_device **list;
	libusb_device *device;
	ssize_t cnt = 0;
	int i;

	/** Get device list	*/
	CHECK_USB( cnt = libusb_get_device_list(NULL, &list) );

	/** Look for the device with the correct bus number and address */
	for (i = 0; i < cnt; i++) 
	{
		device = list[i];

		/** Check the bus number and address of the device	*/
		if ((busNumber == libusb_get_bus_number(device))
		    && (deviceAddress == libusb_get_device_address(device))) 
		{
			/** Device found	*/
			return device;
		}
	} 

	XERROR("No device on bus %d with the address %d found", busNumber, deviceAddress);
xerror:
	printf("\n");
	exit(1);
}

int main(int argc, char *argv[])
{
	int 		busNumber = 0;
	int 		deviceAddress = 0;
        libusb_device 	*device;
        struct libusb_device_descriptor description;
	int 		opt;
	int		eflag = 0;
	int		Eflag = 0;
	int		fflag = 0;
	int		Fflag = 0;
	char		fpgaFileName[256];
	char		eepromFileName[256];

	/** Init libusb	*/
	CHECK_USB( libusb_init(NULL) );

	if (argc == 1)
	{
		fprintf(stderr, USAGE, argv[0]);
		goto xerror;
	}

	/** Looking for the vendor and product id. */
	while ((opt = getopt(argc, argv, "e:E:f:F:b:a:")) != -1) 
	{
		char dummy;
		switch (opt) 
		{
			case 'e':
				eflag = 1;
				CHECK_B( sscanf(optarg, "%s", eepromFileName) == 1 );
				break;
			case 'E':
				Eflag = 1;
				CHECK_B( sscanf(optarg, "%s", eepromFileName) == 1 );
				break;
			case 'f':
				fflag = 1;
				CHECK_B( sscanf(optarg, "%s", fpgaFileName) == 1 );
				break;
			case 'F':
				Fflag = 1;
				CHECK_B( sscanf(optarg, "%s", fpgaFileName) == 1 );
				break;
			case 'b':
				CHECK_B( sscanf(optarg, "%i%c", &busNumber, &dummy) == 1 );
				break;
			case 'a':
				CHECK_B( sscanf(optarg, "%i%c", &deviceAddress, &dummy) == 1 );
				break;
			default:
				fprintf(stderr, USAGE, argv[0]);
				goto xerror;
		}
	}

	device = findDevice(busNumber, deviceAddress);
        CHECK_USB( libusb_get_device_descriptor(device, &description) );

        CMDMSG("Device found /dev/bus/usb/%03d/%03d", libusb_get_bus_number(device), libusb_get_device_address(device));

	/** Open the usb device	*/
	CHECK_USB( libusb_open(device, &usbDeviceHandle) );

	/** Claim interface	*/
#	define INTERFACE	0
	CHECK_USB( libusb_claim_interface(usbDeviceHandle, INTERFACE) );

	if (eflag != 0)
	{
		eepromProgram(eepromFileName);
		eepromCheck(eepromFileName);
	}

	if (Eflag != 0)
	{
		eepromCheck(eepromFileName);
	}

	if (fflag != 0)
	{
		fpgaEraseBulk();
		fpgaProgram(fpgaFileName);
		fpgaCheck(fpgaFileName);
	}

	if (Fflag != 0)
	{
		fpgaCheck(fpgaFileName);
	}

	/** Release interface		*/
	CHECK_USB( libusb_release_interface(usbDeviceHandle, INTERFACE) );

	/** Close Device		*/
	libusb_close(usbDeviceHandle);

	/** OK */
	return 0;

xerror:
        return 1;
}
