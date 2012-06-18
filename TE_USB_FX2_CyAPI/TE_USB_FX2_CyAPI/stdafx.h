#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             

#include <windows.h>

#include <wtypes.h>
#include <dbt.h>

#pragma comment (lib, "CyAPI.lib")

#include "CyAPI.h"
//#include <Pdh.h>

#include <SetupAPI.h>

//enum PI_PipeNumber
//{
//  PI_EP2	= 2,
//  PI_EP4	= 4,
//  PI_EP6	= 3,
//  PI_EP8	= 5
//};

enum ST_Status
{
  ST_OK = 0,
  ST_ERROR = 1
};

