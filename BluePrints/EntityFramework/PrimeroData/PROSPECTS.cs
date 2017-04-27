namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class PROSPECTS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string NAME { get; set; }

        [StringLength(12)]
        public string POST_CODE { get; set; }

        [StringLength(30)]
        public string PHONE { get; set; }

        [StringLength(30)]
        public string FAX { get; set; }

        [StringLength(60)]
        public string EMAIL { get; set; }

        [StringLength(30)]
        public string WEBSITE { get; set; }

        [StringLength(60)]
        public string ADDRESS1 { get; set; }

        [StringLength(30)]
        public string ADDRESS2 { get; set; }

        [StringLength(30)]
        public string ADDRESS3 { get; set; }

        [StringLength(30)]
        public string ADDRESS4 { get; set; }

        [StringLength(30)]
        public string DELADDR1 { get; set; }

        [StringLength(30)]
        public string DELADDR2 { get; set; }

        [StringLength(30)]
        public string DELADDR3 { get; set; }

        [StringLength(30)]
        public string DELADDR4 { get; set; }

        [StringLength(30)]
        public string DELADDR5 { get; set; }

        [StringLength(30)]
        public string DELADDR6 { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        public int PRICENO { get; set; }

        public int? SALESNO { get; set; }

        public int PROSPECT_TYPE { get; set; }

        public int DRACCNO { get; set; }

        public int CRACCNO { get; set; }

        [StringLength(30)]
        public string ADDRESS5 { get; set; }

        [StringLength(15)]
        public string ALPHACODE { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        public int? DRACC_TEMPLATE { get; set; }

        [StringLength(20)]
        public string LINKEDIN { get; set; }

        [StringLength(500)]
        public string TWITTER { get; set; }

        [StringLength(500)]
        public string FACEBOOK { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }
    }
}