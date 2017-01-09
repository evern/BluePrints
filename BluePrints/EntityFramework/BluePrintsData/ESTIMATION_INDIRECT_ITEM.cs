namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_INDIRECT_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid GUID_ESTIMATION_INDIRECT { get; set; }

        public Guid? GUID_WORKPACK { get; set; }

        public Guid? GUID_TIMEGROUP { get; set; }

        public Guid GUID_COMMODITYCODE { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public int UNITS { get; set; }

        public decimal? OPERATOR_RATE { get; set; }

        public decimal? PLANT_RATE { get; set; }

        public decimal? HOURSAWEEK { get; set; }

        public decimal? FREIGHT_FOOTPRINT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        public virtual ESTIMATION_INDIRECT ESTIMATION_INDIRECT { get; set; }

        public virtual TIMEGROUP TIMEGROUP { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }
    }
}
