using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
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
                    sensorData = sensorData.OrderBy(s => s.EventEnqueuedUtcTime);
                    ViewBag.DateSortParam = "date_desc";
                    ViewBag.TempSortParam = "temp_desc";
                    ViewBag.HumSortParam = "hum_desc";
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
                case "date_desc":
                    sensorData = sensorData.OrderByDescending(s => s.EventEnqueuedUtcTime);
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
                    sensorData = sensorData.OrderByDescending(s => s.EventEnqueuedUtcTime);
                    ViewBag.DateSortParam = "date_asc";
                    ViewBag.TempSortParam = "temp_desc";
                    ViewBag.HumSortParam = "hum_desc";
                    ViewBag.CO2SortParam = "co2_desc";
                    break;
            }
            int pageSize = 30;
            int pageNumber = (page ?? 1);            
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
            List<double> temperatures;
            List<string> datesList;
            GetTemperatures(range, out temperatures, out datesList);

            ViewBag.DatesList = string.Join(",", datesList).Trim();
            ViewBag.TemperaturesList = string.Join(",", temperatures).Trim();
            ViewBag.ChartRange = string.IsNullOrEmpty(range) ? "Temperature by day" : $"Temperature by {range}";

            return View();
        }

        private void GetTemperatures(string range, out List<double> temperatures, out List<string> datesList)
        {
            DateTime startDateTime;
            DateTime endDateTime;
            switch (range)
            {
                case "day":
                    startDateTime = DateTime.Today.ToUniversalTime();
                    endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddTicks(-1);
                    break;
                case "week":
                    startDateTime = DateTime.Today.AddDays(
                            ((int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 1) -
                            (int) DateTime.Today.DayOfWeek)
                        .ToUniversalTime();
                    endDateTime = startDateTime.AddDays(7).AddTicks(-1);
                    break;
                case "month":
                    startDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToUniversalTime();
                    endDateTime = startDateTime.ToUniversalTime().AddMonths(1).AddDays(-1).AddTicks(-1);
                    break;
                default:
                    startDateTime = DateTime.Today.ToUniversalTime();
                    endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddTicks(-1);
                    break;
            }


            var tempData = (from s in db.SensorData
                where (s.EventEnqueuedUtcTime >= startDateTime && s.EventEnqueuedUtcTime <= endDateTime)
                select new
                {
                    s.temperature,
                    s.EventEnqueuedUtcTime
                });

            var dates = (from t in tempData
                select t.EventEnqueuedUtcTime).ToList();
            temperatures = (from t in tempData
                select t.temperature).ToList();

            datesList = dates.Select(d => d.ToLocalTime()
                    .ToString("yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture))
                .Select(str => $"\"{str}\"")
                .ToList();
        }

        public ActionResult HumidityChart()
        {
            DateTime startDateTime = DateTime.Today.ToUniversalTime();
            DateTime endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddTicks(-1);

            var tempData = (from s in db.SensorData
                where (s.EventEnqueuedUtcTime >= startDateTime && s.EventEnqueuedUtcTime <= endDateTime)
                select new
                {
                    s.humidity,
                    s.EventEnqueuedUtcTime
                });

            var dates = (from t in tempData
                select t.EventEnqueuedUtcTime).ToList();
            var humidities = (from t in tempData
                select t.humidity).ToList();

            var datesList = dates.Select(d => d.ToLocalTime().ToString("yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture)).Select(str => $"\"{str}\"").ToList();

            ViewBag.DatesList = string.Join(",", datesList).Trim();
            ViewBag.HumiditiesList = string.Join(",", humidities).Trim();

            return View();
        }

        public ActionResult Co2Chart()
        {
            DateTime startDateTime = DateTime.Today.ToUniversalTime();
            DateTime endDateTime = DateTime.Today.ToUniversalTime().AddDays(1).AddTicks(-1);

            var tempData = (from s in db.SensorData
                where (s.EventEnqueuedUtcTime >= startDateTime && s.EventEnqueuedUtcTime <= endDateTime)
                select new
                {
                    s.co2,
                    s.EventEnqueuedUtcTime
                });

            var dates = (from t in tempData
                select t.EventEnqueuedUtcTime).ToList();
            var co2 = (from t in tempData
                select t.co2).ToList();

            var datesList = dates.Select(d => d.ToLocalTime().ToString("yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture)).Select(str => $"\"{str}\"").ToList();

            ViewBag.DatesList = string.Join(",", datesList).Trim();
            ViewBag.CO2List = string.Join(",", co2).Trim();

            return View();
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
