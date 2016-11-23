namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CL_END_OF_YEAR
    {
        [Key]
        public int SEQNO { get; set; }

        public int WIDGETID { get; set; }

        public int? CTX_SEQNO { get; set; }

        public int? END_OF_YEAR { get; set; }

        public int? END_OF_PERIOD__YEAR { get; set; }

        public int? LOCK_LEDGER_PERIODS { get; set; }

        public int? REVIEW_NEW_YEAR_SETUP { get; set; }

        public int? SETUP_ANALYTICS_YEARS { get; set; }

        public int? SETUP_ANALYTICS_MONTHS { get; set; }

        public int? SETUP_ANALYTICS_DAY_PLAN { get; set; }

        public int? SETUP_ANALYTICS_BUDGETS { get; set; }

        public int? SETUP_GL_BUDGETS { get; set; }
    }
}
