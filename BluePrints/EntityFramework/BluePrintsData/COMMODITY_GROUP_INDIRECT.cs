namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class COMMODITY_GROUP_INDIRECT
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public Guid? GUID_COMMODITYCODE { get; set; }

        public Guid? GUID_PARENT { get; set; }

        [Required]
        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public DateTime? START { get; set; }

        public DateTime? FINISH { get; set; }

        public int? UNITS { get; set; }

        public decimal? OPERATOR_RATE { get; set; }

        public decimal? PLANT_RATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }
    }
}
