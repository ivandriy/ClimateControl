using ClimateControl.Data.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClimateControl.Common;
using Newtonsoft.Json;

namespace ClimateControl.Web.Controllers
{
    public class SensorController : Controller
    {
        private readonly ISensorDataRepository _repository;
        private readonly SensorDataProcessing _processing;

        public SensorController(ISensorDataRepository repository)
        {
            this._repository = repository;
            _processing = new SensorDataProcessing(repository);
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
                r.Timestamp = DateTimeHelper.ConvertUtcToLocalTime(r.Timestamp);
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
            sensorData.Timestamp = DateTimeHelper.ConvertUtcToLocalTime(sensorData.Timestamp);
            return View(sensorData);
        }

        public ActionResult TemperatureChart()
        {            
            return View();
        }
        public JsonResult GetTemperatures(string range)
        {
            var humidityData = _processing.GetSensorDataByRange(range)
                .Select(d => new 
                {
                    d.Temperature,
                    d.Timestamp
                })
                .ToList();                    
            var temperatures = new
            {
                Temperatures = humidityData.Select(t=>t.Temperature),
                Timestamps = humidityData.Select(t=>ConvertAndFormatDate(t.Timestamp)),
                ChartRange = string.IsNullOrEmpty(range) ? "Temperature by day" : $"Temperature by {range}",
                AverageTemperature = humidityData.Select(t => t.Temperature).Average().ToString("F")
        };            
            return Json(temperatures, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHumidities(string range)
        {
            var humidityData = _processing.GetSensorDataByRange(range)
                .Select(h => new
                {
                    h.Humidity,
                    h.Timestamp
                })
                .ToList();

            var humidities = new
            {
                Humidities = humidityData.Select(h=> h.Humidity),
                Timestamps = humidityData.Select(h=> ConvertAndFormatDate(h.Timestamp)),
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
            var co2Data = _processing.GetSensorDataByRange(range)
                .Select(c => new
                {
                    c.CO2,
                    c.Timestamp
                })
                .ToList();

            var co2 = new
            {
                CO2 = co2Data.Select(c => c.CO2),
                Timestamps = co2Data.Select(c => ConvertAndFormatDate(c.Timestamp)),
                ChartRange = string.IsNullOrEmpty(range) ? "CO2 by day" : $"CO2 by {range}",
                AverageCO2 = co2Data.Select(c => c.CO2).Average().ToString("F")
            };

            return Json(co2, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Co2Chart(string range)
        {            
            return View();
        }        
        private static string ConvertAndFormatDate(DateTime date)
        {
            return (DateTimeHelper.ConvertUtcToLocalTime(date)).ToString("yyyy-MM-dd HH:mm",
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
