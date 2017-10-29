using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClimateControl.Web.Models;

namespace ClimateControl.Web.Controllers
{
    public class HomeController : Controller
    {
        private ClimateControlEntities db = new ClimateControlEntities();
        public ActionResult Index()
        {
            var latestSensorData = (from s in db.SensorData
                                   orderby s.EventEnqueuedUtcTime descending 
                                select s).Take(1).SingleOrDefault();
                        
            return View(latestSensorData);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}