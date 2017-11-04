using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClimateControl.Web.Models;

namespace ClimateControl.Web.Helpers
{
    public static class TimeZoneConverter
    {
        private const string TimeZone = "FLE Standard Time";        
        public static DateTime Convert(DateTime utcTime)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
            return localTime;
        }
    }
}