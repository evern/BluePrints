namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class CRM_BUDGET
    {
        public CRM_BUDGET()
        {
            CRM_BUDGET_LINE = new HashSet<CRM_BUDGET_LINE>();
            DR_ACCGROUP2S = new HashSet<DR_ACCGROUP2S>();
            DR_ACCGROUPS = new HashSet<DR_ACCGROUPS>();
            BRANCHES = new HashSet<BRANCHES>();
            DR_ACCS = new HashSet<DR_ACCS>();
            STAFF = new HashSet<STAFF>();
            STOCK_GROUP2S = new HashSet<STOCK_GROUP2S>();
            STOCK_GROUPS = new HashSet<STOCK_GROUPS>();
            STOCK_ITEMS = new HashSet<STOCK_ITEMS>();
        }

        [Key]
        public int SEQNO { get; set; }

        [Required]
        [StringLength(1024)]
        public string NAME { get; set; }

        public int HEADER_SEQNO { get; set; }

        public int MANREP_PERIOD_SEQNO { get; set; }

        public int? ACTUAL_PERIOD_STATUS_SEQNO { get; set; }

        public int? LYACTUAL_PERIOD_STATUS_SEQNO { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        public bool LEVEL_ACCGROUP { get; set; }

        public bool LEVEL_ACCGROUP2 { get; set; }

        public bool LEVEL_ACCNO { get; set; }

        public bool LEVEL_STOCKGROUP { get; set; }

        public bool LEVEL_STOCKGROUP2 { get; set; }

        public bool LEVEL_STOCKCODE { get; set; }

        public bool LEVEL_BRANCH { get; set; }

        public bool LEVEL_STAFF { get; set; }

        public DateTime? RECALC_LASTRUN { get; set; }

        public virtual CRM_BUDGET_HDR CRM_BUDGET_HDR { get; set; }

        public virtual ICollection<CRM_BUDGET_LINE> CRM_BUDGET_LINE { get; set; }

        public virtual MANREP_PERIOD MANREP_PERIOD { get; set; }

        public virtual PERIOD_STATUS PERIOD_STATUS { get; set; }

        public virtual PERIOD_STATUS PERIOD_STATUS1 { get; set; }

        public virtual ICollection<DR_ACCGROUP2S> DR_ACCGROUP2S { get; set; }

        public virtual ICollection<DR_ACCGROUPS> DR_ACCGROUPS { get; set; }

        public virtual ICollection<BRANCHES> BRANCHES { get; set; }

        public virtual ICollection<DR_ACCS> DR_ACCS { get; set; }

        public virtual ICollection<STAFF> STAFF { get; set; }

        public virtual ICollection<STOCK_GROUP2S> STOCK_GROUP2S { get; set; }

        public virtual ICollection<STOCK_GROUPS> STOCK_GROUPS { get; set; }

        public virtual ICollection<STOCK_ITEMS> STOCK_ITEMS { get; set; }
    }
}