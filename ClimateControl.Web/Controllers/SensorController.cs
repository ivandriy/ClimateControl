using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClimateControl.Web.Helpers;
using ClimateControl.Web.Models;
using Microsoft.Practices.ObjectBuilder2;
using PagedList;

namespace ClimateControl.Web.Controllers
{
    public class SensorController : Controller
    {
        private ClimateControlEntities db = new ClimateControlEntities();        

        public ActionResult Index(string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            var sensorData = from s in db.SensorData
                                select s;
            switch (sortOrder)
            {
                case "date_asc":
                    sensorData = sensorData.OrderBy(s => s.timestamp);
                    ViewBag.DateSortParam = "date_desc";
                    ViewBag.TempSortParam = "temp_desc";
                    ViewBag.HumSortParam = "hum_desc";
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
                case "date_desc":
                    sensorData = sensorData.OrderByDescending(s => s.timestamp);
                    ViewBag.DateSortParam = "date_asc";
                    ViewBag.TempSortParam = "temp_asc";
                    ViewBag.HumSortParam = "hum_asc";
                    ViewBag.CO2SortParam = "co2_asc";
                    break;
                case "temp_asc":
                    sensorData = sensorData.OrderBy(s => s.temperature);
                    ViewBag.TempSortParam = "temp_desc";
                    break;
                case "temp_desc":
                    sensorData = sensorData.OrderByDescending(s => s.temperature);
                    ViewBag.TempSortParam = "temp_asc";
                    break;
                case "hum_asc":
                    sensorData = sensorData.OrderBy(s => s.humidity);
                    ViewBag.HumSortParam = "hum_desc";
                    break;
                case "hum_desc":
                    sensorData = sensorData.OrderByDescending(s => s.humidity);
                    ViewBag.HumSortParam = "hum_asc";
                    break;
                case "co2_asc":
                    sensorData = sensorData.OrderBy(s => s.co2);
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
                case "co2_desc":
                    sensorData = sensorData.OrderByDescending(s => s.co2);
                    ViewBag.CO2SortParam = "co2_asc";
                    break;
                default:
                    sensorData = sensorData.OrderByDescending(s => s.timestamp);
                    ViewBag.DateSortParam = "date_asc";
                    ViewBag.TempSortParam = "temp_desc";
                    ViewBag.HumSortParam = "hum_desc";
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
            }
            int pageSize = 30;
            int pageNumber = (page ?? 1);            
            foreach (SensorData data in sensorData)
            {                
                data.timestamp = TimeZoneConverter.Convert(data.timestamp);
            }
            return View(sensorData.ToPagedList(pageNumber, pageSize));
        }


        // GET: Sensor/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SensorData sensorData = db.SensorData.Find(id);
            if (sensorData == null)
            {
                return HttpNotFound();
            }
            return View(sensorData);
        }

        public ActionResult TemperatureChart(string range)
        {
            List<double> temperaturesList;
            List<string> datesList;
            GetTemperatures(range, out temperaturesList, out datesList);

            ViewBag.DatesList = string.Join(",", datesList).Trim();
            ViewBag.TemperaturesList = string.Join(",", temperaturesList).Trim();
            ViewBag.TempChartRange = string.IsNullOrEmpty(range) ? "Temperature by day" : $"Temperature by {range}";

            return View();
        }

        private void GetTemperatures(string range, out List<double> temperaturesList, out List<string> datesList)
        {
            var sensorData = GetSensorDataByRange(range);

            var dates = (from t in sensorData
                         select t.timestamp).ToList();
            temperaturesList = (from t in sensorData
                            select t.temperature).ToList();

            datesList = dates.Select(d => TimeZoneConverter.Convert(d)
                    .ToString("yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture))
                .Select(str => $"\"{str}\"")
                .ToList();
        }

        private IQueryable<SensorData> GetSensorDataByRange(string range)
        {
            
            var utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
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
                case "week":
                    startDateTime = DateTime.Today.AddDays(
                            ((int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) -
                            (int) DateTime.Today.DayOfWeek)
                        .ToUniversalTime();
                    endDateTime = startDateTime.AddDays(7).AddTicks(-1);
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
                = from s in db.SensorData
                where (s.timestamp >= startDateTime && s.timestamp <= endDateTime)
                select s;
            return sensorData;
        }

        public ActionResult HumidityChart(string range)
        {
            List<double> humiditiesList;
            List<string> datesList;
            GetHumidities(range, out humiditiesList, out datesList);

            ViewBag.DatesList = string.Join(",", datesList).Trim();
            ViewBag.HumiditiesList = string.Join(",", humiditiesList).Trim();
            ViewBag.HumChartRange = string.IsNullOrEmpty(range) ? "Humidity by day" : $"Humidity by {range}";

            return View();
        }

        private void GetHumidities(string range, out List<double> humiditiesList, out List<string> datesList)
        {
            var sensorData = GetSensorDataByRange(range);

            var dates = (from t in sensorData
                select t.timestamp).ToList();
            humiditiesList = (from t in sensorData
                select t.humidity).ToList();

            datesList = dates.Select(d => TimeZoneConverter.Convert(d)
                    .ToString("yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture))
                .Select(str => $"\"{str}\"")
                .ToList();
        }

        public ActionResult Co2Chart(string range)
        {
            List<double> co2List;
            List<string> datesList;
            GetCo2(range, out co2List, out datesList);

            ViewBag.DatesList = string.Join(",", datesList).Trim();
            ViewBag.CO2List = string.Join(",", co2List).Trim();
            ViewBag.CO2ChartRange = string.IsNullOrEmpty(range) ? "CO2 by day" : $"CO2 by {range}";

            return View();
        }

        private void GetCo2(string range, out List<double> co2List, out List<string> datesList)
        {
            var sensorData = GetSensorDataByRange(range);
            var dates = (from t in sensorData
                select t.timestamp).ToList();
            co2List = (from t in sensorData
                select t.co2).ToList();

            datesList = dates.Select(d => TimeZoneConverter.Convert(d)
                    .ToString("yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture))
                .Select(str => $"\"{str}\"")
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
