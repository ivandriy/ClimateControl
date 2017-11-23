using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateControl.Data.Entities
{
    public interface ISensorDataRepository:IDisposable
    {
        IEnumerable<Sensor> GetSensorData();
        IEnumerable<Sensor> GetSensorData(DateTime startTime, DateTime endTime);
        Sensor GetSensorData(int? id);
        Sensor GetLatestSensorData();
    }
}
