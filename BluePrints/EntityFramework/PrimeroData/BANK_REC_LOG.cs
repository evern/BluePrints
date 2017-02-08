namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BANK_REC_LOG
    {
        [Key]
        public int SEQNO { get; set; }

        public int? ACCNO { get; set; }

        public int? SUBACCNO { get; set; }

        public int? RECONCILENO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public double? STMNT_OPENBAL { get; set; }

        public double? STMNT_CLOSEBAL { get; set; }

        public double? GL_BAL { get; set; }

        public double? DEPOSITS { get; set; }

        public double? PAYMENTS { get; set; }

        public DateTime? STMNT_DATE { get; set; }

        [StringLength(5)]
        public string RECTYPE { get; set; }

        [StringLength(120)]
        public string CSVFILE { get; set; }

        public int? GLTRANS_SEQNO { get; set; }

        [StringLength(1)]
        public string LOCKED { get; set; }

        [StringLength(1)]
        public string PRE6180 { get; set; }
    }
}