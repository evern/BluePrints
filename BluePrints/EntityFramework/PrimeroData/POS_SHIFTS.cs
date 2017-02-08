namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POS_SHIFTS
    {
        [Key]
        public int SHIFTNO { get; set; }

        [StringLength(30)]
        public string NAME { get; set; }

        public DateTime? TIMESTARTED { get; set; }

        [StringLength(30)]
        public string TERMINAL_ID { get; set; }

        public DateTime? TIMEENDED { get; set; }

        public int? BRANCHNO { get; set; }

        public int? STATUS { get; set; }

        public double SHIFT_TOTAL { get; set; }

        public DateTime? COUNT1_TIME { get; set; }

        public int? COUNT1_STAFF { get; set; }

        public double COUNT1_TOTAL { get; set; }

        public DateTime? COUNT2_TIME { get; set; }

        public int? COUNT2_STAFF { get; set; }

        public double COUNT2_TOTAL { get; set; }

        public double COUNT2_ADJUST { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }
    }
}