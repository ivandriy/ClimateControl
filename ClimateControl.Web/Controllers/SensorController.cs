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
using Newtonsoft.Json;

namespace ClimateControl.Web.Controllers
{
    public class SensorController : Controller
    {
        private readonly ISensorDataRepository _repository;

        public SensorController(ISensorDataRepository repository)
        {
            this._repository = repository;
        }

        public ActionResult Index(string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            var sensorData = _repository.GetSensorData();
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
            Sensor sensorData = _repository.GetSensorData(id);
            if (sensorData == null)
            {
                return HttpNotFound();
            }
            sensorData.Timestamp = TimeZoneConverter.Convert(sensorData.Timestamp);
            return View(sensorData);
        }

        public ActionResult TemperatureChart()
        {            
            return View();
        }

        public JsonResult GetTemperatures(string range)
        {
            var humidityData = FilterSensorDataByRange(range)
                .Select(d => new 
                {
                    d.Temperature,
                    d.Timestamp
                })
                .ToList();                    
            var temperatures = new
            {
                Temperatures = humidityData.Select(t=>t.Temperature),
                Timestamps = humidityData.Select(t=>ConverAndFormatDate(t.Timestamp)),
                ChartRange = string.IsNullOrEmpty(range) ? "Temperature by day" : $"Temperature by {range}",
                AverageTemperature = humidityData.Select(t => t.Temperature).Average().ToString("F")
        };            
            return Json(temperatures, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NewTemperatureChart()
        {
            return View();
        }


        public JsonResult GetHumidities(string range)
        {
            var humidityData = FilterSensorDataByRange(range)
                .Select(h => new
                {
                    h.Humidity,
                    h.Timestamp
                })
                .ToList();

            var humidities = new
            {
                Humidities = humidityData.Select(h=> h.Humidity),
                Timestamps = humidityData.Select(h=> ConverAndFormatDate(h.Timestamp)),
                ChartRange = string.IsNullOrEmpty(range) ? "Humidity by day" : $"Humidity by {range}",
                AverageHumidity = humidityData.Select(h => h.Humidity).Average().ToString("F")

            };
            return Json(humidities, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HumidityChart()
        {
            return View();
        }


        public JsonResult GetCo2(string range)
        {
            var co2Data = FilterSensorDataByRange(range)
                .Select(c => new
                {
                    c.CO2,
                    c.Timestamp
                })
                .ToList();

            var co2 = new
            {
                CO2 = co2Data.Select(c => c.CO2),
                Timestamps = co2Data.Select(c => ConverAndFormatDate(c.Timestamp)),
                ChartRange = string.IsNullOrEmpty(range) ? "CO2 by day" : $"CO2 by {range}",
                AverageCO2 = co2Data.Select(c => c.CO2).Average().ToString("F")
            };

            return Json(co2, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Co2Chart(string range)
        {            
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
                = _repository.GetSensorData().Where(d => d.Timestamp >= startDateTime && d.Timestamp <= endDateTime);
            return sensorData;
        }

        private static string ConverAndFormatDate(DateTime date)
        {
            return (TimeZoneConverter.Convert(date)).ToString("yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
