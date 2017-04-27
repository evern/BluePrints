namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class CAMPAIGN_WAVE
    {
        [Key]
        public int SEQNO { get; set; }

        public int? WAVE_NO { get; set; }

        public int CAMPAIGN_SEQNO { get; set; }

        [StringLength(50)]
        public string DESCRIPT { get; set; }

        public DateTime? STARTDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        public int? COMMUNICATION_METHOD { get; set; }

        [StringLength(150)]
        public string TRACKER_KEY { get; set; }

        [StringLength(150)]
        public string OPT_IN_URL { get; set; }

        [StringLength(150)]
        public string OPT_OUT_URL { get; set; }

        [StringLength(150)]
        public string LANDING_SITE { get; set; }

        [StringLength(1)]
        public string PROCESSED { get; set; }

        [StringLength(1)]
        public string COMPLETE { get; set; }

        public int? DOC_BATCH_HDR_SEQNO { get; set; }

        [StringLength(500)]
        public string SOCIAL_MEDIA_TEXT { get; set; }

        [StringLength(100)]
        public string FACEBOOK_POST_ID { get; set; }

        [StringLength(100)]
        public string TWITTER_POST_ID { get; set; }

        public DateTime? LINKEDIN_POST_DATE { get; set; }

        [StringLength(4096)]
        public string SETTINGS { get; set; }
    }
}