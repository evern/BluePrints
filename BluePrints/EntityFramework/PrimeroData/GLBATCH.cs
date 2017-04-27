namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("GLBATCH")]
    public partial class GLBATCH
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BATCHNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int BRANCHNO { get; set; }

        [StringLength(1)]
        public string SOURCE { get; set; }

        [StringLength(255)]
        public string NARRATIVE { get; set; }

        public int STAFFNO { get; set; }

        [StringLength(3)]
        public string INITIALS { get; set; }

        public double? DRAMOUNT { get; set; }

        public double? CRAMOUNT { get; set; }

        public double? FC_DRAMOUNT { get; set; }

        public double? FC_CRAMOUNT { get; set; }

        [StringLength(1)]
        public string AUTO_REVERSE { get; set; }

        public int? COMPANYNO { get; set; }

        public int? REVERSED_IN_BATCHNO { get; set; }

        public int? REVERSAL_OF_BATCHNO { get; set; }

        public int? COPIED_FROM_BATCHNO { get; set; }

        [StringLength(1)]
        public string PRINTED { get; set; }

        public int? NARRATIVE_SEQNO { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? PERIODNO { get; set; }

        public int? POSTRUNSEQNO { get; set; }
    }
}