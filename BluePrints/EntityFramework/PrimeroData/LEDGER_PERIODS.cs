namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LEDGER_PERIODS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [StringLength(20)]
        public string LEDGER { get; set; }

        public int? PERIOD_SEQNO { get; set; }

        public int DEF_POSTING_PERIOD { get; set; }

        public int CURRENT_YEAR { get; set; }

        public int? ANALYSIS_AGE_LIMIT { get; set; }
    }
}