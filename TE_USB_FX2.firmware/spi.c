/*
Copyright (C) 2012 Trenz Electronic

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included 
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
IN THE SOFTWARE.
*/
#include "fx2.h"
#include "fx2sdly.h"
#include "spi.h"
#include "fpga.h"

/*
    Put byte to SPI port
*/
void putcSPI(BYTE data_wr) {
    unsigned char wr_cnt, spi_digit;
    spi_digit = 128;
    SPI_CLK = 0;
    SYNCDELAY;
    for (wr_cnt = 0; wr_cnt < 8; wr_cnt++) {
        if ( (data_wr & spi_digit) != 0 )
            SPI_D = 1;
        else
            SPI_D = 0;
        SPI_CLK = 1;
        SYNCDELAY;
        SPI_CLK = 0;
        spi_digit = spi_digit >> 1;
    }
}

/*
    Get byte from SPI port
*/
unsigned char getcSPI(void) {
    unsigned char cnt, data_rd;

    SPI_CLK = 0;
    SYNCDELAY;
    data_rd = 0;
    for (cnt = 0; cnt < 8; cnt++) {
        SPI_CLK = 1;
        SYNCDELAY;
        data_rd = data_rd << 1;
        if (SPI_Q)
            data_rd = data_rd + 1;
        SPI_CLK = 0;
        SYNCDELAY;
    }
    return data_rd;
}

/*
    Write page to flash
*/
void page_write (BYTE addhighest, BYTE addhigh, BYTE addlow, 
    unsigned char *wrptr, WORD p_write_size) {
    volatile unsigned int pw_count, pw_adress;
    volatile unsigned char add_high;

    OED = 0x71;		    // 0b01110001;
    FPGA_POWER_ON = 0;	// power off fpga

    pw_count = p_write_size;
    add_high = addhighest;
    write_enable ();

    pw_adress = (unsigned int)addhigh;
    pw_adress = (pw_adress << 8) + addlow;

    FLASH_ENABLE        // assert chip select
    putcSPI(SPI_WRITE); // send write command
    putcSPI(add_high);  // send high byte of address
    putcSPI(addhigh);   // send high byte of address
    putcSPI(addlow);    // send low byte of address
    while (pw_count != 0) {
        pw_count = pw_count - 1;
        putcSPI(*wrptr);
        wrptr++;
        pw_adress++;
        if (pw_adress == 0) {	// if address pass limit
            add_high++;
            FLASH_DISABLE       // negate chip select
            busy_polling();
            write_enable ();
            FLASH_ENABLE        // assert chip select
            putcSPI(SPI_WRITE); // send write command
            putcSPI(add_high);  // send high byte of address
            putcSPI(addhigh);   // send high byte of address
            putcSPI(addlow);    // send low byte of address
        }
    }
    FLASH_DISABLE //negate chip select
    busy_polling();
}

/*
    Read page from Flash
*/
void page_read (BYTE addhighest, BYTE addhigh, BYTE addlow,	BYTE *rdptr, 
    WORD count) {
    unsigned char rd_buff;
    volatile unsigned int pr_count, pr_address;

    OED = 0x71;		    // 0b01110001;
    FPGA_POWER_ON = 0;	// power off fpga

    pr_count = count;
    if (pr_count > 256) pr_count = pr_count & 0x00ff;

    pr_address = (unsigned int) addhigh;
    pr_address = (pr_address << 8) + addlow;

    FLASH_ENABLE            // assert chip select
    putcSPI(SPI_READ);      // send read command
    putcSPI(addhighest);    // send high byte of address
    putcSPI(addhigh);       // send high byte of address
    putcSPI(addlow);        // send low byte of address
    while (pr_count != 0) {
        pr_count = pr_count - 1;
        rd_buff = getcSPI();
        *rdptr = rd_buff;
        rdptr++;
        pr_address++;
        if (pr_address == 0) {
            addhighest++;
            FLASH_DISABLE

            FLASH_ENABLE
            putcSPI(SPI_READ);      // send read command
            putcSPI(addhighest);    // send high byte of address
            putcSPI(addhigh);       // send high byte of address
            putcSPI(addlow);        // send low byte of address
        }
    }
    FLASH_DISABLE
}

/*
    Send Wite Enable command to Flash
*/
void write_enable (void) {
    FLASH_ENABLE
    putcSPI(SPI_WREN);  // write status command
    FLASH_DISABLE       // negate chip select
}

/*
    Wait till Flash ready
*/
void busy_polling (void) {
    unsigned char var_bp = 1;

    while ((var_bp & 0x01) != 0) {  //stay in loop until !busy
        FLASH_ENABLE                //assert chip select
        putcSPI(SPI_RDSR);          //send read status command
        var_bp = getcSPI();         //read data byte
        FLASH_DISABLE               //negate chip select
    }
}

/*
    Send Chip Erace command to Flash
*/
void bulk_erase(void) {
    OED = 0x71;		                // 0b01110001;
    FPGA_POWER_ON = 0;	            // power off fpga
    write_enable ();
    FLASH_ENABLE                    // assert chip select
    putcSPI(0xC7);                  // send SE command
    FLASH_DISABLE                   // negate chip select
    //busy_polling();
}

/*
    Send Sector Erase command to Flash
*/
void sector_erase(BYTE sector) {
	OED = 0x71;		    // 0b01110001;
	FPGA_POWER_ON = 0;	// power off fpga
	write_enable ();
	
	FLASH_ENABLE        // assert chip select
	putcSPI(0xD8);      // send SE command
	putcSPI(sector);	// addr 23:16
	putcSPI(0);			// addr 15:8
	putcSPI(0);			// addr  7:0
	FLASH_DISABLE       // negate chip select
	busy_polling();
}

/*
    Check Flash busy flag
*/
char check_flash_busy(void) {
    BYTE busy;

    OED = 0x71;		    // 0b01110001;
    FLASH_ENABLE        // assert chip select
    putcSPI(SPI_RDSR);  // send read status command
    busy = getcSPI();   // read data byte
    FLASH_DISABLE       // negate chip select
    if((busy & 0x01) != 0) {
        return 1;
    }
    return 0;
}

void spi_command(BYTE CmdLen, unsigned char *CmdData, BYTE RdLen, 
    unsigned char *RdData) {
    volatile unsigned char spi_count, rd_buff;// pr_address;

    OED = 0x71;		    // 0b01110001;
    FPGA_POWER_ON = 0;	//power off fpga

    FLASH_ENABLE        //assert chip select
    //Write command
    spi_count = CmdLen;
    if (spi_count > 64) 
        spi_count = 64;
    while (spi_count > 0) {
        putcSPI(*CmdData); //send read command
        CmdData++;
        spi_count = spi_count - 1;
    }
    //Read response
    spi_count = RdLen;
    if (spi_count > 64) 
        spi_count = 64;
    while (spi_count > 0) {
        rd_buff = getcSPI();
        *RdData = rd_buff;
        RdData++;
        spi_count = spi_count - 1;
    }
    FLASH_DISABLE
}
