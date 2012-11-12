# FX2 Commands definition
CMD_FX2_READ_VERSION        = b'\x00'
CMD_FX2_INITALIZE           = b'\xA0'
CMD_FX2_READ_STATUS			= b'\xA1'
CMD_FX2_WRITE_REGISTER		= b'\xA2'
CMD_FX2_READ_REGISTER		= b'\xA3'
CMD_FX2_RESET_FIFO_STATUS	= b'\xA4'
CMD_FX2_FLASH_READ			= b'\xA5'
CMD_FX2_FLASH_WRITE			= b'\xA6'
CMD_FX2_FLASH_ERASE			= b'\xA7'
CMD_FX2_SECTOR_ERASE		= b'\xF7'
CMD_FX2_EEPROM_READ			= b'\xA8'
CMD_FX2_EEPROM_WRITE		= b'\xA9'
CMD_FX2_GET_FIFO_STATUS		= b'\xAC'
CMD_FX2_I2C_WRITE			= b'\xAD'
CMD_FX2_I2C_READ			= b'\xAE'
CMD_FX2_POWER_ON			= b'\xAF'
CMD_FX2_FLASH_WRITE_COMMAND	= b'\xAA'
CMD_FX2_SET_INTERRUPT		= b'\xB0'
CMD_FX2_GET_INTERRUPT		= b'\xB1'
CMD_FX2_DEV_LOCK			= b'\xBB'
CMD_FX2_INTERNAL_TEST		= b'\xBD'

# Microblaze Commands definition
CMD_MB_NOP			        = b'\x00'
CMD_MB_GETVERSION	        = b'\x01'
CMD_MB_START_TX	            = b'\x02'
CMD_MB_START_RX	            = b'\x03'
CMD_MB_STOP		            = b'\x04'
CMD_MB_PING		            = b'\x05'
CMD_MB_TEST		            = b'\x06'
CMD_MB_PASSED	            = b'\x07'
CMD_MB_ERROR	            = b'\x08'

# Other definitions
MB_I2C_ADRESS				= b'\x3F'
I2C_BYTES					= b'\x0C'	# 12
PI_EP2						= 2
PI_EP4						= 4
PI_EP6						= 3
PI_EP8						= 5
