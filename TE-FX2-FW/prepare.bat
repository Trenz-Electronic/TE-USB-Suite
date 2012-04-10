@mkdir Cypress
@cd Cypress
@install\wget.exe -O cy3684_ez_usb_fx2lp_development_kit_15.exe http://www.cypress.com/?docID=6018
@echo "Press any key to start EZ-USB Development Kit installation"
@echo "Install to default (C:\Cypress\USB) destination folder"
@echo "Select Typical instalation"
@pause
@start cy3684_ez_usb_fx2lp_development_kit_15.exe
@cd ..
