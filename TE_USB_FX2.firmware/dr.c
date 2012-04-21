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
#include "fx2regs.h"
#include "fx2sdly.h"            // SYNCDELAY macro

BYTE Configuration;                 // Current configuration
BYTE AlternateSetting;              // Alternate settings
BOOL in_enable = FALSE;             // flag to enable IN transfers
BOOL enum_high_speed = FALSE;       // flag to let firmware know FX2 enumerated
									// at high speed

//-----------------------------------------------------------------------------
// Device Request hooks
//   The following hooks are called by the end point 0 device request parser.
//-----------------------------------------------------------------------------

BOOL DR_GetDescriptor(void)
{
   return(TRUE);
}

BOOL DR_SetConfiguration(void)   	// Called when a Set Configuration command 
{									// is received
  if( EZUSB_HIGHSPEED( ) )
  { // FX2 enumerated at high speed
    SYNCDELAY;                  // 
    EP6AUTOINLENH = 0x02;       // set AUTOIN commit length to 512 bytes
    SYNCDELAY;                  // 
    EP6AUTOINLENL = 0x00;
    SYNCDELAY;                  
    enum_high_speed = TRUE;
  }
  else
  { // FX2 enumerated at full speed
    SYNCDELAY;                   
    EP6AUTOINLENH = 0x00;       // set AUTOIN commit length to 64 bytes
    SYNCDELAY;                   
    EP6AUTOINLENL = 0x40;
    SYNCDELAY;                  
    enum_high_speed = FALSE;
  }

  Configuration = SETUPDAT[2];
  return(TRUE);            // Handled by user code
}

BOOL DR_GetConfiguration(void)   	// Called when a Get Configuration command 
{									// is received
   EP0BUF[0] = Configuration;
   EP0BCH = 0;
   EP0BCL = 1;
   return(TRUE);            // Handled by user code
}

BOOL DR_SetInterface(void)       	// Called when a Set Interface command 
{									// is received
   AlternateSetting = SETUPDAT[2];
   return(TRUE);            // Handled by user code
}

BOOL DR_GetInterface(void)       	// Called when a Set Interface command 
{									// is received
   EP0BUF[0] = AlternateSetting;
   EP0BCH = 0;
   EP0BCL = 1;
   return(TRUE);            // Handled by user code
}

BOOL DR_GetStatus(void)
{
   return(TRUE);
}

BOOL DR_ClearFeature(void)
{
   return(TRUE);
}

BOOL DR_SetFeature(void)
{
   return(TRUE);
}

#define VX_B2 0xB2 // reset the external FIFO
#define VX_B3 0xB3 // enable IN transfers
#define VX_B4 0xB4 // disable IN transfers
#define VX_B5 0xB5 // read GPIFREADYSTAT register
#define VX_B6 0xB6 // read GPIFTRIG register

BOOL DR_VendorCmnd(void)
{
  return(FALSE);
}
