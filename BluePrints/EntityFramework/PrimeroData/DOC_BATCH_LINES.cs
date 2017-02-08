namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DOC_BATCH_LINES
    {
        [Key]
        public int SEQNO { get; set; }

        public int? COMPUTER_SEQNO { get; set; }

        public int? HDR_SEQNO { get; set; }

        public int? ACCNO { get; set; }

        [StringLength(1)]
        public string PRIOR_PERIOD { get; set; }

        [StringLength(1)]
        public string EMAIL { get; set; }

        [StringLength(1)]
        public string TO_PRINTER { get; set; }

        [StringLength(100)]
        public string PRINTER_URL { get; set; }

        [StringLength(255)]
        public string EMAIL_PRIMARY { get; set; }

        [StringLength(255)]
        public string EMAIL_CC { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public string ATTACHMENT_URL { get; set; }

        [StringLength(255)]
        public string COVERING_LETTER_CLM { get; set; }

        [StringLength(255)]
        public string EMAIL_BODY_CLE { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        public int BATCHRUNSEQNO { get; set; }

        [StringLength(31)]
        public string COMPANYID { get; set; }

        [Required]
        [StringLength(1)]
        public string OPTOUT { get; set; }
    }
}