namespace ClimateControl.AzureDb.DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ClimateControlEntities : DbContext
    {
        public ClimateControlEntities()
            : base("name=ClimateControlEntities")
        {
        }

        public virtual DbSet<SensorData> SensorData { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
