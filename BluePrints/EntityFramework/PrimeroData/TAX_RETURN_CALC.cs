namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TAX_RETURN_CALC
    {
        public int? CALC_ID { get; set; }

        [StringLength(1)]
        public string DRCR { get; set; }

        [StringLength(1)]
        public string ISALLOCATED { get; set; }

        [StringLength(5)]
        public string KEY_POINT { get; set; }

        public int? PYMT_SEQNO { get; set; }

        public DateTime? PYMT_TRANSDATE { get; set; }

        [StringLength(1)]
        public string ALLOCATED { get; set; }

        public double? ALLOCATEDBAL { get; set; }

        public int? ALLOC_COUNT { get; set; }

        public int? DEPOSIT_STATUS { get; set; }

        public int? ALLOCNO { get; set; }

        public int? TRANSTYPE { get; set; }

        public int? SEQNO { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public double? PYMT_ALLOC { get; set; }

        public double? TOT_INV_ALLOC { get; set; }

        public double? TOT_PYMT_ALLOC { get; set; }

        public double? INV_TOT { get; set; }

        public double? INV_ALLOC { get; set; }

        public int? LINE_SEQNO { get; set; }

        public double? LINE_ALLOC { get; set; }

        public double? CALC_NET { get; set; }

        public double? CALC_TAX { get; set; }

        public int? PYMT_AGE { get; set; }

        [Key]
        [Column(Order = 0)]
        public double INV_ALLOCATED { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIOD_SEQNO { get; set; }

        public int? AGE_STAMP { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? AGE { get; set; }
    }
}