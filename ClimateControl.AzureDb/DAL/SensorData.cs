namespace ClimateControl.AzureDb.DAL
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
        public double temperature { get; set; }        
        public double humidity { get; set; }
        public double co2 { get; set; }        
        public DateTime timestamp { get; set; }
    }
}
