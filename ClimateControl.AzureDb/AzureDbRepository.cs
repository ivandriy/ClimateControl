using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClimateControl.AzureDb.DAL;
using ClimateControl.Data.Entities;
using SensorData = ClimateControl.Data.Entities.Sensor;

namespace ClimateControl.AzureDb
{
    public class AzureDbRepository:ISensorDataRepository
    {
        private readonly ClimateControlEntities _context = new ClimateControlEntities();

        public IQueryable<Sensor> GetSensorData()
        {
            return from s in _context.SensorData select new Sensor
            {
                Id = s.Id,
                DeviceId = s.deviceId,
                Temperature = s.temperature,
                Humidity = s.humidity,
                CO2 = s.co2,
                Timestamp = s.timestamp
            };
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

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
