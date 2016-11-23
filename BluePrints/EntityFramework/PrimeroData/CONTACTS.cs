namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CONTACTS
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(4)]
        public string SALUTATION { get; set; }

        [StringLength(30)]
        public string FIRSTNAME { get; set; }

        [StringLength(30)]
        public string LASTNAME { get; set; }

        [StringLength(30)]
        public string TITLE { get; set; }

        [StringLength(30)]
        public string MOBILE { get; set; }

        [StringLength(30)]
        public string DIRECTPHONE { get; set; }

        [StringLength(30)]
        public string DIRECTFAX { get; set; }

        [StringLength(30)]
        public string HOMEPHONE { get; set; }

        [StringLength(60)]
        public string EMAIL { get; set; }

        [StringLength(4096)]
        public string NOTES { get; set; }

        [StringLength(30)]
        public string ADDRESS1 { get; set; }

        [StringLength(30)]
        public string ADDRESS2 { get; set; }

        [StringLength(30)]
        public string ADDRESS3 { get; set; }

        [StringLength(30)]
        public string ADDRESS4 { get; set; }

        [StringLength(30)]
        public string ADDRESS5 { get; set; }

        [StringLength(12)]
        public string POST_CODE { get; set; }

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

        [StringLength(1)]
        public string ISACTIVE { get; set; }

        [StringLength(1)]
        public string SUB1 { get; set; }

        [StringLength(1)]
        public string SUB2 { get; set; }

        [StringLength(1)]
        public string SUB3 { get; set; }

        [StringLength(1)]
        public string SUB4 { get; set; }

        [StringLength(1)]
        public string SUB5 { get; set; }

        [StringLength(1)]
        public string SUB6 { get; set; }

        [StringLength(1)]
        public string SUB7 { get; set; }

        [StringLength(1)]
        public string SUB8 { get; set; }

        [StringLength(1)]
        public string SUB9 { get; set; }

        [StringLength(1)]
        public string SUB10 { get; set; }

        [StringLength(1)]
        public string SUB11 { get; set; }

        [StringLength(1)]
        public string SUB12 { get; set; }

        [StringLength(1)]
        public string SUB13 { get; set; }

        [StringLength(1)]
        public string SUB14 { get; set; }

        [StringLength(1)]
        public string SUB15 { get; set; }

        [StringLength(1)]
        public string SUB16 { get; set; }

        [StringLength(1)]
        public string SUB17 { get; set; }

        [StringLength(1)]
        public string SUB18 { get; set; }

        [StringLength(1)]
        public string SUB19 { get; set; }

        [StringLength(1)]
        public string SUB20 { get; set; }

        public int? ADVERTSOURCE { get; set; }

        public int? SALESNO { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(61)]
        public string FULLNAME { get; set; }

        [StringLength(1)]
        public string SUB21 { get; set; }

        [StringLength(1)]
        public string SUB22 { get; set; }

        [StringLength(1)]
        public string SUB23 { get; set; }

        [StringLength(1)]
        public string SUB24 { get; set; }

        [StringLength(1)]
        public string SUB25 { get; set; }

        [StringLength(1)]
        public string SUB26 { get; set; }

        public int? COMPANY_ACCNO { get; set; }

        public int? COMPANY_ACCTYPE { get; set; }

        [StringLength(45)]
        public string MSN_ID { get; set; }

        [StringLength(45)]
        public string YAHOO_ID { get; set; }

        [StringLength(45)]
        public string SKYPE_ID { get; set; }

        public DateTime? LAST_UPDATED { get; set; }

        [Required]
        [StringLength(1)]
        public string SYNC_CONTACTS { get; set; }

        [StringLength(20)]
        public string LINKEDIN { get; set; }

        [StringLength(500)]
        public string TWITTER { get; set; }

        [StringLength(500)]
        public string FACEBOOK { get; set; }

        [StringLength(1)]
        public string OPTOUT_EMARKETING { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }
    }
}
