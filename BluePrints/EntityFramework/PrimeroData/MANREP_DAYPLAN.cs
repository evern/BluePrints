namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MANREP_DAYPLAN
    {
        [Key]
        [Column(Order = 0)]
        public DateTime THEDATE { get; set; }

        public double? RUNRATE { get; set; }

        [StringLength(1)]
        public string WORKDAY { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIOD_SEQNO { get; set; }
    }
}