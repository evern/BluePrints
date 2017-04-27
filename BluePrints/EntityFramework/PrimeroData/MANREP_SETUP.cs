namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MANREP_SETUP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? VERSION_NO { get; set; }

        [StringLength(20)]
        public string PERIOD_NAME { get; set; }

        public int? PERIOD_WORK_DAYS { get; set; }

        public int? MAX_LOCS { get; set; }

        [StringLength(1)]
        public string EXCLUDE_QUOTES { get; set; }

        [StringLength(1)]
        public string USE_ACTUALDATE { get; set; }

        public DateTime? YEAR_STARTDATE { get; set; }

        public double? PERIOD_SALES_BUDGET { get; set; }

        public DateTime? PERIOD_STARTDATE { get; set; }

        public DateTime? PERIOD_STOPDATE { get; set; }
    }
}