namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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