namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class MANREP_BUDGET
    {
        [Key]
        public int BUDGET_SEQNO { get; set; }

        public double? BUDGETVALUE { get; set; }

        public double? MARGINPERCENT { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? BUDGET_CLASS { get; set; }

        public int? SCOPETYPE { get; set; }

        public int? SCOPEID { get; set; }
    }
}