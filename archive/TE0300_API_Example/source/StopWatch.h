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
#pragma once

/// Takes the time
class CStopWatch
{
public:
  /// constructor, starts time measurement
  CStopWatch()
  {
    Start();
  }
  
  /// Start the stop watch 
  void Start()
  {
    QueryPerformanceCounter(&m_StartTime);
  }
  
  /// Stop. The elapsed time is returned. The stop watch may be started again
  double Stop(bool StartAgain)
  {
    QueryPerformanceCounter(&m_StopTime);
    double theElapsedTime = ElapsedTime();
    if(StartAgain)
      m_StartTime = m_StopTime; 
    return theElapsedTime;
  }
  
  /// Return the elapsed time in seconds between start() and stop()
  double ElapsedTime()
  {
    LARGE_INTEGER timerFrequency;
    QueryPerformanceFrequency(&timerFrequency);
    
    __int64 oldTicks = ((__int64)m_StartTime.HighPart << 32) + (__int64)m_StartTime.LowPart;
    __int64 newTicks = ((__int64)m_StopTime.HighPart << 32) + (__int64)m_StopTime.LowPart;
    long double timeDifference = (long double) (newTicks - oldTicks);
    
    long double ticksPerSecond = (long double) (((__int64)timerFrequency.HighPart << 32) 
      + (__int64)timerFrequency.LowPart);
    
    return (double)(timeDifference / ticksPerSecond);
  }
  
protected:
  /// zero-point for time measurment
  LARGE_INTEGER m_StartTime;
  
  /// last time stamp
  LARGE_INTEGER m_StopTime;
};
