namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CAMPAIGN_HIST
    {
        [Key]
        public int SEQNO { get; set; }

        public int CAMPAIGN_SEQNO { get; set; }

        public int? SALESNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(80)]
        public string SUBJECT { get; set; }

        [StringLength(4096)]
        public string NOTE { get; set; }

        [StringLength(40)]
        public string OUTLOOK_LINK { get; set; }
    }
}
