using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class MicrosecondsEpochDateTimeConverter 
{
    // Hàm chuyển đổi microseconds epoch thành DateTime
    public static DateTime? Convert(long? microseconds)
    {
        if (microseconds == null)
        {
            return null;
        }
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddTicks(microseconds ?? 0 * 10); // 1 microsecond = 10 ticks
    }
    
    public static DateTime? ToDateTimeForEpochMSec(long? microseconds)
    {
        if (microseconds == null)
        {
            return null;
        }
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
        long ticks = (long)(microseconds * ticksPerMicrosecond);
        DateTime tempDate = epoch.AddTicks(ticks);
        return tempDate;
    }  
}