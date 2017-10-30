using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClimateControl.Web.Models;

namespace ClimateControl.Web.Helpers
{
    public static class TimeZoneConverter
    {
        private const string ConvertToTimeZone = "FLE Standard Time";        
        public static DateTime Convert(DateTime utcTime)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));
            return localTime;
        }
    }
}