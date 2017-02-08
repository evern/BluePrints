namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATION_DIRECT_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid? GUID_ORIGINAL_PARENT { get; set; }

        public Guid GUID_ESTIMATION_DIRECT { get; set; }

        public Guid? GUID_VARIATION { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_SUPPLYWORKPACK { get; set; }

        public Guid? GUID_INSTALLWORKPACK { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_COMMODITY_CODE { get; set; }

        public Guid? GUID_COMMODITY_GROUP_DIRECT { get; set; }

        public int? COMMODITY_GROUP_DIRECT_ID { get; set; }

        public Guid? GUID_TIMEGROUP { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_SUPPLY { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? HOURS_INSTALL { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE_FREIGHT { get; set; }

        public decimal ESTIMATED_QUANTITY { get; set; }

        public decimal VAR_QUANTITY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? CANCELLED { get; set; }

        public Guid? CANCELLEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        public virtual COMMODITY_GROUP_DIRECT COMMODITY_GROUP_DIRECT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual ESTIMATION_DIRECT ESTIMATION_DIRECT { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }

        public virtual WORKPACK WORKPACK1 { get; set; }

        public virtual TIMEGROUP TIMEGROUP { get; set; }
    }
}