'! \file init.c
'! \brief
'! Initialisation module of STM300 application

#Include "EO3100I_API.h"
#Include "EO3100I_CFG.h"
#Include "DolphinSTM.h"
#Include "string.h"

'lint -restore
'structure for reading LOG_AREA
'LOG_AREA code log_read  _at_ LOG_ADDR;

'***************************Functions*********************************


Private Sub powerUpInitialization()
	'initialize all RAM0 shadow data with 0
	memset(AddressOfsctgram0, 0, sizeof(RAM0_SHADOW))
End Sub

Private Sub init()
	' Configuration in EO3000I_CFG.h
	mainInit()
	' Read back saved values of RAM0
	mem_readRAM0(CType(AddressOfsctgram0, uint8*), &H0, sizeof(RAM0_SHADOW))
	'lint !e534
End Sub