namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_SJLINES
    {
        [Key]
        [Column(Order = 0)]
        public int SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int HDR_SEQNO { get; set; }

        public int? ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        [StringLength(30)]
        public string DETAILS { get; set; }

        [StringLength(12)]
        public string CHQNO { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        public double? AMOUNT { get; set; }

        public int? BRANCHNO { get; set; }

        public double FCAMOUNT { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int? SESSION_ID { get; set; }

        public int? POSTRUNSOURCE { get; set; }

        public int? SORTORDER { get; set; }

        public int? SOURCE_SEQ { get; set; }

        public int? SOURCE_ACCNO { get; set; }

        public int? JOBNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int? ACCOUNTTYPE { get; set; }

        public int? ERRORCODE { get; set; }

        public double? TOTALDISCOUNT { get; set; }

        public int? SECTIONID { get; set; }

        public int? LEDGERID { get; set; }

        public double? ROUNDAMOUNT { get; set; }

        public int? SOURCE_LINESEQNO { get; set; }

        public int SOURCE_INVLINEID { get; set; }

        [StringLength(3)]
        public string TRANSTYPE { get; set; }
    }
}