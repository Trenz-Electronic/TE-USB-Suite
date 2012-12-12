# FX2 firmware Projects
This repository contains binary firmware files for Trenz Electronic modules equipped with a USB microcontroller.
## generation 2 (_DW)
### not recommended for new designs
Recommend firmware file: *TE-USB-FX2 current __DW__.iic*

System requirements:

- Trenz Electronic hardware: TE0300 series, TE0320 series, TE0630 series
- VID (vendor identifier): 0547 (Anchor Chips, now Cypress Semiconductor)
- PID (product identifier): 1002 (Python2 WDM Encoder)
- device drivers: DEWESoft USB FX2 (generation 2)
- API: DEWESoft USB FX2 (generation 2)

Please check your VID/PID in device manager (devmgmt.msc > properties > details > Hardware Ids).

## generation 3 (_TE)
Recommend firmware file: *TE-USB-FX2 current __TE__.iic*

System requirements:

- Trenz Electronic hardware: TE0300 series, TE0320 series, TE0630 series
- VID (vendor identifier): 0BD0 (Trenz Electronic GmbH)
- PID (product identifier): 0300 (USB 2.0 FPGA modules)
- device drivers: Trenz Electronic USB FX2 (generation 3)
- API: Trenz Electronic USB FX2 (generation 3)

Please check your VID/PID in device manager (devmgmt.msc > properties > details > Hardware Ids).
