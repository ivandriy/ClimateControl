using System;

namespace ClimateControl.Common
{
    public static class DateTimeHelper
    {
        private const string TimeZone = "FLE Standard Time";        
        public static DateTime ConvertUtcToLocalTime(DateTime utcTime)
        {
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
            return result;
        }
    }
}