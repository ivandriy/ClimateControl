using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClimateControl.Web.Models;

namespace ClimateControl.Web.Controllers
{
    public class SensorController : Controller
    {
        private ClimateControlEntities db = new ClimateControlEntities();

        // GET: Sensor
        public ActionResult Index()
        {
            return View(db.SensorData.ToList());
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

        // GET: Sensor/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: Sensor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,deviceId,temperature,humidity,co2,EventProcessedUtcTime,EventEnqueuedUtcTime")] SensorData sensorData)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.SensorData.Add(sensorData);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(sensorData);
        //}

        // GET: Sensor/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SensorData sensorData = db.SensorData.Find(id);
        //    if (sensorData == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sensorData);
        //}

        // POST: Sensor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,deviceId,temperature,humidity,co2,EventProcessedUtcTime,EventEnqueuedUtcTime")] SensorData sensorData)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(sensorData).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(sensorData);
        //}

        // GET: Sensor/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SensorData sensorData = db.SensorData.Find(id);
        //    if (sensorData == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sensorData);
        //}

        // POST: Sensor/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    SensorData sensorData = db.SensorData.Find(id);
        //    db.SensorData.Remove(sensorData);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
