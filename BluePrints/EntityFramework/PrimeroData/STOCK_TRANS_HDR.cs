namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STOCK_TRANS_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime TRANSDATE { get; set; }

        public int TRANSTYPE { get; set; }

        [StringLength(30)]
        public string REFERENCE { get; set; }

        public int STAFFNO { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }
    }
}
