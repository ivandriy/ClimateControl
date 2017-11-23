using System.Linq;
using System.Web.Mvc;
using ClimateControl.Common;
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
            var latestSensorData = _repository.GetLatestSensorData();
            if (latestSensorData != null)
            {
                latestSensorData.Timestamp = DateTimeHelper.ConvertUtcToLocalTime(latestSensorData.Timestamp);
            }
            return View(latestSensorData);
        }

        [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 30)]
        public ActionResult ClimateNow()
        {
            var latestSensorData = _repository.GetLatestSensorData();
            if (latestSensorData != null)
            {
                latestSensorData.Timestamp = DateTimeHelper.ConvertUtcToLocalTime(latestSensorData.Timestamp);
            }
            return PartialView("ClimateNow",latestSensorData);
        }

        public ActionResult About()
        {            
            return View();
        }

        public ActionResult ClimateDashboard()
        {
            var latestSensorData = _repository.GetLatestSensorData();
            if (latestSensorData != null)
            {
                latestSensorData.Timestamp = DateTimeHelper.ConvertUtcToLocalTime(latestSensorData.Timestamp);
            }
            return View(latestSensorData);            
        }
    }
}