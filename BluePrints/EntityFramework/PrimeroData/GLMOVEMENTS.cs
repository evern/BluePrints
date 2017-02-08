namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLMOVEMENTS
    {
        [Key]
        public int SEQNO { get; set; }

        public int ACCNO { get; set; }

        public int SUBACCNO { get; set; }

        public int BRANCHNO { get; set; }

        public int COMPANYNO { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public double AMOUNT { get; set; }

        public double AMOUNT_FC { get; set; }
    }
}