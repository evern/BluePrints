namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_JOBCOST_LINES_AUDIT
    {
        [Key]
        [Column(Order = 0)]
        public Guid GUID { get; set; }

        public int JOBCOST_LINES_SEQNO { get; set; }

        public string JOBCODE { get; set; }

        public string DISCIPLINE_CODE { get; set; }

        public string COMMODITY_CODE { get; set; }

        public string STOCK_CODE { get; set; }

        public string VARIATION_CODE { get; set; }

        public decimal? BUDGET_FROM { get; set; }

        public decimal? BUDGET_TO { get; set; }

        public DateTime? BUDGET_UPDATED { get; set; }

        public Guid? BUDGET_UPDATEDBY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        [StringLength(100)]
        public string CREATED_BY_USER { get; set; }

        [StringLength(100)]
        public string BUDGET_UPDATED_BY_USER { get; set; }
    }
}
