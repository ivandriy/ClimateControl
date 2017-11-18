using ClimateControl.Web.Helpers;
using System.Linq;
using System.Web.Mvc;
using ClimateControl.Data.Entities;

namespace ClimateControl.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISensorDataRepository _repository;

        public HomeController(ISensorDataRepository repository)
        {
            this._repository = repository;
        }

        public ActionResult Index()
        {
            var latestSensorData = GetLatestSensorData();
            return View(latestSensorData);
        }

        private Sensor GetLatestSensorData()
        {
            Sensor latestSensorData;
            latestSensorData = _repository.GetSensorData().OrderByDescending(d => d.Timestamp).Take(1).SingleOrDefault();
            if (latestSensorData != null)
            {
                latestSensorData.Timestamp = TimeZoneConverter.Convert(latestSensorData.Timestamp);
            }
            return latestSensorData;
        }

        [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 30)]
        public ActionResult ClimateNow()
        {
            var latestSensorData = GetLatestSensorData();
            return PartialView("ClimateNow",latestSensorData);
        }

        public ActionResult About()
        {            
            return View();
        }
    }
}