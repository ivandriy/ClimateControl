namespace ClimateControl.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SensorData")]
    public partial class SensorData
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string deviceId { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double temperature { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double humidity { get; set; }

        public double co2 { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime timestamp { get; set; }
    }
}
