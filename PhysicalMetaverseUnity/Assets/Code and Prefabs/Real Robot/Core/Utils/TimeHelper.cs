using System;
using UnityEngine;

public static class TimeHelper
{
    public static TimeSpan SubtractTime(DateTime prev, DateTime now)
    {
        try
        {
            return now.Subtract(prev);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[TimeHelper][SubtractTime] - could not complete subtraction of datetime now: '{now}' and prev: '{prev}'.\nError: '{e}'");
            return TimeSpan.Zero;
        }
    }
    
    public static int TimeSubtractionInMinutes(DateTime prev, DateTime now)
    {
        var temp = SubtractTime(prev, now);
        return temp.Hours * 60 + temp.Minutes;
    }
}