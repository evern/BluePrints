namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("OPPORTUNITY")]
    public partial class OPPORTUNITY
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(60)]
        public string DESCRIPTION { get; set; }

        public int OPPORTUNITY_TYPE { get; set; }

        public int OPPORTUNITY_LEAD { get; set; }

        public int OPPORTUNITY_STAGE { get; set; }

        public int? PROBABILITY { get; set; }

        public int? ASSIGNED_TO { get; set; }

        public int? ASSIGNED_BY { get; set; }

        [StringLength(50)]
        public string COMPANYID { get; set; }

        public int? CONTACTSEQNO { get; set; }

        public double? AMOUNT { get; set; }

        public DateTime? START_DATE { get; set; }

        public DateTime? DUE_DATE { get; set; }

        public DateTime? CLOSE_DATE { get; set; }

        [StringLength(4096)]
        public string DETAILS { get; set; }

        public int? CREATEDBY { get; set; }

        public DateTime? CREATEDATE { get; set; }

        public int? MODIFIEDBY { get; set; }

        public DateTime? MODIFIEDDATE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Required]
        [StringLength(1)]
        public string IS_CLOSE { get; set; }

        public double ESTIMATE { get; set; }

        public double LOST_VALUE { get; set; }

        public int? CURRENCYNO { get; set; }

        public double? EXCHRATE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? WEIGHTED_VALUE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? WEIGHTED_ESTIMATE { get; set; }

        public double CLOSED_VALUE { get; set; }

        public double TAXTOTAL { get; set; }

        public double TAXROUNDING { get; set; }

        public int? CAMPAIGN_WAVE_SEQNO { get; set; }

        public int? X_LENGTH { get; set; }

        public DateTime? X_STARTJOB { get; set; }

        [StringLength(200)]
        public string X_DOCSLINK { get; set; }

        public DateTime? X_TAP { get; set; }

        public int? X_PROJECT_VALUE { get; set; }
    }
}