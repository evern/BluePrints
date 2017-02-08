namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CASHBOOKHEADER")]
    public partial class CASHBOOKHEADER
    {
        [Key]
        public int SEQNO { get; set; }

        public int? BANK_ACCNO { get; set; }

        public int? BANK_SUBACCNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(3)]
        public string INITIALS { get; set; }

        [StringLength(1)]
        public string CONSOLIDATE_GL_POSTINGS { get; set; }

        [StringLength(80)]
        public string DESCRIPTION { get; set; }

        [StringLength(1)]
        public string TEMPONLY { get; set; }

        [StringLength(30)]
        public string CONSOLIDATE_DETAILS { get; set; }

        [StringLength(12)]
        public string CHQNO { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? PERIODNO { get; set; }
    }
}