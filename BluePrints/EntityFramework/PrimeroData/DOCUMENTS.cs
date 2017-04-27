namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DOCUMENTS
    {
        [Key]
        public int SEQNO { get; set; }

        public DateTime? SAVEDATE { get; set; }

        public DateTime? DOCDATE { get; set; }

        [StringLength(50)]
        public string MODULE { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(20)]
        public string REFCODE { get; set; }

        [StringLength(20)]
        public string DOCCODE { get; set; }

        [StringLength(100)]
        public string DESCRIPTION { get; set; }

        [StringLength(100)]
        public string FILENAME { get; set; }

        [StringLength(200)]
        public string FILEPATH { get; set; }

        [Column(TypeName = "image")]
        public byte[] DOCDATA { get; set; }

        [StringLength(1)]
        public string DOCLINK { get; set; }

        public int JOBNO { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        [StringLength(23)]
        public string STOCKCODE { get; set; }

        public int? CAMPAIGN_SEQNO { get; set; }

        public int? OPPORTUNITY_SEQNO { get; set; }

        public int? PERIOD_SEQNO { get; set; }
    }
}