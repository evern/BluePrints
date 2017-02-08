namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_SJHDR
    {
        [Key]
        [Column(Order = 0)]
        public int SEQNO { get; set; }

        [StringLength(80)]
        public string DESCRIPTION { get; set; }

        [StringLength(1)]
        public string TEMPONLY { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int? POSTRUNSEQNO { get; set; }

        public int? HDRSTATUS { get; set; }

        public int? PERIOD_SEQNO { get; set; }

        [StringLength(3)]
        public string INITIALS { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public DateTime? CREATEDDATE { get; set; }

        public int? HDRTYPE { get; set; }

        [StringLength(1)]
        public string AUTO_REVERSE { get; set; }

        public double? IMBALANCE { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LEDGERID { get; set; }

        public double? DRAMOUNT { get; set; }

        public double? CRAMOUNT { get; set; }

        public double? FC_DRAMOUNT { get; set; }

        public double? FC_CRAMOUNT { get; set; }
    }
}