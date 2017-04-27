namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class MANREP_BRANCH_BUDGET
    {
        [Key]
        public int SEQNO { get; set; }

        public int BUDGET_SEQNO { get; set; }

        public int BRANCHNO { get; set; }

        public double? BUDGETVALUE { get; set; }

        public double? MARGINPERCENT { get; set; }
    }
}