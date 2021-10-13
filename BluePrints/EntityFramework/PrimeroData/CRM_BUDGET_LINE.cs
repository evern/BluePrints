namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class CRM_BUDGET_LINE
    {
        [Key]
        public int SEQNO { get; set; }

        public int BUDGET_SEQNO { get; set; }

        public decimal VALUE { get; set; }

        public decimal? LYACTUAL { get; set; }

        public int? ACCGROUP { get; set; }

        public bool? ACCGROUP_EXPANSION { get; set; }

        public int? ACCGROUP2 { get; set; }

        public bool? ACCGROUP2_EXPANSION { get; set; }

        public int? ACCNO { get; set; }

        public bool? ACCNO_EXPANSION { get; set; }

        public int? STOCK_GROUPNO { get; set; }

        public bool? STOCK_GROUPNO_EXPANSION { get; set; }

        public int? STOCK_GROUPNO2 { get; set; }

        public bool? STOCK_GROUPNO2_EXPANSION { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public bool? STOCKCODE_EXPANSION { get; set; }

        public int? BRANCHNO { get; set; }

        public bool? BRANCHNO_EXPANSION { get; set; }

        public int? SALESNO { get; set; }

        public bool? SALESNO_EXPANSION { get; set; }

        public decimal? ACTUAL { get; set; }

        public virtual BRANCHES BRANCHES { get; set; }

        public virtual CRM_BUDGET CRM_BUDGET { get; set; }

        public virtual DR_ACCGROUP2S DR_ACCGROUP2S { get; set; }

        public virtual DR_ACCGROUPS DR_ACCGROUPS { get; set; }

        public virtual DR_ACCS DR_ACCS { get; set; }

        public virtual STAFF STAFF { get; set; }

        public virtual STOCK_GROUP2S STOCK_GROUP2S { get; set; }

        public virtual STOCK_GROUPS STOCK_GROUPS { get; set; }

        //public virtual STOCK_ITEMS STOCK_ITEMS { get; set; }
    }
}