using ClimateControl.Data.Entities;
using ClimateControl.Web.Helpers;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ClimateControl.Web.Controllers
{
    public class SensorController : Controller
    {
        private readonly ISensorDataRepository repository;

        public SensorController(ISensorDataRepository repository)
        {
            this.repository = repository;
        }

        public ActionResult Index(string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            var sensorData = repository.GetSensorData();
            switch (sortOrder)
            {
                case "date_asc":
                    sensorData = sensorData.OrderBy(s => s.Timestamp);
                    ViewBag.DateSortParam = "date_desc";
                    ViewBag.TempSortParam = "temp_desc";
                    ViewBag.HumSortParam = "hum_desc";
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
                case "date_desc":
                    sensorData = sensorData.OrderByDescending(s => s.Timestamp);
                    ViewBag.DateSortParam = "date_asc";
                    ViewBag.TempSortParam = "temp_asc";
                    ViewBag.HumSortParam = "hum_asc";
                    ViewBag.CO2SortParam = "co2_asc";
                    break;
                case "temp_asc":
                    sensorData = sensorData.OrderBy(s => s.Temperature);
                    ViewBag.TempSortParam = "temp_desc";
                    break;
                case "temp_desc":
                    sensorData = sensorData.OrderByDescending(s => s.Temperature);
                    ViewBag.TempSortParam = "temp_asc";
                    break;
                case "hum_asc":
                    sensorData = sensorData.OrderBy(s => s.Humidity);
                    ViewBag.HumSortParam = "hum_desc";
                    break;
                case "hum_desc":
                    sensorData = sensorData.OrderByDescending(s => s.Humidity);
                    ViewBag.HumSortParam = "hum_asc";
                    break;
                case "co2_asc":
                    sensorData = sensorData.OrderBy(s => s.CO2);
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
                case "co2_desc":
                    sensorData = sensorData.OrderByDescending(s => s.CO2);
                    ViewBag.CO2SortParam = "co2_asc";
                    break;
                default:
                    sensorData = sensorData.OrderByDescending(s => s.Timestamp);
                    ViewBag.DateSortParam = "date_asc";
                    ViewBag.TempSortParam = "temp_desc";
                    ViewBag.HumSortParam = "hum_desc";
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
            }
            int pageSize = 30;
            int pageNumber = (page ?? 1);
            var result = sensorData.ToPagedList(pageNumber, pageSize);
            foreach (var r in result)
            {
                r.Timestamp = TimeZoneConverter.Convert(r.Timestamp);
            }            
            return View(result);
        }       

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sensor sensorData = repository.GetSensorData(id);
            if (sensorData == null)
            {
                return HttpNotFound();
            }
            sensorData.Timestamp = TimeZoneConverter.Convert(sensorData.Timestamp);
            return View(sensorData);
        }

        public ActionResult TemperatureChart(string range)
        {
            var temperatureData = FilterSensorDataByRange(range)
                .Select(d=> new
                    {
                        d.Temperature,
                        d.Timestamp
                    })
                .ToList();
           
            ViewBag.DatesList = string.Join(",", NormalizeDates(temperatureData.Select(t => t.Timestamp))).Trim();
            ViewBag.TemperaturesList = string.Join(",", temperatureData.Select(t=>t.Temperature)).Trim();
            ViewBag.TempChartRange = string.IsNullOrEmpty(range) ? "Temperature by day" : $"Temperature by {range}";
            ViewBag.AvgTemperature = temperatureData.Select(t=>t.Temperature).Average().ToString("F");
            return View();
        }

        public ActionResult HumidityChart(string range)
        {
            var humidityData = FilterSensorDataByRange(range)
                .Select(h => new
                    {
                        h.Humidity,
                        h.Timestamp
                    })
                    .ToList();

            ViewBag.DatesList = string.Join(",", NormalizeDates(humidityData.Select(h=>h.Timestamp))).Trim();
            ViewBag.HumiditiesList = string.Join(",", humidityData.Select(h=>h.Humidity)).Trim();
            ViewBag.HumChartRange = string.IsNullOrEmpty(range) ? "Humidity by day" : $"Humidity by {range}";
            ViewBag.AvgHumidity = humidityData.Select(h => h.Humidity).Average().ToString("F");

            return View();
        }

        public ActionResult Co2Chart(string range)
        {
            var co2Data = FilterSensorDataByRange(range)
                .Select(c => new
                {
                    c.CO2,
                    c.Timestamp
                })
                .ToList();

            ViewBag.DatesList = string.Join(",", NormalizeDates(co2Data.Select(c=>c.Timestamp))).Trim();
            ViewBag.CO2List = string.Join(",", co2Data.Select(c=>c.CO2)).Trim();
            ViewBag.CO2ChartRange = string.IsNullOrEmpty(range) ? "CO2 by day" : $"CO2 by {range}";
            ViewBag.AvgCO2 = co2Data.Select(c => c.CO2).Average().ToString("F");
            return View();
        }

        private IQueryable<Sensor> FilterSensorDataByRange(string range)
        {
            //Time zone difference between local time and UTC
            var utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            //Time zone difference between FLE time (Kiev zone) and UTC
            var fleOffset = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time").BaseUtcOffset;
            DateTime startDateTime;
            DateTime endDateTime;

            switch (range)
            {
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
                = repository.GetSensorData().Where(d => d.Timestamp >= startDateTime && d.Timestamp <= endDateTime);
            return sensorData;
        }

        private static IEnumerable<string> NormalizeDates(IEnumerable<DateTime> dates)
        {
            var datesList = dates.Select(d => TimeZoneConverter.Convert(d)
                    .ToString("yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture))
                .Select(str => $"\"{str}\"")
                .ToList();
            return datesList;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
