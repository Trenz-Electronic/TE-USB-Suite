#pragma once

#include <windows.h>

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
