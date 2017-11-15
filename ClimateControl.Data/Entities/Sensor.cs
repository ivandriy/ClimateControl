using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateControl.Data.Entities
{
    public class Sensor
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0}")]
        public double Temperature { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0}")]
        public double Humidity { get; set; }
        public double CO2 { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime Timestamp { get; set; }
    }
}
