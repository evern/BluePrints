namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DR_ALLOCATIONS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SEQNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ALLOCNO { get; set; }

        public int? TRANS_SEQNO { get; set; }

        public double? AMOUNT { get; set; }

        public int? CURRENCY { get; set; }

        [StringLength(1)]
        public string TAKENUP { get; set; }

        public DateTime? ALLOCTIME { get; set; }

        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }

        public double EXCHRATE { get; set; }
    }
}
