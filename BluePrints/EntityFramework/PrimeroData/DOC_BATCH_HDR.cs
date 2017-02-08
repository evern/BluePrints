namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DOC_BATCH_HDR
    {
        [Key]
        public int SEQNO { get; set; }

        [StringLength(50)]
        public string DOC_TYPE { get; set; }

        public int? COMPUTER_SEQNO { get; set; }

        [StringLength(1)]
        public string PRIOR_PERIOD { get; set; }

        [StringLength(1)]
        public string EMAIL { get; set; }

        [StringLength(1)]
        public string TO_PRINTER { get; set; }

        [StringLength(100)]
        public string PRINTER_URL { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public string ATTACHMENT_URL { get; set; }

        [StringLength(255)]
        public string COVERING_LETTER_CLM { get; set; }

        [StringLength(255)]
        public string EMAIL_BODY_CLE { get; set; }

        public byte? EMAIL_MODE { get; set; }

        [StringLength(1)]
        public string EMAIL_BODY { get; set; }

        public int? PERIOD_SEQNO { get; set; }

        public string FILTERSQL { get; set; }

        public DateTime? BATCHRUNDATE { get; set; }

        public int BATCHRUNSEQNO { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        public int? CREATEDBY { get; set; }

        public DateTime? CREATED { get; set; }

        public int? CAMPAIGN_SEQNO { get; set; }
    }
}