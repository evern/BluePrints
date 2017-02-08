namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PAYRUN_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        public int PAY_NO { get; set; }

        public DateTime? PAY_PERIOD_ENDATE { get; set; }

        public DateTime? POSTTIME { get; set; }

        public int? NARRATIVE_SEQ { get; set; }

        public int? SESSION_ID { get; set; }

        public int? GJHDRID { get; set; }
    }
}