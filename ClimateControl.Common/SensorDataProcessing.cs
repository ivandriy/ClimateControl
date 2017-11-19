using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClimateControl.Data.Entities;

namespace ClimateControl.Common
{
    public class SensorDataProcessing
    {
        private readonly ISensorDataRepository _repository;
        public SensorDataProcessing(ISensorDataRepository repository)
        {
            _repository = repository;
        }

       public IQueryable<Sensor> GetSensorDataByRange(string range)
        {
            //Time zone difference between local time and UTC
            var utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            //Time zone difference between FLE time (Kiev zone) and UTC
            var fleOffset = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time").BaseUtcOffset;
            DateTime startDateTime;
            DateTime endDateTime;

            switch (range)
            {
                case "4 hours":
                    startDateTime = DateTime.UtcNow.AddHours(-4);
                    endDateTime = DateTime.UtcNow;
                    break;

                case "8 hours":
                    startDateTime = DateTime.UtcNow.AddHours(-8);
                    endDateTime = DateTime.UtcNow;
                    break;
                case "day":
                    if (utcOffset.Hours < 0)
                    {
                        startDateTime = DateTime.Today.ToUniversalTime();
                        endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddTicks(-1);
                    }
                    else
                    {
                        startDateTime = DateTime.Today.AddHours(-fleOffset.Hours);
                        endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddHours(-fleOffset.Hours).AddTicks(-1);
                    }
                    break;
                case "24 hours":
                    startDateTime = DateTime.UtcNow.AddHours(-24);
                    endDateTime = DateTime.UtcNow;
                    break;
                case "7 days":
                    startDateTime = DateTime.UtcNow.AddDays(-7);
                    endDateTime = DateTime.UtcNow;
                    break;
                case "week":
                    if (utcOffset.Hours < 0)
                    {
                        startDateTime = DateTime.Today.AddDays(
                                ((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) -
                                (int)DateTime.Today.DayOfWeek)
                            .ToUniversalTime();
                        endDateTime = startDateTime.AddDays(7).AddTicks(-1);
                    }
                    else
                    {
                        startDateTime = DateTime.Today.AddDays(
                                ((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) -
                                (int)DateTime.Today.DayOfWeek)
                            .ToUniversalTime().AddHours(-fleOffset.Hours);
                        endDateTime = startDateTime.AddDays(7).AddHours(-fleOffset.Hours).AddTicks(-1);
                    }
                    break;
                case "30 days":
                    startDateTime = DateTime.UtcNow.AddDays(-30);
                    endDateTime = DateTime.UtcNow;
                    break;
                case "month":
                    startDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToUniversalTime();
                    endDateTime = startDateTime.ToUniversalTime().AddMonths(1).AddDays(-1).AddTicks(-1);
                    break;
                default:
                    if (utcOffset.Hours < 0)
                    {
                        startDateTime = DateTime.Today.ToUniversalTime();
                        endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddTicks(-1);
                    }
                    else
                    {
                        startDateTime = DateTime.Today.AddHours(-fleOffset.Hours);
                        endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddHours(-fleOffset.Hours).AddTicks(-1);
                    }
                    break;
            }

            var sensorData
                = _repository.GetSensorData().Where(d => d.Timestamp >= startDateTime && d.Timestamp <= endDateTime);
            return sensorData;
        }

        public Sensor GetLatestSensorData()
        {
            var latestSensorData = _repository.GetSensorData().OrderByDescending(d => d.Timestamp).Take(1).SingleOrDefault();
            return latestSensorData;
        }

    }
}
