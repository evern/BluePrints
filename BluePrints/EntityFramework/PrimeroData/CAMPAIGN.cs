namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CAMPAIGN")]
    public partial class CAMPAIGN
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string TITLE { get; set; }

        [StringLength(15)]
        public string REFERENCE { get; set; }

        [StringLength(5000)]
        public string DESCRIPT { get; set; }

        public int? OWNER { get; set; }

        public DateTime? CREATEDATE { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        public DateTime? ENDDATE { get; set; }

        public int? CAMPAIGN_STAGE { get; set; }

        public double? EST_COST { get; set; }

        public double? EST_REVENUE { get; set; }

        public double? EST_RESPONSE { get; set; }

        public int CAMPAIGN_TYPE { get; set; }

        public int? JOB_LINK { get; set; }
    }
}