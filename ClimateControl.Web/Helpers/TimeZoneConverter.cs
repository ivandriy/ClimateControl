using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateControl.Web.Helpers
{
    public static class TimeZoneConverter
    {
        private const string TimeZone = "FLE Standard Time";        
        public static DateTime Convert(DateTime utcTime)
        {
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
            return result;
        }
    }
}