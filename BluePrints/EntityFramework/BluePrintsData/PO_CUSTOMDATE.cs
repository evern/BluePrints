namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PO_CUSTOMDATE")]
    public partial class PO_CUSTOMDATE
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid FORECAST_PO_GUID { get; set; }

        public DateTime PAYMENT_DATE { get; set; }

        public decimal PAYMENT_PERCENT { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual FORECAST_PO FORECAST_PO { get; set; }
    }
}
