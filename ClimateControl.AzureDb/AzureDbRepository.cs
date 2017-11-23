using ClimateControl.AzureDb.DAL;
using ClimateControl.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClimateControl.AzureDb
{
    public class AzureDbRepository:ISensorDataRepository
    {
        private readonly ClimateControlEntities _context = new ClimateControlEntities();

        public IEnumerable<Sensor> GetSensorData()
        {
            var result = GetSensorDataFromDb();
            return result.ToList();
        }

        private IQueryable<Sensor> GetSensorDataFromDb()
        {
            var result = from s in _context.SensorData
                select new Sensor
                {
                    Id = s.Id,
                    DeviceId = s.deviceId,
                    Temperature = s.temperature,
                    Humidity = s.humidity,
                    CO2 = s.co2,
                    Timestamp = s.timestamp
                };
            return result;
        }

        public IEnumerable<Sensor> GetSensorData(DateTime startTime, DateTime endTime)
        {
            var result = GetSensorDataFromDb().Where(s => s.Timestamp >= startTime && s.Timestamp <= endTime);
            return result.ToList();
        }

        public Sensor GetSensorData(int? id)
        {
            Sensor sensor = new Sensor();
            var data = _context.SensorData.Find(id);
            if (data != null)
            {
                sensor.Id = data.Id;
                sensor.DeviceId = data.deviceId;
                sensor.Temperature = data.temperature;
                sensor.Humidity = data.humidity;
                sensor.CO2 = data.co2;
                sensor.Timestamp = data.timestamp;
            }

            return sensor;
        }

        public Sensor GetLatestSensorData()
        {
            return GetSensorDataFromDb().OrderByDescending(s => s.Timestamp).Take(1).SingleOrDefault();            
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
