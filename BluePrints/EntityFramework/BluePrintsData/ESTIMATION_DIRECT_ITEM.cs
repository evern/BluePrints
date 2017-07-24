namespace BluePrints.Data
{
    using BluePrints.Common;
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

        public Guid? GUID_WORKPACK { get; set; }

        public Guid GUID_ESTIMATION_DIRECT { get; set; }

        public Guid? GUID_VARIATION { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_SUBAREA { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_STOCK_CODE { get; set; }

        public Guid? GUID_COMMODITY_CODE { get; set; }

        [Required]
        public int DISCIPLINE_NUM { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        public decimal ESTIMATED_QUANTITY { get; set; }

        public Estimation_DirectProgressType PROGRESS_TYPE { get; set; }

        public decimal? PRODUCTIVITY_OVERRIDE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? CANCELLED { get; set; }

        public Guid? CANCELLEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual AREA AREA1 { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual ESTIMATION_DIRECT ESTIMATION_DIRECT { get; set; }

        public virtual STOCK_CODE STOCK_CODE { get; set; }

        public virtual VARIATION VARIATION { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }
    }
}
