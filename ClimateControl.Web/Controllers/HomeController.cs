using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClimateControl.Web.Helpers;
using ClimateControl.Web.Models;

namespace ClimateControl.Web.Controllers
{
    public class HomeController : Controller
    {
        private ClimateControlEntities db = new ClimateControlEntities();
        public ActionResult Index()
        {
            var latestSensorData = GetLatestSensorData();
            return View(latestSensorData);
        }

        private SensorData GetLatestSensorData()
        {
            SensorData latestSensorData;
            latestSensorData = (from s in db.SensorData
                    orderby s.timestamp descending
                    select s).Take(1)
                .SingleOrDefault();
            if (latestSensorData != null)
            {
                latestSensorData.timestamp = TimeZoneConverter.Convert(latestSensorData.timestamp);
            }
            return latestSensorData;
        }

        [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 30)]
        public ActionResult ClimateNow()
        {
            var latestSensorData = GetLatestSensorData();
            return PartialView("ClimateNow",latestSensorData);
        }
        
    }
}