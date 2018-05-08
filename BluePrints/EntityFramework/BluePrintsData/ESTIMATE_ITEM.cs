namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ESTIMATE_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ORIGINAL { get; set; }

        public Guid? GUID_PSUBJOB { get; set; }

        public Guid? GUID_SUBJOB { get; set; }

        public Guid? GUID_ESTIMATE { get; set; }

        public Guid? GUID_WORKPACK { get; set; }

        public Guid? GUID_VARIATION { get; set; }

        public Guid? GUID_PHASE { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_SUBAREA { get; set; }

        public Guid? GUID_DEPARTMENT { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public Guid? GUID_COMMODITY_CODE { get; set; }

        public Guid? GUID_ESTIMATE_STOCK_CODE { get; set; }

        public Guid? GUID_BUDGET_STOCK_CODE { get; set; }

        public Guid? GUID_STOCK_GROUP { get; set; }

        [Required]
        public int DISCIPLINE_NUM { get; set; }

        public bool BY_DURATION { get; set; }

        [StringLength(1000)]
        public string NAME { get; set; }

        [StringLength(1000)]
        public string COMMENTS { get; set; }

        public decimal? BUDGET_TRUCK_PERCENTAGE { get; set; }

        public decimal? ESTIMATE_TRUCK_PERCENTAGE { get; set; }

        public decimal ESTIMATE_QUANTITY { get; set; }

        public decimal? BUDGET_QUANTITY { get; set; }

        public decimal DC_QUANTITY { get; set; }

        public decimal ESTIMATE_INSTALL_RATE { get; set; }

        public decimal BUDGET_INSTALL_RATE { get; set; }

        public EstimateProgressType PROGRESS_TYPE { get; set; }

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

        public virtual PHASE PHASE { get; set; }

        public virtual DEPARTMENT DEPARTMENT { get; set; }

        public virtual DISCIPLINE DISCIPLINE { get; set; }

        public virtual ESTIMATE ESTIMATE { get; set; }

        public virtual COMMODITY_CODE COMMODITY_CODE { get; set; }

        public virtual STOCK_CODE STOCK_CODE { get; set; }

        public virtual STOCK_CODE STOCK_CODE1 { get; set; }

        public virtual STOCK_GROUP STOCK_GROUP { get; set; }

        public virtual VARIATION VARIATION { get; set; }

        public virtual SUBJOB SUBJOB { get; set; }

        public virtual SUBJOB SUBJOB1 { get; set; }

        public virtual WORKPACK WORKPACK { get; set; }
    }
}
