namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MANREP_PERIOD
    {
        public MANREP_PERIOD()
        {
            CRM_BUDGET = new HashSet<CRM_BUDGET>();
            CRM_BUDGET_HDR = new HashSet<CRM_BUDGET_HDR>();
        }

        [Key]
        public int PERIOD_SEQNO { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        [StringLength(20)]
        public string PERIOD_NAME { get; set; }

        public int? PARENT_PERIOD_SEQNO { get; set; }

        [StringLength(1)]
        public string CALENDAR { get; set; }

        public virtual ICollection<CRM_BUDGET> CRM_BUDGET { get; set; }

        public virtual ICollection<CRM_BUDGET_HDR> CRM_BUDGET_HDR { get; set; }
    }
}
