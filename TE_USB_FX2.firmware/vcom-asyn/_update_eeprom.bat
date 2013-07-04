@mode.com COM1: PARITY=O BAUD=222222 
@copy /b usbserial.iic COM1:
@timeout /T 3
@echo "Update finished. Reconnect module to apply changes"
@pause