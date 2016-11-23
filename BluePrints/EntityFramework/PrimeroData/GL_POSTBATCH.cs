namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_POSTBATCH
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        public int? COMPANYNO { get; set; }

        public int? BRANCHNO { get; set; }

        [StringLength(12)]
        public string CHQNO { get; set; }

        [StringLength(20)]
        public string INVNO { get; set; }

        [StringLength(30)]
        public string DETAILS { get; set; }

        public double? AMOUNT { get; set; }

        public double? TOTALDISCOUNT { get; set; }

        public int? SOURCE_SEQ { get; set; }

        public int? SOURCE_ACCNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public double? FCAMOUNT { get; set; }

        public int? SOURCE_LINE { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int? JOBNO { get; set; }

        public int? SESSION_ID { get; set; }
    }
}
