using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateControl.Data.Entities
{
    public interface ISensorDataRepository:IDisposable
    {
        IQueryable<Sensor> GetSensorData();
        Sensor GetSensorData(int? id);
    }
}
